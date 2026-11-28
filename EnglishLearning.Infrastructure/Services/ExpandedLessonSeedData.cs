using EnglishLearning.Domain.Entities;
using EnglishLearning.Domain.Enums;

namespace EnglishLearning.Infrastructure.Services;

internal static class ExpandedLessonSeedData
{
    private sealed record Exercise(TipoEjercicio Type, string Prompt, string Answer);

    private static Exercise Choice(string prompt, string answer, string options) =>
        new(TipoEjercicio.OpcionMultiple, $"{prompt} [{options}]", answer);

    private static Exercise Reading(string prompt, string answer, string options) =>
        new(TipoEjercicio.Lectura, $"{prompt} [{options}]", answer);

    private static Exercise Listening(string prompt, string answer, string options) =>
        new(TipoEjercicio.Escucha, $"{prompt} [{options}]", answer);

    private static Exercise Writing(string prompt, string answer) =>
        new(TipoEjercicio.Escritura, prompt, answer);

    private static Leccion Lesson(NivelIngles level, string title, string description, params Exercise[] exercises)
    {
        var lesson = Leccion.Create(0, title, description, level).GetValueOrThrow();
        foreach (var exercise in exercises)
            lesson.AgregarEjercicio(Ejercicio.Create(0, 0, exercise.Prompt, exercise.Answer, exercise.Type).GetValueOrThrow());
        return lesson;
    }

