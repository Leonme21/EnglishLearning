using EnglishLearning.Application.Features.Auth.Commands;
using EnglishLearning.Application.Features.Auth.DTOs;
using FluentValidation;

namespace EnglishLearning.Application.Features.Auth.Validators;

public sealed class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email es requerido")
            .EmailAddress().WithMessage("Formato de email inválido");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password es requerido")
            .MinimumLength(8).WithMessage("Password debe tener al menos 8 caracteres");
    }
}

public sealed class RegisterCommandValidator : AbstractValidator<RegisterCommand>
{
    public RegisterCommandValidator()
    {
        RuleFor(x => x.Nombre)
            .NotEmpty().WithMessage("Nombre es requerido")
            .MinimumLength(2).WithMessage("Nombre debe tener al menos 2 caracteres")
            .MaximumLength(100).WithMessage("Nombre no puede exceder 100 caracteres");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email es requerido")
            .EmailAddress().WithMessage("Formato de email inválido")
            .MaximumLength(150).WithMessage("Email no puede exceder 150 caracteres");

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

public sealed class RefreshTokenCommandValidator : AbstractValidator<RefreshTokenCommand>
{
    public RefreshTokenCommandValidator()
    {
        RuleFor(x => x.RefreshToken)
            .NotEmpty().WithMessage("Refresh token es requerido");
    }
}

public sealed class RevokeTokenCommandValidator : AbstractValidator<RevokeTokenCommand>
{
    public RevokeTokenCommandValidator()
    {
        RuleFor(x => x.RefreshToken)
            .NotEmpty().WithMessage("Refresh token es requerido");
    }
}

public sealed class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email es requerido")
            .EmailAddress().WithMessage("Formato de email inválido");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password es requerido");
    }
}

public sealed class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
    public RegisterRequestValidator()
    {
        RuleFor(x => x.Nombre)
            .NotEmpty().WithMessage("Nombre es requerido")
            .MinimumLength(2).WithMessage("Nombre debe tener al menos 2 caracteres")
            .MaximumLength(100).WithMessage("Nombre no puede exceder 100 caracteres");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email es requerido")
            .EmailAddress().WithMessage("Formato de email inválido")
            .MaximumLength(150).WithMessage("Email no puede exceder 150 caracteres");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password es requerido")
            .MinimumLength(8).WithMessage("Password debe tener al menos 8 caracteres")
            .MaximumLength(100).WithMessage("Password no puede exceder 100 caracteres");
    }
}

public sealed class RefreshTokenRequestValidator : AbstractValidator<RefreshTokenRequest>
{
    public RefreshTokenRequestValidator()
    {
        RuleFor(x => x.RefreshToken)
            .NotEmpty().WithMessage("Refresh token es requerido");
    }
}

public sealed class RevokeTokenRequestValidator : AbstractValidator<RevokeTokenRequest>
{
    public RevokeTokenRequestValidator()
    {
        RuleFor(x => x.RefreshToken)
            .NotEmpty().WithMessage("Refresh token es requerido");
    }
}

public sealed class VerifyOtpCommandValidator : AbstractValidator<VerifyOtpCommand>
{
    public VerifyOtpCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email es requerido")
            .EmailAddress().WithMessage("Formato de email inválido");

        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("El código OTP es requerido")
            .Length(6).WithMessage("El código OTP debe ser de 6 dígitos")
            .Matches("^[0-9]{6}$").WithMessage("El código OTP debe contener únicamente dígitos");
    }
}

public sealed class VerifyOtpRequestValidator : AbstractValidator<VerifyOtpRequest>
{
    public VerifyOtpRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email es requerido")
            .EmailAddress().WithMessage("Formato de email inválido");

        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("El código OTP es requerido")
            .Length(6).WithMessage("El código OTP debe ser de 6 dígitos");
    }
}

public sealed class ResendOtpCommandValidator : AbstractValidator<ResendOtpCommand>
{
    public ResendOtpCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email es requerido")
            .EmailAddress().WithMessage("Formato de email inválido");
    }
}

public sealed class ResendOtpRequestValidator : AbstractValidator<ResendOtpRequest>
{
    public ResendOtpRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email es requerido")
            .EmailAddress().WithMessage("Formato de email inválido");
    }
}