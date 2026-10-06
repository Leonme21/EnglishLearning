using EnglishLearning.Domain.Entities;
using EnglishLearning.Domain.Enums;
using EnglishLearning.Domain.Interfaces;
using EnglishLearning.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EnglishLearning.Infrastructure.Repositories.Assessments;

public sealed class AssessmentQueryRepository : IAssessmentQueryRepository
{
    private readonly EnglishLearningDbContext _context;

    public AssessmentQueryRepository(EnglishLearningDbContext context) => _context = context;

    public async Task<IEnumerable<PreguntaDiagnostica>> ObtenerPreguntasAsync(CancellationToken cancellationToken = default)
    {
        return await _context.PreguntasDiagnosticas
            .AsNoTracking()
            .OrderBy(p => p.Id)
            .ToListAsync(cancellationToken);
    }

    public async Task<PreguntaDiagnostica?> ObtenerPreguntaPorIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.PreguntasDiagnosticas
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<PreguntaDiagnostica>> ObtenerPreguntasPorHabilidadAsync(Habilidad habilidad, CancellationToken cancellationToken = default)
    {
        return await _context.PreguntasDiagnosticas
            .AsNoTracking()
            .Where(p => p.Habilidad == habilidad)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<PreguntaDiagnostica>> ObtenerPreguntasPorNivelAsync(NivelIngles nivel, CancellationToken cancellationToken = default)
    {
        return await _context.PreguntasDiagnosticas
            .AsNoTracking()
            .Where(p => p.Nivel == nivel)
            .ToListAsync(cancellationToken);
    }

    public async Task<EvaluacionDiagnostica?> ObtenerUltimaEvaluacionAsync(int usuarioId, CancellationToken cancellationToken = default)
    {
        return await _context.EvaluacionesDiagnosticas
            .AsNoTracking()
            .Where(e => e.UsuarioId == usuarioId)
            .OrderByDescending(e => e.Fecha)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IEnumerable<EvaluacionDiagnostica>> ObtenerEvaluacionesPorUsuarioAsync(int usuarioId, CancellationToken cancellationToken = default)
    {
        return await _context.EvaluacionesDiagnosticas
            .AsNoTracking()
            .Where(e => e.UsuarioId == usuarioId)
            .OrderByDescending(e => e.Fecha)
            .ToListAsync(cancellationToken);
    }
}

public sealed class AssessmentCommandRepository : IAssessmentCommandRepository
{
    private readonly EnglishLearningDbContext _context;

    public AssessmentCommandRepository(EnglishLearningDbContext context) => _context = context;

    public async Task<PreguntaDiagnostica> AgregarPreguntaAsync(PreguntaDiagnostica pregunta, CancellationToken cancellationToken = default)
    {
        await _context.PreguntasDiagnosticas.AddAsync(pregunta, cancellationToken);
        return pregunta;
    }

    public Task<PreguntaDiagnostica> ActualizarPreguntaAsync(PreguntaDiagnostica pregunta, CancellationToken cancellationToken = default)
    {
        _context.PreguntasDiagnosticas.Update(pregunta);
        return Task.FromResult(pregunta);
    }

    public async Task EliminarPreguntaAsync(int id, CancellationToken cancellationToken = default)
    {
        var pregunta = await _context.PreguntasDiagnosticas.FindAsync(new object[] { id }, cancellationToken);
        if (pregunta != null)
            _context.PreguntasDiagnosticas.Remove(pregunta);
    }

    public async Task GuardarEvaluacionAsync(EvaluacionDiagnostica evaluacion, CancellationToken cancellationToken = default)
    {
        await _context.EvaluacionesDiagnosticas.AddAsync(evaluacion, cancellationToken);
    }
}