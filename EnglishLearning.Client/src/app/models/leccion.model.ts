export enum NivelIngles {
  SinAsignar = 'SinAsignar',
  A1 = 'A1',
  A2 = 'A2',
  B1 = 'B1',
  B2 = 'B2',
  C1 = 'C1'
}

export type Habilidad = 'Reading' | 'Listening' | 'Writing' | 'Speaking';

export interface Leccion {
  id: number;
  titulo: string;
  descripcion: string;
  nivel: string;
  ejercicios: Ejercicio[];
}

export interface Ejercicio {
  id: number;
  contenido: string;
  tipo: string;
}

export interface RespuestaUsuario {
  usuarioId: number;
  ejercicioId: number;
  respuestaTexto: string;
}

export interface ResultadoEvaluacion {
  esCorrecta: boolean;
  mensaje: string;
  nuevoNivel?: string;
}

// Placement Test DTOs — espejo de AssessmentDtos.cs
export interface PreguntaDiagnostica {
  id: number;
  habilidad: Habilidad;
  contenido: string;
  opciones: string[];
  nivel: string;
}

export interface SubmitAssessment {
  usuarioId: number;
  respuestas: { preguntaId: number; respuestaTexto: string }[];
}

export interface ResultadoAssessment {
  nivelAsignado: string;
  puntajeTotal: number;
  puntajesPorHabilidad: Record<Habilidad, number>;
  mensaje: string;
  siguienteRecomendacion: string;
}

export interface ReporteProgreso {
  usuarioId: number;
  nivelActual: string;
  progresos: { nivel: string; porcentajeCompletado: number }[];
  metricasPorHabilidad: Record<Habilidad, { aciertos: number; total: number; porcentaje: number }>;
  recomendaciones: string[];
}
