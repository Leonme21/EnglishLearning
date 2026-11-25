using EnglishLearning.Application.Features.Assessments.DTOs;
using EnglishLearning.Application.Features.Users.DTOs;
using EnglishLearning.Application.Features.Users.Queries;
using EnglishLearning.Domain.Common;
using EnglishLearning.Domain.Interfaces;
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

        var practice = (await _usuarioQueryRepository.ObtenerRespuestasPorUsuarioAsync(usuario.Id, cancellationToken))
            .Where(answer => answer.EjercicioId > 0).ToArray();
        var correct = practice.Count(answer => answer.EsCorrecta);

        var profile = new UserProfileDto(
            Id: usuario.Id,
            Nombre: usuario.Nombre,
            Correo: usuario.Correo,
            NivelActual: usuario.NivelActual.ToString(),
            Progresos: usuario.Progresos.Select(p => new EnglishLearning.Application.Features.Users.DTOs.LevelProgressDto(p.Nivel.ToString(), p.PorcentajeCompletado)).ToList(),
            Practice: new SkillMetricDto(correct, practice.Length, EnglishLearning.Domain.Services.PracticeMetrics.Percent(correct, practice.Length))
        );

        return Result<UserProfileDto>.Success(profile);
    }
}