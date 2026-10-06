using EnglishLearning.Application.Features.Lessons.DTOs;
using EnglishLearning.Domain.Common;
using MediatR;

namespace EnglishLearning.Application.Features.Lessons.Commands;

public sealed record EvaluateExerciseCommand(
    int UsuarioId,
    int EjercicioId,
    string RespuestaTexto
) : IRequest<Result<EvaluateExerciseResponse>>;