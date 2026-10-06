using EnglishLearning.Application.Features.Assessments.Commands;
using EnglishLearning.Application.Features.Assessments.DTOs;
using EnglishLearning.Application.Features.Assessments.Queries;
using FluentValidation;

namespace EnglishLearning.Application.Features.Assessments.Validators;

public sealed class GetProgressReportQueryValidator : AbstractValidator<GetProgressReportQuery>
{
    public GetProgressReportQueryValidator()
    {
        RuleFor(x => x.UsuarioId)
            .GreaterThan(0)
            .WithMessage("UsuarioId must be greater than zero");
    }
}

public sealed class SubmitAssessmentCommandValidator : AbstractValidator<SubmitAssessmentCommand>
{
    public SubmitAssessmentCommandValidator()
    {
        RuleFor(x => x.UsuarioId)
            .GreaterThan(0)
            .WithMessage("UsuarioId must be greater than zero");

        RuleFor(x => x.Respuestas)
            .NotEmpty()
            .WithMessage("At least one answer is required")
            .Must(r => r.Count >= 8)
            .WithMessage("Minimum 8 answers required")
            .Must(r => r.Count <= 20)
            .WithMessage("Maximum 20 answers allowed")
            .Must(r => r.Select(a => a.PreguntaId).Distinct().Count() == r.Count)
            .WithMessage("Duplicate question IDs are not allowed");

        RuleForEach(x => x.Respuestas)
            .SetValidator(new SubmitAnswerDtoValidator());
    }
}

public sealed class SubmitAnswerDtoValidator : AbstractValidator<SubmitAnswerDto>
{
    public SubmitAnswerDtoValidator()
    {
        RuleFor(x => x.PreguntaId)
            .GreaterThan(0)
            .WithMessage("PreguntaId must be greater than zero");

        RuleFor(x => x.RespuestaTexto)
            .NotEmpty()
            .WithMessage("RespuestaTexto is required")
            .MaximumLength(2000)
            .WithMessage("RespuestaTexto cannot exceed 2000 characters");
    }
}