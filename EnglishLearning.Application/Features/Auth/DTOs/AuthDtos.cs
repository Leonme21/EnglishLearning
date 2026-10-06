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

public sealed record AuthResponse(
    bool Success,
    string? AccessToken,
    string? RefreshToken,
    DateTime? ExpiresAt,
    string? ErrorMessage,
    UserDto? User
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