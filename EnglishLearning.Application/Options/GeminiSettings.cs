namespace EnglishLearning.Application.Options;

public sealed class GeminiSettings
{
    public const string SectionName = "Gemini";

    public string ApiKey { get; init; } = string.Empty;
    public string Model { get; init; } = "gemini-1.5-flash";
    public string FallbackModel { get; init; } = "gemini-1.5-pro";
    public int TimeoutSeconds { get; init; } = 25;
}
