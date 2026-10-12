using System.Security.Cryptography;
using EnglishLearning.Domain.Services;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace EnglishLearning.Infrastructure.Services;

public sealed class OtpService : IOtpService
{
    private readonly IMemoryCache _cache;
    private readonly ILogger<OtpService> _logger;
    private static readonly TimeSpan DefaultOtpLifetime = TimeSpan.FromMinutes(5);

    public OtpService(IMemoryCache cache, ILogger<OtpService> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    public string GenerateOtp(string email)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(email);

        var normalizedEmail = NormalizeEmail(email);
        var code = RandomNumberGenerator.GetInt32(100000, 1000000).ToString();

        var cacheKey = GetCacheKey(normalizedEmail);
        _cache.Set(cacheKey, code, new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = DefaultOtpLifetime
        });

        _logger.LogInformation("Código OTP generado satisfactoriamente para {Email} con expiración de {Minutes} minutos.",
            normalizedEmail, DefaultOtpLifetime.TotalMinutes);

        return code;
    }

    public bool ValidateOtp(string email, string code)
    {
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(code))
            return false;

        var normalizedEmail = NormalizeEmail(email);
        var trimmedCode = code.Trim();

        var cacheKey = GetCacheKey(normalizedEmail);
        if (_cache.TryGetValue<string>(cacheKey, out var storedCode))
        {
            if (string.Equals(storedCode, trimmedCode, StringComparison.Ordinal))
            {
                // Código de un solo uso: consumir de inmediato
                _cache.Remove(cacheKey);
                _logger.LogInformation("Código OTP validado y consumido con éxito para {Email}.", normalizedEmail);
                return true;
            }
        }

        _logger.LogWarning("Intento fallido de validación OTP para {Email}.", normalizedEmail);
        return false;
    }

    public bool HasPendingOtp(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        return _cache.TryGetValue(GetCacheKey(NormalizeEmail(email)), out _);
    }

    private static string NormalizeEmail(string email) => email.Trim().ToLowerInvariant();

    private static string GetCacheKey(string normalizedEmail) => $"otp:{normalizedEmail}";
}
