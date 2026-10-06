namespace EnglishLearning.Client.Models;

public record LessonDto(int Id, string Titulo, string Descripcion, string Nivel, List<ExerciseDto> Ejercicios);
public record ExerciseDto(int Id, string Contenido, string Tipo);

public record EvaluateExerciseRequest(int UsuarioId, int EjercicioId, string RespuestaTexto);
public record EvaluateExerciseResponse(bool EsCorrecta, string Mensaje, string? NuevoNivel);
