using EnglishLearning.Domain.Entities;
using EnglishLearning.Domain.Enums;
using EnglishLearning.Domain.Interfaces;

namespace EnglishLearning.Tests.Fakes;

public class FakeLeccionQueryRepository : ILeccionQueryRepository
{
    private readonly Dictionary<int, Ejercicio> _ejercicios = new();
    private readonly Dictionary<int, Leccion> _lecciones = new();

    public void AgregarEjercicio(Ejercicio e) => _ejercicios[e.Id] = e;
    public void AgregarLeccion(Leccion l) => _lecciones[l.Id] = l;

    public Task<IEnumerable<Leccion>> ObtenerPorNivelAsync(NivelIngles nivel, CancellationToken cancellationToken = default)
        => Task.FromResult<IEnumerable<Leccion>>(_lecciones.Values.Where(l => l.NivelRequerido == nivel).ToArray());

    public Task<Leccion?> ObtenerPorIdAsync(int id, CancellationToken cancellationToken = default)
        => Task.FromResult(_lecciones.TryGetValue(id, out var l) ? l : null);

    public Task<IEnumerable<Ejercicio>> ObtenerEjerciciosPorLeccionAsync(int leccionId, CancellationToken cancellationToken = default)
        => Task.FromResult<IEnumerable<Ejercicio>>(_ejercicios.Values.Where(e => e.LeccionId == leccionId).ToArray());

    public Task<Ejercicio?> ObtenerEjercicioPorIdAsync(int ejercicioId, CancellationToken cancellationToken = default)
        => Task.FromResult(_ejercicios.TryGetValue(ejercicioId, out var e) ? e : null);
}

public class FakeLeccionCommandRepository : ILeccionCommandRepository
{
    private readonly Dictionary<int, Leccion> _lecciones = new();
    private readonly Dictionary<int, Ejercicio> _ejercicios = new();

    public Task<Leccion> AgregarAsync(Leccion leccion, CancellationToken cancellationToken = default)
    {
        _lecciones[leccion.Id] = leccion;
        return Task.FromResult(leccion);
    }

    public Task<Leccion> ActualizarAsync(Leccion leccion, CancellationToken cancellationToken = default)
    {
        _lecciones[leccion.Id] = leccion;
        return Task.FromResult(leccion);
    }

    public Task EliminarAsync(int id, CancellationToken cancellationToken = default)
    {
        _lecciones.Remove(id);
        return Task.CompletedTask;
    }

    public Task<Ejercicio> AgregarEjercicioAsync(Ejercicio ejercicio, CancellationToken cancellationToken = default)
    {
        _ejercicios[ejercicio.Id] = ejercicio;
        return Task.FromResult(ejercicio);
    }

    public Task<Ejercicio> ActualizarEjercicioAsync(Ejercicio ejercicio, CancellationToken cancellationToken = default)
    {
        _ejercicios[ejercicio.Id] = ejercicio;
        return Task.FromResult(ejercicio);
    }

    public Task EliminarEjercicioAsync(int ejercicioId, CancellationToken cancellationToken = default)
    {
        _ejercicios.Remove(ejercicioId);
        return Task.CompletedTask;
    }
}

public class FakeUsuarioQueryRepository : IUsuarioQueryRepository
{
    private readonly Dictionary<int, Usuario> _usuarios = new();
    private readonly List<RespuestaUsuario> _respuestas = new();

    public void AgregarUsuario(Usuario u) => _usuarios[u.Id] = u;
    public void SembrarRespuestas(IEnumerable<RespuestaUsuario> respuestas) => _respuestas.AddRange(respuestas);
    public IReadOnlyList<RespuestaUsuario> RespuestasPersistidas => _respuestas;

    public Task<Usuario?> ObtenerPorIdAsync(int id, CancellationToken cancellationToken = default)
        => Task.FromResult(_usuarios.TryGetValue(id, out var u) ? u : null);

