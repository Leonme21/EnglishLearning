using System.Text.Json;
using EnglishLearning.Application.Dtos;
using EnglishLearning.Application.Mappings;
using EnglishLearning.Domain.Entities;
using EnglishLearning.Domain.Enums;
using EnglishLearning.Domain.Interfaces;
using EnglishLearning.Domain.Services;

namespace EnglishLearning.Application.Services;

public interface IAssessmentService
{
    Task<IEnumerable<PreguntaDiagnosticaDto>> ObtenerPreguntasAsync();
    Task<ResultadoAssessmentDto> SubmitAsync(SubmitAssessmentDto dto);
    Task<ReporteProgresoDto> ObtenerReporteAsync(int usuarioId);
}

public class AssessmentService : IAssessmentService
{
    private readonly IAssessmentRepositorio _assessmentRepo;
    private readonly IUsuarioRepositorio _usuarioRepo;

    public AssessmentService(IAssessmentRepositorio assessmentRepo, IUsuarioRepositorio usuarioRepo)
    {
        _assessmentRepo = assessmentRepo;
        _usuarioRepo = usuarioRepo;
    }

    public async Task<IEnumerable<PreguntaDiagnosticaDto>> ObtenerPreguntasAsync()
    {
        var preguntas = await _assessmentRepo.ObtenerPreguntasAsync();
        return preguntas.Select(p => p.ToDto());
    }

    public async Task<ResultadoAssessmentDto> SubmitAsync(SubmitAssessmentDto dto)
    {
        var usuario = await _usuarioRepo.ObtenerPorIdAsync(dto.UsuarioId);
        if (usuario == null) throw new KeyNotFoundException("Usuario no encontrado.");

        if (dto.Respuestas.Select(r => r.PreguntaId).Distinct().Count() != dto.Respuestas.Count)
            throw new ArgumentException("Preguntas duplicadas.");

        var preguntas = (await _assessmentRepo.ObtenerPreguntasAsync()).ToDictionary(p => p.Id);

        // Validar que todas las preguntaId existen
        foreach (var r in dto.Respuestas)
            if (!preguntas.ContainsKey(r.PreguntaId))
                throw new ArgumentException($"Pregunta {r.PreguntaId} no existe.");

        int correctas = 0;
        var porHabilidad = new Dictionary<Habilidad, (int correctas, int total)>();
        foreach (Habilidad h in Enum.GetValues<Habilidad>()) porHabilidad[h] = (0, 0);

        foreach (var resp in dto.Respuestas)
        {
            var pregunta = preguntas[resp.PreguntaId];
            var esperada = pregunta.RespuestaCorrecta.Trim();
            var recibida = resp.RespuestaTexto?.Trim() ?? "";
            bool esCorrecta = !string.IsNullOrWhiteSpace(recibida) && string.Equals(esperada, recibida, StringComparison.OrdinalIgnoreCase);
            if (esCorrecta) correctas++;
            var acc = porHabilidad[pregunta.Habilidad];
            acc.total++;
            if (esCorrecta) acc.correctas++;
            porHabilidad[pregunta.Habilidad] = acc;

            // Persistir como RespuestaUsuario para trazabilidad por habilidad (EjercicioId = PreguntaId negativo offset para distinguir)
            await _usuarioRepo.GuardarRespuestaAsync(new RespuestaUsuario
            {
                UsuarioId = dto.UsuarioId,
                EjercicioId = -resp.PreguntaId,
                RespuestaTexto = resp.RespuestaTexto ?? string.Empty,
                EsCorrecta = esCorrecta,
                FechaRealizacion = DateTime.UtcNow
            });
        }

        var nivel = CalculadoraNivel.CalcularPorcentaje(correctas, dto.Respuestas.Count);
        var puntajes = porHabilidad.ToDictionary(kv => kv.Key.ToString(), kv => kv.Value.total == 0 ? 0 : Math.Round((double)kv.Value.correctas / kv.Value.total * 100, 1));
        var puntajesEnum = porHabilidad.ToDictionary(kv => kv.Key, kv => kv.Value.total == 0 ? 0 : (double)kv.Value.correctas / kv.Value.total * 100);

        await _usuarioRepo.ActualizarNivelAsync(usuario.Id, nivel);

        var evaluacion = new EvaluacionDiagnostica
        {
            UsuarioId = dto.UsuarioId,
            Fecha = DateTime.UtcNow,
            NivelAsignado = nivel,
            PuntajeTotal = (int)Math.Round((double)correctas / dto.Respuestas.Count * 100),
            PuntajesPorHabilidadJson = JsonSerializer.Serialize(puntajes)
        };
        await _assessmentRepo.GuardarEvaluacionAsync(evaluacion);

        var recomendacion = CalculadoraNivel.RecomendacionPara(nivel, puntajesEnum);

        return new ResultadoAssessmentDto
        {
            NivelAsignado = nivel.ToString(),
            PuntajeTotal = evaluacion.PuntajeTotal,
            PuntajesPorHabilidad = puntajes,
            Mensaje = $"Nivel asignado: {nivel} ({evaluacion.PuntajeTotal}%).",
            SiguienteRecomendacion = recomendacion
        };
    }

