using EnglishLearning.Application.Features.Lessons.DTOs;
using EnglishLearning.Application.Features.Lessons.Queries;
using EnglishLearning.Application.Features.Lessons.Commands;
using EnglishLearning.Domain.Common;
using EnglishLearning.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace EnglishLearning.Api.Controllers.v1;

public sealed class LessonsController : BaseApiController
{
    private readonly IMediator _mediator;

    public LessonsController(IMediator mediator) => _mediator = mediator;

    [HttpGet("by-level/{nivel}")]
    public async Task<IActionResult> GetByLevel(NivelIngles nivel, CancellationToken cancellationToken)
    {
        var query = new GetLessonsByLevelQuery(nivel);
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var query = new GetLessonByIdQuery(id);
        var result = await _mediator.Send(query, cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpGet("exercises/{id:int}")]
    public async Task<IActionResult> GetExercise(int id, CancellationToken cancellationToken)
    {
        var query = new GetExerciseByIdQuery(id);
        var result = await _mediator.Send(query, cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPost("evaluate")]
    public async Task<IActionResult> EvaluateExercise([FromBody] EvaluateExerciseRequest request, CancellationToken cancellationToken)
    {
        var command = new EvaluateExerciseCommand(request.UsuarioId, request.EjercicioId, request.RespuestaTexto);
        var result = await _mediator.Send(command, cancellationToken);
        return HandleResult(result);
    }
}