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

public sealed class GetProgressReportHandler(
    IUsuarioQueryRepository users,
    IAssessmentQueryRepository assessments,
    ILeccionQueryRepository lessons,
    IRecommendationEngine recommendations) : IRequestHandler<GetProgressReportQuery, ProgressReportDto>
{
    public async Task<ProgressReportDto> Handle(GetProgressReportQuery request, CancellationToken ct)
    {
        var user = await users.ObtenerPorIdAsync(request.UsuarioId, ct)
            ?? throw new KeyNotFoundException("Usuario no encontrado.");
        var answers = (await users.ObtenerRespuestasPorUsuarioAsync(user.Id, ct)).ToArray();
        var practice = answers.Where(answer => answer.EjercicioId > 0).ToArray();
        var questions = (await assessments.ObtenerPreguntasAsync(ct)).ToDictionary(q => -q.Id, q => q.Habilidad);
        var diagnostic = answers.Where(answer => questions.ContainsKey(answer.EjercicioId))
            .GroupBy(answer => questions[answer.EjercicioId])
            .ToDictionary(group => group.Key.ToString(), group => ToMetric(group));

        var levels = new List<LevelProgressDto>();
        IReadOnlyList<Leccion> currentLessons = [];
        foreach (var level in user.Progresos.Select(progress => progress.Nivel).Append(user.NivelActual).Distinct().Order())
        {
            if (level == NivelIngles.SinAsignar)
                continue;

            var catalog = (await lessons.ObtenerPorNivelAsync(level, ct)).ToArray();
            var metrics = PracticeMetrics.ForLevel(catalog, practice);
            levels.Add(new LevelProgressDto(level.ToString(), metrics.CompletionPercentage, metrics.CompletedLessons.Count, metrics.LessonCount));
            if (level == user.NivelActual)
                currentLessons = catalog;
        }

        var recentAssessment = await assessments.ObtenerUltimaEvaluacionAsync(user.Id, ct);
        var scores = ReadSkillScores(recentAssessment);
        var suggested = Suggest(currentLessons, practice, scores);
        var advice = user.NivelActual == NivelIngles.SinAsignar
            ? ["Completa el diagnóstico para ver recomendaciones."]
            : recommendations.GenerateRecommendations(user.NivelActual, scores, 3);
        return new ProgressReportDto(user.Id, user.NivelActual.ToString(), levels, diagnostic,
            advice, ToMetric(practice), suggested);
    }

    private static SkillMetricDto ToMetric(IEnumerable<RespuestaUsuario> answers)
    {
        var rows = answers.ToArray();
        var correct = rows.Count(answer => answer.EsCorrecta);
        return new SkillMetricDto(correct, rows.Length, PracticeMetrics.Percent(correct, rows.Length));
    }

    private static IReadOnlyDictionary<Habilidad, Percentage> ReadSkillScores(EvaluacionDiagnostica? assessment)
    {
        var scores = new Dictionary<Habilidad, Percentage>();
        if (assessment is null)
            return scores;

        try
        {
            var source = JsonSerializer.Deserialize<Dictionary<string, double>>(assessment.PuntajesPorHabilidadJson);
            foreach (var (name, score) in source ?? [])
            {
                if (Enum.TryParse<Habilidad>(name, out var skill) && Enum.IsDefined(skill) &&
                    Percentage.Create(score) is { IsSuccess: true } value)
                    scores[skill] = value.GetValueOrThrow();
            }
        }
        catch (JsonException)
        {
            // Older records without readable scores can still show practice progress.
        }
        return scores;
    }

    private static PracticeRecommendationDto? Suggest(IReadOnlyList<Leccion> lessons,
        IReadOnlyList<RespuestaUsuario> answers, IReadOnlyDictionary<Habilidad, Percentage> diagnostic)
    {
        if (lessons.Count == 0)
            return null;

        var exerciseTypes = lessons.SelectMany(l => l.Ejercicios).ToDictionary(e => e.Id, e => SkillOf(e.Tipo));
        var scores = diagnostic.ToDictionary(pair => pair.Key, pair => pair.Value.Value);
        foreach (var group in answers.Where(a => exerciseTypes.ContainsKey(a.EjercicioId)).GroupBy(a => exerciseTypes[a.EjercicioId]))
            scores[group.Key] = ToMetric(group).Porcentaje;

        if (scores.Count == 0)
            return new PracticeRecommendationDto("Práctica general", "Explora tu catálogo", "/lessons");

        var weakest = scores.OrderBy(pair => pair.Value).ThenBy(pair => pair.Key).First().Key;
        var completed = PracticeMetrics.ForLevel(lessons, answers).CompletedLessons;
        var lesson = lessons.Where(l => l.Ejercicios.Any(e => SkillOf(e.Tipo) == weakest))
            .OrderBy(l => completed.Contains(l.Id)).ThenBy(l => l.Id).FirstOrDefault();
        return lesson is null
            ? new PracticeRecommendationDto(weakest.ToString(), "Practica con el tutor", "/chat")
            : new PracticeRecommendationDto(weakest.ToString(), lesson.Titulo, $"/lessons/{lesson.Id}");
    }

    private static Habilidad SkillOf(TipoEjercicio type) => type switch
    {
        TipoEjercicio.Escucha => Habilidad.Listening,
        TipoEjercicio.Escritura => Habilidad.Writing,
        _ => Habilidad.Reading
    };
}
