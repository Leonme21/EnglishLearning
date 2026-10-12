using EnglishLearning.Application.Features.Auth.Commands;
using EnglishLearning.Application.Features.Auth.DTOs;
using EnglishLearning.Application.Abstractions;
using EnglishLearning.Domain.Common;
using EnglishLearning.Domain.Interfaces;
using MediatR;

namespace EnglishLearning.Application.Features.Auth.Handlers;

public sealed class RefreshTokenHandler : IRequestHandler<RefreshTokenCommand, Result<AuthResponse>>
{
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IRefreshTokenQueryRepository _refreshTokenQueryRepository;
    private readonly IUsuarioQueryRepository _usuarioQueryRepository;
    private readonly IUnitOfWork _unitOfWork;

    public RefreshTokenHandler(
        IJwtTokenService jwtTokenService,
        IRefreshTokenQueryRepository refreshTokenQueryRepository,
        IUsuarioQueryRepository usuarioQueryRepository,
        IUnitOfWork unitOfWork)
    {
        _jwtTokenService = jwtTokenService;
        _refreshTokenQueryRepository = refreshTokenQueryRepository;
        _usuarioQueryRepository = usuarioQueryRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<AuthResponse>> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        // Validate refresh token
        var isValid = await _jwtTokenService.ValidateRefreshTokenAsync(request.RefreshToken, cancellationToken);
        if (!isValid)
            return Result<AuthResponse>.Failure(Error.Unauthorized("Auth.InvalidRefreshToken", "Refresh token inválido o expirado"));

        // Get the stored token to get user ID
        var storedToken = await _refreshTokenQueryRepository.ObtenerPorTokenAsync(request.RefreshToken, cancellationToken);
        if (storedToken == null)
            return Result<AuthResponse>.Failure(Error.Unauthorized("Auth.InvalidRefreshToken", "Refresh token no encontrado"));

        // Rotate refresh token
        var newRefreshToken = await _jwtTokenService.RotateRefreshTokenAsync(request.RefreshToken, cancellationToken);
        if (newRefreshToken == null)
            return Result<AuthResponse>.Failure(Error.Unauthorized("Auth.InvalidRefreshToken", "No se pudo rotar el refresh token"));

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Get user
        var usuario = await _usuarioQueryRepository.ObtenerPorIdAsync(storedToken.UsuarioId, cancellationToken);
        if (usuario == null)
            return Result<AuthResponse>.Failure(Error.NotFound("User.NotFound", "Usuario no encontrado"));

        // Generate new access token
        var accessToken = _jwtTokenService.GenerateAccessToken(usuario.Id, usuario.Correo, usuario.NivelActual.ToString());

        var response = new AuthResponse(
            Success: true,
            AccessToken: accessToken,
            RefreshToken: newRefreshToken,
            ExpiresAt: DateTime.UtcNow.AddMinutes(15),
            ErrorMessage: null,
            User: new UserDto(usuario.Id, usuario.Nombre, usuario.Correo, usuario.NivelActual.ToString())
        );

        return Result<AuthResponse>.Success(response);
    }
}