    public Task<Usuario?> ObtenerPorCorreoAsync(string correo, CancellationToken cancellationToken = default)
        => Task.FromResult(_usuarios.Values.FirstOrDefault(u => u.Correo == correo));

    public Task<IEnumerable<RespuestaUsuario>> ObtenerRespuestasRecientesAsync(int usuarioId, int cantidad, CancellationToken cancellationToken = default)
    {
        var recientes = _respuestas
            .Where(r => r.UsuarioId == usuarioId)
            .OrderByDescending(r => r.FechaRealizacion)
            .Take(cantidad);
        return Task.FromResult<IEnumerable<RespuestaUsuario>>(recientes.ToList());
    }

    public Task<IEnumerable<RespuestaUsuario>> ObtenerRespuestasPorUsuarioAsync(int usuarioId, CancellationToken cancellationToken = default)
    {
        var todas = _respuestas.Where(r => r.UsuarioId == usuarioId).ToList();
        return Task.FromResult<IEnumerable<RespuestaUsuario>>(todas);
    }

    public Task<bool> ExisteCorreoAsync(string correo, CancellationToken cancellationToken = default)
        => Task.FromResult(_usuarios.Values.Any(u => u.Correo == correo));
}

public class FakeUsuarioCommandRepository : IUsuarioCommandRepository
{
    private readonly Dictionary<int, Usuario> _usuarios = new();
    private readonly List<RespuestaUsuario> _respuestas = new();
    private readonly List<ProgresoNivel> _progresos = new();

    public void AgregarUsuario(Usuario u) => _usuarios[u.Id] = u;
    public IReadOnlyList<RespuestaUsuario> RespuestasPersistidas => _respuestas;

    public Task<Usuario> AgregarAsync(Usuario usuario, CancellationToken cancellationToken = default)
    {
        _usuarios[usuario.Id] = usuario;
        return Task.FromResult(usuario);
    }

    public Task<Usuario> ActualizarAsync(Usuario usuario, CancellationToken cancellationToken = default)
    {
        _usuarios[usuario.Id] = usuario;
        return Task.FromResult(usuario);
    }

    public Task ActualizarNivelAsync(int usuarioId, NivelIngles nuevoNivel, CancellationToken cancellationToken = default)
    {
        if (_usuarios.TryGetValue(usuarioId, out var u)) u.ActualizarNivel(nuevoNivel);
        return Task.CompletedTask;
    }

    public Task GuardarRespuestaAsync(RespuestaUsuario respuesta, CancellationToken cancellationToken = default)
    {
        var withId = RespuestaUsuario.Create(
            _respuestas.Count + 1,
            respuesta.UsuarioId,
            respuesta.EjercicioId,
            respuesta.RespuestaTexto,
            respuesta.EsCorrecta,
            respuesta.FechaRealizacion).Value;
        _respuestas.Add(withId);
        return Task.CompletedTask;
    }

    public Task<ProgresoNivel> AgregarProgresoAsync(ProgresoNivel progreso, CancellationToken cancellationToken = default)
    {
        _progresos.Add(progreso);
        return Task.FromResult(progreso);
    }

    public Task<ProgresoNivel> ActualizarProgresoAsync(ProgresoNivel progreso, CancellationToken cancellationToken = default)
    {
        var existing = _progresos.FirstOrDefault(p => p.Id == progreso.Id);
        if (existing != null)
        {
            var idx = _progresos.IndexOf(existing);
            _progresos[idx] = progreso;
        }
        return Task.FromResult(progreso);
    }

    public Task ActualizarPasswordHashAsync(int usuarioId, string passwordHash, CancellationToken cancellationToken = default)
    {
        if (_usuarios.TryGetValue(usuarioId, out var u)) u.ActualizarPasswordHash(passwordHash);
        return Task.CompletedTask;
    }

