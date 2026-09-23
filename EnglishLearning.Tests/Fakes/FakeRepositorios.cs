using EnglishLearning.Domain.Entities;
using EnglishLearning.Domain.Enums;
using EnglishLearning.Domain.Interfaces;

namespace EnglishLearning.Tests.Fakes;

public class FakeLeccionRepositorio : ILeccionRepositorio
{
    private readonly Dictionary<int, Ejercicio> _ejercicios = new();

    public void AgregarEjercicio(Ejercicio e) => _ejercicios[e.Id] = e;

    public Task<IEnumerable<Leccion>> ObtenerPorNivelAsync(NivelIngles nivel) => Task.FromResult<IEnumerable<Leccion>>(Array.Empty<Leccion>());
    public Task<Leccion?> ObtenerPorIdAsync(int id) => Task.FromResult<Leccion?>(null);
    public Task<IEnumerable<Ejercicio>> ObtenerEjerciciosPorLeccionAsync(int leccionId) => Task.FromResult<IEnumerable<Ejercicio>>(_ejercicios.Values.Where(e => e.LeccionId == leccionId));
    public Task<Ejercicio?> ObtenerEjercicioPorIdAsync(int ejercicioId) => Task.FromResult(_ejercicios.TryGetValue(ejercicioId, out var e) ? e : null);
}

public class FakeUsuarioRepositorio : IUsuarioRepositorio
{
    private readonly Dictionary<int, Usuario> _usuarios = new();
    private readonly List<RespuestaUsuario> _respuestas = new();

    public void AgregarUsuario(Usuario u) => _usuarios[u.Id] = u;
    public void SembrarRespuestas(IEnumerable<RespuestaUsuario> respuestas) => _respuestas.AddRange(respuestas);
    public IReadOnlyList<RespuestaUsuario> RespuestasPersistidas => _respuestas;

    public Task<Usuario?> ObtenerPorIdAsync(int id) => Task.FromResult(_usuarios.TryGetValue(id, out var u) ? u : null);

    public Task ActualizarNivelAsync(int usuarioId, NivelIngles nuevoNivel)
    {
        if (_usuarios.TryGetValue(usuarioId, out var u)) u.NivelActual = nuevoNivel;
        return Task.CompletedTask;
    }

    public Task GuardarRespuestaAsync(RespuestaUsuario respuesta)
    {
        respuesta.Id = _respuestas.Count + 1;
        _respuestas.Add(respuesta);
        return Task.CompletedTask;
    }

    public Task<IEnumerable<RespuestaUsuario>> ObtenerRespuestasRecientesAsync(int usuarioId, int cantidad)
    {
        var recientes = _respuestas
            .Where(r => r.UsuarioId == usuarioId)
            .OrderByDescending(r => r.FechaRealizacion)
            .Take(cantidad);
        return Task.FromResult<IEnumerable<RespuestaUsuario>>(recientes.ToList());
    }

    public Task<IEnumerable<RespuestaUsuario>> ObtenerRespuestasPorUsuarioAsync(int usuarioId)
    {
        var todas = _respuestas.Where(r => r.UsuarioId == usuarioId).ToList();
        return Task.FromResult<IEnumerable<RespuestaUsuario>>(todas);
    }
}
