using EnglishLearning.Application.Abstractions;
using EnglishLearning.Application.Features.Auth.Commands;
using EnglishLearning.Application.Features.Auth.DTOs;
using EnglishLearning.Application.Features.Auth.Templates;
using EnglishLearning.Domain.Common;
using EnglishLearning.Domain.Entities;
using EnglishLearning.Domain.Interfaces;
using EnglishLearning.Domain.Services;
using MediatR;
using Microsoft.Extensions.Logging;

namespace EnglishLearning.Application.Features.Auth.Handlers;

public sealed class EmailChallengeHandlers(
    IUnitOfWork unitOfWork,
    IPasswordHasher passwords,
    IOtpService otp,
    IEmailService email,
    IJwtTokenService tokens,
    ILogger<EmailChallengeHandlers> logger) :
    IRequestHandler<LoginCommand, Result<AuthResponse>>,
    IRequestHandler<RegisterCommand, Result<AuthResponse>>,
    IRequestHandler<VerifyOtpCommand, Result<AuthResponse>>,
    IRequestHandler<ResendOtpCommand, Result<bool>>,
    IRequestHandler<RequestPasswordResetCommand, Result<AuthResponse>>,
    IRequestHandler<ResetPasswordCommand, Result<bool>>
{
    private const string CodeMessage = "Si el código puede enviarse, llegará al correo indicado.";
    private static readonly Error InvalidChallenge =
        Error.Unauthorized("Auth.InvalidChallenge", "El código o la solicitud no son válidos o han vencido.");

    public async Task<Result<AuthResponse>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var user = await unitOfWork.UsuariosQuery.ObtenerPorCorreoAsync(request.Email.Trim(), cancellationToken);
        if (user is null || !passwords.VerifyPassword(request.Password, user.PasswordHash))
            return Error.Unauthorized("Auth.InvalidCredentials", "Credenciales inválidas.");

        return await BeginAsync(new(user.Correo, OtpPurpose.Login, user.Id, user.Nombre, user.PasswordHash), cancellationToken);
    }

    public async Task<Result<AuthResponse>> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        var address = request.Email.Trim();
        if (await unitOfWork.UsuariosQuery.ExisteCorreoAsync(address, cancellationToken))
            return Error.Conflict("Auth.EmailExists", "El correo ya está registrado.");

        var result = Usuario.Create(0, request.Nombre, address, passwords.HashPassword(request.Password));
        if (result.IsFailure)
            return result.Error;
        var user = result.GetValueOrThrow();
        return await BeginAsync(new(user.Correo, OtpPurpose.Registration, Name: user.Nombre, PasswordHash: user.PasswordHash),
            cancellationToken);
    }

    public async Task<Result<AuthResponse>> Handle(VerifyOtpCommand request, CancellationToken cancellationToken)
    {
        var identity = otp.Consume(request.ChallengeId, request.Code);
        if (identity is null)
            return InvalidChallenge;

        if (identity.Purpose != OtpPurpose.Registration)
            return await unitOfWork.WithUserLockAsync(identity.UserId, async () =>
            {
                var existing = await unitOfWork.UsuariosQuery.ObtenerPorIdAsync(identity.UserId, cancellationToken);
                return existing is null || existing.PasswordHash != identity.PasswordHash
                    ? Result<AuthResponse>.Failure(InvalidChallenge)
                    : await CreateSessionAsync(existing, cancellationToken);
            }, cancellationToken);

        if (await unitOfWork.UsuariosQuery.ExisteCorreoAsync(identity.Email, cancellationToken))
            return Error.Conflict("Auth.EmailExists", "El correo ya está registrado.");

        var user = Usuario.Create(0, identity.Name!, identity.Email, identity.PasswordHash!).GetValueOrThrow();
        await unitOfWork.UsuariosCommand.AgregarAsync(user, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return await CreateSessionAsync(user, cancellationToken);
    }

    private async Task<Result<AuthResponse>> CreateSessionAsync(Usuario user, CancellationToken cancellationToken)
    {
        var accessToken = tokens.GenerateAccessToken(user.Id, user.Correo, user.NivelActual.ToString());
        var refreshToken = await tokens.GenerateAndStoreRefreshTokenAsync(user.Id, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return new AuthResponse(true, accessToken, refreshToken, null, null,
            new UserDto(user.Id, user.Nombre, user.Correo, user.NivelActual.ToString()));
    }

    public async Task<Result<bool>> Handle(ResendOtpCommand request, CancellationToken cancellationToken)
    {
        var delivery = otp.Resend(request.ChallengeId);
        if (delivery is null)
            return InvalidChallenge;

        await TrySendAsync(delivery, cancellationToken);
        return true;
    }

    public async Task<Result<AuthResponse>> Handle(RequestPasswordResetCommand request, CancellationToken cancellationToken)
    {
        var address = request.Email.Trim().ToLowerInvariant();
        var user = await unitOfWork.UsuariosQuery.ObtenerPorCorreoAsync(address, cancellationToken);
        var identity = user is null
            ? new OtpIdentity(address, OtpPurpose.Unavailable)
            : new OtpIdentity(user.Correo, OtpPurpose.PasswordReset, user.Id, user.Nombre, user.PasswordHash);
        var delivery = otp.Begin(identity);
        if (delivery is not null)
            await TrySendAsync(delivery, cancellationToken);
        return Pending(delivery?.ChallengeId ?? Guid.NewGuid().ToString("N"));
    }

    public async Task<Result<bool>> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
    {
        var identity = otp.Consume(request.ChallengeId, request.Code, resetPassword: true);
        if (identity is null)
            return InvalidChallenge;
        return await unitOfWork.WithUserLockAsync<Result<bool>>(identity.UserId, async () =>
        {
            var user = await unitOfWork.UsuariosQuery.ObtenerPorIdAsync(identity.UserId, cancellationToken);
            if (user is null || user.PasswordHash != identity.PasswordHash)
                return InvalidChallenge;

            await unitOfWork.UsuariosCommand.ActualizarPasswordHashAsync(user.Id, passwords.HashPassword(request.Password), cancellationToken);
            await unitOfWork.RefreshTokensCommand.RevocarTodosPorUsuarioAsync(user.Id, cancellationToken);
            await unitOfWork.UsuariosCommand.RevocarRefreshTokenAsync(user.Id, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            return true;
        }, cancellationToken);
    }

    private async Task<Result<AuthResponse>> BeginAsync(OtpIdentity identity, CancellationToken cancellationToken)
    {
        var delivery = otp.Begin(identity);
        if (delivery is null)
            return Error.Validation("Auth.WaitForCode", "Espera un minuto antes de solicitar otro código.");
        var sent = await TrySendAsync(delivery, cancellationToken);
        return Pending(delivery.ChallengeId, sent ? "Enviamos un código de verificación a tu correo."
            : "El correo no pudo enviarse en este momento. Puedes solicitar un reenvío.");
    }

    private static AuthResponse Pending(string challengeId, string message = CodeMessage) =>
        new(true, null, null, null, null, null, RequiresOtp: true, Message: message, ChallengeId: challengeId);

    private async Task<bool> TrySendAsync(OtpDelivery delivery, CancellationToken cancellationToken)
    {
        if (delivery.Identity.Purpose == OtpPurpose.Unavailable)
            return true;
        try
        {
            using var deadline = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            deadline.CancelAfter(TimeSpan.FromSeconds(15));
            await email.SendEmailAsync(delivery.Identity.Email, "Código de verificación de English Learning",
                OtpEmailTemplates.BuildOtpEmailHtml(delivery.Identity.Name ?? "Estudiante", delivery.Code), deadline.Token);
            return true;
        }
        catch (Exception error) when (!cancellationToken.IsCancellationRequested)
        {
            logger.LogWarning("No se pudo enviar un código de verificación. Tipo de error: {ErrorType}", error.GetType().Name);
            return false;
        }
    }
}
