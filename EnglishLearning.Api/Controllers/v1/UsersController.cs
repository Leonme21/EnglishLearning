using EnglishLearning.Application.Features.Users.DTOs;
using EnglishLearning.Application.Features.Users.Queries;
using EnglishLearning.Application.Features.Users.Commands;
using EnglishLearning.Domain.Common;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace EnglishLearning.Api.Controllers.v1;

public sealed class UsersController : BaseApiController
{
    private readonly IMediator _mediator;

    public UsersController(IMediator mediator) => _mediator = mediator;

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var query = new GetUserByIdQuery(id);
        var result = await _mediator.Send(query, cancellationToken);
        return HandleResult(result);
    }

    [HttpGet("{id:int}/profile")]
    public async Task<IActionResult> GetProfile(int id, CancellationToken cancellationToken)
    {
        var query = new GetUserProfileQuery(id);
        var result = await _mediator.Send(query, cancellationToken);
        return HandleResult(result);
    }
}