    public static IEnumerable<Leccion> Build()
    {
        // A1: everyday vocabulary and short, concrete exchanges.
        yield return Lesson(NivelIngles.A1, "Mi familia y amigos", "Describe relaciones y posesiones con have y adjetivos posesivos.",
            Choice("This is my sister. ___ name is Lucy.", "Her", "His|Her|Their"),
            Reading("Lee: «I live with my mother and two brothers.» ¿Cuántos hermanos tiene?", "Two", "One|Two|Three"),
            Listening("Listen: «Ben is my cousin.» Who is Ben?", "Cousin", "Friend|Brother|Cousin"),
            Writing("Traduce: Mi padre es profesor.", "My father is a teacher"));

        yield return Lesson(NivelIngles.A1, "Números, horas y fechas", "Pregunta y comprende horarios, números y días.",
            Choice("What time ___ it?", "is", "is|are|do"),
            Reading("Lee: «The shop opens at nine and closes at six.» ¿A qué hora abre?", "Nine", "Six|Nine|Ten"),
            Listening("Listen: «The class starts at half past three.» What time?", "3:30", "3:00|3:30|4:30"),
            Writing("Escribe en inglés: Hoy es martes.", "Today is Tuesday"));

        yield return Lesson(NivelIngles.A1, "Comidas y bebidas", "Pide alimentos y expresa gustos de manera sencilla.",
            Choice("I ___ apples, but I don't like bananas.", "like", "like|likes|liking"),
            Reading("Lee: «For lunch I have rice and chicken.» ¿Qué come?", "Rice and chicken", "Bread and milk|Rice and chicken|Fish and salad"),
            Listening("Listen: «I'd like a glass of water.» What does the speaker want?", "Water", "Tea|Juice|Water"),
            Writing("Traduce: Me gusta el café.", "I like coffee"));

        yield return Lesson(NivelIngles.A1, "Mi casa y mis objetos", "Usa there is, there are y preposiciones de lugar.",
            Choice("There ___ two chairs in the room.", "are", "is|are|am"),
            Reading("Lee: «The keys are under the table.» ¿Dónde están las llaves?", "Under the table", "On the table|Under the table|In the bag"),
            Listening("Listen: «My room has a bed and a desk.» What is in the room?", "A desk", "A sofa|A desk|A fridge"),
            Writing("Traduce: Hay un libro sobre la mesa.", "There is a book on the table"));

        yield return Lesson(NivelIngles.A1, "Compras y precios", "Pregunta precios y utiliza this, that y cantidades.",
            Choice("How ___ is this shirt?", "much", "many|much|long"),
            Reading("Lee: «The blue bag costs ten dollars.» ¿Cuánto cuesta?", "Ten dollars", "Five dollars|Ten dollars|Twenty dollars"),
            Listening("Listen: «I need three apples, please.» How many?", "Three", "Two|Three|Four"),
            Writing("Traduce: Quiero esta camiseta.", "I want this shirt"));

        yield return Lesson(NivelIngles.A1, "Mi ciudad", "Identifica lugares y pregunta dónde están.",
            Choice("Where ___ the library?", "is", "is|are|do"),
            Reading("Lee: «The bank is next to the supermarket.» ¿Dónde está el banco?", "Next to the supermarket", "Behind the school|Next to the supermarket|Inside the park"),
            Listening("Listen: «Turn left at the park.» Which direction?", "Left", "Right|Straight ahead|Left"),
            Writing("Traduce: La farmacia está cerca.", "The pharmacy is near"));

        yield return Lesson(NivelIngles.A1, "Ropa y clima", "Describe el tiempo y la ropa con adjetivos básicos.",
            Choice("It's cold today. Put ___ your coat.", "on", "on|at|in"),
            Reading("Lee: «It's sunny and hot. Emma wears a dress.» ¿Qué clima hace?", "Sunny and hot", "Rainy and cold|Sunny and hot|Cloudy and windy"),
            Listening("Listen: «Take an umbrella. It is raining.» What do you need?", "An umbrella", "A hat|An umbrella|A scarf"),
            Writing("Traduce: Hoy hace frío.", "It is cold today"));

        yield return Lesson(NivelIngles.A1, "Habilidades y pasatiempos", "Habla de actividades y capacidades usando can.",
            Choice("I can ___ a bike.", "ride", "rides|riding|ride"),
            Reading("Lee: «Sara plays tennis every Saturday.» ¿Qué deporte practica?", "Tennis", "Basketball|Tennis|Football"),
            Listening("Listen: «I can swim, but I cannot dance.» Which activity can the speaker do?", "Swim", "Dance|Swim|Sing"),
            Writing("Traduce: Puedo tocar la guitarra.", "I can play the guitar"));

        // A2: predictable social and travel situations.
        yield return Lesson(NivelIngles.A2, "Planes de fin de semana", "Expresa planes con be going to y expresiones temporales.",
            Choice("They ___ to visit their grandparents tomorrow.", "are going", "are going|went|goes"),
            Reading("Lee: «If the weather is good, we'll have a picnic on Sunday.» ¿Cuándo será?", "Sunday", "Friday|Saturday|Sunday"),
            Listening("Listen: «I'm going to meet Ana at the station at seven.» Where?", "At the station", "At the cinema|At the station|At home"),
            Writing("Traduce: Voy a cocinar esta noche.", "I am going to cook tonight"));

        yield return Lesson(NivelIngles.A2, "Comparaciones y preferencias", "Usa comparativos para elegir objetos y servicios.",
            Choice("This train is ___ than the bus.", "faster", "fastest|faster|more fast"),
            Reading("Lee: «Room A is bigger, but Room B is cheaper.» ¿Cuál cuesta menos?", "Room B", "Room A|Room B|Both rooms"),
            Listening("Listen: «The red jacket is warmer than the blue one.» Which is warmer?", "The red jacket", "The blue jacket|The red jacket|Neither"),
            Writing("Traduce: Mi habitación es más pequeña que la tuya.", "My room is smaller than yours"));

        yield return Lesson(NivelIngles.A2, "Viajes y transporte", "Comprende horarios, billetes y señales de viaje.",
            Choice("I'd like a return ___ to Oxford, please.", "ticket", "ticket|trip|platform"),
            Reading("Lee: «The 10:15 train has been cancelled. Take the 11:00 train instead.» ¿Cuál debes tomar?", "11:00", "10:15|11:00|11:15"),
            Listening("Listen: «Your bus leaves from platform four.» Which platform?", "Four", "Two|Three|Four"),
            Writing("Traduce: Perdí mi pasaporte ayer.", "I lost my passport yesterday"));

        yield return Lesson(NivelIngles.A2, "En el restaurante", "Solicita comida, cuenta y cambios con cortesía.",
            Choice("Could I have the ___, please? I'd like to pay.", "bill", "menu|bill|order"),
            Reading("Lee: «Today's soup contains nuts. The salad does not.» ¿Cuál no contiene nueces?", "The salad", "The soup|The salad|Both"),
            Listening("Listen: «Would you like rice or potatoes with your fish?» What is being offered?", "A choice of sides", "A dessert|A choice of sides|A drink"),
            Writing("Traduce: Me gustaría reservar una mesa para dos.", "I would like to book a table for two"));

        yield return Lesson(NivelIngles.A2, "Salud y consejos cotidianos", "Describe síntomas sencillos y practica should y must.",
            Choice("You ___ drink more water when it's hot.", "should", "should|can to|would to"),
            Reading("Lee: «The clinic is closed on Sundays. For appointments, call on Monday.» ¿Cuándo debes llamar?", "Monday", "Sunday|Monday|Saturday"),
            Listening("Listen: «I have a sore throat and I feel tired.» Which symptom is mentioned?", "A sore throat", "A broken arm|A sore throat|An earache"),
            Writing("Traduce: Deberías descansar un poco.", "You should rest a little"));

        yield return Lesson(NivelIngles.A2, "Experiencias y recuerdos", "Cuenta acontecimientos pasados y pregunta por experiencias.",
            Choice("We ___ a great time at the festival last year.", "had", "have|had|has"),
            Reading("Lee: «Last July, Nina travelled alone for the first time. She was nervous but enjoyed it.» ¿Cómo terminó el viaje?", "She enjoyed it", "She hated it|She enjoyed it|She did not travel"),
            Listening("Listen: «I visited the museum, but I didn't see the exhibition.» What did the speaker miss?", "The exhibition", "The museum|The exhibition|The bus"),
            Writing("Traduce: No vi la película ayer.", "I did not see the film yesterday"));

        yield return Lesson(NivelIngles.A2, "Trabajo y estudios", "Describe horarios y obligaciones con have to y don't have to.",
            Choice("I ___ finish my homework before dinner.", "have to", "has to|have to|having to"),
            Reading("Lee: «The office closes at five, but staff must arrive by eight.» ¿A qué hora deben llegar?", "Eight", "Five|Eight|Ten"),
            Listening("Listen: «You don't have to bring a laptop. We have computers here.» What is optional?", "Bringing a laptop", "Attending class|Bringing a laptop|Using a computer"),
            Writing("Traduce: Tengo que trabajar mañana.", "I have to work tomorrow"));

        yield return Lesson(NivelIngles.A2, "Mensajes e invitaciones", "Invita, acepta y rechaza planes de forma educada.",
            Choice("___ you like to come to my birthday party?", "Would", "Do|Would|Are"),
            Reading("Lee: «Thanks for inviting me. I can't come on Friday. Could we meet on Saturday?» ¿Qué propone?", "Meet on Saturday", "Meet on Friday|Meet on Saturday|Cancel all plans"),
            Listening("Listen: «Sorry, I'm running ten minutes late.» How late?", "Ten minutes", "Five minutes|Ten minutes|Twenty minutes"),
            Writing("Traduce: ¿Te gustaría ir al cine?", "Would you like to go to the cinema"));

        // B1: connected communication and explanations.
        yield return Lesson(NivelIngles.B1, "Historias y narración", "Ordena acciones usando past simple, past continuous y conectores.",
            Choice("While I ___ home, it started to rain.", "was walking", "walked|was walking|have walked"),
            Reading("Lee: «At first, the trip seemed impossible. Eventually, the group found a safer route.» ¿Qué ocurrió al final?", "They found another route", "They stayed at home|They found another route|They got lost"),
            Listening("Listen: «By the time we arrived, the concert had already started.» What happened first?", "The concert started", "We arrived|The concert started|We bought tickets"),
            Writing("Une las ideas usando while: I cooked dinner. My brother set the table.", "I cooked dinner while my brother set the table"));

        yield return Lesson(NivelIngles.B1, "Opiniones y argumentos", "Expresa opiniones, justificaciones y desacuerdos.",
            Choice("I understand your point. ___, I see things differently.", "However", "Because|However|Therefore"),
            Reading("Lee: «Public transport reduces traffic, although it must be reliable to attract users.» ¿Qué condición menciona?", "Reliability", "High prices|Reliability|Private cars"),
            Listening("Listen: «I agree up to a point, but the costs concern me.» What is the speaker's position?", "Partial agreement", "Complete disagreement|Partial agreement|No opinion"),
            Writing("Traduce: Estoy de acuerdo, pero tengo algunas dudas.", "I agree but I have some doubts"));

        yield return Lesson(NivelIngles.B1, "Problemas y soluciones", "Propón soluciones mediante could, should y condiciones reales.",
            Choice("If we ___ now, we'll catch the last train.", "leave", "left|leave|will leave"),
            Reading("Lee: «The website was slow because images were too large. Compressing them reduced loading time.» ¿Qué mejoró el problema?", "Compressing images", "Adding images|Compressing images|Removing the website"),
            Listening("Listen: «Why don't we divide the work into smaller tasks?» What is suggested?", "Splitting the work", "Stopping work|Splitting the work|Hiring a manager"),
            Writing("Traduce: Podríamos probar otra solución.", "We could try another solution"));

        yield return Lesson(NivelIngles.B1, "Noticias y medios", "Comprende información general y practica la voz pasiva.",
            Choice("The new library ___ opened last Monday.", "was", "has|was|were"),
            Reading("Lee: «Local businesses welcomed the transport plan, but residents asked for more consultation.» ¿Quién pidió más participación?", "Residents", "Local businesses|Residents|Tourists"),
            Listening("Listen: «The event has been postponed until next month.» What changed?", "The date", "The location|The date|The speaker"),
            Writing("Pasa a voz pasiva: They built the bridge in 2010.", "The bridge was built in 2010"));

        yield return Lesson(NivelIngles.B1, "Objetivos y aprendizaje", "Explica hábitos, objetivos y avances personales.",
            Choice("I've been studying English ___ six months.", "for", "since|for|during"),
            Reading("Lee: «Rather than memorising every word, Leo reviews key phrases regularly.» ¿Qué método utiliza?", "Regular review of phrases", "Memorising everything|Regular review of phrases|Avoiding practice"),
            Listening("Listen: «I used to be afraid of speaking, but I practise every day now.» What has changed?", "The speaker practises regularly", "The speaker stopped learning|The speaker practises regularly|Nothing"),
            Writing("Traduce: Llevo dos años aprendiendo inglés.", "I have been learning English for two years"));

        yield return Lesson(NivelIngles.B1, "Entrevistas y experiencia profesional", "Explica responsabilidades y experiencia con ejemplos.",
            Choice("I'm responsible ___ organising team meetings.", "for", "of|for|at"),
            Reading("Lee: «The role requires communication skills, but previous management experience is optional.» ¿Qué es obligatorio?", "Communication skills", "Management experience|Communication skills|A company car"),
            Listening("Listen: «I helped resolve a problem by listening carefully to both colleagues.» Which ability is demonstrated?", "Conflict resolution", "Technical writing|Conflict resolution|Budget planning"),
            Writing("Traduce: He trabajado en varios proyectos.", "I have worked on several projects"));

        yield return Lesson(NivelIngles.B1, "Medio ambiente y hábitos", "Describe causa, consecuencia y posibilidades futuras.",
            Choice("If people recycle more, they ___ less waste.", "will produce", "produced|would produced|will produce"),
            Reading("Lee: «Although reusable bags cost more initially, they reduce waste when used repeatedly.» ¿Cuál es su ventaja?", "Less waste", "Lower initial price|Less waste|Greater weight"),
            Listening("Listen: «We reduced electricity use by turning off unused lights.» What action helped?", "Turning off lights", "Buying more lights|Turning off lights|Opening windows"),
            Writing("Traduce: Deberíamos reducir el consumo de energía.", "We should reduce energy consumption"));

        yield return Lesson(NivelIngles.B1, "Correo y comunicación laboral", "Redacta solicitudes y aclaraciones con lenguaje profesional cotidiano.",
            Choice("Could you let me know ___ the meeting starts?", "when", "what|when|which"),
            Reading("Lee: «Please send the draft by Thursday so I can review it before Friday's meeting.» ¿Cuál es la fecha de entrega del borrador?", "Thursday", "Wednesday|Thursday|Friday"),
            Listening("Listen: «I'm following up on the message I sent last week.» Why is the speaker writing?", "To check on an earlier message", "To introduce themselves|To check on an earlier message|To cancel a project"),
            Writing("Traduce: Gracias por tu rápida respuesta.", "Thank you for your quick response"));

        // B2: nuance, hypotheses and professional discussion.
        yield return Lesson(NivelIngles.B2, "Debates y contraargumentos", "Matiza afirmaciones y reconoce concesiones.",
            Choice("___ the evidence is promising, further trials are necessary.", "Although", "Unless|Although|Despite"),
            Reading("Lee: «The policy may be costly in the short term, yet its long-term benefits cannot be ignored.» ¿Qué reconoce el autor?", "A short-term cost and long-term value", "No benefits|A short-term cost and long-term value|A guaranteed loss"),
            Listening("Listen: «That is a valid concern; nevertheless, we should consider alternatives.» What does the speaker do?", "Acknowledges an objection", "Rejects discussion|Acknowledges an objection|Agrees unconditionally"),
            Writing("Une usando although: The proposal is expensive. It may be worthwhile.", "Although the proposal is expensive it may be worthwhile"));

        yield return Lesson(NivelIngles.B2, "Hipótesis y decisiones", "Explora consecuencias hipotéticas con condicionales mixtos.",
            Choice("If I had accepted the offer, I ___ in Berlin now.", "would be living", "will live|would be living|had lived"),
            Reading("Lee: «Had we checked the figures earlier, the error might have been avoided.» ¿Qué implica?", "The figures were not checked early", "The error was avoided|The figures were not checked early|There were no figures"),
            Listening("Listen: «If I were more confident, I would have applied for the position.» What prevented the application?", "Lack of confidence", "Lack of qualifications|Lack of confidence|An expired passport"),
            Writing("Reescribe con if: I didn't save enough, so I can't travel now.", "If I had saved enough I could travel now"));

        yield return Lesson(NivelIngles.B2, "Ciencia y divulgación", "Interpreta resultados, evidencia y limitaciones de un estudio.",
            Choice("The findings ___ be interpreted with caution.", "should", "should|would to|must to"),
            Reading("Lee: «A strong association was observed, but this alone does not establish causation.» ¿Qué advierte el texto?", "Association does not prove cause", "Causation was established|Association does not prove cause|There was no association"),
            Listening("Listen: «The study was limited by its small sample size.» What is the limitation?", "Few participants", "Too much data|Few participants|No hypothesis"),
            Writing("Traduce: Se necesita más evidencia para confirmar los resultados.", "More evidence is needed to confirm the results"));

        yield return Lesson(NivelIngles.B2, "Negociación y acuerdos", "Usa expresiones de compromiso, condiciones y concesiones.",
            Choice("We can accept the deadline ___ you provide additional support.", "provided that", "provided that|even though|due to"),
            Reading("Lee: «The supplier agreed to lower the price in exchange for a longer contract.» ¿Qué pidió a cambio?", "A longer contract", "A shorter contract|A longer contract|Faster payment"),
            Listening("Listen: «We might meet halfway if both sides make concessions.» What is proposed?", "A compromise", "Ending negotiations|A compromise|Accepting every demand"),
            Writing("Traduce: Estamos dispuestos a reconsiderar nuestra oferta.", "We are willing to reconsider our offer"));

        yield return Lesson(NivelIngles.B2, "Cultura y perspectivas", "Interpreta implicaciones culturales y usa expresiones de contraste.",
            Choice("What seems polite in one culture ___ be perceived differently in another.", "may", "may|ought|must to"),
            Reading("Lee: «Silence can signal respect in some contexts, whereas in others it may suggest discomfort.» ¿Cuál es la idea principal?", "Silence has different meanings", "Silence is always rude|Silence has different meanings|Everyone interprets silence alike"),
            Listening("Listen: «I took the comment literally, but it was meant as a joke.» What caused the misunderstanding?", "A literal interpretation", "A translation error|A literal interpretation|Background noise"),
            Writing("Traduce: Debemos evitar hacer suposiciones sobre otras culturas.", "We should avoid making assumptions about other cultures"));

        yield return Lesson(NivelIngles.B2, "Relaciones y comunicación", "Explica intenciones y usa verbos de reporte.",
            Choice("She denied ___ the message without permission.", "reading", "to read|reading|read"),
            Reading("Lee: «Although he apologised for the delay, he insisted that the decision had been justified.» ¿De qué se disculpó?", "The delay", "The decision|The delay|The apology"),
            Listening("Listen: «She suggested that we discuss the issue in private.» What did she propose?", "A private conversation", "A public meeting|A private conversation|Avoiding the issue"),
            Writing("Pasa a estilo indirecto: He said, 'I am busy.'", "He said that he was busy"));

        yield return Lesson(NivelIngles.B2, "Presentaciones profesionales", "Organiza explicaciones, transiciones y conclusiones de una presentación.",
            Choice("___ to the next point, let's examine the figures.", "Moving on", "Looking up|Moving on|Getting over"),
            Reading("Lee: «Sales increased by 12%, while operating costs remained stable.» ¿Qué se mantuvo estable?", "Operating costs", "Sales|Operating costs|Demand"),
            Listening("Listen: «To sum up, the project remains viable despite the initial delay.» What is the conclusion?", "The project is still viable", "The project was cancelled|The project is still viable|The project has no delays"),
            Writing("Traduce: Me gustaría destacar tres puntos importantes.", "I would like to highlight three important points"));

        yield return Lesson(NivelIngles.B2, "Tecnología y sociedad", "Evalúa ventajas, riesgos y argumentos sobre innovación.",
            Choice("Technology can enhance productivity ___ it is used effectively.", "as long as", "as long as|in spite of|even though"),
            Reading("Lee: «Automation can free workers from repetitive tasks, but training is essential for adapting to new roles.» ¿Qué considera necesario?", "Training", "Longer shifts|Training|Fewer skills"),
            Listening("Listen: «The tool is not intended to replace human judgement, but to support it.» What is its purpose?", "Supporting human judgement", "Replacing judgement|Supporting human judgement|Removing decisions"),
            Writing("Traduce: La innovación debe ir acompañada de responsabilidad.", "Innovation must be accompanied by responsibility"));

        // C1: precision, register and inference.
        yield return Lesson(NivelIngles.C1, "Inversión y énfasis", "Emplea inversión tras expresiones negativas o restrictivas.",
            Choice("Rarely ___ such a compelling argument.", "have I heard", "I have heard|have I heard|I heard have"),
            Reading("Lee: «Not until the final review did the team recognise the full extent of the issue.» ¿Cuándo se reconoció?", "During the final review", "Before the review|During the final review|Never"),
            Listening("Listen: «Under no circumstances should the original files be altered.» What does the expression convey?", "An absolute prohibition", "A minor preference|An absolute prohibition|An optional suggestion"),
            Writing("Reescribe con inversión: I had scarcely sat down when the phone rang.", "Scarcely had I sat down when the phone rang"));

        yield return Lesson(NivelIngles.C1, "Matices y precisión léxica", "Distingue sinónimos próximos y connotaciones en contexto.",
            Choice("His explanation was superficially convincing but ultimately ___.", "specious", "meticulous|specious|tangible"),
            Reading("Lee: «Her response was measured; she neither dismissed the criticism nor endorsed it outright.» ¿Qué caracteriza su respuesta?", "Restraint and balance", "Unqualified agreement|Restraint and balance|Hostility"),
            Listening("Listen: «The decision was pragmatic, rather than principled.» What motivated the decision?", "Practical considerations", "Abstract moral rules|Practical considerations|Personal hostility"),
            Writing("Sustituye 'very important' por una palabra formal: The distinction is very important.", "The distinction is crucial"));

        yield return Lesson(NivelIngles.C1, "Ensayos y argumentación académica", "Conecta tesis, evidencia y limitaciones con registro académico.",
            Choice("The claim is plausible, ___ insufficiently substantiated.", "albeit", "whereby|albeit|thereby"),
            Reading("Lee: «While the framework accounts for observed patterns, it fails to explain outliers.» ¿Qué objeción formula?", "It does not explain exceptions", "It contradicts every observation|It does not explain exceptions|It lacks a framework"),
            Listening("Listen: «The argument rests on the assumption that participants had equal access to resources.» What is the underlying assumption?", "Equal access to resources", "Identical opinions|Equal access to resources|Unlimited funding"),
            Writing("Parafrasea formalmente: The data show that the effect is significant.", "The data indicate that the effect is significant"));

        yield return Lesson(NivelIngles.C1, "Inferencias y subtexto", "Interpreta críticas indirectas y significados implícitos.",
            Choice("Calling the project 'ambitious' in this context may be a ___ way of expressing doubt.", "diplomatic", "literal|diplomatic|unrelated"),
            Reading("Lee: «The manager praised his enthusiasm before suggesting that a more realistic timetable might be useful.» ¿Qué sugiere indirectamente?", "The timetable is unrealistic", "The employee lacks enthusiasm|The timetable is unrealistic|No changes are needed"),
            Listening("Listen: «Well, that was an interesting interpretation.» In a sceptical tone, what may this imply?", "Disagreement or doubt", "Complete agreement|Disagreement or doubt|A factual certainty"),
            Writing("Reformula diplomáticamente: Your proposal is unrealistic.", "Your proposal may need further refinement"));

        yield return Lesson(NivelIngles.C1, "Registro profesional avanzado", "Redacta comunicaciones formales y atenúa afirmaciones delicadas.",
            Choice("We would appreciate it if you could ___ the matter at your earliest convenience.", "address", "confront with|address|address to"),
            Reading("Lee: «We are unable to endorse the proposal in its current form, pending clarification of several provisions.» ¿Qué podría cambiar la decisión?", "Clarification of provisions", "Immediate approval|Clarification of provisions|Withdrawal of all documents"),
            Listening("Listen: «I am writing to enquire whether an extension might be feasible.» What is being requested?", "More time", "Less work|More time|A different position"),
            Writing("Traduce formalmente: Agradeceríamos una respuesta a su mayor brevedad.", "We would appreciate a response at your earliest convenience"));

        yield return Lesson(NivelIngles.C1, "Sintaxis compleja y cohesión", "Usa cláusulas reducidas y conectores para enlazar información densa.",
            Choice("___ the report, the committee proceeded to vote.", "Having reviewed", "Having reviewed|To reviewing|Reviewed by"),
            Reading("Lee: «The proposal, being both costly and time-consuming, was deferred until the next quarter.» ¿Por qué fue aplazada?", "Cost and time requirements", "A legal requirement|Cost and time requirements|Lack of a proposal"),
            Listening("Listen: «The initial findings were inconclusive, thereby necessitating a second round of experiments.» What followed?", "Further experiments", "A final conclusion|Further experiments|A rejected hypothesis"),
            Writing("Reduce la cláusula: After she had completed the analysis, she published the results.", "Having completed the analysis she published the results"));

        yield return Lesson(NivelIngles.C1, "Retórica y evaluación crítica", "Reconoce estrategias persuasivas y diferencia evidencia de opinión.",
            Choice("The speaker's argument appeals primarily to emotion ___ empirical evidence.", "rather than", "as though|rather than|as for"),
            Reading("Lee: «The editorial cites a handful of cases as though they were representative of the entire population.» ¿Cuál es la debilidad del argumento?", "Overgeneralisation", "Excessive evidence|Overgeneralisation|A balanced sample"),
            Listening("Listen: «The conclusion follows logically only if the premises are accepted.» What condition applies?", "The premises must be accepted", "The conclusion must be popular|The premises must be accepted|No condition applies"),
            Writing("Traduce: La evidencia anecdótica no debe confundirse con evidencia sistemática.", "Anecdotal evidence should not be confused with systematic evidence"));

        yield return Lesson(NivelIngles.C1, "Expresiones avanzadas y reformulación", "Reformula ideas complejas y comprende lenguaje idiomático preciso.",
            Choice("The unexpected development threw a ___ in the works.", "spanner", "hammer|spanner|bolt"),
            Reading("Lee: «Rather than missing the forest for the trees, we should consider the broader implications.» ¿Qué recomienda el autor?", "Consider the bigger picture", "Focus exclusively on details|Consider the bigger picture|Avoid all analysis"),
            Listening("Listen: «The organisation has come under increasing scrutiny in recent months.» What has increased?", "Critical examination", "Financial support|Critical examination|Employee numbers"),
            Writing("Traduce: La decisión podría tener consecuencias de gran alcance.", "The decision could have far-reaching consequences"));
    }
}
