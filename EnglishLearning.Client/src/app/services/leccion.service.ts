import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Leccion, RespuestaUsuario, ResultadoEvaluacion } from '../models/leccion.model';

@Injectable({
  providedIn: 'root'
})
export class LeccionService {
  private apiUrl = 'https://localhost:7001/api/lecciones'; // URL base de la API

  constructor(private http: HttpClient) { }

  getLeccionesPorNivel(nivel: string): Observable<Leccion[]> {
    return this.http.get<Leccion[]>(`${this.apiUrl}/nivel/${nivel}`);
  }

  evaluarEjercicio(respuesta: RespuestaUsuario): Observable<ResultadoEvaluacion> {
    return this.http.post<ResultadoEvaluacion>(`${this.apiUrl}/evaluar`, respuesta);
  }
}
