using EnglishLearning.Application.Features.Users.DTOs;
using EnglishLearning.Domain.Common;
using MediatR;

namespace EnglishLearning.Application.Features.Users.Queries;

public sealed record GetUserByIdQuery(int Id) : IRequest<Result<UserDto>>;

public sealed record GetUserProfileQuery(int Id) : IRequest<Result<UserProfileDto>>;