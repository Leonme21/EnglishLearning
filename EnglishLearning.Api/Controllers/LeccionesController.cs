using EnglishLearning.Application.Dtos;
using EnglishLearning.Application.Services;
using EnglishLearning.Domain.Enums;
using Microsoft.AspNetCore.Mvc;

namespace EnglishLearning.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LeccionesController : ControllerBase
{
    private readonly ILeccionServicio _leccionServicio;

    public LeccionesController(ILeccionServicio leccionServicio)
    {
        _leccionServicio = leccionServicio;
    }

    [HttpGet("nivel/{nivel}")]
    public async Task<ActionResult<IEnumerable<LeccionDto>>> ObtenerPorNivel(NivelIngles nivel)
    {
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
