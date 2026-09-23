import { Component, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ApiService } from '../../core/services/api.service';
import { Leccion } from '../../models/leccion.model';

@Component({
  selector: 'app-reading',
  standalone: true,
  imports: [CommonModule],
  template: `
  <section class="max-w-4xl mx-auto p-6">
    <h1 class="text-xl font-bold">Reading — Comprensión con vocabulario contextual</h1>
    <p class="text-sm text-muted">Textos adaptados a {{nivel()}} con glosario ES→EN.</p>
    <ul class="mt-4 space-y-3">
      <li *ngFor="let l of lecciones()" class="border rounded p-4">
        <h2 class="font-semibold">{{l.titulo}} ({{l.nivel}})</h2>
        <p class="text-sm">{{l.descripcion}}</p>
      </li>
      <li *ngIf="lecciones().length===0" class="text-center py-8 text-muted">Sin lecciones para este nivel.</li>
    </ul>
  </section>
  `
})
export class ReadingComponent implements OnInit {
  lecciones = signal<Leccion[]>([]);
  nivel = signal(localStorage.getItem('nivelActual') ?? 'A1');
  constructor(private api: ApiService) {}
  ngOnInit(): void {
    this.api.getLeccionesPorNivel(this.nivel(), Number(localStorage.getItem('usuarioId') ?? '1')).subscribe(v=>this.lecciones.set(v));
  }
}
