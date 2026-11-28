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

    public SeedDataService(
        EnglishLearningDbContext context,
        IOptions<SeedDataOptions> options)
    {
        _context = context;
        _options = options.Value;
    }

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        if (!_options.AutoSeedOnStartup) return;

        await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        await _context.Database.ExecuteSqlRawAsync("SELECT pg_advisory_xact_lock(271817)", cancellationToken);

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
                            _context.PreguntasDiagnosticas.Add(pregunta.GetValueOrThrow());
                    }
                    await _context.SaveChangesAsync(cancellationToken);
                }
            }
            else
            {
                await SeedDefaultQuestionsAsync(cancellationToken);
            }

            await AlignQuestionSequenceAsync(cancellationToken);
        }

        await AddMissingLessonsAsync(cancellationToken);

        await AddPracticeExercisesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }

    private async Task AddMissingLessonsAsync(CancellationToken ct)
    {
        var existing = await _context.Lecciones.AsNoTracking()
            .Select(lesson => new { lesson.NivelRequerido, lesson.Titulo })
            .ToListAsync(ct);
        var existingKeys = existing.Select(lesson => (lesson.NivelRequerido, lesson.Titulo)).ToHashSet();

        foreach (var lesson in LessonSeedData.Build().Concat(ExpandedLessonSeedData.Build()))
        {
            if (existingKeys.Add((lesson.NivelRequerido, lesson.Titulo)))
                _context.Lecciones.Add(lesson);
        }

        await _context.SaveChangesAsync(ct);
    }

    private async Task AddPracticeExercisesAsync(CancellationToken ct)
    {
        var lessons = await _context.Lecciones.Include(l => l.Ejercicios).ToListAsync(ct);
        foreach (var lesson in lessons)
        {
            var additional = LessonSeedData.AdditionalPractice(lesson);
            if (additional is not null && lesson.Ejercicios.All(e => e.Contenido != additional.Contenido))
                _context.Ejercicios.Add(additional);
        }
        await _context.SaveChangesAsync(ct);
    }

    private Task AlignQuestionSequenceAsync(CancellationToken ct) =>
        _context.Database.ExecuteSqlRawAsync("""
            SELECT setval(pg_get_serial_sequence('"PreguntasDiagnosticas"', 'Id'),
                          GREATEST((SELECT COALESCE(MAX("Id"), 0) FROM "PreguntasDiagnosticas"), 1));
            """, ct);

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