using EnglishLearning.Application.Features.Auth.Commands;
using EnglishLearning.Application.Features.Auth.DTOs;
using EnglishLearning.Domain.Common;
using EnglishLearning.Domain.Interfaces;
using EnglishLearning.Domain.Services;
using MediatR;

namespace EnglishLearning.Application.Features.Auth.Handlers;

public sealed class LoginHandler : IRequestHandler<LoginCommand, Result<AuthResponse>>
{
    private readonly IUsuarioQueryRepository _usuarioQueryRepository;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IUnitOfWork _unitOfWork;

    public LoginHandler(
        IUsuarioQueryRepository usuarioQueryRepository,
        IJwtTokenService jwtTokenService,
        IPasswordHasher passwordHasher,
        IUnitOfWork unitOfWork)
    {
        _usuarioQueryRepository = usuarioQueryRepository;
        _jwtTokenService = jwtTokenService;
        _passwordHasher = passwordHasher;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<AuthResponse>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var usuario = await _usuarioQueryRepository.ObtenerPorCorreoAsync(request.Email, cancellationToken);
        
        if (usuario == null)
            return Result<AuthResponse>.Failure(Error.Unauthorized("Auth.InvalidCredentials", "Credenciales inválidas"));

        if (!_passwordHasher.VerifyPassword(request.Password, usuario.PasswordHash))
            return Result<AuthResponse>.Failure(Error.Unauthorized("Auth.InvalidCredentials", "Credenciales inválidas"));

        var accessToken = _jwtTokenService.GenerateAccessToken(usuario.Id, usuario.Correo, usuario.NivelActual.ToString());
        var refreshToken = await _jwtTokenService.GenerateAndStoreRefreshTokenAsync(usuario.Id, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

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