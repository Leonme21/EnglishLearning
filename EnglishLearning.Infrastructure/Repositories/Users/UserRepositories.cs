using EnglishLearning.Domain.Entities;
using EnglishLearning.Domain.Enums;
using EnglishLearning.Domain.Interfaces;
using EnglishLearning.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EnglishLearning.Infrastructure.Repositories.Users;

public sealed class UsuarioQueryRepository : IUsuarioQueryRepository
{
    private readonly EnglishLearningDbContext _context;

    public UsuarioQueryRepository(EnglishLearningDbContext context) => _context = context;

    public async Task<Usuario?> ObtenerPorIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.Usuarios
            .AsNoTracking()
            .Include(u => u.Progresos)
            .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
    }

    public async Task<Usuario?> ObtenerPorCorreoAsync(string correo, CancellationToken cancellationToken = default)
    {
        return await _context.Usuarios
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Correo == correo.ToLowerInvariant(), cancellationToken);
    }

    public async Task<IEnumerable<RespuestaUsuario>> ObtenerRespuestasRecientesAsync(int usuarioId, int cantidad, CancellationToken cancellationToken = default)
    {
        return await _context.RespuestasUsuarios
            .AsNoTracking()
            .Where(r => r.UsuarioId == usuarioId)
            .OrderByDescending(r => r.FechaRealizacion)
            .Take(cantidad)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<RespuestaUsuario>> ObtenerRespuestasPorUsuarioAsync(int usuarioId, CancellationToken cancellationToken = default)
    {
        return await _context.RespuestasUsuarios
            .AsNoTracking()
            .Where(r => r.UsuarioId == usuarioId)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ExisteCorreoAsync(string correo, CancellationToken cancellationToken = default)
    {
        return await _context.Usuarios
            .AsNoTracking()
            .AnyAsync(u => u.Correo == correo.ToLowerInvariant(), cancellationToken);
    }
}

public sealed class UsuarioCommandRepository : IUsuarioCommandRepository
{
    private readonly EnglishLearningDbContext _context;

    public UsuarioCommandRepository(EnglishLearningDbContext context) => _context = context;

    public async Task<Usuario> AgregarAsync(Usuario usuario, CancellationToken cancellationToken = default)
    {
        await _context.Usuarios.AddAsync(usuario, cancellationToken);
        return usuario;
    }

    public Task<Usuario> ActualizarAsync(Usuario usuario, CancellationToken cancellationToken = default)
    {
        _context.Usuarios.Update(usuario);
        return Task.FromResult(usuario);
    }

    public async Task ActualizarNivelAsync(int usuarioId, NivelIngles nuevoNivel, CancellationToken cancellationToken = default)
    {
        var usuario = await _context.Usuarios.FindAsync(new object[] { usuarioId }, cancellationToken);
        if (usuario != null)
        {
            usuario.ActualizarNivel(nuevoNivel);
            _context.Usuarios.Update(usuario);
        }
    }

    public async Task GuardarRespuestaAsync(RespuestaUsuario respuesta, CancellationToken cancellationToken = default)
    {
        await _context.RespuestasUsuarios.AddAsync(respuesta, cancellationToken);
    }

    public async Task<ProgresoNivel> AgregarProgresoAsync(ProgresoNivel progreso, CancellationToken cancellationToken = default)
    {
        await _context.ProgresosNiveles.AddAsync(progreso, cancellationToken);
        return progreso;
    }

    public Task<ProgresoNivel> ActualizarProgresoAsync(ProgresoNivel progreso, CancellationToken cancellationToken = default)
    {
        _context.ProgresosNiveles.Update(progreso);
        return Task.FromResult(progreso);
    }

    public async Task ActualizarPasswordHashAsync(int usuarioId, string passwordHash, CancellationToken cancellationToken = default)
    {
        var usuario = await _context.Usuarios.FindAsync(new object[] { usuarioId }, cancellationToken);
        if (usuario != null)
        {
            usuario.ActualizarPasswordHash(passwordHash);
            _context.Usuarios.Update(usuario);
        }
    }

    public async Task ActualizarRefreshTokenAsync(int usuarioId, string refreshToken, DateTime expiry, CancellationToken cancellationToken = default)
    {
        var usuario = await _context.Usuarios.FindAsync(new object[] { usuarioId }, cancellationToken);
        if (usuario != null)
        {
            usuario.ActualizarRefreshToken(refreshToken, expiry);
            _context.Usuarios.Update(usuario);
        }
    }

    public async Task RevocarRefreshTokenAsync(int usuarioId, CancellationToken cancellationToken = default)
    {
        var usuario = await _context.Usuarios.FindAsync(new object[] { usuarioId }, cancellationToken);
        if (usuario != null)
        {
            usuario.RevocarRefreshToken();
            _context.Usuarios.Update(usuario);
        }
    }
}