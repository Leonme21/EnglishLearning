using System.Net.Http.Json;
using EnglishLearning.Application.Abstractions;
using EnglishLearning.Application.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace EnglishLearning.Infrastructure.Messaging;

public sealed class BrevoEmailService : IEmailService
{
    private readonly HttpClient _httpClient;
    private readonly BrevoSettings _settings;
    private readonly ILogger<BrevoEmailService> _logger;

    public BrevoEmailService(
        HttpClient httpClient,
        IOptions<BrevoSettings> options,
        ILogger<BrevoEmailService> logger)
    {
        _httpClient = httpClient;
        _settings = options.Value;
        _logger = logger;
    }

    public async Task SendEmailAsync(string to, string subject, string body, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(to);
        ArgumentException.ThrowIfNullOrWhiteSpace(subject);

        var apiKey = !string.IsNullOrWhiteSpace(_settings.ApiKey) && _settings.ApiKey != "YOUR_BREVO_API_KEY"
            ? _settings.ApiKey
            : Environment.GetEnvironmentVariable("Brevo__ApiKey")
              ?? Environment.GetEnvironmentVariable("Brevo:ApiKey")
              ?? Environment.GetEnvironmentVariable("BREVO_API_KEY");

        if (string.IsNullOrWhiteSpace(apiKey))
        {
            _logger.LogError("Brevo API Key no está configurada en el servidor (Brevo__ApiKey). Correo a {To} no enviado.", to);
            throw new InvalidOperationException("La clave API de Brevo no está configurada en las variables de entorno del servidor (Brevo__ApiKey).");
        }

        var fromEmail = !string.IsNullOrWhiteSpace(_settings.FromEmail) && _settings.FromEmail != "YOUR_BREVO_FROM_EMAIL"
            ? _settings.FromEmail
            : Environment.GetEnvironmentVariable("Brevo__FromEmail")
              ?? Environment.GetEnvironmentVariable("Brevo:FromEmail")
              ?? Environment.GetEnvironmentVariable("BREVO_FROM_EMAIL")
              ?? "mleonme21@gmail.com";

        var fromName = !string.IsNullOrWhiteSpace(_settings.FromName)
            ? _settings.FromName
            : Environment.GetEnvironmentVariable("Brevo__FromName")
              ?? "English Learning Platform";

        try
        {
            var payload = new
            {
                sender = new
                {
                    name = fromName,
                    email = fromEmail
                },
                to = new[]
                {
                    new { email = to }
                },
                subject = subject,
                htmlContent = body
            };

            using var request = new HttpRequestMessage(HttpMethod.Post, "https://api.brevo.com/v3/smtp/email")
            {
                Content = JsonContent.Create(payload)
            };

            request.Headers.Add("api-key", apiKey.Trim());
            request.Headers.Add("accept", "application/json");

            var response = await _httpClient.SendAsync(request, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError("Error al enviar correo con Brevo. Status: {StatusCode}, Respuesta: {ErrorBody}", response.StatusCode, errorBody);
                throw new InvalidOperationException($"Error Brevo API ({response.StatusCode}): {errorBody}");
            }

            _logger.LogInformation("Correo enviado con éxito vía Brevo API a {To} (Asunto: {Subject})", to, subject);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fallo al enviar correo con Brevo API a {To}", to);
            throw;
        }
    }
}

