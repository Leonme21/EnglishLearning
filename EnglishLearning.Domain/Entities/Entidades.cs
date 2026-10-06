using EnglishLearning.Domain.Common;
using EnglishLearning.Domain.Enums;
using EnglishLearning.Domain.ValueObjects;

namespace EnglishLearning.Domain.Entities;

public class Usuario : AggregateRoot
{
    public string Nombre { get; private set; } = null!;
    public string Correo { get; private set; } = null!;
    public string PasswordHash { get; private set; } = null!;
    public string? RefreshToken { get; private set; }
    public DateTime? RefreshTokenExpiry { get; private set; }
    public NivelIngles NivelActual { get; private set; }
    private readonly List<ProgresoNivel> _progresos = new();
    public IReadOnlyList<ProgresoNivel> Progresos => _progresos.AsReadOnly();

    private Usuario() { } // EF Core

    public Usuario(int id, string nombre, string correo, string passwordHash, NivelIngles nivelActual = NivelIngles.SinAsignar) : base(id)
    {
        Nombre = nombre ?? throw new ArgumentNullException(nameof(nombre));
        Correo = correo ?? throw new ArgumentNullException(nameof(correo));
        PasswordHash = passwordHash ?? throw new ArgumentNullException(nameof(passwordHash));
        NivelActual = nivelActual;
    }

    public static Result<Usuario> Create(int id, string nombre, string correo, string passwordHash, NivelIngles nivelActual = NivelIngles.SinAsignar)
    {
        if (string.IsNullOrWhiteSpace(nombre))
            return Error.Validation("Usuario.EmptyName", "Nombre cannot be empty");

        var emailResult = Email.Create(correo);
        if (emailResult.IsFailure)
            return emailResult.Error;

        if (string.IsNullOrWhiteSpace(passwordHash))
            return Error.Validation("Usuario.EmptyPasswordHash", "PasswordHash cannot be empty");

        return new Usuario(id, nombre.Trim(), emailResult.Value.Value, passwordHash, nivelActual);
    }

    public void ActualizarNivel(NivelIngles nuevoNivel)
    {
        if (nuevoNivel == NivelActual) return;

        var previousLevel = NivelActual;
        NivelActual = nuevoNivel;

        AddDomainEvent(new Events.LevelChangedEvent
        {
            UserId = new ValueObjects.UserId(Id),
            PreviousLevel = previousLevel,
            NewLevel = nuevoNivel,
            AchievementPercentage = ValueObjects.Percentage.Zero
        });
    }

    public void ActualizarPasswordHash(string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new ArgumentException("PasswordHash cannot be empty", nameof(passwordHash));
        PasswordHash = passwordHash;
    }

    public void ActualizarRefreshToken(string refreshToken, DateTime expiry)
    {
        RefreshToken = refreshToken;
        RefreshTokenExpiry = expiry;
    }

    public void RevocarRefreshToken()
    {
        RefreshToken = null;
        RefreshTokenExpiry = null;
    }

    public bool ValidarRefreshToken(string refreshToken)
    {
        return RefreshToken == refreshToken && 
               RefreshTokenExpiry.HasValue && 
               RefreshTokenExpiry.Value > DateTime.UtcNow;
    }

    public void AgregarProgreso(ProgresoNivel progreso)
    {
        _progresos.Add(progreso);
    }
}

public class Leccion : AggregateRoot
{
    public string Titulo { get; private set; } = null!;
    public string Descripcion { get; private set; } = null!;
    public NivelIngles NivelRequerido { get; private set; }
    private readonly List<Ejercicio> _ejercicios = new();
    public IReadOnlyList<Ejercicio> Ejercicios => _ejercicios.AsReadOnly();

    private Leccion() { } // EF Core

    public Leccion(int id, string titulo, string descripcion, NivelIngles nivelRequerido) : base(id)
    {
        Titulo = titulo ?? throw new ArgumentNullException(nameof(titulo));
        Descripcion = descripcion ?? throw new ArgumentNullException(nameof(descripcion));
        NivelRequerido = nivelRequerido;
    }

    public static Result<Leccion> Create(int id, string titulo, string descripcion, NivelIngles nivelRequerido)
    {
        if (string.IsNullOrWhiteSpace(titulo))
            return Error.Validation("Leccion.EmptyTitle", "Título cannot be empty");
        if (string.IsNullOrWhiteSpace(descripcion))
            return Error.Validation("Leccion.EmptyDescription", "Descripción cannot be empty");
        if (titulo.Length > 200)
            return Error.Validation("Leccion.TitleTooLong", "Título cannot exceed 200 characters");

        return new Leccion(id, titulo.Trim(), descripcion.Trim(), nivelRequerido);
    }

    public void AgregarEjercicio(Ejercicio ejercicio)
    {
        _ejercicios.Add(ejercicio);
    }

