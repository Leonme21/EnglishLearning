using EnglishLearning.Application.Features.Assessments.DTOs;
using EnglishLearning.Application.Features.Assessments.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EnglishLearning.Api.Controllers.v1;

[Authorize]
public sealed class ProgressController : BaseApiController
{
    private readonly IMediator _mediator;

    public ProgressController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    [HttpGet("{usuarioId:int}")]
    public async Task<IActionResult> GetProgress(int? usuarioId, CancellationToken cancellationToken)
    {
        if (CurrentUserId is null)
            return Unauthorized();

        var query = new GetProgressReportQuery(CurrentUserId.Value);
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }
}