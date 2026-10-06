using EnglishLearning.Domain.Enums;
using EnglishLearning.Domain.ValueObjects;

namespace EnglishLearning.Domain.Services;

public interface IRecommendationEngine
{
    string GenerateRecommendation(NivelIngles level, IReadOnlyDictionary<Habilidad, Percentage> scoresBySkill);
    IReadOnlyList<string> GenerateRecommendations(NivelIngles level, IReadOnlyDictionary<Habilidad, Percentage> scoresBySkill, int count = 3);
}