    public void RemoverEjercicio(int ejercicioId)
    {
        var ejercicio = _ejercicios.FirstOrDefault(e => e.Id == ejercicioId);
        if (ejercicio != null)
            _ejercicios.Remove(ejercicio);
    }
}

public class Ejercicio : Entity
{
    public int LeccionId { get; private set; }
    public string Contenido { get; private set; } = null!;
    public string RespuestaCorrecta { get; private set; } = null!;
    public TipoEjercicio Tipo { get; private set; }
    public Leccion? Leccion { get; private set; }

    private Ejercicio() { } // EF Core

    public Ejercicio(int id, int leccionId, string contenido, string respuestaCorrecta, TipoEjercicio tipo) : base(id)
    {
        LeccionId = leccionId;
        Contenido = contenido ?? throw new ArgumentNullException(nameof(contenido));
        RespuestaCorrecta = respuestaCorrecta ?? throw new ArgumentNullException(nameof(respuestaCorrecta));
        Tipo = tipo;
    }

    public static Result<Ejercicio> Create(int id, int leccionId, string contenido, string respuestaCorrecta, TipoEjercicio tipo)
    {
        if (string.IsNullOrWhiteSpace(contenido))
            return Error.Validation("Ejercicio.EmptyContent", "Contenido cannot be empty");
        if (string.IsNullOrWhiteSpace(respuestaCorrecta))
            return Error.Validation("Ejercicio.EmptyAnswer", "Respuesta correcta cannot be empty");

        return new Ejercicio(id, leccionId, contenido.Trim(), respuestaCorrecta.Trim(), tipo);
    }

    public void ActualizarContenido(string contenido, string respuestaCorrecta)
    {
        if (string.IsNullOrWhiteSpace(contenido))
            throw new ArgumentException("Contenido cannot be empty", nameof(contenido));
        if (string.IsNullOrWhiteSpace(respuestaCorrecta))
            throw new ArgumentException("Respuesta correcta cannot be empty", nameof(respuestaCorrecta));

        Contenido = contenido.Trim();
        RespuestaCorrecta = respuestaCorrecta.Trim();
    }
}

public class RespuestaUsuario : Entity
{
    public int UsuarioId { get; private set; }
    public int EjercicioId { get; private set; }
    public string RespuestaTexto { get; private set; } = null!;
    public bool EsCorrecta { get; private set; }
    public DateTime FechaRealizacion { get; private set; }

    private RespuestaUsuario() { } // EF Core

    public RespuestaUsuario(int id, int usuarioId, int ejercicioId, string respuestaTexto, bool esCorrecta, DateTime fechaRealizacion) : base(id)
    {
        UsuarioId = usuarioId;
        EjercicioId = ejercicioId;
        RespuestaTexto = respuestaTexto ?? string.Empty;
        EsCorrecta = esCorrecta;
        FechaRealizacion = fechaRealizacion;
    }

    public static Result<RespuestaUsuario> Create(int id, int usuarioId, int ejercicioId, string respuestaTexto, bool esCorrecta, DateTime? fechaRealizacion = null)
    {
        if (usuarioId <= 0)
            return Error.Validation("RespuestaUsuario.InvalidUserId", "UsuarioId must be greater than zero");
        if (ejercicioId == 0)
            return Error.Validation("RespuestaUsuario.InvalidExerciseId", "EjercicioId cannot be zero");

        return new RespuestaUsuario(id, usuarioId, ejercicioId, respuestaTexto ?? string.Empty, esCorrecta, fechaRealizacion ?? DateTime.UtcNow);
    }
}

public class ProgresoNivel : Entity
{
    public int UsuarioId { get; private set; }
    public NivelIngles Nivel { get; private set; }
    public double PorcentajeCompletado { get; private set; }

    private ProgresoNivel() { } // EF Core

    public ProgresoNivel(int id, int usuarioId, NivelIngles nivel, double porcentajeCompletado) : base(id)
    {
        UsuarioId = usuarioId;
        Nivel = nivel;
        PorcentajeCompletado = Math.Clamp(porcentajeCompletado, 0, 100);
    }

    public static Result<ProgresoNivel> Create(int id, int usuarioId, NivelIngles nivel, double porcentajeCompletado)
    {
        if (usuarioId <= 0)
            return Error.Validation("ProgresoNivel.InvalidUserId", "UsuarioId must be greater than zero");
        if (porcentajeCompletado < 0 || porcentajeCompletado > 100)
            return Error.Validation("ProgresoNivel.InvalidPercentage", "Porcentaje must be between 0 and 100");

        return new ProgresoNivel(id, usuarioId, nivel, porcentajeCompletado);
    }

    public void ActualizarPorcentaje(double porcentaje)
    {
        PorcentajeCompletado = Math.Clamp(porcentaje, 0, 100);
    }
}