using EnglishLearning.Application.Dtos;
using EnglishLearning.Application.Mappings;
using EnglishLearning.Domain.Entities;
using EnglishLearning.Domain.Enums;
using EnglishLearning.Domain.Interfaces;

namespace EnglishLearning.Application.Services;

public interface ILeccionServicio
{
    Task<IEnumerable<LeccionDto>> ObtenerLeccionesPorNivelAsync(NivelIngles nivel);
    Task<ResultadoEvaluacionDto> EvaluarEjercicioAsync(RespuestaDto respuestaDto);
}

public class LeccionServicio : ILeccionServicio
{
    private readonly ILeccionRepositorio _leccionRepositorio;
    private readonly IUsuarioRepositorio _usuarioRepositorio;

    public LeccionServicio(ILeccionRepositorio leccionRepositorio, IUsuarioRepositorio usuarioRepositorio)
    {
        _leccionRepositorio = leccionRepositorio;
        _usuarioRepositorio = usuarioRepositorio;
    }

    public async Task<IEnumerable<LeccionDto>> ObtenerLeccionesPorNivelAsync(NivelIngles nivel)
    {
        var lecciones = await _leccionRepositorio.ObtenerPorNivelAsync(nivel);
        return lecciones.Select(l => l.ToDto());
    }

    public async Task<ResultadoEvaluacionDto> EvaluarEjercicioAsync(RespuestaDto respuestaDto)
    {
        var ejercicio = await _leccionRepositorio.ObtenerEjercicioPorIdAsync(respuestaDto.EjercicioId);
        if (ejercicio == null)
        {
            return new ResultadoEvaluacionDto
            {
                EsCorrecta = false,
                Mensaje = "Ejercicio no encontrado.",
                NuevoNivel = null
            };
        }

        var respuestaTexto = respuestaDto.RespuestaTexto?.Trim() ?? string.Empty;
        var esperada = ejercicio.RespuestaCorrecta?.Trim() ?? string.Empty;

        bool esCorrecta = !string.IsNullOrWhiteSpace(respuestaTexto)
            && string.Equals(esperada, respuestaTexto, StringComparison.OrdinalIgnoreCase);

        string mensaje = esCorrecta ? "¡Excelente trabajo!" : "Sigue practicando.";

        var respuesta = new RespuestaUsuario
        {
            UsuarioId = respuestaDto.UsuarioId,
            EjercicioId = respuestaDto.EjercicioId,
            RespuestaTexto = respuestaDto.RespuestaTexto ?? string.Empty,
            EsCorrecta = esCorrecta,
            FechaRealizacion = DateTime.UtcNow
        };

        await _usuarioRepositorio.GuardarRespuestaAsync(respuesta);

        var respuestasRecientes = await _usuarioRepositorio.ObtenerRespuestasRecientesAsync(respuestaDto.UsuarioId, 10);
        var total = respuestasRecientes.Count();
        var porcentajeAciertos = total > 0
            ? (double)respuestasRecientes.Count(r => r.EsCorrecta) / total * 100
            : 0;

        string? nuevoNivel = null;
        if (porcentajeAciertos >= 80 && total >= 10)
        {
            var usuario = await _usuarioRepositorio.ObtenerPorIdAsync(respuestaDto.UsuarioId);
            if (usuario != null && usuario.NivelActual < NivelIngles.C1)
            {
                var nivelSiguiente = usuario.NivelActual + 1;
                await _usuarioRepositorio.ActualizarNivelAsync(usuario.Id, nivelSiguiente);
                nuevoNivel = nivelSiguiente.ToString();
                mensaje += $" ¡Felicidades! Has subido al nivel {nuevoNivel}.";
            }
        }

        return new ResultadoEvaluacionDto
        {
            EsCorrecta = esCorrecta,
            Mensaje = mensaje,
            NuevoNivel = nuevoNivel
        };
    }
}