    public Task ActualizarRefreshTokenAsync(int usuarioId, string refreshToken, DateTime expiry, CancellationToken cancellationToken = default)
    {
        if (_usuarios.TryGetValue(usuarioId, out var u)) u.ActualizarRefreshToken(refreshToken, expiry);
        return Task.CompletedTask;
    }

    public Task RevocarRefreshTokenAsync(int usuarioId, CancellationToken cancellationToken = default)
    {
        if (_usuarios.TryGetValue(usuarioId, out var u)) u.RevocarRefreshToken();
        return Task.CompletedTask;
    }
}

public class FakeAssessmentQueryRepository : IAssessmentQueryRepository
{
    public Dictionary<int, PreguntaDiagnostica> Preguntas = new();
    public List<EvaluacionDiagnostica> Evaluaciones = new();

    public Task<IEnumerable<PreguntaDiagnostica>> ObtenerPreguntasAsync(CancellationToken cancellationToken = default)
        => Task.FromResult<IEnumerable<PreguntaDiagnostica>>(Preguntas.Values.OrderBy(p => p.Id).ToArray());

    public Task<PreguntaDiagnostica?> ObtenerPreguntaPorIdAsync(int id, CancellationToken cancellationToken = default)
        => Task.FromResult(Preguntas.TryGetValue(id, out var p) ? p : null);

    public Task<IEnumerable<PreguntaDiagnostica>> ObtenerPreguntasPorHabilidadAsync(EnglishLearning.Domain.Enums.Habilidad habilidad, CancellationToken cancellationToken = default)
        => Task.FromResult<IEnumerable<PreguntaDiagnostica>>(Preguntas.Values.Where(p => p.Habilidad == habilidad).ToArray());

    public Task<IEnumerable<PreguntaDiagnostica>> ObtenerPreguntasPorNivelAsync(NivelIngles nivel, CancellationToken cancellationToken = default)
        => Task.FromResult<IEnumerable<PreguntaDiagnostica>>(Preguntas.Values.Where(p => p.Nivel == nivel).ToArray());

    public Task<EvaluacionDiagnostica?> ObtenerUltimaEvaluacionAsync(int usuarioId, CancellationToken cancellationToken = default)
        => Task.FromResult(Evaluaciones.Where(e => e.UsuarioId == usuarioId).OrderByDescending(e => e.Fecha).FirstOrDefault());

    public Task<IEnumerable<EvaluacionDiagnostica>> ObtenerEvaluacionesPorUsuarioAsync(int usuarioId, CancellationToken cancellationToken = default)
        => Task.FromResult<IEnumerable<EvaluacionDiagnostica>>(Evaluaciones.Where(e => e.UsuarioId == usuarioId).OrderByDescending(e => e.Fecha).ToArray());
}

public class FakeAssessmentCommandRepository : IAssessmentCommandRepository
{
    public Dictionary<int, PreguntaDiagnostica> Preguntas = new();
    public List<EvaluacionDiagnostica> Evaluaciones = new();

    public void Agregar(PreguntaDiagnostica p) => Preguntas[p.Id] = p;

    public Task<PreguntaDiagnostica> AgregarPreguntaAsync(PreguntaDiagnostica pregunta, CancellationToken cancellationToken = default)
    {
        Preguntas[pregunta.Id] = pregunta;
        return Task.FromResult(pregunta);
    }

    public Task<PreguntaDiagnostica> ActualizarPreguntaAsync(PreguntaDiagnostica pregunta, CancellationToken cancellationToken = default)
    {
        Preguntas[pregunta.Id] = pregunta;
        return Task.FromResult(pregunta);
    }

    public Task EliminarPreguntaAsync(int id, CancellationToken cancellationToken = default)
    {
        Preguntas.Remove(id);
        return Task.CompletedTask;
    }

    public Task GuardarEvaluacionAsync(EvaluacionDiagnostica evaluacion, CancellationToken cancellationToken = default)
    {
        Evaluaciones.Add(evaluacion);
        return Task.CompletedTask;
    }
}