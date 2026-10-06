using EnglishLearning.Domain.Entities;
using EnglishLearning.Domain.Enums;

namespace EnglishLearning.Infrastructure.Services;

/// <summary>
/// Lecciones semilla por nivel CEFR. Cada lección incluye un ejercicio por tipo.
/// Convenciones de contenido consumidas por el cliente:
///  - Opciones de respuesta al final del contenido: <c>[opcion1|opcion2|opcion3]</c>.
///  - Texto entre « » es un pasaje: en ejercicios de Escucha se reproduce con TTS (oculto),
///    en el resto se muestra como texto destacado.
/// </summary>
internal static class LessonSeedData
{
    private sealed record Ex(TipoEjercicio Tipo, string Contenido, string Respuesta);
    private sealed record Lesson(NivelIngles Nivel, string Titulo, string Descripcion, Ex[] Ejercicios);

    private static readonly Lesson[] Lessons =
    {
        // ───────────── A1 ─────────────
        new(NivelIngles.A1, "Saludos y presentaciones",
            "Aprende a saludar, presentarte y decir tu edad correctamente (¡sin decir 'I have 20 years'!).",
            new Ex[]
            {
                new(TipoEjercicio.OpcionMultiple, "Completa: Hello, my name ___ Anna. [is|am|are]", "is"),
                new(TipoEjercicio.Lectura, "Lee: «Hi! I'm Tom. I am ten years old and I live in London.» ¿Dónde vive Tom? [Paris|London|Madrid]", "London"),
                new(TipoEjercicio.Escucha, "Escucha y elige el número que oyes: «I have three brothers.» [two|three|four]", "three"),
                new(TipoEjercicio.Escritura, "Traduce al inglés: \"Tengo veinte años.\"", "I am twenty years old"),
            }),
        new(NivelIngles.A1, "Mi rutina diaria",
            "Presente simple para hablar de hábitos: la 's' de la tercera persona y los días de la semana.",
            new Ex[]
            {
                new(TipoEjercicio.OpcionMultiple, "Completa: She ___ coffee every morning. [drink|drinks|drinking]", "drinks"),
                new(TipoEjercicio.Lectura, "Lee: «Maria gets up at seven. She has breakfast and goes to work by bus.» ¿Cómo va Maria al trabajo? [by car|by bus|on foot]", "by bus"),
                new(TipoEjercicio.Escucha, "Escucha y elige el día: «See you on Friday!» [Monday|Thursday|Friday]", "Friday"),
                new(TipoEjercicio.Escritura, "Escribe en inglés: \"Yo no hablo francés.\"", "I don't speak French"),
            }),

        // ───────────── A2 ─────────────
        new(NivelIngles.A2, "El pasado simple",
            "Verbos regulares e irregulares para contar lo que hiciste ayer, el finde o en vacaciones.",
            new Ex[]
            {
                new(TipoEjercicio.OpcionMultiple, "Completa: Yesterday I ___ to the cinema. [go|went|gone]", "went"),
                new(TipoEjercicio.Lectura, "Lee: «Last summer we visited Rome. We ate pizza every day and saw the Colosseum.» ¿Qué comieron cada día? [pasta|pizza|ice cream]", "pizza"),
                new(TipoEjercicio.Escucha, "Escucha y elige el verbo: «I bought a new phone last week.» [brought|bought|built]", "bought"),
                new(TipoEjercicio.Escritura, "Corrige el error y escribe la frase: \"I am agree with you.\"", "I agree with you"),
            }),
        new(NivelIngles.A2, "Make vs Do y phrasal verbs",
            "Dos verbos que confunden a todo hispanohablante y los phrasal verbs más comunes.",
            new Ex[]
            {
                new(TipoEjercicio.OpcionMultiple, "Completa: Can you ___ me a favour? [make|do|have]", "do"),
                new(TipoEjercicio.Lectura, "Lee: «Please turn off the lights when you leave the room.» 'Turn off' significa... [encender|apagar|girar]", "apagar"),
                new(TipoEjercicio.Escucha, "Escucha: ¿puede o no puede? «I can't swim very well.» [can|can't]", "can't"),
                new(TipoEjercicio.Escritura, "Traduce: \"Tengo que hacer los deberes.\"", "I have to do my homework"),
            }),

        // ───────────── B1 ─────────────
        new(NivelIngles.B1, "Present perfect vs past simple",
            "Cuándo usar 'have done' y cuándo 'did'. Conectores de contraste como however y although.",
            new Ex[]
            {
                new(TipoEjercicio.OpcionMultiple, "Completa: I ___ in Madrid since 2019. [live|lived|have lived]", "have lived"),
                new(TipoEjercicio.Lectura, "Lee: «Although it was raining, they decided to go hiking. However, they returned early.» 'However' expresa... [contraste|causa|tiempo]", "contraste"),
                new(TipoEjercicio.Escucha, "Escucha: ¿cuánto tiempo lleva esperando? «I've been waiting for twenty minutes.» [twelve minutes|twenty minutes|two minutes]", "twenty minutes"),
                new(TipoEjercicio.Escritura, "Escribe en inglés: \"Nunca he estado en Londres.\"", "I have never been to London"),
            }),
        new(NivelIngles.B1, "Falsos amigos",
            "Palabras que parecen españolas pero significan otra cosa: actually, embarrassed, carpet...",
            new Ex[]
            {
                new(TipoEjercicio.OpcionMultiple, "'Actually' significa... [actualmente|en realidad|activamente]", "en realidad"),
                new(TipoEjercicio.Lectura, "Lee: «He felt so embarrassed when he forgot her name.» ¿Cómo se sintió? [embarazado|avergonzado|enfadado]", "avergonzado"),
                new(TipoEjercicio.Escucha, "Escucha y elige la traducción de la palabra clave: «I need to buy a new carpet.» [carpeta|alfombra|cartera]", "alfombra"),
                new(TipoEjercicio.Escritura, "Traduce usando la expresión correcta: \"Estoy resfriado.\"", "I have a cold"),
            }),

        // ───────────── B2 ─────────────
        new(NivelIngles.B2, "Condicionales y matices",
            "Segundo y tercer condicional, y expresiones para matizar opiniones con precisión.",
            new Ex[]
            {
                new(TipoEjercicio.OpcionMultiple, "Completa: If I ___ more time, I would travel more. [have|had|will have]", "had"),
                new(TipoEjercicio.Lectura, "Lee: «The proposal, while ambitious, overlooks several practical constraints.» El autor piensa que la propuesta es... [perfecta|poco realista en parte|aburrida]", "poco realista en parte"),
                new(TipoEjercicio.Escucha, "Escucha y elige el significado: «I'm on the fence about it.» [indeciso|seguro|enfadado]", "indeciso"),
                new(TipoEjercicio.Escritura, "Reescribe empezando por 'If': \"I didn't study, so I failed.\"", "If I had studied, I wouldn't have failed"),
            }),
        new(NivelIngles.B2, "Registro formal vs informal",
            "Adapta tu inglés al contexto: correos profesionales, conectores formales y cortesía.",
            new Ex[]
            {
                new(TipoEjercicio.OpcionMultiple, "Elige la opción más formal: [Hey, what's up?|Could you please assist me?|Gimme a hand]", "Could you please assist me?"),
                new(TipoEjercicio.Lectura, "Lee: «We regret to inform you that your application has been unsuccessful.» El mensaje comunica... [una aceptación|un rechazo|una invitación]", "un rechazo"),
                new(TipoEjercicio.Escucha, "Escucha y elige el conector: «Nevertheless, we will proceed with the plan.» [Therefore|Nevertheless|Moreover]", "Nevertheless"),
                new(TipoEjercicio.Escritura, "Haz más formal esta frase: \"I want to know the price.\"", "I would like to know the price"),
            }),

        // ───────────── C1 ─────────────
        new(NivelIngles.C1, "Expresiones idiomáticas",
            "Idioms y estructuras enfáticas que usan los nativos: inversión, ironía y subtexto.",
            new Ex[]
            {
                new(TipoEjercicio.OpcionMultiple, "'To beat around the bush' significa... [ir al grano|andarse por las ramas|trabajar duro]", "andarse por las ramas"),
                new(TipoEjercicio.Lectura, "Lee: «His speech was a masterclass in saying a great deal while committing to nothing.» El tono del autor es... [admirativo|irónico|neutral]", "irónico"),
                new(TipoEjercicio.Escucha, "Escucha y elige el significado: «Let's call it a day.» [empezar|terminar por hoy|llamar a alguien]", "terminar por hoy"),
                new(TipoEjercicio.Escritura, "Reescribe con inversión empezando por 'Never': \"I had never seen such a view.\"", "Never had I seen such a view"),
            }),
        new(NivelIngles.C1, "Inglés académico",
            "Vocabulario y estructuras impersonales para ensayos, informes y presentaciones.",
            new Ex[]
            {
                new(TipoEjercicio.OpcionMultiple, "Completa: The results ___ that the hypothesis was correct. [suggest|says|tell]", "suggest"),
                new(TipoEjercicio.Lectura, "Lee: «Notwithstanding the limited sample, the findings are robust.» 'Notwithstanding' equivale a... [debido a|a pesar de|además de]", "a pesar de"),
                new(TipoEjercicio.Escucha, "Escucha y elige la palabra que oyes: «The data were inconclusive.» [inconclusive|inclusive|conclusive]", "inconclusive"),
                new(TipoEjercicio.Escritura, "Parafrasea de forma impersonal empezando por 'It is': \"Lots of people believe that...\"", "It is widely believed that"),
            }),
    };

    public static IEnumerable<Leccion> Build()
    {
        foreach (var l in Lessons)
        {
            var leccion = Leccion.Create(0, l.Titulo, l.Descripcion, l.Nivel).GetValueOrThrow();
            foreach (var e in l.Ejercicios)
            {
                // LeccionId is fixed up by EF through the navigation
                var ejercicio = Ejercicio.Create(0, 0, e.Contenido, e.Respuesta, e.Tipo).GetValueOrThrow();
                leccion.AgregarEjercicio(ejercicio);
            }
            yield return leccion;
        }
    }
}
