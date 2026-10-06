using EnglishLearning.Application.Features.Assessments.DTOs;
using EnglishLearning.Application.Features.Assessments.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace EnglishLearning.Api.Controllers.v1;

public sealed class ProgressController : BaseApiController
{
    private readonly IMediator _mediator;

    public ProgressController(IMediator mediator) => _mediator = mediator;

    [HttpGet("{usuarioId:int}")]
    public async Task<IActionResult> GetProgress(int usuarioId, CancellationToken cancellationToken)
    {
        var query = new GetProgressReportQuery(usuarioId);
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }
}