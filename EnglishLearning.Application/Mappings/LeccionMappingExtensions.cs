using EnglishLearning.Application.Dtos;
using EnglishLearning.Domain.Entities;
using EnglishLearning.Domain.Enums;

namespace EnglishLearning.Application.Mappings;

public static class LeccionExtensions
{
    public static LeccionDto ToDto(this Leccion leccion)
    {
        return new LeccionDto
        {
            Id = leccion.Id,
            Titulo = leccion.Titulo,
            Descripcion = leccion.Descripcion,
            Nivel = leccion.NivelRequerido.ToString(),
            Ejercicios = leccion.Ejercicios.Select(e => e.ToDto()).ToList()
        };
    }

    public static EjercicioDto ToDto(this Ejercicio ejercicio)
    {
        return new EjercicioDto
        {
            Id = ejercicio.Id,
            Contenido = ejercicio.Contenido,
            Tipo = ejercicio.Tipo.ToString()
        };
    }
}
