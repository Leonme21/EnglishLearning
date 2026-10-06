using System.ComponentModel.DataAnnotations;

namespace EnglishLearning.Application.Features.Users.DTOs;

public sealed record UserDto(
    int Id,
    string Nombre,
    string Correo,
    string NivelActual
);

public sealed record UserProfileDto(
    int Id,
    string Nombre,
    string Correo,
    string NivelActual,
    IReadOnlyList<LevelProgressDto> Progresos
);

public sealed record LevelProgressDto(
    string Nivel,
    double PorcentajeCompletado
);

public sealed record RegisterUserRequest(
    [Required][MinLength(2)][MaxLength(100)] string Nombre,
    [Required][EmailAddress][MaxLength(150)] string Correo,
    [Required][MinLength(8)][MaxLength(100)] string Password
);

public sealed record RegisterUserResponse(
    int Id,
    string Nombre,
    string Correo,
    string NivelActual
);