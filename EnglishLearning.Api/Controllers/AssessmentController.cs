using EnglishLearning.Application.Dtos;
using EnglishLearning.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace EnglishLearning.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AssessmentController : ControllerBase
{
    private readonly IAssessmentService _service;
    private readonly ILogger<AssessmentController> _logger;

    public AssessmentController(IAssessmentService service, ILogger<AssessmentController> logger)
    {
        _service = service;
        _logger = logger;
    }

    [HttpGet("preguntas")]
    public async Task<ActionResult<IEnumerable<PreguntaDiagnosticaDto>>> ObtenerPreguntas()
    {
        var preguntas = await _service.ObtenerPreguntasAsync();
        _logger.LogInformation("assessment_preguntas_obtenidas {count} {correlationId}", preguntas.Count(), HttpContext.Items["CorrelationId"]);
        return Ok(preguntas);
    }

    [HttpPost("submit")]
    public async Task<ActionResult<ResultadoAssessmentDto>> Submit([FromBody] SubmitAssessmentDto dto)
    {
        if (!ModelState.IsValid)
        {
            return UnprocessableEntity(new
            {
                error = new
                {
                    code = "VALIDATION_ERROR",
                    message = "Datos de assessment inválidos",
                    details = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)
                }
            });
        }

        if (dto.Respuestas.Select(r => r.PreguntaId).Distinct().Count() != dto.Respuestas.Count)
            return UnprocessableEntity(new { error = new { code = "VALIDATION_ERROR", message = "Preguntas duplicadas" } });

        try
        {
            var resultado = await _service.SubmitAsync(dto);
            _logger.LogInformation("assessment_submit {usuarioId} {nivel} {puntaje} {correlationId}", dto.UsuarioId, resultado.NivelAsignado, resultado.PuntajeTotal, HttpContext.Items["CorrelationId"]);
            return Ok(resultado);
        }
        catch (KeyNotFoundException e)
        {
            return NotFound(new { error = new { code = "NOT_FOUND", message = e.Message } });
        }
        catch (ArgumentException e)
        {
            return UnprocessableEntity(new { error = new { code = "VALIDATION_ERROR", message = e.Message } });
        }
    }
}

[ApiController]
[Route("api/[controller]")]
public class ProgresoController : ControllerBase
{
    private readonly IAssessmentService _service;

    public ProgresoController(IAssessmentService service) => _service = service;

    [HttpGet("{usuarioId:int}")]
    public async Task<ActionResult<ReporteProgresoDto>> Obtener(int usuarioId)
    {
        try
        {
            var reporte = await _service.ObtenerReporteAsync(usuarioId);
            if (reporte.NivelActual == "SinAsignar")
                return Ok(reporte); // 200 con recomendación placement, no 403 para permitir UI mostrar CTA
            return Ok(reporte);
        }
        catch (KeyNotFoundException e)
        {
            return NotFound(new { error = new { code = "NOT_FOUND", message = e.Message } });
        }
    }
}
