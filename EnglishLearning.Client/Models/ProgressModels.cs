namespace EnglishLearning.Client.Models;

public record ProgressReportResponse(
    int UsuarioId,
    string NivelActual,
    List<LevelProgress> Progresos,
    Dictionary<string, SkillMetric> MetricasPorHabilidad,
    List<string> Recomendaciones
);

public record LevelProgress(string Nivel, double PorcentajeCompletado);
public record SkillMetric(int Aciertos, int Total, double Porcentaje);
