using EnglishLearning.Domain.Enums;
using EnglishLearning.Domain.ValueObjects;

namespace EnglishLearning.Domain.Services;

public sealed class RecommendationEngine : IRecommendationEngine
{
    private static readonly IReadOnlyDictionary<NivelIngles, IReadOnlyDictionary<Habilidad, string>> _recommendationsByLevelAndSkill
        = new Dictionary<NivelIngles, IReadOnlyDictionary<Habilidad, string>>
        {
            [NivelIngles.A1] = new Dictionary<Habilidad, string>
            {
                [Habilidad.Reading] = "Refuerza Reading con vocabulario básico y falsos amigos (actually/embarrassed).",
                [Habilidad.Listening] = "Practica Listening: sonidos /θ/ vs /ð/, vocales cortas/largas.",
                [Habilidad.Writing] = "Repasa Writing: estructura sujeto+verbo, 'I am' vs 'I have' edad.",
                [Habilidad.Speaking] = "Mejora Speaking: pronunciación de 'th', entonación preguntas sí/no."
            },
            [NivelIngles.A2] = new Dictionary<Habilidad, string>
            {
                [Habilidad.Reading] = "Practica Reading: rutinas diarias, evita 'I am agree' → 'I agree'.",
                [Habilidad.Listening] = "Entrena Listening: diferencia 'can' vs 'can't', verbos frasales comunes.",
                [Habilidad.Writing] = "Refuerza Writing: make vs do, present perfect vs past simple.",
                [Habilidad.Speaking] = "Practica Speaking: linked speech, 'would you like' para ofrecer."
            },
            [NivelIngles.B1] = new Dictionary<Habilidad, string>
            {
                [Habilidad.Reading] = "Afina Reading: tiempos perfectos, conectores (however, therefore).",
                [Habilidad.Listening] = "Mejora Listening: acentos variados (UK, US, AU), discursos extensos.",
                [Habilidad.Writing] = "Desarrolla Writing: párrafos cohesivos, formal vs informal register.",
                [Habilidad.Speaking] = "Practica Speaking: fluidez en temas abstractos, self-correction."
            },
            [NivelIngles.B2] = new Dictionary<Habilidad, string>
            {
                [Habilidad.Reading] = "Potencia Reading: debate, matices, inferencia, ironía.",
                [Habilidad.Listening] = "Perfecciona Listening: conferencias, podcasts nativos, implicatures.",
                [Habilidad.Writing] = "Domina Writing: ensayos argumentativos, registro académico, cohesión.",
                [Habilidad.Speaking] = "Pulimenta Speaking: debate formal, matiz, pronunciación /θ/ vs /ð/."
            },
            [NivelIngles.C1] = new Dictionary<Habilidad, string>
            {
                [Habilidad.Reading] = "Perfecciona Reading: textos complejos, subtexto, estilo literario.",
                [Habilidad.Listening] = "Mantén Listening: inmersión total, velocidad nativa, dialectos.",
                [Habilidad.Writing] = "Excelencia Writing: writing creativo, académico, profesional nativo.",
                [Habilidad.Speaking] = "Maestría Speaking: registro formal, idioms, pronunciación nativa."
            }
        };

    public string GenerateRecommendation(NivelIngles level, IReadOnlyDictionary<Habilidad, Percentage> scoresBySkill)
    {
        var weakestSkill = scoresBySkill
            .OrderBy(kvp => kvp.Value.Value)
            .FirstOrDefault();

        if (weakestSkill.Key == default)
            return $"Continúa practicando todas las habilidades para consolidar tu nivel {level}.";

        return GetRecommendationFor(level, weakestSkill.Key);
    }

    public IReadOnlyList<string> GenerateRecommendations(NivelIngles level, IReadOnlyDictionary<Habilidad, Percentage> scoresBySkill, int count = 3)
    {
        var recommendations = scoresBySkill
            .OrderBy(kvp => kvp.Value.Value)
            .Take(count)
            .Select(kvp => GetRecommendationFor(level, kvp.Key))
            .ToList();

        if (level == NivelIngles.C1 && recommendations.Count < count)
        {
            recommendations.Add("¡C1! Mantén inmersión: podcasts, debate y writing formal.");
        }

        return recommendations;
    }

    private static string GetRecommendationFor(NivelIngles level, Habilidad skill)
    {
        if (_recommendationsByLevelAndSkill.TryGetValue(level, out var skillDict) &&
            skillDict.TryGetValue(skill, out var recommendation))
        {
            return recommendation;
        }

        return $"Practica {skill} para mejorar tu nivel {level}.";
    }
}