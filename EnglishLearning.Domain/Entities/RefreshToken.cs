using EnglishLearning.Domain.Common;
using EnglishLearning.Domain.ValueObjects;

namespace EnglishLearning.Domain.Entities;

public class RefreshToken : Entity
{
    public int UsuarioId { get; private set; }
    public string Token { get; private set; } = null!;
    public DateTime Expiry { get; private set; }
    public bool Revoked { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? RevokedAt { get; private set; }
    public string? ReplacedByToken { get; private set; }

    private RefreshToken() { } // EF Core

    public RefreshToken(int id, int usuarioId, string token, DateTime expiry) : base(id)
    {
        UsuarioId = usuarioId;
        Token = token ?? throw new ArgumentNullException(nameof(token));
        Expiry = expiry;
        Revoked = false;
        CreatedAt = DateTime.UtcNow;
    }

    public static Result<RefreshToken> Create(int id, int usuarioId, string token, DateTime expiry)
    {
        if (usuarioId <= 0)
            return Error.Validation("RefreshToken.InvalidUserId", "UsuarioId must be greater than zero");
        if (string.IsNullOrWhiteSpace(token))
            return Error.Validation("RefreshToken.EmptyToken", "Token cannot be empty");
        if (expiry <= DateTime.UtcNow)
            return Error.Validation("RefreshToken.InvalidExpiry", "Expiry must be in the future");

        return new RefreshToken(id, usuarioId, token, expiry);
    }

    public void Revocar(string? replacedByToken = null)
    {
        Revoked = true;
        RevokedAt = DateTime.UtcNow;
        ReplacedByToken = replacedByToken;
    }

    public bool EstaValido()
    {
        return !Revoked && Expiry > DateTime.UtcNow;
    }
}