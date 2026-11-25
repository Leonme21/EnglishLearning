using EnglishLearning.Application.Features.Auth.Commands;
using EnglishLearning.Application.Abstractions;
using EnglishLearning.Domain.Common;
using EnglishLearning.Domain.Interfaces;
using MediatR;

namespace EnglishLearning.Application.Features.Auth.Handlers;

public sealed class RevokeTokenHandler : IRequestHandler<RevokeTokenCommand, Result<bool>>
{
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IUnitOfWork _unitOfWork;

    public RevokeTokenHandler(IJwtTokenService jwtTokenService, IUnitOfWork unitOfWork)
    {
        _jwtTokenService = jwtTokenService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<bool>> Handle(RevokeTokenCommand request, CancellationToken cancellationToken)
    {
        var token = await _unitOfWork.RefreshTokensQuery.ObtenerPorTokenAsync(request.RefreshToken, cancellationToken);
        if (token is null)
            return Error.NotFound("Auth.RefreshTokenNotFound", "Refresh token no encontrado.");

        return await _unitOfWork.WithUserLockAsync<Result<bool>>(token.UsuarioId, async () =>
        {
            var success = await _jwtTokenService.RevokeRefreshTokenAsync(request.RefreshToken, cancellationToken);
            if (!success)
                return Error.NotFound("Auth.RefreshTokenNotFound", "Refresh token no encontrado.");

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return true;
        }, cancellationToken);
    }
}
