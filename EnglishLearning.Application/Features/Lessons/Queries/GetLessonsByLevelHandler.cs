using EnglishLearning.Application.Features.Lessons.DTOs;
using EnglishLearning.Application.Features.Lessons.Queries;
using EnglishLearning.Domain.Interfaces;
using Mapster;
using MediatR;

namespace EnglishLearning.Application.Features.Lessons.Queries;

public sealed class GetLessonsByLevelHandler : IRequestHandler<GetLessonsByLevelQuery, IReadOnlyList<LessonDto>>
{
    private readonly ILeccionQueryRepository _repository;

    public GetLessonsByLevelHandler(ILeccionQueryRepository repository) => _repository = repository;

    public async Task<IReadOnlyList<LessonDto>> Handle(GetLessonsByLevelQuery request, CancellationToken cancellationToken)
    {
        var lessons = await _repository.ObtenerPorNivelAsync(request.Nivel, cancellationToken);
        return lessons.Adapt<IReadOnlyList<LessonDto>>();
    }
}

public sealed class GetLessonByIdHandler : IRequestHandler<GetLessonByIdQuery, LessonDto?>
{
    private readonly ILeccionQueryRepository _repository;

    public GetLessonByIdHandler(ILeccionQueryRepository repository) => _repository = repository;

    public async Task<LessonDto?> Handle(GetLessonByIdQuery request, CancellationToken cancellationToken)
    {
        var lesson = await _repository.ObtenerPorIdAsync(request.Id, cancellationToken);
        return lesson?.Adapt<LessonDto>();
    }
}

public sealed class GetExerciseByIdHandler : IRequestHandler<GetExerciseByIdQuery, ExerciseDetailDto?>
{
    private readonly ILeccionQueryRepository _repository;

    public GetExerciseByIdHandler(ILeccionQueryRepository repository) => _repository = repository;

    public async Task<ExerciseDetailDto?> Handle(GetExerciseByIdQuery request, CancellationToken cancellationToken)
    {
        var exercise = await _repository.ObtenerEjercicioPorIdAsync(request.Id, cancellationToken);
        return exercise?.Adapt<ExerciseDetailDto>();
    }
}