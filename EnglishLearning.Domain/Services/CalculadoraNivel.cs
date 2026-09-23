using EnglishLearning.Domain.Enums;

namespace EnglishLearning.Domain.Services;

public static class CalculadoraNivel
{
    public static NivelIngles CalcularPorcentaje(int correctas, int total)
    {
        if (total <= 0) return NivelIngles.A1;
        var pct = (double)correctas / total * 100;
        return CalcularPorPorcentaje(pct);
    }

    public static NivelIngles CalcularPorPorcentaje(double porcentaje)
    {
        if (porcentaje <= 25) return NivelIngles.A1;
        if (porcentaje <= 45) return NivelIngles.A2;
        if (porcentaje <= 65) return NivelIngles.B1;
        if (porcentaje <= 85) return NivelIngles.B2;
        return NivelIngles.C1;
    }

    public static string RecomendacionPara(NivelIngles nivel, Dictionary<Habilidad, double> puntajesPorHabilidad)
    {
        var debil = puntajesPorHabilidad.OrderBy(kv => kv.Value).FirstOrDefault();
        var habilidad = debil.Key.ToString();
        return nivel switch
        {
            NivelIngles.A1 => $"Refuerza {habilidad} con vocabulario básico y falsos amigos (actually/embarrassed).",
            NivelIngles.A2 => $"Practica {habilidad}: rutinas diarias, evita 'I am agree' → 'I agree'.",
            NivelIngles.B1 => $"Afina {habilidad}: tiempos perfectos y conectores; escucha acentos variados.",
            NivelIngles.B2 => $"Potencia {habilidad}: debate y matices, diferencia make/do, say/tell.",
            NivelIngles.C1 => $"Perfecciona {habilidad}: registro formal e idioms, pronunciación th /θ/ vs /ð/.",
            _ => $"Continúa practicando {habilidad}."
        };
    }
}
