using System.ComponentModel.DataAnnotations;

namespace EnglishLearning.Application.Features.Lessons.DTOs;

public sealed record EvaluateExerciseRequest(
    [Required][Range(1, int.MaxValue)] int UsuarioId,
    [Required][Range(1, int.MaxValue)] int EjercicioId,
    [Required][MinLength(1)][MaxLength(2000)] string RespuestaTexto
);

public sealed record EvaluateExerciseResponse(
    bool EsCorrecta,
    string Mensaje,
    string? NuevoNivel
);