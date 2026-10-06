using EnglishLearning.Application.Features.Assessments.DTOs;
using EnglishLearning.Domain.Common;
using MediatR;

namespace EnglishLearning.Application.Features.Assessments.Commands;

public sealed record SubmitAssessmentCommand(
    int UsuarioId,
    IReadOnlyList<SubmitAnswerDto> Respuestas
) : IRequest<Result<SubmitAssessmentResponse>>;