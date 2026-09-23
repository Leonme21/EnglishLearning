using EnglishLearning.Domain.Enums;

namespace EnglishLearning.Domain.Entities;

public class PreguntaDiagnostica
{
    public int Id { get; set; }
    public Habilidad Habilidad { get; set; }
    public string Contenido { get; set; } = null!;
    public string OpcionesJson { get; set; } = "[]"; // JSON array para EF simple
    public string RespuestaCorrecta { get; set; } = null!;
    public NivelIngles Nivel { get; set; }
}

public class EvaluacionDiagnostica
{
    public int Id { get; set; }
    public int UsuarioId { get; set; }
    public DateTime Fecha { get; set; }
    public NivelIngles NivelAsignado { get; set; }
    public int PuntajeTotal { get; set; }
    public string PuntajesPorHabilidadJson { get; set; } = "{}";
}
