export enum NivelIngles {
  A1 = 'A1',
  A2 = 'A2',
  B1 = 'B1',
  B2 = 'B2',
  C1 = 'C1',
  C2 = 'C2'
}

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
