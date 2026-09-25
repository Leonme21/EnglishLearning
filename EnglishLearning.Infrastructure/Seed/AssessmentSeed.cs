using System.Text.Json;
using EnglishLearning.Domain.Entities;
using EnglishLearning.Domain.Enums;
using EnglishLearning.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EnglishLearning.Infrastructure.Seed;

public static class AssessmentSeed
{
    public static async Task SeedAsync(EnglishLearningDbContext ctx)
    {
        if (!await ctx.Usuarios.AnyAsync())
        {
            ctx.Usuarios.Add(new Usuario { Nombre = "Estudiante Demo", Correo = "demo@demo.com", NivelActual = NivelIngles.SinAsignar });
            await ctx.SaveChangesAsync();
        }

        if (await ctx.PreguntasDiagnosticas.AnyAsync()) return;

        var preguntas = new List<PreguntaDiagnostica>
        {
            // Reading — falsos amigos
            new() { Habilidad=Habilidad.Reading, Contenido="Choose correct: 'Actually' means...", OpcionesJson=JsonSerializer.Serialize(new[]{"Actualmente","En realidad","Actualmente y en realidad"}), RespuestaCorrecta="En realidad", Nivel=NivelIngles.A2 },
            new() { Habilidad=Habilidad.Reading, Contenido="Embarrassed means...", OpcionesJson=JsonSerializer.Serialize(new[]{"Embarazada","Avergonzado","Embarcado"}), RespuestaCorrecta="Avergonzado", Nivel=NivelIngles.A1 },
            new() { Habilidad=Habilidad.Reading, Contenido="Notice the transfer: 'I am agree' is incorrect, correct is...", OpcionesJson=JsonSerializer.Serialize(new[]{"I am agree","I agree","I am agreed"}), RespuestaCorrecta="I agree", Nivel=NivelIngles.B1 },
            new() { Habilidad=Habilidad.Reading, Contenido="Constipated means...", OpcionesJson=JsonSerializer.Serialize(new[]{"Estreñido","Constipado (resfriado)","Constante"}), RespuestaCorrecta="Estreñido", Nivel=NivelIngles.B1 },
            // Listening — vicios pronunciación (contenido simula audio transcript)
            new() { Habilidad=Habilidad.Listening, Contenido="Audio: /θ/ vs /ð/ — 'think' starts with...", OpcionesJson=JsonSerializer.Serialize(new[]{"s sound","th voiceless /θ/","d sound"}), RespuestaCorrecta="th voiceless /θ/", Nivel=NivelIngles.A2 },
            new() { Habilidad=Habilidad.Listening, Contenido="Audio: 'I can' vs 'I can't' — difference is...", OpcionesJson=JsonSerializer.Serialize(new[]{"vowel length + /t/","only volume","no difference"}), RespuestaCorrecta="vowel length + /t/", Nivel=NivelIngles.B2 },
            new() { Habilidad=Habilidad.Listening, Contenido="Audio transcript: 'I have lived here for 3 years' — correct question?", OpcionesJson=JsonSerializer.Serialize(new[]{"How long have you lived here?","How long are you living here?","How long you live here?"}), RespuestaCorrecta="How long have you lived here?", Nivel=NivelIngles.B1 },
            new() { Habilidad=Habilidad.Listening, Contenido="Audio: 'ship' vs 'sheep' — /ɪ/ vs /i:/ — 'ship' is...", OpcionesJson=JsonSerializer.Serialize(new[]{"long /i:/","short /ɪ/","diphtong"}), RespuestaCorrecta="short /ɪ/", Nivel=NivelIngles.A2 },
            // Writing — transferencias gramaticales
            new() { Habilidad=Habilidad.Writing, Contenido="Correct: 'I have 30 years' →", OpcionesJson=JsonSerializer.Serialize(new[]{"I have 30 years","I am 30 years old","I have 30 years old"}), RespuestaCorrecta="I am 30 years old", Nivel=NivelIngles.A1 },
            new() { Habilidad=Habilidad.Writing, Contenido="Make vs Do: '___ homework'", OpcionesJson=JsonSerializer.Serialize(new[]{"make","do","make/do both"}), RespuestaCorrecta="do", Nivel=NivelIngles.A2 },
            new() { Habilidad=Habilidad.Writing, Contenido="Correct question: 'Why you are late?' →", OpcionesJson=JsonSerializer.Serialize(new[]{"Why you are late?","Why are you late?","Why you late?"}), RespuestaCorrecta="Why are you late?", Nivel=NivelIngles.B1 },
            new() { Habilidad=Habilidad.Writing, Contenido="____ you like coffee? (offer)", OpcionesJson=JsonSerializer.Serialize(new[]{"Do","Would","Are"}), RespuestaCorrecta="Would", Nivel=NivelIngles.B2 },
            // Speaking — fluidez guiada, pronunciación
            new() { Habilidad=Habilidad.Speaking, Contenido="Pronunciation: 'vegetable' has ... syllables", OpcionesJson=JsonSerializer.Serialize(new[]{"4","3","5"}), RespuestaCorrecta="3", Nivel=NivelIngles.B1 },
            new() { Habilidad=Habilidad.Speaking, Contenido="Choose natural: 'Can you give me a hand?' means...", OpcionesJson=JsonSerializer.Serialize(new[]{"Give hand","Help me","Shake hand"}), RespuestaCorrecta="Help me", Nivel=NivelIngles.B2 },
            new() { Habilidad=Habilidad.Speaking, Contenido="Intonation: Yes/No question ends with...", OpcionesJson=JsonSerializer.Serialize(new[]{"falling tone","rising tone","flat"}), RespuestaCorrecta="rising tone", Nivel=NivelIngles.A2 },
            new() { Habilidad=Habilidad.Speaking, Contenido="Minimal pair: 'v' vs 'b' — 'very' starts with...", OpcionesJson=JsonSerializer.Serialize(new[]{"b sound","v sound /v/","w sound"}), RespuestaCorrecta="v sound /v/", Nivel=NivelIngles.A1 },
        };

        ctx.PreguntasDiagnosticas.AddRange(preguntas);
        await ctx.SaveChangesAsync();
    }
}
