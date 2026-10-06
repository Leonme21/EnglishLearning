using EnglishLearning.Application.Features.Lessons.Commands;
using EnglishLearning.Application.Features.Lessons.DTOs;
using EnglishLearning.Domain.Common;
using EnglishLearning.Domain.Entities;
using EnglishLearning.Domain.Enums;
using EnglishLearning.Domain.Interfaces;
using EnglishLearning.Domain.Services;
using MediatR;

namespace EnglishLearning.Application.Features.Lessons.Commands;

public sealed class EvaluateExerciseHandler : IRequestHandler<EvaluateExerciseCommand, Result<EvaluateExerciseResponse>>
{
    private readonly ILeccionQueryRepository _leccionRepository;
    private readonly IUsuarioCommandRepository _usuarioCommandRepository;
    private readonly IUsuarioQueryRepository _usuarioQueryRepository;
    private readonly IExerciseEvaluator _exerciseEvaluator;
    private readonly ILevelCalculator _levelCalculator;
    private readonly IUnitOfWork _unitOfWork;

    public EvaluateExerciseHandler(
        ILeccionQueryRepository leccionRepository,
        IUsuarioCommandRepository usuarioCommandRepository,
        IUsuarioQueryRepository usuarioQueryRepository,
        IExerciseEvaluator exerciseEvaluator,
        ILevelCalculator levelCalculator,
        IUnitOfWork unitOfWork)
    {
        _leccionRepository = leccionRepository;
        _usuarioCommandRepository = usuarioCommandRepository;
        _usuarioQueryRepository = usuarioQueryRepository;
        _exerciseEvaluator = exerciseEvaluator;
        _levelCalculator = levelCalculator;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<EvaluateExerciseResponse>> Handle(EvaluateExerciseCommand request, CancellationToken cancellationToken)
    {
        // Get exercise
        var exercise = await _leccionRepository.ObtenerEjercicioPorIdAsync(request.EjercicioId, cancellationToken);
        if (exercise == null)
        {
            return Result<EvaluateExerciseResponse>.Failure(
                Error.NotFound("Exercise.NotFound", "Ejercicio no encontrado"));
        }

        // Evaluate answer
        var evaluationResult = _exerciseEvaluator.Evaluate(
            request.RespuestaTexto,
            exercise.RespuestaCorrecta,
            exercise.Tipo);

        if (evaluationResult.IsFailure)
            return Result<EvaluateExerciseResponse>.Failure(evaluationResult.Error);

        var esCorrecta = evaluationResult.Value;
        var mensaje = esCorrecta ? "¡Excelente trabajo!" : "Sigue practicando.";

        // Save user response
        var respuesta = RespuestaUsuario.Create(
            id: 0, // Will be set by DB
            usuarioId: request.UsuarioId,
            ejercicioId: request.EjercicioId,
            respuestaTexto: request.RespuestaTexto,
            esCorrecta: esCorrecta,
            fechaRealizacion: DateTime.UtcNow);

        if (respuesta.IsFailure)
            return Result<EvaluateExerciseResponse>.Failure(respuesta.Error);

        await _usuarioCommandRepository.GuardarRespuestaAsync(respuesta.Value, cancellationToken);

        // Persist now so the promotion window below includes this answer
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Check for level promotion
        var respuestasRecientes = await _usuarioQueryRepository.ObtenerRespuestasRecientesAsync(request.UsuarioId, 10, cancellationToken);
        var total = respuestasRecientes.Count();
        var porcentajeAciertos = total > 0
            ? (double)respuestasRecientes.Count(r => r.EsCorrecta) / total * 100
            : 0;

        string? nuevoNivel = null;
        var usuario = await _usuarioQueryRepository.ObtenerPorIdAsync(request.UsuarioId, cancellationToken);

        if (porcentajeAciertos >= 80 && total >= 10 && usuario != null && usuario.NivelActual < NivelIngles.C1)
        {
            var nivelSiguiente = (int)usuario.NivelActual + 1;
            var nuevoNivelEnum = (NivelIngles)nivelSiguiente;

            await _usuarioCommandRepository.ActualizarNivelAsync(request.UsuarioId, nuevoNivelEnum, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            nuevoNivel = nuevoNivelEnum.ToString();
            mensaje += $" ¡Felicidades! Has subido al nivel {nuevoNivel}.";
        }

        return Result<EvaluateExerciseResponse>.Success(new EvaluateExerciseResponse(
            EsCorrecta: esCorrecta,
            Mensaje: mensaje,
            NuevoNivel: nuevoNivel));
    }
}