using EnglishLearning.Domain.Common;
using EnglishLearning.Domain.Enums;

namespace EnglishLearning.Domain.Entities;

public class PreguntaDiagnostica : Entity
{
    public Habilidad Habilidad { get; private set; }
    public string Contenido { get; private set; } = null!;
    public string OpcionesJson { get; private set; } = "[]";
    public string RespuestaCorrecta { get; private set; } = null!;
    public NivelIngles Nivel { get; private set; }

    private PreguntaDiagnostica() { } // EF Core

    public PreguntaDiagnostica(int id, Habilidad habilidad, string contenido, string opcionesJson, string respuestaCorrecta, NivelIngles nivel) : base(id)
    {
        Habilidad = habilidad;
        Contenido = contenido ?? throw new ArgumentNullException(nameof(contenido));
        OpcionesJson = opcionesJson ?? "[]";
        RespuestaCorrecta = respuestaCorrecta ?? throw new ArgumentNullException(nameof(respuestaCorrecta));
        Nivel = nivel;
    }

    public static Result<PreguntaDiagnostica> Create(int id, Habilidad habilidad, string contenido, string opcionesJson, string respuestaCorrecta, NivelIngles nivel)
    {
        if (string.IsNullOrWhiteSpace(contenido))
            return Error.Validation("PreguntaDiagnostica.EmptyContent", "Contenido cannot be empty");
        if (string.IsNullOrWhiteSpace(respuestaCorrecta))
            return Error.Validation("PreguntaDiagnostica.EmptyAnswer", "Respuesta correcta cannot be empty");

        return new PreguntaDiagnostica(id, habilidad, contenido.Trim(), opcionesJson ?? "[]", respuestaCorrecta.Trim(), nivel);
    }

    public void Actualizar(string contenido, string opcionesJson, string respuestaCorrecta, NivelIngles nivel)
    {
        if (string.IsNullOrWhiteSpace(contenido))
            throw new ArgumentException("Contenido cannot be empty", nameof(contenido));
        if (string.IsNullOrWhiteSpace(respuestaCorrecta))
            throw new ArgumentException("Respuesta correcta cannot be empty", nameof(respuestaCorrecta));

        Contenido = contenido.Trim();
        OpcionesJson = opcionesJson ?? "[]";
        RespuestaCorrecta = respuestaCorrecta.Trim();
        Nivel = nivel;
    }
}

public class EvaluacionDiagnostica : Entity
{
    public int UsuarioId { get; private set; }
    public DateTime Fecha { get; private set; }
    public NivelIngles NivelAsignado { get; private set; }
    public int PuntajeTotal { get; private set; }
    public string PuntajesPorHabilidadJson { get; private set; } = "{}";

    private EvaluacionDiagnostica() { } // EF Core

    public EvaluacionDiagnostica(int id, int usuarioId, DateTime fecha, NivelIngles nivelAsignado, int puntajeTotal, string puntajesPorHabilidadJson) : base(id)
    {
        UsuarioId = usuarioId;
        Fecha = fecha;
        NivelAsignado = nivelAsignado;
        PuntajeTotal = puntajeTotal;
        PuntajesPorHabilidadJson = puntajesPorHabilidadJson ?? "{}";
    }

    public static Result<EvaluacionDiagnostica> Create(int id, int usuarioId, DateTime fecha, NivelIngles nivelAsignado, int puntajeTotal, string puntajesPorHabilidadJson)
    {
        if (usuarioId <= 0)
            return Error.Validation("EvaluacionDiagnostica.InvalidUserId", "UsuarioId must be greater than zero");
        if (puntajeTotal < 0 || puntajeTotal > 100)
            return Error.Validation("EvaluacionDiagnostica.InvalidScore", "Puntaje total must be between 0 and 100");

        return new EvaluacionDiagnostica(id, usuarioId, fecha, nivelAsignado, puntajeTotal, puntajesPorHabilidadJson ?? "{}");
    }
}