import { Component, Input, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { LeccionService } from '../../services/leccion.service';
import { Ejercicio, ResultadoEvaluacion } from '../../models/leccion.model';

@Component({
  selector: 'app-ejercicio',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="card p-4 shadow-sm" *ngIf="ejercicio">
      <h3 class="h5 mb-3">{{ ejercicio.tipo }}</h3>
      <p class="lead">{{ ejercicio.contenido }}</p>

      <div class="form-group mt-4">
        <label for="respuesta">Tu respuesta:</label>
        <input 
          type="text" 
          id="respuesta" 
          class="form-control" 
          [(ngModel)]="respuestaTexto" 
          placeholder="Escribe aquí..."
          [disabled]="procesando"
        >
      </div>

      <button 
        class="btn btn-primary mt-3" 
        (click)="enviarRespuesta()" 
        [disabled]="!respuestaTexto || procesando"
      >
        {{ procesando ? 'Verificando...' : 'Enviar Respuesta' }}
      </button>

      <div class="mt-4" *ngIf="resultado">
        <div class="alert" [ngClass]="resultado.esCorrecta ? 'alert-success' : 'alert-danger'">
          {{ resultado.mensaje }}
        </div>
      </div>
    </div>
  `,
  styles: [`
    .card { border-radius: 12px; }
    .alert { font-weight: 500; }
  `]
})
export class EjercicioComponent {
  @Input() ejercicio!: Ejercicio;
  @Input() usuarioId: number = 1; // Simulado para el ejemplo

  respuestaTexto: string = '';
  procesando: boolean = false;
  resultado?: ResultadoEvaluacion;

  constructor(private leccionService: LeccionService) {}

  enviarRespuesta() {
    this.procesando = true;
    this.resultado = undefined;

    this.leccionService.evaluarEjercicio({
      usuarioId: this.usuarioId,
      ejercicioId: this.ejercicio.id,
      respuestaTexto: this.respuestaTexto
    }).subscribe({
      next: (res) => {
        this.resultado = res;
        this.procesando = false;
      },
      error: (err) => {
        console.error('Error al evaluar:', err);
        this.procesando = false;
      }
    });
  }
}
