namespace EnglishLearning.Domain.Options;

public sealed class LevelCalculationOptions
{
    public const string SectionName = "LevelCalculation";

    public Dictionary<string, int> Thresholds { get; init; } = new()
    {
        { "A1", 0 },
        { "A2", 26 },
        { "B1", 46 },
        { "B2", 66 },
        { "C1", 86 }
    };

    public PromotionRulesOptions PromotionRules { get; init; } = new();
}

public sealed class PromotionRulesOptions
{
    public int MinCorrectPercentage { get; init; } = 80;
    public int MinTotalResponses { get; init; } = 10;
    public string MaxLevel { get; init; } = "C1";
}

public sealed class AssessmentOptions
{
    public const string SectionName = "Assessment";

    public int MinQuestions { get; init; } = 8;
    public int MaxQuestions { get; init; } = 20;
    public int PassingScorePercentage { get; init; } = 60;
}

public sealed class ExerciseEvaluationOptions
{
    public const string SectionName = "ExerciseEvaluation";

    public string DefaultStrategy { get; init; } = "CaseInsensitiveExact";
    public Dictionary<string, EvaluationStrategyConfig> Strategies { get; init; } = new()
    {
        { "CaseInsensitiveExact", new() },
        { "Fuzzy", new() { Threshold = 0.85 } },
        { "Regex", new() }
    };
}

public sealed class EvaluationStrategyConfig
{
    public double Threshold { get; init; } = 0.8;
    public bool IgnoreCase { get; init; } = true;
    public bool IgnoreWhitespace { get; init; } = true;
}

public sealed class SeedDataOptions
{
    public const string SectionName = "SeedData";

    public bool AutoSeedOnStartup { get; init; } = true;
    public string DataPath { get; init; } = "Seed/Data";
}