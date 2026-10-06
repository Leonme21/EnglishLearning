using EnglishLearning.Domain.Common;
using EnglishLearning.Domain.Enums;
using EnglishLearning.Domain.Options;
using EnglishLearning.Domain.ValueObjects;
using Microsoft.Extensions.Options;

namespace EnglishLearning.Domain.Services;

public sealed class LevelCalculator : ILevelCalculator
{
    private readonly LevelCalculationOptions _options;
    private readonly IReadOnlyDictionary<NivelIngles, int> _thresholds;

    public LevelCalculator(IOptions<LevelCalculationOptions> options)
    {
        _options = options.Value;
        _thresholds = BuildThresholds(_options.Thresholds);
    }

    private static IReadOnlyDictionary<NivelIngles, int> BuildThresholds(Dictionary<string, int> configThresholds)
    {
        var dict = new Dictionary<NivelIngles, int>();

        foreach (var kvp in configThresholds)
        {
            if (Enum.TryParse<NivelIngles>(kvp.Key, true, out var level))
            {
                dict[level] = kvp.Value;
            }
        }

        // Ensure all levels have a threshold, fallback to defaults
        var defaults = new Dictionary<NivelIngles, int>
        {
            { NivelIngles.A1, 0 },
            { NivelIngles.A2, 26 },
            { NivelIngles.B1, 46 },
            { NivelIngles.B2, 66 },
            { NivelIngles.C1, 86 }
        };

        foreach (var kvp in defaults)
        {
            if (!dict.ContainsKey(kvp.Key))
                dict[kvp.Key] = kvp.Value;
        }

        return dict.OrderBy(kvp => kvp.Value).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
    }

    public NivelIngles CalculateFromCorrectAnswers(int correctAnswers, int totalQuestions)
    {
        if (totalQuestions <= 0) return NivelIngles.A1;

        var percentage = (double)correctAnswers / totalQuestions * 100;
        return CalculateFromPercentage(percentage);
    }

    public NivelIngles CalculateFromPercentage(double percentage)
    {
        // Clamp percentage
        percentage = Math.Clamp(percentage, 0, 100);

        // Find the highest level where percentage >= threshold
        var matchedLevel = NivelIngles.A1;

        foreach (var (level, threshold) in _thresholds)
        {
            if (percentage >= threshold)
                matchedLevel = level;
            else
                break;
        }

        return matchedLevel;
    }

    public Result<NivelIngles> TryCalculateFromPercentage(double percentage)
    {
        if (percentage < 0 || percentage > 100)
            return Error.Validation("LevelCalculator.InvalidPercentage", "Percentage must be between 0 and 100");

        return CalculateFromPercentage(percentage);
    }
}