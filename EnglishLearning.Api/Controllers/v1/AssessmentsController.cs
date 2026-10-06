using EnglishLearning.Application.Features.Assessments.DTOs;
using EnglishLearning.Application.Features.Assessments.Queries;
using EnglishLearning.Application.Features.Assessments.Commands;
using EnglishLearning.Domain.Common;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace EnglishLearning.Api.Controllers.v1;

public sealed class AssessmentsController : BaseApiController
{
    private readonly IMediator _mediator;

    public AssessmentsController(IMediator mediator) => _mediator = mediator;

    [HttpGet("questions")]
    public async Task<IActionResult> GetQuestions(CancellationToken cancellationToken)
    {
        var query = new GetAssessmentQuestionsQuery();
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    [HttpPost("submit")]
    public async Task<IActionResult> Submit([FromBody] SubmitAssessmentRequest request, CancellationToken cancellationToken)
    {
        var command = new SubmitAssessmentCommand(request.UsuarioId, request.Respuestas);
        var result = await _mediator.Send(command, cancellationToken);
        return HandleResult(result);
    }

    [HttpGet("progress/{usuarioId:int}")]
    public async Task<IActionResult> GetProgress(int usuarioId, CancellationToken cancellationToken)
    {
        var query = new GetProgressReportQuery(usuarioId);
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }
}