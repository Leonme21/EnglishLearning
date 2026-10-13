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

        if (string.IsNullOrWhiteSpace(_settings.ApiKey))
        {
            _logger.LogWarning("Brevo API Key no está configurada. Correo a {To} no enviado.", to);
            return;
        }

        try
        {
            var payload = new
            {
                sender = new
                {
                    name = _settings.FromName,
                    email = _settings.FromEmail
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

            request.Headers.Add("api-key", _settings.ApiKey.Trim());
            request.Headers.Add("accept", "application/json");

            var response = await _httpClient.SendAsync(request, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError("Error al enviar correo con Brevo. Status: {StatusCode}, Respuesta: {ErrorBody}", response.StatusCode, errorBody);
                throw new InvalidOperationException($"Error Brevo API: {response.StatusCode} - {errorBody}");
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
