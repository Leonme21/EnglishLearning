using EnglishLearning.Application.Features.Assessments.DTOs;
using EnglishLearning.Application.Features.Assessments.Queries;
using EnglishLearning.Domain.Entities;
using EnglishLearning.Domain.Enums;
using EnglishLearning.Domain.Interfaces;
using EnglishLearning.Domain.Services;
using EnglishLearning.Domain.ValueObjects;
using Mapster;
using MediatR;
using System.Text.Json;

namespace EnglishLearning.Application.Features.Assessments.Queries;

public sealed class GetAssessmentQuestionsHandler : IRequestHandler<GetAssessmentQuestionsQuery, IReadOnlyList<DiagnosticQuestionDto>>
{
    private readonly IAssessmentQueryRepository _repository;

    public GetAssessmentQuestionsHandler(IAssessmentQueryRepository repository) => _repository = repository;

    public async Task<IReadOnlyList<DiagnosticQuestionDto>> Handle(GetAssessmentQuestionsQuery request, CancellationToken cancellationToken)
    {
        var questions = await _repository.ObtenerPreguntasAsync(cancellationToken);
        return questions.Adapt<IReadOnlyList<DiagnosticQuestionDto>>();
    }
}

public sealed class GetProgressReportHandler : IRequestHandler<GetProgressReportQuery, ProgressReportDto>
{
    private readonly IUsuarioQueryRepository _usuarioRepository;
    private readonly IAssessmentQueryRepository _assessmentRepository;
    private readonly IRecommendationEngine _recommendationEngine;

    public GetProgressReportHandler(
        IUsuarioQueryRepository usuarioRepository,
        IAssessmentQueryRepository assessmentRepository,
        IRecommendationEngine recommendationEngine)
    {
        _usuarioRepository = usuarioRepository;
        _assessmentRepository = assessmentRepository;
        _recommendationEngine = recommendationEngine;
    }

    public async Task<ProgressReportDto> Handle(GetProgressReportQuery request, CancellationToken cancellationToken)
    {
        var usuario = await _usuarioRepository.ObtenerPorIdAsync(request.UsuarioId, cancellationToken);
        if (usuario == null)
            throw new KeyNotFoundException("Usuario no encontrado");

        var respuestas = await _usuarioRepository.ObtenerRespuestasPorUsuarioAsync(request.UsuarioId, cancellationToken);
        var evaluacion = await _assessmentRepository.ObtenerUltimaEvaluacionAsync(request.UsuarioId, cancellationToken);

        // Parse skill scores from last evaluation
        var puntajesHab = new Dictionary<string, double>();
        if (evaluacion != null)
        {
            try
            {
                var parsed = JsonSerializer.Deserialize<Dictionary<string, double>>(evaluacion.PuntajesPorHabilidadJson);
                if (parsed != null) puntajesHab = parsed;
            }
            catch { }
        }

        // Calculate metrics by skill based on diagnostic responses (negative EjercicioId)
        var preguntas = (await _assessmentRepository.ObtenerPreguntasAsync(cancellationToken)).ToDictionary(p => p.Id);
        var metricas = new Dictionary<string, SkillMetricDto>();

        foreach (Habilidad h in Enum.GetValues<Habilidad>())
        {
            var ids = preguntas.Where(kv => kv.Value.Habilidad == h).Select(kv => kv.Key).ToHashSet();
            var deHabilidad = respuestas.Where(r => r.EjercicioId < 0 && ids.Contains(-r.EjercicioId)).ToList();
            var aciertos = deHabilidad.Count(x => x.EsCorrecta);
            var total = deHabilidad.Count;
            metricas[h.ToString()] = new SkillMetricDto(aciertos, total, total == 0 ? 0 : Math.Round((double)aciertos / total * 100, 1));
        }

        // Generate recommendations
        var recomendaciones = new List<string>();
        if (usuario.NivelActual != NivelIngles.SinAsignar)
        {
            var nivel = usuario.NivelActual;
            var puntajesEnum = puntajesHab.ToDictionary(
                kv => Enum.Parse<Habilidad>(kv.Key),
                kv => Percentage.Create(kv.Value).Value);

            var recommendations = _recommendationEngine.GenerateRecommendations(nivel, puntajesEnum, 3);
            recomendaciones.AddRange(recommendations);
        }
        else
        {
            recomendaciones.Add("Completa el Placement Test para recibir plan personalizado.");
        }

        return new ProgressReportDto(
            UsuarioId: request.UsuarioId,
            NivelActual: usuario.NivelActual.ToString(),
            Progresos: usuario.Progresos.Select(p => new LevelProgressDto(p.Nivel.ToString(), p.PorcentajeCompletado)).ToList(),
            MetricasPorHabilidad: metricas,
            Recomendaciones: recomendaciones);
    }
}