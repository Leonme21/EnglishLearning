using EnglishLearning.Application.Features.Users.Commands;
using EnglishLearning.Application.Features.Users.DTOs;
using EnglishLearning.Application.Features.Users.Queries;
using EnglishLearning.Domain.Common;
using EnglishLearning.Domain.Entities;
using EnglishLearning.Domain.Enums;
using EnglishLearning.Domain.Interfaces;
using EnglishLearning.Domain.Services;
using MediatR;

namespace EnglishLearning.Application.Features.Users.Handlers;

public sealed class GetUserByIdHandler : IRequestHandler<GetUserByIdQuery, Result<UserDto>>
{
    private readonly IUsuarioQueryRepository _usuarioQueryRepository;

    public GetUserByIdHandler(IUsuarioQueryRepository usuarioQueryRepository)
    {
        _usuarioQueryRepository = usuarioQueryRepository;
    }

    public async Task<Result<UserDto>> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
    {
        var usuario = await _usuarioQueryRepository.ObtenerPorIdAsync(request.Id, cancellationToken);
        
        if (usuario == null)
            return Result<UserDto>.Failure(Error.NotFound("User.NotFound", "Usuario no encontrado"));

        var dto = new UserDto(usuario.Id, usuario.Nombre, usuario.Correo, usuario.NivelActual.ToString());
        return Result<UserDto>.Success(dto);
    }
}

public sealed class GetUserProfileHandler : IRequestHandler<GetUserProfileQuery, Result<UserProfileDto>>
{
    private readonly IUsuarioQueryRepository _usuarioQueryRepository;

    public GetUserProfileHandler(IUsuarioQueryRepository usuarioQueryRepository)
    {
        _usuarioQueryRepository = usuarioQueryRepository;
    }

    public async Task<Result<UserProfileDto>> Handle(GetUserProfileQuery request, CancellationToken cancellationToken)
    {
        var usuario = await _usuarioQueryRepository.ObtenerPorIdAsync(request.Id, cancellationToken);
        
        if (usuario == null)
            return Result<UserProfileDto>.Failure(Error.NotFound("User.NotFound", "Usuario no encontrado"));

        var profile = new UserProfileDto(
            Id: usuario.Id,
            Nombre: usuario.Nombre,
            Correo: usuario.Correo,
            NivelActual: usuario.NivelActual.ToString(),
            Progresos: usuario.Progresos.Select(p => new LevelProgressDto(p.Nivel.ToString(), p.PorcentajeCompletado)).ToList()
        );

        return Result<UserProfileDto>.Success(profile);
    }
}

public sealed class RegisterUserHandler : IRequestHandler<RegisterUserCommand, Result<RegisterUserResponse>>
{
    private readonly IUsuarioQueryRepository _usuarioQueryRepository;
    private readonly IUsuarioCommandRepository _usuarioCommandRepository;
    private readonly IPasswordHasher _passwordHasher;

    public RegisterUserHandler(
        IUsuarioQueryRepository usuarioQueryRepository,
        IUsuarioCommandRepository usuarioCommandRepository,
        IPasswordHasher passwordHasher)
    {
        _usuarioQueryRepository = usuarioQueryRepository;
        _usuarioCommandRepository = usuarioCommandRepository;
        _passwordHasher = passwordHasher;
    }

    public async Task<Result<RegisterUserResponse>> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        if (await _usuarioQueryRepository.ExisteCorreoAsync(request.Correo, cancellationToken))
            return Result<RegisterUserResponse>.Failure(Error.Conflict("User.EmailExists", "El correo ya está registrado"));

        var passwordHash = _passwordHasher.HashPassword(request.Password);

        var usuarioResult = Usuario.Create(0, request.Nombre, request.Correo, passwordHash, NivelIngles.SinAsignar);
        if (usuarioResult.IsFailure)
            return Result<RegisterUserResponse>.Failure(usuarioResult.Error);

        var usuario = usuarioResult.Value;
        await _usuarioCommandRepository.AgregarAsync(usuario, cancellationToken);

        var response = new RegisterUserResponse(usuario.Id, usuario.Nombre, usuario.Correo, usuario.NivelActual.ToString());
        return Result<RegisterUserResponse>.Success(response);
    }
}