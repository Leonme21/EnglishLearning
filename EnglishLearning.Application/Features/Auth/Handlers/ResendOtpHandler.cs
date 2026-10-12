using EnglishLearning.Application.Abstractions;
using EnglishLearning.Application.Features.Auth.Commands;
using EnglishLearning.Application.Features.Auth.Templates;
using EnglishLearning.Application.Options;
using EnglishLearning.Domain.Common;
using EnglishLearning.Domain.Interfaces;
using EnglishLearning.Domain.Services;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace EnglishLearning.Application.Features.Auth.Handlers;

public sealed class ResendOtpHandler : IRequestHandler<ResendOtpCommand, Result<bool>>
{
    private readonly IUsuarioQueryRepository _usuarioQueryRepository;
    private readonly IOtpService _otpService;
    private readonly IEmailService _emailService;
    private readonly SmtpSettings _smtpSettings;
    private readonly ILogger<ResendOtpHandler> _logger;

    public ResendOtpHandler(
        IUsuarioQueryRepository usuarioQueryRepository,
        IOtpService otpService,
        IEmailService emailService,
        IOptions<SmtpSettings> smtpOptions,
        ILogger<ResendOtpHandler> logger)
    {
        _usuarioQueryRepository = usuarioQueryRepository;
        _otpService = otpService;
        _emailService = emailService;
        _smtpSettings = smtpOptions.Value;
        _logger = logger;
    }

    public async Task<Result<bool>> Handle(ResendOtpCommand request, CancellationToken cancellationToken)
    {
        var usuario = await _usuarioQueryRepository.ObtenerPorCorreoAsync(request.Email, cancellationToken);
        if (usuario == null)
        {
            return Result<bool>.Failure(Error.NotFound("Auth.UserNotFound", "Usuario no encontrado."));
        }

        var otp = _otpService.GenerateOtp(usuario.Correo);
        var recipientEmail = usuario.Correo;

        try
        {
            var emailHtml = OtpEmailTemplates.BuildOtpEmailHtml(usuario.Nombre, otp);
            await _emailService.SendEmailAsync(
                recipientEmail,
                "Tu nuevo código de verificación - English Learning",
                emailHtml,
                cancellationToken);

            _logger.LogInformation("Nuevo código OTP reenviado a {Recipient} para {Email}.", recipientEmail, usuario.Correo);
            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al reenviar código OTP a {Recipient}", recipientEmail);
            return Result<bool>.Failure(Error.Internal("Auth.EmailSendFailed", "No se pudo reenviar el correo de verificación."));
        }
    }
}
