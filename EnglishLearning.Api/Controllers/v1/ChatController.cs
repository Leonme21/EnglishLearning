using System.ComponentModel.DataAnnotations;
using EnglishLearning.Application.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EnglishLearning.Api.Controllers.v1;

[Authorize]
public sealed class ChatController : BaseApiController
{
    private readonly IChatService _chatService;
    private readonly ILogger<ChatController> _logger;

    public ChatController(IChatService chatService, ILogger<ChatController> logger)
    {
        _chatService = chatService;
        _logger = logger;
    }

    [HttpPost("ask")]
    public async Task<IActionResult> Ask([FromBody] ChatRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Message))
        {
            return BadRequest(new
            {
                error = new
                {
                    code = "VALIDATION_ERROR",
                    message = "El mensaje no puede estar vacío."
                }
            });
        }

        var userId = CurrentUserId;
        _logger.LogInformation("Processing chat query for user {UserId}", userId);

        var response = await _chatService.SendMessageAsync(request.Message, cancellationToken);

        return Ok(new ChatResponse(response));
    }
}

public sealed record ChatRequest(
    [Required(ErrorMessage = "El mensaje es requerido.")]
    string Message
);

public sealed record ChatResponse(string Response);
