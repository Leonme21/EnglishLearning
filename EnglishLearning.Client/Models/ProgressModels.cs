namespace EnglishLearning.Client.Models;

public record ProgressReportResponse(
    int UsuarioId,
    string NivelActual,
    List<LevelProgress> Progresos,
    Dictionary<string, SkillMetric> MetricasPorHabilidad,
    List<string> Recomendaciones,
    SkillMetric Practice,
    PracticeRecommendation? SuggestedPractice
);

public record LevelProgress(string Nivel, double PorcentajeCompletado, int LeccionesCompletadas, int TotalLecciones);
public record SkillMetric(int Aciertos, int Total, double Porcentaje);

public record PracticeRecommendation(string Habilidad, string Titulo, string Ruta);
