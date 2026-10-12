using EnglishLearning.Application.Abstractions;
using EnglishLearning.Application.Features.Auth.Commands;
using EnglishLearning.Application.Features.Auth.DTOs;
using EnglishLearning.Domain.Common;
using EnglishLearning.Domain.Interfaces;
using EnglishLearning.Domain.Services;
using MediatR;
using Microsoft.Extensions.Logging;

namespace EnglishLearning.Application.Features.Auth.Handlers;

public sealed class VerifyOtpHandler : IRequestHandler<VerifyOtpCommand, Result<AuthResponse>>
{
    private readonly IUsuarioQueryRepository _usuarioQueryRepository;
    private readonly IOtpService _otpService;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<VerifyOtpHandler> _logger;

    public VerifyOtpHandler(
        IUsuarioQueryRepository usuarioQueryRepository,
        IOtpService otpService,
        IJwtTokenService jwtTokenService,
        IUnitOfWork unitOfWork,
        ILogger<VerifyOtpHandler> logger)
    {
        _usuarioQueryRepository = usuarioQueryRepository;
        _otpService = otpService;
        _jwtTokenService = jwtTokenService;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<AuthResponse>> Handle(VerifyOtpCommand request, CancellationToken cancellationToken)
    {
        var isValid = _otpService.ValidateOtp(request.Email, request.Code);
        if (!isValid)
        {
            _logger.LogWarning("Intento fallido de validación OTP para {Email}", request.Email);
            return Result<AuthResponse>.Failure(Error.Unauthorized("Auth.InvalidOtp", "El código de verificación es inválido o ha expirado."));
        }

        var usuario = await _usuarioQueryRepository.ObtenerPorCorreoAsync(request.Email, cancellationToken);
        if (usuario == null)
        {
            return Result<AuthResponse>.Failure(Error.NotFound("Auth.UserNotFound", "Usuario no encontrado."));
        }

        var accessToken = _jwtTokenService.GenerateAccessToken(usuario.Id, usuario.Correo, usuario.NivelActual.ToString());
        var refreshToken = await _jwtTokenService.GenerateAndStoreRefreshTokenAsync(usuario.Id, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Usuario {Email} autenticado exitosamente mediante OTP 2FA.", usuario.Correo);

        var response = new AuthResponse(
            Success: true,
            AccessToken: accessToken,
            RefreshToken: refreshToken,
            ExpiresAt: DateTime.UtcNow.AddMinutes(15),
            ErrorMessage: null,
            User: new UserDto(usuario.Id, usuario.Nombre, usuario.Correo, usuario.NivelActual.ToString()),
            RequiresOtp: false,
            Email: usuario.Correo,
            Message: "Autenticación completada exitosamente."
        );

        return Result<AuthResponse>.Success(response);
    }
}
