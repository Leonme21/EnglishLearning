using EnglishLearning.Domain.Entities;
using EnglishLearning.Domain.Enums;
using EnglishLearning.Domain.Options;
using EnglishLearning.Domain.Services;

namespace EnglishLearning.Tests.Services;

public sealed class PracticeMetricsTests
{
    [Fact]
    public void PromotionUsesLatestAttemptsForDistinctExercisesAtCurrentLevel()
    {
        var lesson = BuildLesson(1, 10);
        var now = DateTime.UtcNow;
        var history = Enumerable.Range(1, 10).Select(id => Answer(id, id <= 7, now)).ToList();
        history.AddRange([Answer(-1, true, now), Answer(11, true, now), Answer(1, true, now.AddSeconds(1))]);

        var first = PracticeMetrics.ForLevel([lesson], history);
        Assert.Equal(10, first.DistinctExercises);
        Assert.Equal(7, first.LatestCorrect);
        Assert.False(first.CanPromote(NivelIngles.A1, new()));

        history.Add(Answer(8, true, now.AddSeconds(1)));
        var improved = PracticeMetrics.ForLevel([lesson], history);
        Assert.Equal(8, improved.LatestCorrect);
        Assert.True(improved.CanPromote(NivelIngles.A1, new()));
        Assert.False(improved.CanPromote(NivelIngles.C1, new()));
        Assert.False(improved.CanPromote(NivelIngles.A1, new PromotionRulesOptions { MinTotalResponses = 11 }));
    }

    [Fact]
    public void CompletionCountsEachLessonOnceAndKeepsEarlierSuccessfulAnswers()
    {
        var lessons = new[] { BuildLesson(1, 2), BuildLesson(2, 2) };
        var now = DateTime.UtcNow;
        var result = PracticeMetrics.ForLevel(lessons,
            [Answer(1, true, now), Answer(2, true, now), Answer(1, false, now.AddSeconds(1))]);

        Assert.Equal(50, result.CompletionPercentage);
        Assert.Equal(1, Assert.Single(result.CompletedLessons));
    }

    private static Leccion BuildLesson(int id, int count)
    {
        var lesson = new Leccion(id, "Test", "Test", NivelIngles.A1);
        foreach (var index in Enumerable.Range(0, count))
            lesson.AgregarEjercicio(new Ejercicio((id - 1) * count + index + 1, id, "Prompt", "answer", TipoEjercicio.Escritura));
        return lesson;
    }

    private static RespuestaUsuario Answer(int id, bool correct, DateTime date) =>
        new(id, 1, id, "response", correct, date);
}
