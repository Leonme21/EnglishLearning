namespace EnglishLearning.Application.Abstractions;

public sealed record TutorTurn(bool IsStudent, string Text);
public sealed record TutorRubric(string Overview, IReadOnlyList<RubricCriterion> Writing, IReadOnlyList<RubricCriterion> Speaking);
public sealed record RubricCriterion(string Name, int Score, string Feedback);

public interface IChatService
{
    Task<string> SendMessageAsync(string message, string level, IReadOnlyList<TutorTurn> history, CancellationToken ct);
    Task<TutorRubric> EvaluateProductionAsync(string? writing, byte[]? wav, string level, CancellationToken ct);
}

public sealed class TutorUnavailableException(string reason) : Exception(reason);
