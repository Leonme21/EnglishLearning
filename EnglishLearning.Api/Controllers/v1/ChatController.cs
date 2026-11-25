using System.ComponentModel.DataAnnotations;
using EnglishLearning.Application.Abstractions;
using EnglishLearning.Domain.Enums;
using EnglishLearning.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EnglishLearning.Api.Controllers.v1;

[Authorize]
public sealed class ChatController(IChatService chat, IUsuarioQueryRepository users, ILogger<ChatController> logger) : BaseApiController
{
    [HttpPost("ask")]
    [RequestSizeLimit(50_000)]
    public Task<IActionResult> Ask(ChatRequest request, CancellationToken ct)
    {
        var history = request.History ?? [];
        if (string.IsNullOrWhiteSpace(request.Message) || history.Count > 8 || history.Count % 2 != 0 ||
            history.Where((turn, index) => turn is null || turn.IsStudent != (index % 2 == 0) ||
                string.IsNullOrWhiteSpace(turn.Text) || turn.Text.Length > 2_000).Any())
            return Task.FromResult(Invalid("El mensaje o la conversación no son válidos."));

        return WithLevelAsync(async level => new ChatResponse(await chat.SendMessageAsync(request.Message.Trim(), level, history, ct)), ct);
    }

    [HttpPost("production-feedback")]
    [RequestSizeLimit(1_650_000)]
    public Task<IActionResult> ProductionFeedback(ProductionFeedbackRequest request, CancellationToken ct)
    {
        if (!request.Consent)
            return Task.FromResult(Invalid("Autoriza el envío de tu muestra a Gemini para recibir retroalimentación."));

        var writing = string.IsNullOrWhiteSpace(request.Writing) ? null : request.Writing.Trim();
        if (writing is not null && writing.Length < 20)
            return Task.FromResult(Invalid("Escribe al menos 20 caracteres en inglés."));

        byte[]? audio = null;
        if (!string.IsNullOrWhiteSpace(request.AudioBase64))
        {
            try { audio = Convert.FromBase64String(request.AudioBase64); }
            catch (FormatException) { return Task.FromResult(Invalid("La grabación no tiene un formato válido.")); }
            if (!IsSupportedWav(audio))
                return Task.FromResult(Invalid("Envía audio WAV PCM mono de entre 1 y 30 segundos, a 16 kHz."));
        }
        if (writing is null && audio is null)
            return Task.FromResult(Invalid("Agrega una respuesta escrita o una grabación."));

        return WithLevelAsync(level => chat.EvaluateProductionAsync(writing, audio, level, ct), ct);
    }

    private static bool IsSupportedWav(byte[] bytes)
    {
        if (bytes.Length < 32_044 || bytes.Length > 960_044)
            return false;
        var wav = bytes.AsSpan();
        return wav[..4].SequenceEqual("RIFF"u8) && wav.Slice(8, 8).SequenceEqual("WAVEfmt "u8) &&
            wav.Slice(20, 8).SequenceEqual(new byte[] { 1, 0, 1, 0, 128, 62, 0, 0 }) &&
            wav.Slice(34, 6).SequenceEqual(new byte[] { 16, 0, 100, 97, 116, 97 }) &&
            System.Buffers.Binary.BinaryPrimitives.ReadUInt32LittleEndian(wav[40..]) == wav.Length - 44;
    }

    private async Task<IActionResult> WithLevelAsync<T>(Func<string, Task<T>> operation, CancellationToken ct)
    {
        if (CurrentUserId is not int id)
            return Unauthorized();
        var user = await users.ObtenerPorIdAsync(id, ct);
        if (user is null)
            return Unauthorized();

        var level = user.NivelActual == NivelIngles.SinAsignar ? "sin diagnóstico" : user.NivelActual.ToString();
        try { return Ok(await operation(level)); }
        catch (TutorUnavailableException error)
        {
            var correlationId = HttpContext.Items["CorrelationId"]?.ToString() ?? HttpContext.TraceIdentifier;
            logger.LogWarning("Tutor no disponible. CorrelationId: {CorrelationId}. Motivo: {Reason}", correlationId, error.Message);
            return StatusCode(503, new { error = new { code = "Tutor.Unavailable",
                message = "El tutor no está disponible. Intenta de nuevo en unos minutos.", correlationId } });
        }
    }

    private IActionResult Invalid(string message) =>
        BadRequest(new { error = new { code = "Tutor.InvalidInput", message } });
}

public sealed record ChatRequest([Required][MaxLength(2_000)] string Message, IReadOnlyList<TutorTurn>? History = null);
public sealed record ChatResponse(string Response);
public sealed record ProductionFeedbackRequest(bool Consent, [MaxLength(3_000)] string? Writing, [MaxLength(1_300_000)] string? AudioBase64);
