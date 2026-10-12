using EnglishLearning.Domain.Entities;
using EnglishLearning.Domain.Enums;
using EnglishLearning.Domain.Options;
using EnglishLearning.Domain.Services;
using EnglishLearning.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace EnglishLearning.Infrastructure.Services;

public sealed class SeedDataService : ISeedDataService
{
    private readonly EnglishLearningDbContext _context;
    private readonly SeedDataOptions _options;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ILogger<SeedDataService> _logger;

    public SeedDataService(
        EnglishLearningDbContext context,
        IOptions<SeedDataOptions> options,
        IPasswordHasher passwordHasher,
        ILogger<SeedDataService> logger)
    {
        _context = context;
        _options = options.Value;
        _passwordHasher = passwordHasher;
        _logger = logger;
    }

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        if (!_options.AutoSeedOnStartup) return;

        // Purge demo user if exists in DB
        var demoUser = await _context.Usuarios.FirstOrDefaultAsync(u => u.Correo == "demo@demo.com", cancellationToken);
        if (demoUser != null)
        {
            _context.Usuarios.Remove(demoUser);
            await _context.SaveChangesAsync(cancellationToken);
        }

        // Seed diagnostic questions
        if (!await _context.PreguntasDiagnosticas.AnyAsync(cancellationToken))
        {
            var questionsPath = Path.Combine(_options.DataPath, "questions.json");
            if (File.Exists(questionsPath))
            {
                var json = await File.ReadAllTextAsync(questionsPath, cancellationToken);
                var questions = JsonSerializer.Deserialize<List<SeedQuestion>>(json);
                if (questions != null)
                {
                    foreach (var q in questions)
                    {
                        var pregunta = PreguntaDiagnostica.Create(
                            q.Id, q.Habilidad, q.Contenido, q.OpcionesJson, q.RespuestaCorrecta, q.Nivel);
                        if (pregunta.IsSuccess)
                            _context.PreguntasDiagnosticas.Add(pregunta.Value);
                    }
                    await _context.SaveChangesAsync(cancellationToken);
                }
            }
            else
            {
                await SeedDefaultQuestionsAsync(cancellationToken);
            }
        }

        // Seed lessons + exercises
        if (!await _context.Lecciones.AnyAsync(cancellationToken))
        {
            _context.Lecciones.AddRange(LessonSeedData.Build());
            await _context.SaveChangesAsync(cancellationToken);
        }

