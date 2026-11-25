using EnglishLearning.Application.Features.Lessons.DTOs;
using EnglishLearning.Domain.Common;
using EnglishLearning.Domain.Enums;
using EnglishLearning.Domain.Interfaces;
using Mapster;
using MediatR;

namespace EnglishLearning.Application.Features.Lessons.Queries;

public sealed class LessonQueries(ILeccionQueryRepository lessons, IUsuarioQueryRepository users) :
    IRequestHandler<GetLessonsByLevelQuery, Result<IReadOnlyList<LessonDto>>>,
    IRequestHandler<GetLessonByIdQuery, Result<LessonDto>>,
    IRequestHandler<GetExerciseByIdQuery, Result<ExerciseDetailDto>>
{
    internal static readonly Error WrongLevel =
        Error.Forbidden("Lesson.WrongLevel", "Solo puedes practicar las lecciones de tu nivel actual.");
    private static readonly Error MissingLesson = Error.NotFound("Lesson.NotFound", "Lección no encontrada.");

    public async Task<Result<IReadOnlyList<LessonDto>>> Handle(GetLessonsByLevelQuery query, CancellationToken ct)
    {
        var user = await users.ObtenerPorIdAsync(query.UserId, ct);
        if (user is null || user.NivelActual == NivelIngles.SinAsignar ||
            query.Level.HasValue && user.NivelActual != query.Level)
            return WrongLevel;

        var catalog = await lessons.ObtenerPorNivelAsync(user.NivelActual, ct);
        return Result<IReadOnlyList<LessonDto>>.Success(catalog.Adapt<IReadOnlyList<LessonDto>>());
    }

    public async Task<Result<LessonDto>> Handle(GetLessonByIdQuery query, CancellationToken ct)
    {
        var lesson = await lessons.ObtenerPorIdAsync(query.Id, ct);
        if (lesson is null)
            return MissingLesson;
        var user = await users.ObtenerPorIdAsync(query.UserId, ct);
        return user?.NivelActual == lesson.NivelRequerido
            ? Result<LessonDto>.Success(lesson.Adapt<LessonDto>())
            : WrongLevel;
    }

    public async Task<Result<ExerciseDetailDto>> Handle(GetExerciseByIdQuery query, CancellationToken ct)
    {
        var exercise = await lessons.ObtenerEjercicioPorIdAsync(query.Id, ct);
        if (exercise is null)
            return Error.NotFound("Exercise.NotFound", "Ejercicio no encontrado.");

        var lesson = await Handle(new GetLessonByIdQuery(query.UserId, exercise.LeccionId), ct);
        return lesson.IsSuccess ? Result<ExerciseDetailDto>.Success(exercise.Adapt<ExerciseDetailDto>()) : lesson.Error;
    }
}
