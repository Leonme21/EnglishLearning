namespace EnglishLearning.Application.Features.Lessons.DTOs;

public sealed record LessonDto(
    int Id,
    string Titulo,
    string Descripcion,
    string Nivel,
    IReadOnlyList<ExerciseDto> Ejercicios
);

public sealed record ExerciseDto(
    int Id,
    string Contenido,
    string Tipo
);

public sealed record ExerciseDetailDto(
    int Id,
    int LeccionId,
    string Contenido,
    string Tipo,
    string RespuestaCorrecta
);