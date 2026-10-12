using EnglishLearning.Application.Features.Auth.Commands;
using EnglishLearning.Application.Features.Auth.DTOs;
using EnglishLearning.Application.Features.Auth.Templates;
using EnglishLearning.Application.Abstractions;
using EnglishLearning.Application.Options;
using EnglishLearning.Domain.Common;
using EnglishLearning.Domain.Interfaces;
using EnglishLearning.Domain.Services;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace EnglishLearning.Application.Features.Auth.Handlers;

public sealed class LoginHandler : IRequestHandler<LoginCommand, Result<AuthResponse>>
{
    private readonly IUsuarioQueryRepository _usuarioQueryRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IOtpService _otpService;
    private readonly IEmailService _emailService;
    private readonly SmtpSettings _smtpSettings;
    private readonly ILogger<LoginHandler> _logger;

    public LoginHandler(
        IUsuarioQueryRepository usuarioQueryRepository,
        IPasswordHasher passwordHasher,
        IOtpService otpService,
        IEmailService emailService,
        IOptions<SmtpSettings> smtpOptions,
        ILogger<LoginHandler> logger)
    {
        _usuarioQueryRepository = usuarioQueryRepository;
        _passwordHasher = passwordHasher;
        _otpService = otpService;
        _emailService = emailService;
        _smtpSettings = smtpOptions.Value;
        _logger = logger;
    }

    public async Task<Result<AuthResponse>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var usuario = await _usuarioQueryRepository.ObtenerPorCorreoAsync(request.Email, cancellationToken);
        
        if (usuario == null)
            return Result<AuthResponse>.Failure(Error.Unauthorized("Auth.InvalidCredentials", "Credenciales inválidas"));

        if (!_passwordHasher.VerifyPassword(request.Password, usuario.PasswordHash))
            return Result<AuthResponse>.Failure(Error.Unauthorized("Auth.InvalidCredentials", "Credenciales inválidas"));

        // Generar código OTP
        var otp = _otpService.GenerateOtp(usuario.Correo);
        var recipientEmail = usuario.Correo;

        try
        {
            var emailHtml = OtpEmailTemplates.BuildOtpEmailHtml(usuario.Nombre, otp);
            await _emailService.SendEmailAsync(
                recipientEmail,
                "Tu código de verificación - English Learning",
                emailHtml,
                cancellationToken);

            _logger.LogInformation("Código OTP enviado por correo a {Recipient} para el usuario {Email}.", recipientEmail, usuario.Correo);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "No se pudo enviar el correo con el código OTP a {Recipient}. El código sigue activo y puede consultarse en los logs del servidor.", recipientEmail);
        }

        var response = new AuthResponse(
            Success: true,
            AccessToken: null,
            RefreshToken: null,
            ExpiresAt: null,
            ErrorMessage: null,
            User: null,
            RequiresOtp: true,
            Email: usuario.Correo,
            Message: "Se ha enviado un código de verificación de 6 dígitos a tu correo electrónico."
        );

        return Result<AuthResponse>.Success(response);
    }
}