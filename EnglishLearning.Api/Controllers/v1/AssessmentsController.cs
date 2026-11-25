using EnglishLearning.Application.Features.Assessments.DTOs;
using EnglishLearning.Application.Features.Assessments.Queries;
using EnglishLearning.Application.Features.Assessments.Commands;
using EnglishLearning.Domain.Common;
using MediatR;
using Microsoft.AspNetCore.DataProtection;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EnglishLearning.Api.Controllers.v1;

[Authorize]
public sealed class AssessmentsController : BaseApiController
{
    private readonly IMediator _mediator;
    private readonly ITimeLimitedDataProtector _attempts;

    public AssessmentsController(IMediator mediator, IDataProtectionProvider dataProtection)
    {
        _mediator = mediator;
        _attempts = dataProtection.CreateProtector("EnglishLearning.AssessmentAttempt.v1").ToTimeLimitedDataProtector();
    }

    [HttpGet("questions")]
    public async Task<IActionResult> GetQuestions(CancellationToken cancellationToken)
    {
        if (CurrentUserId is not int userId)
            return Unauthorized();
        var query = new GetAssessmentQuestionsQuery();
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(new AssessmentSessionDto(
            _attempts.Protect(AttemptContents(userId, result.Select(q => q.Id)), TimeSpan.FromMinutes(45)),
            result));
    }

    [HttpPost("submit")]
    public async Task<IActionResult> Submit([FromBody] SubmitAssessmentRequest request, CancellationToken cancellationToken)
    {
        if (CurrentUserId is null)
            return Unauthorized();

        try
        {
            if (_attempts.Unprotect(request.Attempt) != AttemptContents(CurrentUserId.Value, request.Respuestas.Select(q => q.PreguntaId)))
                return InvalidAttempt();
        }
        catch (CryptographicException)
        {
            return InvalidAttempt();
        }

        var command = new SubmitAssessmentCommand(CurrentUserId.Value, request.Respuestas);
        var result = await _mediator.Send(command, cancellationToken);
        return HandleResult(result);
    }

    [HttpGet("progress")]
    [HttpGet("progress/{usuarioId:int}")]
    public async Task<IActionResult> GetProgress(int? usuarioId, CancellationToken cancellationToken)
    {
        if (CurrentUserId is null)
            return Unauthorized();

        if (usuarioId.HasValue && usuarioId.Value != CurrentUserId.Value)
            return Forbid();

        var query = new GetProgressReportQuery(CurrentUserId.Value);
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }
    private static string AttemptContents(int userId, IEnumerable<int> ids) =>
        $"{userId}:{string.Join(",", ids.Order())}";

    private IActionResult InvalidAttempt() => HandleResult(Result<bool>.Failure(
        Error.Validation("Assessment.InvalidAttempt", "Completa todas las preguntas. Si el intento venció, vuelve a cargar la evaluación.")));
}