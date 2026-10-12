using EnglishLearning.Application.Features.Auth.DTOs;
using EnglishLearning.Application.Features.Auth.Commands;
using EnglishLearning.Application.Features.Users.Queries;
using EnglishLearning.Domain.Common;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EnglishLearning.Api.Controllers.v1;

public sealed class AuthController : BaseApiController
{
    private readonly IMediator _mediator;

    public AuthController(IMediator mediator) => _mediator = mediator;

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        var command = new LoginCommand(request.Email, request.Password);
        var result = await _mediator.Send(command, cancellationToken);
        return HandleResult(result);
    }

    [HttpPost("verify-otp")]
    public async Task<IActionResult> VerifyOtp([FromBody] VerifyOtpRequest request, CancellationToken cancellationToken)
    {
        var command = new VerifyOtpCommand(request.Email, request.Code);
        var result = await _mediator.Send(command, cancellationToken);
        return HandleResult(result);
    }

    [HttpPost("resend-otp")]
    public async Task<IActionResult> ResendOtp([FromBody] ResendOtpRequest request, CancellationToken cancellationToken)
    {
        var command = new ResendOtpCommand(request.Email);
        var result = await _mediator.Send(command, cancellationToken);
        return HandleResult(result);
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request, CancellationToken cancellationToken)
    {
        var command = new RegisterCommand(request.Nombre, request.Email, request.Password);
        var result = await _mediator.Send(command, cancellationToken);
        return HandleResult(result);
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequest request, CancellationToken cancellationToken)
    {
        var command = new RefreshTokenCommand(request.RefreshToken);
        var result = await _mediator.Send(command, cancellationToken);
        return HandleResult(result);
    }

    [HttpPost("revoke")]
    public async Task<IActionResult> Revoke([FromBody] RevokeTokenRequest request, CancellationToken cancellationToken)
    {
        var command = new RevokeTokenCommand(request.RefreshToken);
        var result = await _mediator.Send(command, cancellationToken);
        return HandleResult(result);
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> Me(CancellationToken cancellationToken)
    {
        if (CurrentUserId is null)
            return Unauthorized();

        var query = new GetUserProfileQuery(CurrentUserId.Value);
        var result = await _mediator.Send(query, cancellationToken);
        return HandleResult(result);
    }
}