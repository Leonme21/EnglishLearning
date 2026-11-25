using EnglishLearning.Application.Features.Lessons.DTOs;
using EnglishLearning.Domain.Common;
using EnglishLearning.Domain.Enums;
using MediatR;

namespace EnglishLearning.Application.Features.Lessons.Queries;

public sealed record GetLessonsByLevelQuery(int UserId, NivelIngles? Level = null) : IRequest<Result<IReadOnlyList<LessonDto>>>;
public sealed record GetLessonByIdQuery(int UserId, int Id) : IRequest<Result<LessonDto>>;
public sealed record GetExerciseByIdQuery(int UserId, int Id) : IRequest<Result<ExerciseDetailDto>>;
