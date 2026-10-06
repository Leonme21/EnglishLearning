using EnglishLearning.Domain.Entities;
using EnglishLearning.Domain.Enums;

namespace EnglishLearning.Domain.Interfaces;

public interface ILeccionQueryRepository
{
    Task<IEnumerable<Leccion>> ObtenerPorNivelAsync(NivelIngles nivel, CancellationToken cancellationToken = default);
    Task<Leccion?> ObtenerPorIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Ejercicio>> ObtenerEjerciciosPorLeccionAsync(int leccionId, CancellationToken cancellationToken = default);
    Task<Ejercicio?> ObtenerEjercicioPorIdAsync(int ejercicioId, CancellationToken cancellationToken = default);
}

public interface ILeccionCommandRepository
{
    Task<Leccion> AgregarAsync(Leccion leccion, CancellationToken cancellationToken = default);
    Task<Leccion> ActualizarAsync(Leccion leccion, CancellationToken cancellationToken = default);
    Task EliminarAsync(int id, CancellationToken cancellationToken = default);
    Task<Ejercicio> AgregarEjercicioAsync(Ejercicio ejercicio, CancellationToken cancellationToken = default);
    Task<Ejercicio> ActualizarEjercicioAsync(Ejercicio ejercicio, CancellationToken cancellationToken = default);
    Task EliminarEjercicioAsync(int ejercicioId, CancellationToken cancellationToken = default);
}

public interface IUsuarioQueryRepository
{
    Task<Usuario?> ObtenerPorIdAsync(int id, CancellationToken cancellationToken = default);
    Task<Usuario?> ObtenerPorCorreoAsync(string correo, CancellationToken cancellationToken = default);
    Task<IEnumerable<RespuestaUsuario>> ObtenerRespuestasRecientesAsync(int usuarioId, int cantidad, CancellationToken cancellationToken = default);
    Task<IEnumerable<RespuestaUsuario>> ObtenerRespuestasPorUsuarioAsync(int usuarioId, CancellationToken cancellationToken = default);
    Task<bool> ExisteCorreoAsync(string correo, CancellationToken cancellationToken = default);
}

public interface IUsuarioCommandRepository
{
    Task<Usuario> AgregarAsync(Usuario usuario, CancellationToken cancellationToken = default);
    Task<Usuario> ActualizarAsync(Usuario usuario, CancellationToken cancellationToken = default);
    Task ActualizarNivelAsync(int usuarioId, NivelIngles nuevoNivel, CancellationToken cancellationToken = default);
    Task GuardarRespuestaAsync(RespuestaUsuario respuesta, CancellationToken cancellationToken = default);
    Task<ProgresoNivel> AgregarProgresoAsync(ProgresoNivel progreso, CancellationToken cancellationToken = default);
    Task<ProgresoNivel> ActualizarProgresoAsync(ProgresoNivel progreso, CancellationToken cancellationToken = default);
    Task ActualizarPasswordHashAsync(int usuarioId, string passwordHash, CancellationToken cancellationToken = default);
    Task ActualizarRefreshTokenAsync(int usuarioId, string refreshToken, DateTime expiry, CancellationToken cancellationToken = default);
    Task RevocarRefreshTokenAsync(int usuarioId, CancellationToken cancellationToken = default);
}

public interface IAssessmentQueryRepository
{
    Task<IEnumerable<PreguntaDiagnostica>> ObtenerPreguntasAsync(CancellationToken cancellationToken = default);
    Task<PreguntaDiagnostica?> ObtenerPreguntaPorIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IEnumerable<PreguntaDiagnostica>> ObtenerPreguntasPorHabilidadAsync(Habilidad habilidad, CancellationToken cancellationToken = default);
    Task<IEnumerable<PreguntaDiagnostica>> ObtenerPreguntasPorNivelAsync(NivelIngles nivel, CancellationToken cancellationToken = default);
    Task<EvaluacionDiagnostica?> ObtenerUltimaEvaluacionAsync(int usuarioId, CancellationToken cancellationToken = default);
    Task<IEnumerable<EvaluacionDiagnostica>> ObtenerEvaluacionesPorUsuarioAsync(int usuarioId, CancellationToken cancellationToken = default);
}

public interface IAssessmentCommandRepository
{
    Task<PreguntaDiagnostica> AgregarPreguntaAsync(PreguntaDiagnostica pregunta, CancellationToken cancellationToken = default);
    Task<PreguntaDiagnostica> ActualizarPreguntaAsync(PreguntaDiagnostica pregunta, CancellationToken cancellationToken = default);
    Task EliminarPreguntaAsync(int id, CancellationToken cancellationToken = default);
    Task GuardarEvaluacionAsync(EvaluacionDiagnostica evaluacion, CancellationToken cancellationToken = default);
}

public interface IRefreshTokenQueryRepository
{
    Task<RefreshToken?> ObtenerPorTokenAsync(string token, CancellationToken cancellationToken = default);
    Task<IEnumerable<RefreshToken>> ObtenerPorUsuarioAsync(int usuarioId, CancellationToken cancellationToken = default);
    Task<RefreshToken?> ObtenerActivoPorUsuarioAsync(int usuarioId, CancellationToken cancellationToken = default);
}

public interface IRefreshTokenCommandRepository
{
    Task<RefreshToken> AgregarAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default);
    Task<RefreshToken> ActualizarAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default);
    Task RevocarAsync(string token, string? replacedByToken = null, CancellationToken cancellationToken = default);
    Task RevocarTodosPorUsuarioAsync(int usuarioId, CancellationToken cancellationToken = default);
}