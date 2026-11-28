using EnglishLearning.Domain.Entities;
using EnglishLearning.Domain.Enums;
using EnglishLearning.Domain.Options;

namespace EnglishLearning.Domain.Services;

public sealed record PracticeMetrics(int DistinctExercises, int LatestCorrect, IReadOnlySet<int> CompletedLessons, int LessonCount, int ExerciseCount)
{
    public double CompletionPercentage => Percent(CompletedLessons.Count, LessonCount);

    public bool CanPromote(NivelIngles level, PromotionRulesOptions rules) =>
        level >= NivelIngles.A1 &&
        Enum.TryParse<NivelIngles>(rules.MaxLevel, out var maxLevel) && Enum.IsDefined(maxLevel) &&
        level < maxLevel && ExerciseCount > 0 &&
        DistinctExercises == ExerciseCount && DistinctExercises >= rules.MinTotalResponses &&
        Percent(LatestCorrect, DistinctExercises) >= rules.MinCorrectPercentage;

    public static PracticeMetrics ForLevel(IReadOnlyList<Leccion> lessons, IEnumerable<RespuestaUsuario> history)
    {
        var ids = lessons.SelectMany(l => l.Ejercicios).Select(e => e.Id).ToHashSet();
        var groups = history.Where(attempt => ids.Contains(attempt.EjercicioId)).GroupBy(r => r.EjercicioId).ToArray();
        var latestCorrect = groups.Count(g => g.OrderByDescending(r => r.FechaRealizacion)
            .ThenByDescending(r => r.Id == 0 ? int.MaxValue : r.Id).First().EsCorrecta);
        var solved = groups.Where(g => g.Any(r => r.EsCorrecta)).Select(g => g.Key).ToHashSet();
        var completed = lessons.Where(l => l.Ejercicios.Count > 0 && l.Ejercicios.All(e => solved.Contains(e.Id)))
            .Select(l => l.Id).ToHashSet();
        return new PracticeMetrics(groups.Length, latestCorrect, completed, lessons.Count, ids.Count);
    }

    public static double Percent(int correct, int total) => total == 0 ? 0 : 100.0 * correct / total;
}
