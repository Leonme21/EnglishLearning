namespace EnglishLearning.Application.Options;

public sealed class GeminiSettings
{
    public const string SectionName = "Gemini";

    public string ApiKey { get; init; } = string.Empty;
    public string Model { get; init; } = "gemini-flash-lite";
    public string FallbackModel { get; init; } = "gemini-flash-latest";
    public int TimeoutSeconds { get; init; } = 10;
}
