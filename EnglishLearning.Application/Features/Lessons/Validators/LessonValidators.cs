using EnglishLearning.Application.Features.Lessons.Commands;
using EnglishLearning.Application.Features.Lessons.Queries;
using FluentValidation;

namespace EnglishLearning.Application.Features.Lessons.Validators;

public sealed class GetLessonsByLevelQueryValidator : AbstractValidator<GetLessonsByLevelQuery>
{
    public GetLessonsByLevelQueryValidator()
    {
        RuleFor(x => x.Nivel)
            .IsInEnum()
            .WithMessage("Nivel must be a valid English level");
    }
}

public sealed class GetLessonByIdQueryValidator : AbstractValidator<GetLessonByIdQuery>
{
    public GetLessonByIdQueryValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0)
            .WithMessage("Lesson ID must be greater than zero");
    }
}

public sealed class GetExerciseByIdQueryValidator : AbstractValidator<GetExerciseByIdQuery>
{
    public GetExerciseByIdQueryValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0)
            .WithMessage("Exercise ID must be greater than zero");
    }
}

public sealed class EvaluateExerciseCommandValidator : AbstractValidator<EvaluateExerciseCommand>
{
    public EvaluateExerciseCommandValidator()
    {
        RuleFor(x => x.UsuarioId)
            .GreaterThan(0)
            .WithMessage("UsuarioId must be greater than zero");

        RuleFor(x => x.EjercicioId)
            .GreaterThan(0)
            .WithMessage("EjercicioId must be greater than zero");

        RuleFor(x => x.RespuestaTexto)
            .NotEmpty()
            .WithMessage("RespuestaTexto is required")
            .MaximumLength(2000)
            .WithMessage("RespuestaTexto cannot exceed 2000 characters");
    }
}