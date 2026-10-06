using EnglishLearning.Domain.Entities;
using EnglishLearning.Domain.Enums;
using EnglishLearning.Domain.Interfaces;
using EnglishLearning.Infrastructure.Persistence;
using EnglishLearning.Infrastructure.Repositories.Base;
using Microsoft.EntityFrameworkCore;

namespace EnglishLearning.Infrastructure.Repositories.Lessons;

public sealed class LeccionQueryRepository : QueryRepository<Leccion>, ILeccionQueryRepository
{
    public LeccionQueryRepository(EnglishLearningDbContext context) : base(context) { }

    public async Task<IEnumerable<Leccion>> ObtenerPorNivelAsync(NivelIngles nivel, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Include(l => l.Ejercicios)
            .Where(l => l.NivelRequerido == nivel)
            .ToListAsync(cancellationToken);
    }

    public async Task<Leccion?> ObtenerPorIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Include(l => l.Ejercicios)
            .FirstOrDefaultAsync(l => l.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<Ejercicio>> ObtenerEjerciciosPorLeccionAsync(int leccionId, CancellationToken cancellationToken = default)
    {
        return await _context.Ejercicios
            .AsNoTracking()
            .Where(e => e.LeccionId == leccionId)
            .ToListAsync(cancellationToken);
    }

    public async Task<Ejercicio?> ObtenerEjercicioPorIdAsync(int ejercicioId, CancellationToken cancellationToken = default)
    {
        return await _context.Ejercicios
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == ejercicioId, cancellationToken);
    }
}

public sealed class LeccionCommandRepository : ILeccionCommandRepository
{
    private readonly EnglishLearningDbContext _context;

    public LeccionCommandRepository(EnglishLearningDbContext context)
    {
        _context = context;
    }

    public async Task<Leccion> AgregarAsync(Leccion leccion, CancellationToken cancellationToken = default)
    {
        await _context.Lecciones.AddAsync(leccion, cancellationToken);
        return leccion;
    }

    public Task<Leccion> ActualizarAsync(Leccion leccion, CancellationToken cancellationToken = default)
    {
        _context.Lecciones.Update(leccion);
        return Task.FromResult(leccion);
    }

    public async Task EliminarAsync(int id, CancellationToken cancellationToken = default)
    {
        var leccion = await _context.Lecciones.FindAsync(new object[] { id }, cancellationToken);
        if (leccion != null)
            _context.Lecciones.Remove(leccion);
    }

    public async Task<Ejercicio> AgregarEjercicioAsync(Ejercicio ejercicio, CancellationToken cancellationToken = default)
    {
        await _context.Ejercicios.AddAsync(ejercicio, cancellationToken);
        return ejercicio;
    }

    public Task<Ejercicio> ActualizarEjercicioAsync(Ejercicio ejercicio, CancellationToken cancellationToken = default)
    {
        _context.Ejercicios.Update(ejercicio);
        return Task.FromResult(ejercicio);
    }

    public async Task EliminarEjercicioAsync(int ejercicioId, CancellationToken cancellationToken = default)
    {
        var ejercicio = await _context.Ejercicios.FindAsync(new object[] { ejercicioId }, cancellationToken);
        if (ejercicio != null)
            _context.Ejercicios.Remove(ejercicio);
    }
}