        await SyncIdentitySequencesAsync(cancellationToken);
    }

    /// <summary>
    /// Rows seeded with explicit Ids don't advance PostgreSQL identity sequences,
    /// which makes the next generated Id collide with an existing PK. Realign them.
    /// </summary>
    private async Task SyncIdentitySequencesAsync(CancellationToken cancellationToken)
    {
        if (!_context.Database.IsNpgsql()) return;

        string[] tables = { "Usuarios", "PreguntasDiagnosticas", "Lecciones", "Ejercicios" };
        foreach (var table in tables)
        {
            try
            {
                var sql = $"SELECT setval(pg_get_serial_sequence('\"{table}\"', 'Id'), " +
                          $"GREATEST((SELECT COALESCE(MAX(\"Id\"), 0) FROM \"{table}\"), 1));";
                await _context.Database.ExecuteSqlRawAsync(sql, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "No se pudo sincronizar la secuencia para la tabla {Table}. Continuando ejecución.", table);
            }
        }
    }

    private async Task SeedDefaultQuestionsAsync(CancellationToken cancellationToken)
    {
        var preguntas = new List<PreguntaDiagnostica>
        {
            // Reading — falsos amigos
            PreguntaDiagnostica.Create(1, Habilidad.Reading, "Choose correct: 'Actually' means...", 
                JsonSerializer.Serialize(new[]{"Actualmente","En realidad","Actualmente y en realidad"}), "En realidad", NivelIngles.A2).Value!,
            PreguntaDiagnostica.Create(2, Habilidad.Reading, "Embarrassed means...", 
                JsonSerializer.Serialize(new[]{"Embarazada","Avergonzado","Embarcado"}), "Avergonzado", NivelIngles.A1).Value!,
            PreguntaDiagnostica.Create(3, Habilidad.Reading, "Notice the transfer: 'I am agree' is incorrect, correct is...", 
                JsonSerializer.Serialize(new[]{"I am agree","I agree","I am agreed"}), "I agree", NivelIngles.B1).Value!,
            PreguntaDiagnostica.Create(4, Habilidad.Reading, "Constipated means...", 
                JsonSerializer.Serialize(new[]{"Estreñido","Constipado (resfriado)","Constante"}), "Estreñido", NivelIngles.B1).Value!,
            // Listening — vicios pronunciación
            PreguntaDiagnostica.Create(5, Habilidad.Listening, "Audio: /θ/ vs /ð/ — 'think' starts with...", 
                JsonSerializer.Serialize(new[]{"s sound","th voiceless /θ/","d sound"}), "th voiceless /θ/", NivelIngles.A2).Value!,
            PreguntaDiagnostica.Create(6, Habilidad.Listening, "Audio: 'I can' vs 'I can't' — difference is...", 
                JsonSerializer.Serialize(new[]{"vowel length + /t/","only volume","no difference"}), "vowel length + /t/", NivelIngles.B2).Value!,
            PreguntaDiagnostica.Create(7, Habilidad.Listening, "Audio transcript: 'I have lived here for 3 years' — correct question?", 
                JsonSerializer.Serialize(new[]{"How long have you lived here?","How long are you living here?","How long you live here?"}), "How long have you lived here?", NivelIngles.B1).Value!,
            PreguntaDiagnostica.Create(8, Habilidad.Listening, "Audio: 'ship' vs 'sheep' — /ɪ/ vs /i:/ — 'ship' is...", 
                JsonSerializer.Serialize(new[]{"long /i:/","short /ɪ/","diphtong"}), "short /ɪ/", NivelIngles.A2).Value!,
            // Writing — transferencias gramaticales
            PreguntaDiagnostica.Create(9, Habilidad.Writing, "Correct: 'I have 30 years' →", 
                JsonSerializer.Serialize(new[]{"I have 30 years","I am 30 years old","I have 30 years old"}), "I am 30 years old", NivelIngles.A1).Value!,
            PreguntaDiagnostica.Create(10, Habilidad.Writing, "Make vs Do: '___ homework'", 
                JsonSerializer.Serialize(new[]{"make","do","make/do both"}), "do", NivelIngles.A2).Value!,
            PreguntaDiagnostica.Create(11, Habilidad.Writing, "Correct question: 'Why you are late?' →", 
                JsonSerializer.Serialize(new[]{"Why you are late?","Why are you late?","Why you late?"}), "Why are you late?", NivelIngles.B1).Value!,
            PreguntaDiagnostica.Create(12, Habilidad.Writing, "____ you like coffee? (offer)", 
                JsonSerializer.Serialize(new[]{"Do","Would","Are"}), "Would", NivelIngles.B2).Value!,
            // Speaking — fluidez guiada, pronunciación
            PreguntaDiagnostica.Create(13, Habilidad.Speaking, "Pronunciation: 'vegetable' has ... syllables", 
                JsonSerializer.Serialize(new[]{"4","3","5"}), "3", NivelIngles.B1).Value!,
            PreguntaDiagnostica.Create(14, Habilidad.Speaking, "Choose natural: 'Can you give me a hand?' means...", 
                JsonSerializer.Serialize(new[]{"Give hand","Help me","Shake hand"}), "Help me", NivelIngles.B2).Value!,
            PreguntaDiagnostica.Create(15, Habilidad.Speaking, "Intonation: Yes/No question ends with...", 
                JsonSerializer.Serialize(new[]{"falling tone","rising tone","flat"}), "rising tone", NivelIngles.A2).Value!,
            PreguntaDiagnostica.Create(16, Habilidad.Speaking, "Minimal pair: 'v' vs 'b' — 'very' starts with...", 
                JsonSerializer.Serialize(new[]{"b sound","v sound /v/","w sound"}), "v sound /v/", NivelIngles.A1).Value!,
        };

        _context.PreguntasDiagnosticas.AddRange(preguntas);
        await _context.SaveChangesAsync(cancellationToken);
    }

    private sealed record SeedQuestion(
        int Id,
        Habilidad Habilidad,
        string Contenido,
        string OpcionesJson,
        string RespuestaCorrecta,
        NivelIngles Nivel);
}

public interface ISeedDataService
{
    Task SeedAsync(CancellationToken cancellationToken = default);
}