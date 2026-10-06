using EnglishLearning.Domain.Common;
using EnglishLearning.Domain.Enums;
using EnglishLearning.Domain.ValueObjects;

namespace EnglishLearning.Domain.Services;

public interface IExerciseEvaluator
{
    Result<bool> Evaluate(string userAnswer, string correctAnswer, TipoEjercicio exerciseType);
    Result<bool> Evaluate(string userAnswer, string correctAnswer, string strategyName);
}

public interface IEvaluationStrategy
{
    string Name { get; }
    bool Evaluate(string userAnswer, string correctAnswer);
}