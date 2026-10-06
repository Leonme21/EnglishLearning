using EnglishLearning.Domain.Common;
using EnglishLearning.Domain.Enums;
using EnglishLearning.Domain.Options;
using EnglishLearning.Domain.ValueObjects;
using Microsoft.Extensions.Options;

namespace EnglishLearning.Domain.Services;

public sealed class ExerciseEvaluator : IExerciseEvaluator
{
    private readonly IReadOnlyDictionary<string, IEvaluationStrategy> _strategies;
    private readonly IEvaluationStrategy _defaultStrategy;

    public ExerciseEvaluator(IOptions<ExerciseEvaluationOptions> options)
    {
        var config = options.Value;

        _strategies = new Dictionary<string, IEvaluationStrategy>(StringComparer.OrdinalIgnoreCase)
        {
            ["CaseInsensitiveExact"] = new CaseInsensitiveExactStrategy(),
            ["Fuzzy"] = new FuzzyStrategy(config.Strategies.GetValueOrDefault("Fuzzy")?.Threshold ?? 0.85),
            ["Regex"] = new RegexStrategy()
        };

        _defaultStrategy = _strategies.TryGetValue(config.DefaultStrategy, out var strategy)
            ? strategy
            : _strategies["CaseInsensitiveExact"];
    }

    public Result<bool> Evaluate(string userAnswer, string correctAnswer, TipoEjercicio exerciseType)
    {
        var strategyName = exerciseType switch
        {
            TipoEjercicio.OpcionMultiple => "CaseInsensitiveExact",
            TipoEjercicio.Escritura => "Fuzzy",
            TipoEjercicio.Lectura => "CaseInsensitiveExact",
            TipoEjercicio.Escucha => "CaseInsensitiveExact",
            _ => _defaultStrategy.Name
        };

        return Evaluate(userAnswer, correctAnswer, strategyName);
    }

    public Result<bool> Evaluate(string userAnswer, string correctAnswer, string strategyName)
    {
        if (!_strategies.TryGetValue(strategyName, out var strategy))
            return Error.Validation("ExerciseEvaluator.UnknownStrategy", $"Evaluation strategy '{strategyName}' not found");

        var normalizedUser = NormalizeInput(userAnswer);
        var normalizedCorrect = NormalizeInput(correctAnswer);

        var isCorrect = strategy.Evaluate(normalizedUser, normalizedCorrect);
        return isCorrect;
    }

    private static string NormalizeInput(string input)
    {
        if (string.IsNullOrWhiteSpace(input)) return string.Empty;
        return input.Trim();
    }

    private sealed class CaseInsensitiveExactStrategy : IEvaluationStrategy
    {
        public string Name => "CaseInsensitiveExact";

        public bool Evaluate(string userAnswer, string correctAnswer)
            => string.Equals(userAnswer, correctAnswer, StringComparison.OrdinalIgnoreCase);
    }

    private sealed class FuzzyStrategy : IEvaluationStrategy
    {
        public string Name => "Fuzzy";
        private readonly double _threshold;

        public FuzzyStrategy(double threshold) => _threshold = threshold;

        public bool Evaluate(string userAnswer, string correctAnswer)
        {
            if (string.Equals(userAnswer, correctAnswer, StringComparison.OrdinalIgnoreCase))
                return true;

            // Simple Levenshtein-based similarity
            var similarity = CalculateSimilarity(userAnswer, correctAnswer);
            return similarity >= _threshold;
        }

        private static double CalculateSimilarity(string s1, string s2)
        {
            if (string.IsNullOrEmpty(s1) && string.IsNullOrEmpty(s2)) return 1.0;
            if (string.IsNullOrEmpty(s1) || string.IsNullOrEmpty(s2)) return 0.0;

            var len1 = s1.Length;
            var len2 = s2.Length;
            var matrix = new int[len1 + 1, len2 + 1];

            for (var i = 0; i <= len1; i++) matrix[i, 0] = i;
            for (var j = 0; j <= len2; j++) matrix[0, j] = j;

            for (var i = 1; i <= len1; i++)
            {
                for (var j = 1; j <= len2; j++)
                {
                    var cost = s1[i - 1] == s2[j - 1] ? 0 : 1;
                    matrix[i, j] = Math.Min(
                        Math.Min(matrix[i - 1, j] + 1, matrix[i, j - 1] + 1),
                        matrix[i - 1, j - 1] + cost);
                }
            }

            var maxLen = Math.Max(len1, len2);
            return 1.0 - (double)matrix[len1, len2] / maxLen;
        }
    }

    private sealed class RegexStrategy : IEvaluationStrategy
    {
        public string Name => "Regex";

        public bool Evaluate(string userAnswer, string correctAnswer)
        {
            try
            {
                // Treat correctAnswer as regex pattern
                return System.Text.RegularExpressions.Regex.IsMatch(userAnswer, correctAnswer, System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            }
            catch
            {
                // Fallback to exact match if regex is invalid
                return string.Equals(userAnswer, correctAnswer, StringComparison.OrdinalIgnoreCase);
            }
        }
    }
}