using EnglishLearning.Domain.Enums;

namespace EnglishLearning.Domain.Entities;

public class Usuario
{
    public int Id { get; set; }
    public string Nombre { get; set; } = null!;
    public string Correo { get; set; } = null!;
    public NivelIngles NivelActual { get; set; }
    public List<ProgresoNivel> Progresos { get; set; } = new();
}

public class Leccion
{
    public int Id { get; set; }
    public string Titulo { get; set; } = null!;
    public string Descripcion { get; set; } = null!;
    public NivelIngles NivelRequerido { get; set; }
    public List<Ejercicio> Ejercicios { get; set; } = new();
}

public class Ejercicio
{
    public int Id { get; set; }
    public int LeccionId { get; set; }
    public string Contenido { get; set; } = null!; // Pregunta o texto
    public string RespuestaCorrecta { get; set; } = null!;
    public TipoEjercicio Tipo { get; set; }
    public Leccion Leccion { get; set; } = null!;
}

public class RespuestaUsuario
{
    public int Id { get; set; }
    public int UsuarioId { get; set; }
    public int EjercicioId { get; set; }
    public string RespuestaTexto { get; set; } = null!;
    public bool EsCorrecta { get; set; }
    public DateTime FechaRealizacion { get; set; }
}

public class ProgresoNivel
{
    public int Id { get; set; }
    public int UsuarioId { get; set; }
    public NivelIngles Nivel { get; set; }
    public double PorcentajeCompletado { get; set; }
}
