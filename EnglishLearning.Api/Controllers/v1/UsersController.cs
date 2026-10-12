using EnglishLearning.Application.Features.Users.DTOs;
using EnglishLearning.Application.Features.Users.Queries;
using EnglishLearning.Domain.Common;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EnglishLearning.Api.Controllers.v1;

[Authorize]
public sealed class UsersController : BaseApiController
{
    private readonly IMediator _mediator;

    public UsersController(IMediator mediator) => _mediator = mediator;

    [HttpGet("me")]
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int? id, CancellationToken cancellationToken)
    {
        if (CurrentUserId is null)
            return Unauthorized();

        var query = new GetUserByIdQuery(CurrentUserId.Value);
        var result = await _mediator.Send(query, cancellationToken);
        return HandleResult(result);
    }

    [HttpGet("profile")]
    [HttpGet("{id:int}/profile")]
    public async Task<IActionResult> GetProfile(int? id, CancellationToken cancellationToken)
    {
        if (CurrentUserId is null)
            return Unauthorized();

        var query = new GetUserProfileQuery(CurrentUserId.Value);
        var result = await _mediator.Send(query, cancellationToken);
        return HandleResult(result);
    }
}