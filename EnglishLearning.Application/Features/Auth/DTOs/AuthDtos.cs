using System.ComponentModel.DataAnnotations;

namespace EnglishLearning.Application.Features.Auth.DTOs;

public sealed record LoginRequest(
    [Required][EmailAddress] string Email,
    [Required][MinLength(1)] string Password
);

public sealed record RegisterRequest(
    [Required][MinLength(2)][MaxLength(100)] string Nombre,
    [Required][EmailAddress][MaxLength(150)] string Email,
    [Required][MinLength(8)][MaxLength(100)] string Password
);

public sealed record RefreshTokenRequest(
    [Required] string RefreshToken
);

public sealed record RevokeTokenRequest(
    [Required] string RefreshToken
);

public sealed record VerifyOtpRequest(
    [Required][StringLength(48, MinimumLength = 32)] string ChallengeId,
    [Required][RegularExpression("^[0-9]{6}$")] string Code);

public sealed record ResendOtpRequest(
    [Required][StringLength(48, MinimumLength = 32)] string ChallengeId);

public sealed record RequestPasswordResetRequest(
    [Required][EmailAddress][MaxLength(150)] string Email);

public sealed record ResetPasswordRequest(
    [Required][StringLength(48, MinimumLength = 32)] string ChallengeId,
    [Required][RegularExpression("^[0-9]{6}$")] string Code,
    [Required] string Password);

public sealed record AuthResponse(
    bool Success,
    string? AccessToken,
    string? RefreshToken,
    DateTime? ExpiresAt,
    string? ErrorMessage,
    UserDto? User,
    bool RequiresOtp = false,
    string? Email = null,
    string? Message = null,
    string? ChallengeId = null
);

public sealed record UserDto(
    int Id,
    string Nombre,
    string Email,
    string NivelActual
);

public sealed record TokenDto(
    string AccessToken,
    string RefreshToken,
    DateTime ExpiresAt
);