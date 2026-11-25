using EnglishLearning.Application.Features.Lessons.DTOs;
using EnglishLearning.Application.Features.Lessons.Queries;
using EnglishLearning.Domain.Common;
using EnglishLearning.Domain.Entities;
using EnglishLearning.Domain.Enums;
using EnglishLearning.Domain.Interfaces;
using EnglishLearning.Domain.Options;
using EnglishLearning.Domain.Services;
using MediatR;
using Microsoft.Extensions.Options;

namespace EnglishLearning.Application.Features.Lessons.Commands;

public sealed class EvaluateExerciseHandler(
    IUnitOfWork unitOfWork, IExerciseEvaluator evaluator, IOptions<LevelCalculationOptions> options)
    : IRequestHandler<EvaluateExerciseCommand, Result<EvaluateExerciseResponse>>
{
    public Task<Result<EvaluateExerciseResponse>> Handle(EvaluateExerciseCommand request, CancellationToken ct) =>
        unitOfWork.WithUserLockAsync(request.UsuarioId, () => EvaluateAsync(request, ct), ct);

    private async Task<Result<EvaluateExerciseResponse>> EvaluateAsync(EvaluateExerciseCommand request, CancellationToken ct)
    {
        var user = await unitOfWork.UsuariosQuery.ObtenerPorIdAsync(request.UsuarioId, ct);
        if (user is null || user.NivelActual == NivelIngles.SinAsignar)
            return LessonQueries.WrongLevel;

        var exercise = await unitOfWork.LeccionesQuery.ObtenerEjercicioPorIdAsync(request.EjercicioId, ct);
        if (exercise is null)
            return Error.NotFound("Exercise.NotFound", "Ejercicio no encontrado.");

        var lessons = (await unitOfWork.LeccionesQuery.ObtenerPorNivelAsync(user.NivelActual, ct)).ToArray();
        if (lessons.All(l => l.Id != exercise.LeccionId))
            return LessonQueries.WrongLevel;

        var result = exercise.EstrategiaEvaluacion is { } strategy
            ? evaluator.Evaluate(request.RespuestaTexto, exercise.RespuestaCorrecta, strategy)
            : evaluator.Evaluate(request.RespuestaTexto, exercise.RespuestaCorrecta, exercise.Tipo);
        if (result.IsFailure)
            return result.Error;

        var attempt = RespuestaUsuario.Create(0, user.Id, exercise.Id, request.RespuestaTexto, result.Value).GetValueOrThrow();
        var history = (await unitOfWork.UsuariosQuery.ObtenerRespuestasPorUsuarioAsync(user.Id, ct)).Append(attempt).ToArray();
        var practice = PracticeMetrics.ForLevel(lessons, history);

        await unitOfWork.UsuariosCommand.GuardarRespuestaAsync(attempt, ct);
        var progress = user.Progresos.SingleOrDefault(p => p.Nivel == user.NivelActual);
        if (progress is null)
        {
            await unitOfWork.UsuariosCommand.AgregarProgresoAsync(
                ProgresoNivel.Create(0, user.Id, user.NivelActual, practice.CompletionPercentage).GetValueOrThrow(), ct);
        }
        else
        {
            progress.ActualizarPorcentaje(practice.CompletionPercentage);
            await unitOfWork.UsuariosCommand.ActualizarProgresoAsync(progress, ct);
        }

        var rules = options.Value.PromotionRules;
        string? nextLevel = null;
        var message = result.Value ? "¡Excelente trabajo!" : "Sigue practicando.";
        if (practice.CanPromote(user.NivelActual, rules))
        {
            var next = user.NivelActual + 1;
            await unitOfWork.UsuariosCommand.ActualizarNivelAsync(user.Id, next, ct);
            nextLevel = next.ToString();
            message += $" ¡Felicidades! Has subido al nivel {nextLevel}.";
        }

        await unitOfWork.SaveChangesAsync(ct);
        return new EvaluateExerciseResponse(result.Value, message, nextLevel);
    }
}
