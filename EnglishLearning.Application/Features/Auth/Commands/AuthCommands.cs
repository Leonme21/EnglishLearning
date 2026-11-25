using EnglishLearning.Application.Features.Auth.DTOs;
using EnglishLearning.Domain.Common;
using MediatR;

namespace EnglishLearning.Application.Features.Auth.Commands;

public sealed record LoginCommand(
    string Email,
    string Password
) : IRequest<Result<AuthResponse>>;

public sealed record RegisterCommand(
    string Nombre,
    string Email,
    string Password
) : IRequest<Result<AuthResponse>>;

public sealed record RefreshTokenCommand(
    string RefreshToken
) : IRequest<Result<AuthResponse>>;

public sealed record RevokeTokenCommand(
    string RefreshToken
) : IRequest<Result<bool>>;

public sealed record VerifyOtpCommand(string ChallengeId, string Code) : IRequest<Result<AuthResponse>>;
public sealed record ResendOtpCommand(string ChallengeId) : IRequest<Result<bool>>;
public sealed record RequestPasswordResetCommand(string Email) : IRequest<Result<AuthResponse>>;
public sealed record ResetPasswordCommand(string ChallengeId, string Code, string Password) : IRequest<Result<bool>>;
