using System.ComponentModel.DataAnnotations;
using EnglishLearning.Domain.Enums;

namespace EnglishLearning.Application.Features.Assessments.DTOs;

public sealed record DiagnosticQuestionDto(
    int Id,
    string Habilidad,
    string Contenido,
    IReadOnlyList<string> Opciones,
    string Nivel
);

public sealed record AssessmentSessionDto(string Attempt, IReadOnlyList<DiagnosticQuestionDto> Questions);

public sealed record SubmitAssessmentRequest(
    [Required][StringLength(4096)] string Attempt,
    [Required][MinLength(8)][MaxLength(20)] IReadOnlyList<SubmitAnswerDto> Respuestas
);

public sealed record SubmitAnswerDto(
    [Required][Range(1, int.MaxValue)] int PreguntaId,
    [Required][MinLength(1)][MaxLength(2000)] string RespuestaTexto
);

public sealed record SubmitAssessmentResponse(
    string NivelAsignado,
    int PuntajeTotal,
    IReadOnlyDictionary<string, double> PuntajesPorHabilidad,
    string Mensaje,
    string SiguienteRecomendacion
);

public sealed record ProgressReportDto(
    int UsuarioId,
    string NivelActual,
    IReadOnlyList<LevelProgressDto> Progresos,
    IReadOnlyDictionary<string, SkillMetricDto> MetricasPorHabilidad,
    IReadOnlyList<string> Recomendaciones,
    SkillMetricDto Practice,
    PracticeRecommendationDto? SuggestedPractice
);

public sealed record LevelProgressDto(
    string Nivel,
    double PorcentajeCompletado,
    int LeccionesCompletadas,
    int TotalLecciones
);

public sealed record SkillMetricDto(
    int Aciertos,
    int Total,
    double Porcentaje
);
public sealed record PracticeRecommendationDto(string Habilidad, string Titulo, string Ruta);