    public async Task<ReporteProgresoDto> ObtenerReporteAsync(int usuarioId)
    {
        var usuario = await _usuarioRepo.ObtenerPorIdAsync(usuarioId);
        if (usuario == null) throw new KeyNotFoundException("Usuario no encontrado.");

        var respuestas = await _usuarioRepo.ObtenerRespuestasPorUsuarioAsync(usuarioId);
        var evaluacion = await _assessmentRepo.ObtenerUltimaEvaluacionAsync(usuarioId);

        Dictionary<string, double> puntajesHab = new();
        if (evaluacion != null)
        {
            try { puntajesHab = JsonSerializer.Deserialize<Dictionary<string, double>>(evaluacion.PuntajesPorHabilidadJson) ?? new(); }
            catch { }
        }

        // Métricas por habilidad basadas en RespuestaUsuario con EjercicioId negativo (diagnóstico) mapeando a habilidad vía pregunta
        var preguntas = (await _assessmentRepo.ObtenerPreguntasAsync()).ToDictionary(p => p.Id);
        var metricas = new Dictionary<string, MetricaHabilidadDto>();
        foreach (Habilidad h in Enum.GetValues<Habilidad>())
        {
            var ids = preguntas.Where(kv => kv.Value.Habilidad == h).Select(kv => kv.Key).ToHashSet();
            var deHabilidad = respuestas.Where(r => r.EjercicioId < 0 && ids.Contains(-r.EjercicioId)).ToList();
            var acc = deHabilidad.Count(x => x.EsCorrecta);
            var tot = deHabilidad.Count;
            metricas[h.ToString()] = new MetricaHabilidadDto
            {
                Aciertos = acc,
                Total = tot,
                Porcentaje = tot == 0 ? 0 : Math.Round((double)acc / tot * 100, 1)
            };
        }

        var recomendaciones = new List<string>();
        if (usuario.NivelActual != NivelIngles.SinAsignar)
        {
            var nivel = usuario.NivelActual;
            var débiles = metricas.OrderBy(kv => kv.Value.Porcentaje).Take(2).Select(kv => kv.Key);
            foreach (var h in débiles)
                recomendaciones.Add($"Refuerza {h}: {nivel} — practica ejercicios de {h.ToLower()} con feedback hispanohablante.");
            if (nivel == NivelIngles.C1) recomendaciones.Add("¡C1! Mantén inmersión: podcasts, debate y writing formal.");
        }
        else
        {
            recomendaciones.Add("Completa el Placement Test para recibir plan personalizado.");
        }

        return new ReporteProgresoDto
        {
            UsuarioId = usuarioId,
            NivelActual = usuario.NivelActual.ToString(),
            Progresos = usuario.Progresos.Select(p => new ProgresoNivelDto { Nivel = p.Nivel.ToString(), PorcentajeCompletado = p.PorcentajeCompletado }).ToList(),
            MetricasPorHabilidad = metricas,
            Recomendaciones = recomendaciones
        };
    }
}
