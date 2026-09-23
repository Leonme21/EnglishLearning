using System.ComponentModel.DataAnnotations;
using EnglishLearning.Domain.Enums;

namespace EnglishLearning.Application.Dtos;

public class PreguntaDiagnosticaDto
{
    public int Id { get; set; }
    public string Habilidad { get; set; } = null!;
    public string Contenido { get; set; } = null!;
    public List<string> Opciones { get; set; } = new();
    public string Nivel { get; set; } = null!;
}

public class SubmitAssessmentDto
{
    [Range(1, int.MaxValue)] public int UsuarioId { get; set; }

    [MinLength(8, ErrorMessage = "Se requieren al menos 8 respuestas")]
    [MaxLength(20)] public List<RespuestaAssessmentDto> Respuestas { get; set; } = new();
}

public class RespuestaAssessmentDto
{
    [Range(1, int.MaxValue)] public int PreguntaId { get; set; }
    [Required][MinLength(1)][MaxLength(2000)] public string RespuestaTexto { get; set; } = null!;
}

public class ResultadoAssessmentDto
{
    public string NivelAsignado { get; set; } = null!;
    public int PuntajeTotal { get; set; }
    public Dictionary<string, double> PuntajesPorHabilidad { get; set; } = new();
    public string Mensaje { get; set; } = null!;
    public string SiguienteRecomendacion { get; set; } = null!;
}

public class ReporteProgresoDto
{
    public int UsuarioId { get; set; }
    public string NivelActual { get; set; } = null!;
    public List<ProgresoNivelDto> Progresos { get; set; } = new();
    public Dictionary<string, MetricaHabilidadDto> MetricasPorHabilidad { get; set; } = new();
    public List<string> Recomendaciones { get; set; } = new();
}

public class ProgresoNivelDto
{
    public string Nivel { get; set; } = null!;
    public double PorcentajeCompletado { get; set; }
}

public class MetricaHabilidadDto
{
    public int Aciertos { get; set; }
    public int Total { get; set; }
    public double Porcentaje { get; set; }
}
