using System.ComponentModel.DataAnnotations;

namespace EnglishLearning.Application.Dtos;

public class LeccionDto
{
    public int Id { get; set; }
    public string Titulo { get; set; } = null!;
    public string Descripcion { get; set; } = null!;
    public string Nivel { get; set; } = null!;
    public List<EjercicioDto> Ejercicios { get; set; } = new();
}

public class EjercicioDto
{
    public int Id { get; set; }
    public string Contenido { get; set; } = null!;
    public string Tipo { get; set; } = null!;
}

public class RespuestaDto
{
    [Range(1, int.MaxValue, ErrorMessage = "UsuarioId debe ser > 0")]
    public int UsuarioId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "EjercicioId debe ser > 0")]
    public int EjercicioId { get; set; }

    [Required(ErrorMessage = "RespuestaTexto es requerida")]
    [MinLength(1, ErrorMessage = "RespuestaTexto no puede estar vacía")]
    [MaxLength(2000, ErrorMessage = "RespuestaTexto excede 2000 caracteres")]
    public string RespuestaTexto { get; set; } = null!;
}

public class ResultadoEvaluacionDto
{
    public bool EsCorrecta { get; set; }
    public string Mensaje { get; set; } = null!;
    public string? NuevoNivel { get; set; }
}
