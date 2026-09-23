using System.Text.Json;
using EnglishLearning.Application.Dtos;
using EnglishLearning.Domain.Entities;

namespace EnglishLearning.Application.Mappings;

public static class AssessmentMappingExtensions
{
    public static PreguntaDiagnosticaDto ToDto(this PreguntaDiagnostica p)
    {
        List<string> opciones;
        try { opciones = JsonSerializer.Deserialize<List<string>>(p.OpcionesJson) ?? new(); }
        catch { opciones = new(); }
        return new PreguntaDiagnosticaDto
        {
            Id = p.Id,
            Habilidad = p.Habilidad.ToString(),
            Contenido = p.Contenido,
            Opciones = opciones,
            Nivel = p.Nivel.ToString()
        };
    }
}
