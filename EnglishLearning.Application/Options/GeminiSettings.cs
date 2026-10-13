namespace EnglishLearning.Application.Options;

public sealed class GeminiSettings
{
    public const string SectionName = "Gemini";

    public string ApiKey { get; init; } = string.Empty;
    public string Model { get; init; } = "gemini-3.8-flash";
    public string FallbackModel { get; init; } = "gemini-3.5-flash-lite";
    public int TimeoutSeconds { get; init; } = 25;
}
