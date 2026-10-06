using EnglishLearning.Domain.Common;
using EnglishLearning.Domain.Enums;
using EnglishLearning.Domain.ValueObjects;

namespace EnglishLearning.Domain.Services;

public interface ILevelCalculator
{
    NivelIngles CalculateFromCorrectAnswers(int correctAnswers, int totalQuestions);
    NivelIngles CalculateFromPercentage(double percentage);
    Result<NivelIngles> TryCalculateFromPercentage(double percentage);
}