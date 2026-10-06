using EnglishLearning.Application.Features.Lessons.DTOs;
using EnglishLearning.Domain.Enums;
using MediatR;

namespace EnglishLearning.Application.Features.Lessons.Queries;

public sealed record GetLessonsByLevelQuery(NivelIngles Nivel) : IRequest<IReadOnlyList<LessonDto>>;

public sealed record GetLessonByIdQuery(int Id) : IRequest<LessonDto?>;

public sealed record GetExerciseByIdQuery(int Id) : IRequest<ExerciseDetailDto?>;