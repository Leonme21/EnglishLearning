using EnglishLearning.Application.Features.Lessons.DTOs;
using EnglishLearning.Application.Features.Lessons.Queries;
using EnglishLearning.Application.Features.Lessons.Commands;
using EnglishLearning.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EnglishLearning.Api.Controllers.v1;

[Authorize]
public sealed class LessonsController(IMediator mediator) : BaseApiController
{
    [HttpGet]
    [HttpGet("by-level/{nivel}")]
    public async Task<IActionResult> GetByLevel(NivelIngles? nivel, CancellationToken ct) =>
        CurrentUserId is int id ? HandleResult(await mediator.Send(new GetLessonsByLevelQuery(id, nivel), ct)) : Unauthorized();

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken ct) =>
        CurrentUserId is int userId ? HandleResult(await mediator.Send(new GetLessonByIdQuery(userId, id), ct)) : Unauthorized();

    [HttpGet("exercises/{id:int}")]
    public async Task<IActionResult> GetExercise(int id, CancellationToken ct) =>
        CurrentUserId is int userId ? HandleResult(await mediator.Send(new GetExerciseByIdQuery(userId, id), ct)) : Unauthorized();

    [HttpPost("evaluate")]
    public async Task<IActionResult> EvaluateExercise(EvaluateExerciseRequest request, CancellationToken ct) =>
        CurrentUserId is int userId
            ? HandleResult(await mediator.Send(new EvaluateExerciseCommand(userId, request.EjercicioId, request.RespuestaTexto), ct))
            : Unauthorized();
}
