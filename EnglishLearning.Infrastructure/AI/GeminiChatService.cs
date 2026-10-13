using System.Diagnostics;
using System.Net.Http.Json;
using System.Text.Json;
using EnglishLearning.Application.Abstractions;
using EnglishLearning.Application.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Polly;
using Polly.Wrap;

namespace EnglishLearning.Infrastructure.AI;

public sealed class GeminiChatService : IChatService
{
    private readonly HttpClient _httpClient;
    private readonly GeminiSettings _settings;
    private readonly ILogger<GeminiChatService> _logger;

    private const string SystemInstruction =
        "Eres el Tutor de Inglés con Inteligencia Artificial de EnglishLearning Platform. " +
        "Tu propósito es ayudar a los estudiantes a aprender y perfeccionar su inglés, adaptándote a su nivel (Marco Común Europeo: A1, A2, B1, B2, C1).\n\n" +
        "TUS CAPACIDADES Y OBJETIVOS PRINCIPALES:\n" +
        "1. ENSEÑANZA Y PEDAGOGÍA: Diseñar planes de estudio y mejora, explicar conceptos gramaticales de forma clara y con ejemplos prácticos, resolver dudas de vocabulario, diferencias entre palabras (ej. make vs do), falsos amigos y modismos.\n" +
        "2. CORRECCIÓN Y PRÁCTICA: Corregir oraciones y textos en inglés explicando el motivo del error amablemente, simular conversaciones naturales para práctica y dar consejos de pronunciación y fonética (sonidos IPA).\n" +
        "3. ORIENTACIÓN EN LA PLATAFORMA: Guiar al estudiante sobre cómo usar las lecciones, evaluaciones diagnósticas y reportes de progreso dentro de EnglishLearning Platform.\n" +
        "4. TONO: Cercano, motivador, didáctico y profesional. Responde en español o en inglés según prefiera o hable el estudiante.\n" +
        "5. PERÍMETRO: Mantente enfocado en la enseñanza del inglés y en esta plataforma. Si te piden temas totalmente ajenos (ej. recetas de cocina, política partidaria, escribir código en otros lenguajes), redirige amablemente la conversación hacia el aprendizaje de inglés.";

    public GeminiChatService(
        HttpClient httpClient,
        IOptions<GeminiSettings> options,
        ILogger<GeminiChatService> logger)
    {
        _httpClient = httpClient;
        _settings = options.Value;
        _logger = logger;
    }

    private static volatile string? _cachedWorkingModel;

