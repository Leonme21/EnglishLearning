namespace EnglishLearning.Application.Options;

public sealed class BrevoSettings
{
    public const string SectionName = "Brevo";

    public string ApiKey { get; init; } = string.Empty;
    public string FromEmail { get; init; } = "mleonme21@gmail.com";
    public string FromName { get; init; } = "English Learning Platform";
}
