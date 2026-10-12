using EnglishLearning.Application.Abstractions;
using EnglishLearning.Application.Options;
using EnglishLearning.Domain.Entities;
using EnglishLearning.Domain.Interfaces;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace EnglishLearning.Infrastructure.Services;

public sealed class JwtTokenService : IJwtTokenService
{
    private readonly JwtOptions _options;
    private readonly SymmetricSecurityKey _signingKey;
    private readonly IRefreshTokenCommandRepository _refreshTokenCommandRepository;
    private readonly IRefreshTokenQueryRepository _refreshTokenQueryRepository;
    private readonly IUsuarioCommandRepository _usuarioCommandRepository;

    public JwtTokenService(
        IOptions<JwtOptions> options,
        IRefreshTokenCommandRepository refreshTokenCommandRepository,
        IRefreshTokenQueryRepository refreshTokenQueryRepository,
        IUsuarioCommandRepository usuarioCommandRepository)
    {
        _options = options.Value;
        _signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.SecretKey));
        _refreshTokenCommandRepository = refreshTokenCommandRepository;
        _refreshTokenQueryRepository = refreshTokenQueryRepository;
        _usuarioCommandRepository = usuarioCommandRepository;
    }

    public string GenerateAccessToken(int userId, string email, string nivelActual)
    {
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, email),
            new Claim("nivel", nivelActual),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var credentials = new SigningCredentials(_signingKey, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_options.AccessTokenExpirationMinutes),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public string GenerateRefreshToken()
    {
        var randomBytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes);
    }

    public async Task<string> GenerateAndStoreRefreshTokenAsync(int userId, CancellationToken cancellationToken = default)
    {
        var token = GenerateRefreshToken();
        var expiry = DateTime.UtcNow.AddDays(_options.RefreshTokenExpirationDays);

        await _refreshTokenCommandRepository.RevocarTodosPorUsuarioAsync(userId, cancellationToken);

        var refreshToken = RefreshToken.Create(0, userId, token, expiry).Value;
        await _refreshTokenCommandRepository.AgregarAsync(refreshToken, cancellationToken);

        await _usuarioCommandRepository.ActualizarRefreshTokenAsync(userId, token, expiry, cancellationToken);

        return token;
    }

    public async Task<bool> ValidateRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        var storedToken = await _refreshTokenQueryRepository.ObtenerPorTokenAsync(refreshToken, cancellationToken);
        
        if (storedToken == null || !storedToken.EstaValido())
            return false;

        return true;
    }

    public async Task<bool> RevokeRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        var storedToken = await _refreshTokenQueryRepository.ObtenerPorTokenAsync(refreshToken, cancellationToken);
        
        if (storedToken == null)
            return false;

        await _refreshTokenCommandRepository.RevocarAsync(refreshToken, cancellationToken: cancellationToken);
        
        await _usuarioCommandRepository.RevocarRefreshTokenAsync(storedToken.UsuarioId, cancellationToken);

        return true;
    }

    public async Task<string?> RotateRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        var storedToken = await _refreshTokenQueryRepository.ObtenerPorTokenAsync(refreshToken, cancellationToken);
        
        if (storedToken == null || !storedToken.EstaValido())
            return null;

        var userId = storedToken.UsuarioId;
        
        await _refreshTokenCommandRepository.RevocarAsync(refreshToken, cancellationToken: cancellationToken);

        var newToken = await GenerateAndStoreRefreshTokenAsync(userId, cancellationToken);

        return newToken;
    }

    public ClaimsPrincipal? ValidateToken(string token)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var validationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = _options.Issuer,
            ValidAudience = _options.Audience,
            IssuerSigningKey = _signingKey,
            ClockSkew = TimeSpan.Zero
        };

        try
        {
            var principal = tokenHandler.ValidateToken(token, validationParameters, out _);
            return principal;
        }
        catch
        {
            return null;
        }
    }

    public int? GetUserIdFromToken(string token)
    {
        var principal = ValidateToken(token);
        if (principal == null) return null;

        var userIdClaim = principal.FindFirst(JwtRegisteredClaimNames.Sub);
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
            return null;

        return userId;
    }
}