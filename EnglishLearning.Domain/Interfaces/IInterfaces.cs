using EnglishLearning.Domain.Entities;
using EnglishLearning.Domain.Enums;

namespace EnglishLearning.Domain.Interfaces;

public interface ILeccionRepositorio
{
    Task<IEnumerable<Leccion>> ObtenerPorNivelAsync(NivelIngles nivel);
    Task<Leccion?> ObtenerPorIdAsync(int id);
    Task<IEnumerable<Ejercicio>> ObtenerEjerciciosPorLeccionAsync(int leccionId);
    Task<Ejercicio?> ObtenerEjercicioPorIdAsync(int ejercicioId);
}

public interface IUsuarioRepositorio
{
    Task<Usuario?> ObtenerPorIdAsync(int id);
    Task ActualizarNivelAsync(int usuarioId, NivelIngles nuevoNivel);
    Task GuardarRespuestaAsync(RespuestaUsuario respuesta);
    Task<IEnumerable<RespuestaUsuario>> ObtenerRespuestasRecientesAsync(int usuarioId, int cantidad);
}
