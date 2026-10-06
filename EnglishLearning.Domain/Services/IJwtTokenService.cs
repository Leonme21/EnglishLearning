using EnglishLearning.Domain.Common;
using System.Security.Claims;

namespace EnglishLearning.Domain.Services;

public interface IJwtTokenService
{
    string GenerateAccessToken(int userId, string email, string nivelActual);
    string GenerateRefreshToken();
    Task<string> GenerateAndStoreRefreshTokenAsync(int userId, CancellationToken cancellationToken = default);
    Task<bool> ValidateRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default);
    Task<bool> RevokeRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default);
    Task<string?> RotateRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default);
    ClaimsPrincipal? ValidateToken(string token);
    int? GetUserIdFromToken(string token);
}