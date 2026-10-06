using EnglishLearning.Domain.Common;
using EnglishLearning.Domain.Enums;
using EnglishLearning.Domain.Services;
using EnglishLearning.Domain.ValueObjects;
using Microsoft.Extensions.Options;

namespace EnglishLearning.Tests.Services;

public class LevelCalculatorTests
{
    private readonly LevelCalculator _calculator;

    public LevelCalculatorTests()
    {
        var options = Options.Create(new EnglishLearning.Domain.Options.LevelCalculationOptions
        {
            Thresholds = new Dictionary<string, int>
            {
                { "A1", 0 },
                { "A2", 26 },
                { "B1", 46 },
                { "B2", 66 },
                { "C1", 86 }
            }
        });
        _calculator = new LevelCalculator(options);
    }

    [Theory]
    [InlineData(0, 10, NivelIngles.A1)]
    [InlineData(2, 10, NivelIngles.A1)]
    [InlineData(3, 10, NivelIngles.A2)]
    [InlineData(4, 10, NivelIngles.A2)]
    [InlineData(5, 10, NivelIngles.B1)]
    [InlineData(6, 10, NivelIngles.B1)]
    [InlineData(7, 10, NivelIngles.B2)]
    [InlineData(8, 10, NivelIngles.B2)]
    [InlineData(9, 10, NivelIngles.C1)]
    [InlineData(10, 10, NivelIngles.C1)]
    public void CalculateFromCorrectAnswers_Thresholds_Correct(int correctas, int total, NivelIngles expected)
    {
        var nivel = _calculator.CalculateFromCorrectAnswers(correctas, total);
        Assert.Equal(expected, nivel);
    }

    [Theory]
    [InlineData(25, NivelIngles.A1)]
    [InlineData(26, NivelIngles.A2)]
    [InlineData(45, NivelIngles.A2)]
    [InlineData(46, NivelIngles.B1)]
    [InlineData(65, NivelIngles.B1)]
    [InlineData(66, NivelIngles.B2)]
    [InlineData(85, NivelIngles.B2)]
    [InlineData(86, NivelIngles.C1)]
    public void CalculateFromPercentage_Boundaries_Correct(double pct, NivelIngles expected)
    {
        var result = _calculator.TryCalculateFromPercentage(pct);
        Assert.True(result.IsSuccess);
        Assert.Equal(expected, result.Value);
    }

    [Fact]
    public void CalculateFromPercentage_WithTotalZero_ReturnsA1()
    {
        var nivel = _calculator.CalculateFromCorrectAnswers(0, 0);
        Assert.Equal(NivelIngles.A1, nivel);
    }

    [Fact]
    public void TryCalculateFromPercentage_OutOfRange_ReturnsError()
    {
        var result = _calculator.TryCalculateFromPercentage(-1);
        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Validation, result.Error.Type);
    }
}

public class RecommendationEngineTests
{
    private readonly RecommendationEngine _engine = new();

    [Fact]
    public void GenerateRecommendation_ForWeakestSkill_ReturnsSkillSpecificAdvice()
    {
        var puntajes = new Dictionary<Habilidad, Percentage>
        {
            [Habilidad.Reading] = Percentage.Create(90).Value,
            [Habilidad.Listening] = Percentage.Create(30).Value,
            [Habilidad.Writing] = Percentage.Create(80).Value,
            [Habilidad.Speaking] = Percentage.Create(70).Value
        };

        var rec = _engine.GenerateRecommendation(NivelIngles.A1, puntajes);

        Assert.Contains("Listening", rec);
        Assert.Contains("sonidos", rec.ToLower());
    }

    [Fact]
    public void GenerateRecommendations_ReturnsMultipleRecommendations()
    {
        var puntajes = new Dictionary<Habilidad, Percentage>
        {
            [Habilidad.Reading] = Percentage.Create(90).Value,
            [Habilidad.Listening] = Percentage.Create(30).Value,
            [Habilidad.Writing] = Percentage.Create(80).Value,
            [Habilidad.Speaking] = Percentage.Create(70).Value
        };

        var recs = _engine.GenerateRecommendations(NivelIngles.A1, puntajes, 2);

        Assert.Equal(2, recs.Count);
        Assert.All(recs, r => Assert.NotEmpty(r));
    }
}