    public async Task<string> SendMessageAsync(string message, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(message);

        var apiKey = ResolveApiKey();
        var primaryModel = GetEffectiveModel();
        var fallbackModel = !string.IsNullOrWhiteSpace(_settings.FallbackModel) && !_settings.FallbackModel.Contains("1.5") && !_settings.FallbackModel.Contains("2.0") && !_settings.FallbackModel.Contains("2.5")
            ? _settings.FallbackModel
            : "gemini-3.5-flash-lite";

        // Polly Resilience Pipeline: Fallback wrapping Retry policy
        var fallbackPolicy = Policy
            .HandleResult<HttpResponseMessage>(r => (int)r.StatusCode == 429 || (int)r.StatusCode >= 500)
            .Or<HttpRequestException>()
            .Or<TimeoutException>()
            .Or<TaskCanceledException>()
            .FallbackAsync(
                fallbackAction: async ct => await ExecuteHttpRequestAsync(apiKey, fallbackModel, message, ct),
                onFallbackAsync: async outcome =>
                {
                    var traceId = Activity.Current?.TraceId.ToString() 
                                  ?? Activity.Current?.Id 
                                  ?? Guid.NewGuid().ToString();

                    var statusCode = outcome.Result?.StatusCode.ToString() ?? "Exception";
                    var reason = outcome.Exception?.Message ?? $"HTTP Status {(int)(outcome.Result?.StatusCode ?? 0)}";

                    _logger.LogWarning(
                        "Fallback policy activated. Switching to secondary model '{FallbackModel}'. TraceId: {TraceId}. Reason: {Reason} (Status: {StatusCode})",
                        fallbackModel, traceId, reason, statusCode);

                    await Task.CompletedTask;
                });

        var retryPolicy = Policy
            .HandleResult<HttpResponseMessage>(r => (int)r.StatusCode == 429 || (int)r.StatusCode >= 500)
            .Or<HttpRequestException>()
            .Or<TimeoutException>()
            .WaitAndRetryAsync(
                retryCount: 2,
                sleepDurationProvider: attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)),
                onRetry: (outcome, timespan, attempt, context) =>
                {
                    _logger.LogWarning(
                        "Transient failure contacting Gemini on model '{Model}'. Retry attempt {Attempt} after {Seconds}s. Status: {StatusCode}",
                        primaryModel, attempt, timespan.TotalSeconds, outcome.Result?.StatusCode);
                });

        AsyncPolicyWrap<HttpResponseMessage> policyWrap = Policy.WrapAsync(fallbackPolicy, retryPolicy);

        HttpResponseMessage response = await policyWrap.ExecuteAsync(
            async ct => await ExecuteHttpRequestAsync(apiKey, primaryModel, message, ct),
            cancellationToken);

        // Si el modelo retorna 404 (modelo retirado o no soportado en esta versión de API), auto-descubrir modelos disponibles
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            _logger.LogWarning("Modelo '{Model}' retornó 404. Iniciando auto-descubrimiento de modelos compatibles...", primaryModel);
            var discoveredModel = await AutoDiscoverSupportedModelAsync(apiKey, cancellationToken);
            if (!string.IsNullOrEmpty(discoveredModel))
            {
                _logger.LogInformation("Reintentando con modelo auto-descubierto: '{DiscoveredModel}'", discoveredModel);
                response = await ExecuteHttpRequestAsync(apiKey, discoveredModel, message, cancellationToken);
            }
        }

        if (!response.IsSuccessStatusCode)
        {
            var errorBody = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogError("Error al invocar Google Gemini API. HTTP {StatusCode}. Respuesta: {ErrorBody}", response.StatusCode, errorBody);
            throw new HttpRequestException($"Error Gemini API ({response.StatusCode}): {errorBody}");
        }

        var responseJson = await response.Content.ReadAsStringAsync(cancellationToken);
        return ExtractTextFromResponse(responseJson);
    }

    private string ResolveApiKey()
    {
        var apiKey = !string.IsNullOrWhiteSpace(_settings.ApiKey) && _settings.ApiKey != "YOUR_GEMINI_API_KEY"
            ? _settings.ApiKey
            : Environment.GetEnvironmentVariable("Gemini__ApiKey")
              ?? Environment.GetEnvironmentVariable("Gemini:ApiKey")
              ?? Environment.GetEnvironmentVariable("GEMINI_API_KEY");

        if (string.IsNullOrWhiteSpace(apiKey) || apiKey == "YOUR_GEMINI_API_KEY")
        {
            _logger.LogError("Gemini API Key no está configurada en las variables de entorno (Gemini__ApiKey).");
            throw new InvalidOperationException("La clave API de Gemini (Gemini__ApiKey) no está configurada en el servidor.");
        }

        return apiKey.Trim();
    }

    private string GetEffectiveModel()
    {
        if (!string.IsNullOrEmpty(_cachedWorkingModel))
        {
            return _cachedWorkingModel;
        }

        if (!string.IsNullOrWhiteSpace(_settings.Model) && !_settings.Model.Contains("1.5") && !_settings.Model.Contains("2.0") && !_settings.Model.Contains("2.5"))
        {
            return _settings.Model;
        }

        return "gemini-3.8-flash";
    }

    private async Task<string?> AutoDiscoverSupportedModelAsync(string apiKey, CancellationToken cancellationToken)
    {
        try
        {
            var listUrl = $"https://generativelanguage.googleapis.com/v1beta/models?key={apiKey.Trim()}";
            using var response = await _httpClient.GetAsync(listUrl, cancellationToken);
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync(cancellationToken);
                using var doc = JsonDocument.Parse(json);
                if (doc.RootElement.TryGetProperty("models", out var modelsArray))
                {
                    var candidates = new List<string>();
                    foreach (var modelElement in modelsArray.EnumerateArray())
                    {
                        var name = modelElement.GetProperty("name").GetString() ?? "";
                        var supportsGenerate = false;
                        if (modelElement.TryGetProperty("supportedGenerationMethods", out var methods))
                        {
                            foreach (var m in methods.EnumerateArray())
                            {
                                if (m.GetString() == "generateContent")
                                {
                                    supportsGenerate = true;
                                    break;
                                }
                            }
                        }

                        if (supportsGenerate)
                        {
                            var cleanName = name.StartsWith("models/") ? name.Substring("models/".Length) : name;
                            candidates.Add(cleanName);
                        }
                    }

                    var validCandidates = candidates.Where(c => !c.Contains("1.5") && !c.Contains("2.0") && !c.Contains("2.5")).ToList();

                    var selected = validCandidates.FirstOrDefault(c => c.Contains("3.8-flash"))
                        ?? validCandidates.FirstOrDefault(c => c.Contains("flash"))
                        ?? validCandidates.FirstOrDefault(c => c.Contains("gemini"))
                        ?? "gemini-3.8-flash";

                    if (!string.IsNullOrEmpty(selected))
                    {
                        _logger.LogInformation("Modelo Gemini descubierto automáticamente: {SelectedModel}", selected);
                        _cachedWorkingModel = selected;
                        return selected;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error al auto-descubrir modelos disponibles en Google Gemini API.");
        }

        return "gemini-3.8-flash";
    }

    private async Task<HttpResponseMessage> ExecuteHttpRequestAsync(string apiKey, string model, string message, CancellationToken cancellationToken)
    {
        var url = $"https://generativelanguage.googleapis.com/v1beta/models/{model}:generateContent?key={apiKey.Trim()}";

        var payload = new
        {
            system_instruction = new
            {
                parts = new[] { new { text = SystemInstruction } }
            },
            contents = new[]
            {
                new
                {
                    parts = new[] { new { text = message } }
                }
            }
        };

        // Enforce max timeout per attempt
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        var timeout = _settings.TimeoutSeconds > 0 ? _settings.TimeoutSeconds : 25;
        cts.CancelAfter(TimeSpan.FromSeconds(timeout));

        return await _httpClient.PostAsJsonAsync(url, payload, cts.Token);
    }

    private static string ExtractTextFromResponse(string json)
    {
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        if (root.TryGetProperty("candidates", out var candidates) && candidates.GetArrayLength() > 0)
        {
            var candidate = candidates[0];
            if (candidate.TryGetProperty("content", out var content) &&
                content.TryGetProperty("parts", out var parts) && parts.GetArrayLength() > 0)
            {
                var text = parts[0].GetProperty("text").GetString();
                if (!string.IsNullOrWhiteSpace(text))
                {
                    return text.Trim();
                }
            }
        }

        throw new InvalidOperationException("Failed to extract response text from Gemini API output.");
    }
}
