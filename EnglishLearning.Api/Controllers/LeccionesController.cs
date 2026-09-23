using EnglishLearning.Application.Dtos;
using EnglishLearning.Application.Services;
using EnglishLearning.Domain.Enums;
using EnglishLearning.Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace EnglishLearning.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LeccionesController : ControllerBase
{
    private readonly ILeccionServicio _leccionServicio;
    private readonly IUsuarioRepositorio _usuarioRepo;

    public LeccionesController(ILeccionServicio leccionServicio, IUsuarioRepositorio usuarioRepo)
    {
        _leccionServicio = leccionServicio;
        _usuarioRepo = usuarioRepo;
    }

    [HttpGet("nivel/{nivel}")]
    public async Task<ActionResult<IEnumerable<LeccionDto>>> ObtenerPorNivel(NivelIngles nivel, [FromQuery] int? usuarioId)
    {
        if (usuarioId.HasValue)
        {
            var usuario = await _usuarioRepo.ObtenerPorIdAsync(usuarioId.Value);
            if (usuario == null) return NotFound(new { error = new { code = "NOT_FOUND", message = "Usuario no encontrado" } });
            if (usuario.NivelActual == NivelIngles.SinAsignar)
                return StatusCode(403, new { error = new { code = "PLACEMENT_REQUIRED", message = "Completa el Placement Test antes de acceder a lecciones" } });
        }

        var lecciones = await _leccionServicio.ObtenerLeccionesPorNivelAsync(nivel);
        return Ok(lecciones);
    }

    [HttpPost("evaluar")]
    public async Task<ActionResult<ResultadoEvaluacionDto>> Evaluar([FromBody] RespuestaDto respuesta)
    {
        if (!ModelState.IsValid)
        {
            return UnprocessableEntity(new
            {
                error = new
                {
                    code = "VALIDATION_ERROR",
                    message = "Datos de respuesta inválidos",
                    details = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)
                }
            });
        }

        var resultado = await _leccionServicio.EvaluarEjercicioAsync(respuesta);
        if (resultado.Mensaje == "Ejercicio no encontrado.")
            return NotFound(new { error = new { code = "NOT_FOUND", message = resultado.Mensaje } });

        return Ok(resultado);
    }
}
