namespace EnglishLearning.Client.Models;

public record ChatRequest(string Message, IReadOnlyList<TutorTurn> History);

public record ChatResponse(string Response);

public class ChatMessage
{
    public string Content { get; set; } = string.Empty;
    public bool IsUser { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.Now;
    public bool IsLoading { get; set; }

    public static ChatMessage User(string content) => new()
    {
        Content = content,
        IsUser = true,
        Timestamp = DateTime.Now
    };

    public static ChatMessage Bot(string content) => new()
    {
        Content = content,
        IsUser = false,
        Timestamp = DateTime.Now
    };

    public static ChatMessage Loading() => new()
    {
        Content = string.Empty,
        IsUser = false,
        IsLoading = true,
        Timestamp = DateTime.Now
    };
}


public record TutorTurn(bool IsStudent, string Text);
public record ProductionFeedbackRequest(bool Consent, string? Writing, string? AudioBase64);
public record TutorRubric(string Overview, List<RubricCriterion> Writing, List<RubricCriterion> Speaking);
public record RubricCriterion(string Name, int Score, string Feedback);
