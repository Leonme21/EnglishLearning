using EnglishLearning.Domain.Enums;
using EnglishLearning.Domain.Services;

namespace EnglishLearning.Tests.Services;

public class CalculadoraNivelTests
{
    [Theory]
    [InlineData(0, 10, NivelIngles.A1)]
    [InlineData(2, 10, NivelIngles.A1)] // 20%
    [InlineData(3, 10, NivelIngles.A2)] // 30%
    [InlineData(4, 10, NivelIngles.A2)] // 40%
    [InlineData(5, 10, NivelIngles.B1)] // 50%
    [InlineData(6, 10, NivelIngles.B1)] // 60%
    [InlineData(7, 10, NivelIngles.B2)] // 70%
    [InlineData(8, 10, NivelIngles.B2)] // 80%
    [InlineData(9, 10, NivelIngles.C1)] // 90%
    [InlineData(10, 10, NivelIngles.C1)]
    public void CalcularPorcentaje_umbrales_correctos(int correctas, int total, NivelIngles esperado)
    {
        var nivel = CalculadoraNivel.CalcularPorcentaje(correctas, total);
        Assert.Equal(esperado, nivel);
    }

    [Theory]
    [InlineData(25, NivelIngles.A1)]
    [InlineData(25.1, NivelIngles.A2)]
    [InlineData(45, NivelIngles.A2)]
    [InlineData(45.1, NivelIngles.B1)]
    [InlineData(65, NivelIngles.B1)]
    [InlineData(65.1, NivelIngles.B2)]
    [InlineData(85, NivelIngles.B2)]
    [InlineData(85.1, NivelIngles.C1)]
    public void CalcularPorPorcentaje_limites_inclusive(double pct, NivelIngles esperado)
    {
        Assert.Equal(esperado, CalculadoraNivel.CalcularPorPorcentaje(pct));
    }

    [Fact]
    public void Calcular_conTotalCero_retornaA1()
    {
        Assert.Equal(NivelIngles.A1, CalculadoraNivel.CalcularPorcentaje(0, 0));
    }

    [Fact]
    public void Recomendacion_para_habilidad_debil_contiene_habilidad()
    {
        var puntajes = new Dictionary<Habilidad, double>
        {
            [Habilidad.Reading] = 90,
            [Habilidad.Listening] = 30,
            [Habilidad.Writing] = 80,
            [Habilidad.Speaking] = 70
        };
        var rec = CalculadoraNivel.RecomendacionPara(NivelIngles.A1, puntajes);
        Assert.Contains("Listening", rec);
        Assert.Contains("actually", rec.ToLower());
    }
}
