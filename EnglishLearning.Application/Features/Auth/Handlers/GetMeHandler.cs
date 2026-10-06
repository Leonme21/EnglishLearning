using EnglishLearning.Application.Features.Users.DTOs;
using EnglishLearning.Application.Features.Users.Queries;
using EnglishLearning.Domain.Common;
using EnglishLearning.Domain.Interfaces;
using MediatR;

namespace EnglishLearning.Application.Features.Auth.Handlers;

public sealed class GetMeHandler : IRequestHandler<GetUserProfileQuery, Result<UserProfileDto>>
{
    private readonly IUsuarioQueryRepository _usuarioQueryRepository;

    public GetMeHandler(IUsuarioQueryRepository usuarioQueryRepository)
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