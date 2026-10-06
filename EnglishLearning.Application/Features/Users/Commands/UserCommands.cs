using EnglishLearning.Application.Features.Users.DTOs;
using EnglishLearning.Domain.Common;
using MediatR;

namespace EnglishLearning.Application.Features.Users.Commands;

public sealed record RegisterUserCommand(
    string Nombre,
    string Correo,
    string Password
) : IRequest<Result<RegisterUserResponse>>;