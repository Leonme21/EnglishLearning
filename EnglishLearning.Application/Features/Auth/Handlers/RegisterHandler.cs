using EnglishLearning.Application.Features.Auth.Commands;
using EnglishLearning.Application.Features.Auth.DTOs;
using EnglishLearning.Application.Abstractions;
using EnglishLearning.Domain.Common;
using EnglishLearning.Domain.Entities;
using EnglishLearning.Domain.Enums;
using EnglishLearning.Domain.Interfaces;
using EnglishLearning.Domain.Services;
using MediatR;
using Microsoft.Extensions.Logging;

namespace EnglishLearning.Application.Features.Auth.Handlers;

public sealed class RegisterHandler : IRequestHandler<RegisterCommand, Result<AuthResponse>>
{
    private readonly IUsuarioQueryRepository _usuarioQueryRepository;
    private readonly IUsuarioCommandRepository _usuarioCommandRepository;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IEmailService _emailService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<RegisterHandler> _logger;

    public RegisterHandler(
        IUsuarioQueryRepository usuarioQueryRepository,
        IUsuarioCommandRepository usuarioCommandRepository,
        IJwtTokenService jwtTokenService,
        IPasswordHasher passwordHasher,
        IEmailService emailService,
        IUnitOfWork unitOfWork,
        ILogger<RegisterHandler> logger)
    {
        _usuarioQueryRepository = usuarioQueryRepository;
        _usuarioCommandRepository = usuarioCommandRepository;
        _jwtTokenService = jwtTokenService;
        _passwordHasher = passwordHasher;
        _emailService = emailService;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<AuthResponse>> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        // Check if email already exists
        if (await _usuarioQueryRepository.ExisteCorreoAsync(request.Email, cancellationToken))
            return Result<AuthResponse>.Failure(Error.Conflict("Auth.EmailExists", "El correo ya está registrado"));

        // Hash password
        var passwordHash = _passwordHasher.HashPassword(request.Password);

        // Create user
        var usuarioResult = Usuario.Create(0, request.Nombre, request.Email, passwordHash, NivelIngles.SinAsignar);
        if (usuarioResult.IsFailure)
            return Result<AuthResponse>.Failure(usuarioResult.Error);

        var usuario = usuarioResult.Value;
        await _usuarioCommandRepository.AgregarAsync(usuario, cancellationToken);

        // Persist first so the user gets its DB-generated Id (required by the refresh token)
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Generate tokens
        var accessToken = _jwtTokenService.GenerateAccessToken(usuario.Id, usuario.Correo, usuario.NivelActual.ToString());
        var refreshToken = await _jwtTokenService.GenerateAndStoreRefreshTokenAsync(usuario.Id, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Enviar correo de bienvenida al nuevo estudiante
        try
        {
            await _emailService.SendEmailAsync(
                usuario.Correo,
                "¡Bienvenido a English Learning Platform!",
                $"Hola {usuario.Nombre},\n\n¡Te damos la bienvenida a English Learning Platform!\nTu cuenta ha sido creada exitosamente con nivel inicial: {usuario.NivelActual}.\nYa puedes comenzar tus lecciones o realizar la evaluación diagnóstica para personalizar tu ruta.\n\n¡Mucho éxito en tu aprendizaje!\nEl equipo de English Learning",
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "No se pudo enviar el correo de bienvenida al usuario {Email}. El registro se completó con éxito.", usuario.Correo);
        }

        var response = new AuthResponse(
            Success: true,
            AccessToken: accessToken,
            RefreshToken: refreshToken,
            ExpiresAt: DateTime.UtcNow.AddMinutes(15),
            ErrorMessage: null,
            User: new UserDto(usuario.Id, usuario.Nombre, usuario.Correo, usuario.NivelActual.ToString())
        );

        return Result<AuthResponse>.Success(response);
    }
}