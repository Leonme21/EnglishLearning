using EnglishLearning.Domain.Entities;
using EnglishLearning.Domain.Enums;
using EnglishLearning.Domain.Interfaces;
using EnglishLearning.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EnglishLearning.Infrastructure.Repositories;

public class LeccionRepositorio : ILeccionRepositorio
{
    private readonly EnglishLearningDbContext _context;

    public LeccionRepositorio(EnglishLearningDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Leccion>> ObtenerPorNivelAsync(NivelIngles nivel)
    {
        return await _context.Lecciones
            .AsNoTracking()
            .Include(l => l.Ejercicios)
            .Where(l => l.NivelRequerido == nivel)
            .ToListAsync();
    }

    public async Task<Leccion?> ObtenerPorIdAsync(int id)
    {
        return await _context.Lecciones
            .AsNoTracking()
            .Include(l => l.Ejercicios)
            .FirstOrDefaultAsync(l => l.Id == id);
    }

    public async Task<IEnumerable<Ejercicio>> ObtenerEjerciciosPorLeccionAsync(int leccionId)
    {
        return await _context.Ejercicios
            .AsNoTracking()
            .Where(e => e.LeccionId == leccionId)
            .ToListAsync();
    }

    public async Task<Ejercicio?> ObtenerEjercicioPorIdAsync(int ejercicioId)
    {
        return await _context.Ejercicios
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == ejercicioId);
    }
}

public class UsuarioRepositorio : IUsuarioRepositorio
{
    private readonly EnglishLearningDbContext _context;

    public UsuarioRepositorio(EnglishLearningDbContext context)
    {
        _context = context;
    }

    public async Task<Usuario?> ObtenerPorIdAsync(int id)
    {
        return await _context.Usuarios
            .AsNoTracking()
            .Include(u => u.Progresos)
            .FirstOrDefaultAsync(u => u.Id == id);
    }

    public async Task ActualizarNivelAsync(int usuarioId, NivelIngles nuevoNivel)
    {
        var usuario = await _context.Usuarios.FindAsync(usuarioId);
        if (usuario != null)
        {
            usuario.NivelActual = nuevoNivel;
            await _context.SaveChangesAsync();
        }
    }

    public async Task GuardarRespuestaAsync(RespuestaUsuario respuesta)
    {
        _context.RespuestasUsuarios.Add(respuesta);
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<RespuestaUsuario>> ObtenerRespuestasRecientesAsync(int usuarioId, int cantidad)
    {
        return await _context.RespuestasUsuarios
            .AsNoTracking()
            .Where(r => r.UsuarioId == usuarioId)
            .OrderByDescending(r => r.FechaRealizacion)
            .Take(cantidad)
            .ToListAsync();
    }
}
