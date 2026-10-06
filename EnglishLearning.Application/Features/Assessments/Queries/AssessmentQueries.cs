using EnglishLearning.Application.Features.Assessments.DTOs;
using MediatR;

namespace EnglishLearning.Application.Features.Assessments.Queries;

public sealed record GetAssessmentQuestionsQuery() : IRequest<IReadOnlyList<DiagnosticQuestionDto>>;

public sealed record GetProgressReportQuery(int UsuarioId) : IRequest<ProgressReportDto>;