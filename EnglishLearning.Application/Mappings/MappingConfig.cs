using EnglishLearning.Application.Features.Assessments.DTOs;
using EnglishLearning.Application.Features.Lessons.DTOs;
using EnglishLearning.Application.Features.Users.DTOs;
using EnglishLearning.Domain.Entities;
using EnglishLearning.Domain.Enums;
using Mapster;

namespace EnglishLearning.Application.Mappings;

public static class MappingConfig
{
    public static void RegisterMappings()
    {
        // Lesson mappings
        TypeAdapterConfig<Leccion, LessonDto>
            .NewConfig()
            .Map(dest => dest.Nivel, src => src.NivelRequerido.ToString())
            .Map(dest => dest.Ejercicios, src => src.Ejercicios);

        TypeAdapterConfig<Ejercicio, ExerciseDto>
            .NewConfig()
            .Map(dest => dest.Tipo, src => src.Tipo.ToString());

        TypeAdapterConfig<Ejercicio, ExerciseDetailDto>
            .NewConfig()
            .Map(dest => dest.Tipo, src => src.Tipo.ToString());

        // Assessment mappings
        TypeAdapterConfig<PreguntaDiagnostica, DiagnosticQuestionDto>
            .NewConfig()
            .Map(dest => dest.Habilidad, src => src.Habilidad.ToString())
            .Map(dest => dest.Nivel, src => src.Nivel.ToString())
            .Map(dest => dest.Opciones, src => ParseOpciones(src.OpcionesJson));

        // User mappings
        TypeAdapterConfig<Usuario, UserDto>
            .NewConfig()
            .Map(dest => dest.NivelActual, src => src.NivelActual.ToString());

        TypeAdapterConfig<Usuario, UserProfileDto>
            .NewConfig()
            .Map(dest => dest.NivelActual, src => src.NivelActual.ToString())
            .Map(dest => dest.Progresos, src => src.Progresos);

        TypeAdapterConfig<ProgresoNivel, EnglishLearning.Application.Features.Users.DTOs.LevelProgressDto>
            .NewConfig()
            .Map(dest => dest.Nivel, src => src.Nivel.ToString());
    }

    private static IReadOnlyList<string> ParseOpciones(string opcionesJson)
    {
        try
        {
            return System.Text.Json.JsonSerializer.Deserialize<List<string>>(opcionesJson) ?? new List<string>();
        }
        catch
        {
            return new List<string>();
        }
    }

    private static IReadOnlyDictionary<string, double> ParsePuntajes(string puntajesJson)
    {
        try
        {
            return System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, double>>(puntajesJson) ?? new Dictionary<string, double>();
        }
        catch
        {
            return new Dictionary<string, double>();
        }
    }
}