using EnglishLearning.Application.Features.Auth.Commands;
using EnglishLearning.Domain.Common;
using EnglishLearning.Domain.Interfaces;
using EnglishLearning.Domain.Services;
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
        var success = await _jwtTokenService.RevokeRefreshTokenAsync(request.RefreshToken, cancellationToken);
        
        if (!success)
            return Result<bool>.Failure(Error.NotFound("Auth.RefreshTokenNotFound", "Refresh token no encontrado"));

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<bool>.Success(true);
    }
}