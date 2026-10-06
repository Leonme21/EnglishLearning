using EnglishLearning.Application.Features.Users.Commands;
using EnglishLearning.Application.Features.Users.Queries;
using FluentValidation;

namespace EnglishLearning.Application.Features.Users.Validators;

public sealed class GetUserByIdQueryValidator : AbstractValidator<GetUserByIdQuery>
{
    public GetUserByIdQueryValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("ID debe ser mayor a cero");
    }
}

public sealed class GetUserProfileQueryValidator : AbstractValidator<GetUserProfileQuery>
{
    public GetUserProfileQueryValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("ID debe ser mayor a cero");
    }
}

public sealed class RegisterUserCommandValidator : AbstractValidator<RegisterUserCommand>
{
    public RegisterUserCommandValidator()
    {
        RuleFor(x => x.Nombre)
            .NotEmpty().WithMessage("Nombre es requerido")
            .MinimumLength(2).WithMessage("Nombre debe tener al menos 2 caracteres")
            .MaximumLength(100).WithMessage("Nombre no puede exceder 100 caracteres");

        RuleFor(x => x.Correo)
            .NotEmpty().WithMessage("Correo es requerido")
            .EmailAddress().WithMessage("Formato de correo inválido")
            .MaximumLength(150).WithMessage("Correo no puede exceder 150 caracteres");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password es requerido")
            .MinimumLength(8).WithMessage("Password debe tener al menos 8 caracteres")
            .MaximumLength(100).WithMessage("Password no puede exceder 100 caracteres")
            .Matches("[A-Z]").WithMessage("Password debe contener al menos una mayúscula")
            .Matches("[a-z]").WithMessage("Password debe contener al menos una minúscula")
            .Matches("[0-9]").WithMessage("Password debe contener al menos un número")
            .Matches("[^a-zA-Z0-9]").WithMessage("Password debe contener al menos un carácter especial");
    }
}