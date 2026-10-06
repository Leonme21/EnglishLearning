using System.ComponentModel.DataAnnotations;

namespace EnglishLearning.Client.Models;

public record DiagnosticQuestion(int Id, string Habilidad, string Contenido, IReadOnlyList<string> Opciones, string Nivel);

public record SubmitAssessmentRequest(int UsuarioId, List<SubmitAnswer> Respuestas);

public record SubmitAnswer(int PreguntaId, string RespuestaTexto);

public record SubmitAssessmentResponse(
    string NivelAsignado,
    int PuntajeTotal,
    Dictionary<string, double> PuntajesPorHabilidad,
    string Mensaje,
    string SiguienteRecomendacion
);
