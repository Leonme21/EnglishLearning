using EnglishLearning.Domain.Entities;
using EnglishLearning.Domain.Interfaces;
using EnglishLearning.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EnglishLearning.Infrastructure.Repositories.Auth;

public sealed class RefreshTokenQueryRepository : IRefreshTokenQueryRepository
{
    private readonly EnglishLearningDbContext _context;

    public RefreshTokenQueryRepository(EnglishLearningDbContext context) => _context = context;

    public async Task<RefreshToken?> ObtenerPorTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        return await _context.RefreshTokens
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Token == token, cancellationToken);
    }

    public async Task<IEnumerable<RefreshToken>> ObtenerPorUsuarioAsync(int usuarioId, CancellationToken cancellationToken = default)
    {
        return await _context.RefreshTokens
            .AsNoTracking()
            .Where(r => r.UsuarioId == usuarioId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<RefreshToken?> ObtenerActivoPorUsuarioAsync(int usuarioId, CancellationToken cancellationToken = default)
    {
        return await _context.RefreshTokens
            .AsNoTracking()
            .Where(r => r.UsuarioId == usuarioId && !r.Revoked && r.Expiry > DateTime.UtcNow)
            .OrderByDescending(r => r.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);
    }
}

public sealed class RefreshTokenCommandRepository : IRefreshTokenCommandRepository
{
    private readonly EnglishLearningDbContext _context;

    public RefreshTokenCommandRepository(EnglishLearningDbContext context) => _context = context;

    public async Task<RefreshToken> AgregarAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default)
    {
        await _context.RefreshTokens.AddAsync(refreshToken, cancellationToken);
        return refreshToken;
    }

    public Task<RefreshToken> ActualizarAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default)
    {
        _context.RefreshTokens.Update(refreshToken);
        return Task.FromResult(refreshToken);
    }

    public async Task RevocarAsync(string token, string? replacedByToken = null, CancellationToken cancellationToken = default)
    {
        var refreshToken = await _context.RefreshTokens
            .FirstOrDefaultAsync(r => r.Token == token, cancellationToken);

        if (refreshToken != null)
        {
            refreshToken.Revocar(replacedByToken);
            _context.RefreshTokens.Update(refreshToken);
        }
    }

    public async Task RevocarTodosPorUsuarioAsync(int usuarioId, CancellationToken cancellationToken = default)
    {
        var tokens = await _context.RefreshTokens
            .Where(r => r.UsuarioId == usuarioId && !r.Revoked)
            .ToListAsync(cancellationToken);

        foreach (var token in tokens)
        {
            token.Revocar();
        }

        _context.RefreshTokens.UpdateRange(tokens);
    }
}