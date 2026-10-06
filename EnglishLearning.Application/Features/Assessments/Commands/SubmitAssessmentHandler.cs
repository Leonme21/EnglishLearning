using EnglishLearning.Application.Features.Assessments.Commands;
using EnglishLearning.Application.Features.Assessments.DTOs;
using EnglishLearning.Domain.Common;
using EnglishLearning.Domain.Entities;
using EnglishLearning.Domain.Enums;
using EnglishLearning.Domain.Interfaces;
using EnglishLearning.Domain.Services;
using EnglishLearning.Domain.ValueObjects;
using MediatR;
using System.Text.Json;

namespace EnglishLearning.Application.Features.Assessments.Commands;

public sealed class SubmitAssessmentHandler : IRequestHandler<SubmitAssessmentCommand, Result<SubmitAssessmentResponse>>
{
    private readonly IAssessmentQueryRepository _assessmentQueryRepository;
    private readonly IAssessmentCommandRepository _assessmentCommandRepository;
    private readonly IUsuarioQueryRepository _usuarioQueryRepository;
    private readonly IUsuarioCommandRepository _usuarioCommandRepository;
    private readonly IExerciseEvaluator _exerciseEvaluator;
    private readonly ILevelCalculator _levelCalculator;
    private readonly IRecommendationEngine _recommendationEngine;
    private readonly IUnitOfWork _unitOfWork;

    public SubmitAssessmentHandler(
        IAssessmentQueryRepository assessmentQueryRepository,
        IAssessmentCommandRepository assessmentCommandRepository,
        IUsuarioQueryRepository usuarioQueryRepository,
        IUsuarioCommandRepository usuarioCommandRepository,
        IExerciseEvaluator exerciseEvaluator,
        ILevelCalculator levelCalculator,
        IRecommendationEngine recommendationEngine,
        IUnitOfWork unitOfWork)
    {
        _assessmentQueryRepository = assessmentQueryRepository;
        _assessmentCommandRepository = assessmentCommandRepository;
        _usuarioQueryRepository = usuarioQueryRepository;
        _usuarioCommandRepository = usuarioCommandRepository;
        _exerciseEvaluator = exerciseEvaluator;
        _levelCalculator = levelCalculator;
        _recommendationEngine = recommendationEngine;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<SubmitAssessmentResponse>> Handle(SubmitAssessmentCommand request, CancellationToken cancellationToken)
    {
        var usuario = await _usuarioQueryRepository.ObtenerPorIdAsync(request.UsuarioId, cancellationToken);
        if (usuario == null)
        {
            return Result<SubmitAssessmentResponse>.Failure(
                Error.NotFound("User.NotFound", "Usuario no encontrado"));
        }

        // Validate distinct questions
        if (request.Respuestas.Select(r => r.PreguntaId).Distinct().Count() != request.Respuestas.Count)
        {
            return Result<SubmitAssessmentResponse>.Failure(
                Error.Validation("Assessment.DuplicateQuestions", "Preguntas duplicadas"));
        }

        // Get all questions
        var preguntas = (await _assessmentQueryRepository.ObtenerPreguntasAsync(cancellationToken))
            .ToDictionary(p => p.Id);

        // Validate all question IDs exist
        foreach (var resp in request.Respuestas)
        {
            if (!preguntas.ContainsKey(resp.PreguntaId))
            {
                return Result<SubmitAssessmentResponse>.Failure(
                    Error.NotFound("Question.NotFound", $"Pregunta {resp.PreguntaId} no existe"));
            }
        }

        int correctas = 0;
        var porHabilidad = new Dictionary<Habilidad, (int correctas, int total)>();
        foreach (Habilidad h in Enum.GetValues<Habilidad>())
            porHabilidad[h] = (0, 0);

        foreach (var resp in request.Respuestas)
        {
            var pregunta = preguntas[resp.PreguntaId];
            // Diagnostic questions are multiple-choice: exact match, case-insensitive
            var evaluationResult = _exerciseEvaluator.Evaluate(
                resp.RespuestaTexto,
                pregunta.RespuestaCorrecta,
                TipoEjercicio.OpcionMultiple);

            if (evaluationResult.IsFailure)
                return Result<SubmitAssessmentResponse>.Failure(evaluationResult.Error);

            bool esCorrecta = evaluationResult.Value;
            if (esCorrecta) correctas++;

            var acc = porHabilidad[pregunta.Habilidad];
            acc.total++;
            if (esCorrecta) acc.correctas++;
            porHabilidad[pregunta.Habilidad] = acc;

            // Persist as RespuestaUsuario for traceability
            var respuesta = RespuestaUsuario.Create(
                id: 0,
                usuarioId: request.UsuarioId,
                ejercicioId: -resp.PreguntaId, // Negative to distinguish diagnostic questions
                respuestaTexto: resp.RespuestaTexto,
                esCorrecta: esCorrecta,
                fechaRealizacion: DateTime.UtcNow);

            if (respuesta.IsFailure)
                return Result<SubmitAssessmentResponse>.Failure(respuesta.Error);

            await _usuarioCommandRepository.GuardarRespuestaAsync(respuesta.Value, cancellationToken);
        }

        // Calculate level
        var nivelResult = _levelCalculator.TryCalculateFromPercentage(
            (double)correctas / request.Respuestas.Count * 100);

        if (nivelResult.IsFailure)
            return Result<SubmitAssessmentResponse>.Failure(nivelResult.Error);

        var nivel = nivelResult.Value;

        // Prepare skill scores
        var puntajes = porHabilidad.ToDictionary(
            kv => kv.Key.ToString(),
            kv => kv.Value.total == 0 ? 0 : Math.Round((double)kv.Value.correctas / kv.Value.total * 100, 1));

        var puntajesEnum = porHabilidad.ToDictionary(
            kv => kv.Key,
            kv => kv.Value.total == 0 ? Percentage.Zero : Percentage.Create(Math.Round((double)kv.Value.correctas / kv.Value.total * 100, 1)).Value);

        // Update user level
        await _usuarioCommandRepository.ActualizarNivelAsync(usuario.Id, nivel, cancellationToken);

        // Save evaluation
        var evaluacion = EvaluacionDiagnostica.Create(
            id: 0,
            usuarioId: request.UsuarioId,
            fecha: DateTime.UtcNow,
            nivelAsignado: nivel,
            puntajeTotal: (int)Math.Round((double)correctas / request.Respuestas.Count * 100),
            puntajesPorHabilidadJson: JsonSerializer.Serialize(puntajes));

        if (evaluacion.IsFailure)
            return Result<SubmitAssessmentResponse>.Failure(evaluacion.Error);

        await _assessmentCommandRepository.GuardarEvaluacionAsync(evaluacion.Value, cancellationToken);

        // Persist responses, new level and evaluation atomically
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Generate recommendation
        var recomendacion = _recommendationEngine.GenerateRecommendation(nivel, puntajesEnum);

        return Result<SubmitAssessmentResponse>.Success(new SubmitAssessmentResponse(
            NivelAsignado: nivel.ToString(),
            PuntajeTotal: evaluacion.Value.PuntajeTotal,
            PuntajesPorHabilidad: puntajes,
            Mensaje: $"Nivel asignado: {nivel} ({evaluacion.Value.PuntajeTotal}%).",
            SiguienteRecomendacion: recomendacion));
    }
}