import { Injectable, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, tap } from 'rxjs';
import { Leccion, RespuestaUsuario, ResultadoEvaluacion, PreguntaDiagnostica, SubmitAssessment, ResultadoAssessment, ReporteProgreso } from '../../models/leccion.model';

@Injectable({ providedIn: 'root' })
export class ApiService {
  private base = 'https://localhost:5001/api';
  // Signals: estado reactivo sin Rx boilerplate
  nivelActual = signal<string | null>(localStorage.getItem('nivelActual'));
  loading = signal(false);

  constructor(private http: HttpClient) {}

  getPreguntas(): Observable<PreguntaDiagnostica[]> {
    return this.http.get<PreguntaDiagnostica[]>(`${this.base}/assessment/preguntas`);
  }

  submitAssessment(dto: SubmitAssessment): Observable<ResultadoAssessment> {
    this.loading.set(true);
    return this.http.post<ResultadoAssessment>(`${this.base}/assessment/submit`, dto).pipe(
      tap(res => {
        this.nivelActual.set(res.nivelAsignado);
        localStorage.setItem('nivelActual', res.nivelAsignado);
        this.loading.set(false);
      })
    );
  }

  getProgreso(usuarioId: number): Observable<ReporteProgreso> {
    return this.http.get<ReporteProgreso>(`${this.base}/progreso/${usuarioId}`);
  }

  getLeccionesPorNivel(nivel: string, usuarioId?: number): Observable<Leccion[]> {
    const q = usuarioId ? `?usuarioId=${usuarioId}` : '';
    return this.http.get<Leccion[]>(`${this.base}/lecciones/nivel/${nivel}${q}`);
  }

  evaluar(res: RespuestaUsuario): Observable<ResultadoEvaluacion> {
    return this.http.post<ResultadoEvaluacion>(`${this.base}/lecciones/evaluar`, res);
  }
}
