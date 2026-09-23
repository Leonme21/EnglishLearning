import { Component, signal, computed, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { ApiService } from '../../core/services/api.service';
import { PreguntaDiagnostica } from '../../models/leccion.model';

@Component({
  selector: 'app-assessment',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
  <main class="max-w-3xl mx-auto p-6" aria-labelledby="title">
    <h1 id="title" class="text-2xl font-bold">Placement Test — Descubre tu nivel</h1>
    <p class="text-sm text-muted">16 preguntas (4 por habilidad) — falsos amigos y transferencias ES→EN. Requerido antes de lecciones.</p>

    <div *ngIf="loading()" role="status" class="py-8">Cargando preguntas...</div>

    <form *ngIf="!loading() && !resultado()" (ngSubmit)="enviar()" class="space-y-6 mt-6">
      <fieldset *ngFor="let p of preguntas(); let i=index" class="border rounded p-4">
        <legend class="font-medium">{{i+1}}. [{{p.habilidad}}] {{p.contenido}}</legend>
        <label *ngFor="let op of p.opciones" class="flex gap-2 py-1 cursor-pointer">
          <input type="radio" [name]="'q'+p.id" [value]="op" (change)="responder(p.id, op)" [checked]="respuestas()[p.id]===op" />
          <span>{{op}}</span>
        </label>
        <input *ngIf="p.opciones.length===0" type="text" class="border rounded px-2 py-1 w-full" placeholder="Escribe tu respuesta" (input)="responder(p.id, $any($event.target).value)" />
      </fieldset>
      <button type="submit" [disabled]="!puedeEnviar()" class="bg-black text-white px-6 py-2 rounded disabled:opacity-50">Enviar y ver nivel</button>
      <p *ngIf="error()" class="text-red-600" role="alert">{{error()}}</p>
    </form>

    <section *ngIf="resultado()" class="mt-8 border rounded p-6 bg-green-50" aria-live="polite">
      <h2 class="text-xl font-semibold">Nivel asignado: {{resultado()!.nivelAsignado}} ({{resultado()!.puntajeTotal}}%)</h2>
      <p>{{resultado()!.mensaje}}</p>
      <p class="mt-2 font-medium">{{resultado()!.siguienteRecomendacion}}</p>
      <div class="mt-3 text-sm">Por habilidad: {{ puntajesTexto() }}</div>
      <button (click)="irALecciones()" class="mt-4 bg-black text-white px-4 py-2 rounded">Ir a lecciones</button>
    </section>
  </main>
  `
})
export class AssessmentComponent implements OnInit {
  preguntas = signal<PreguntaDiagnostica[]>([]);
  respuestas = signal<Record<number,string>>({});
  loading = signal(true);
  error = signal<string | null>(null);
  resultado = signal<any | null>(null);

  puedeEnviar = computed(() => Object.keys(this.respuestas()).length >= 8);
  puntajesTexto = computed(() => {
    const r = this.resultado();
    return r ? Object.entries(r.puntajesPorHabilidad).map(([k,v])=>`${k}:${v}%`).join(' | ') : '';
  });

  constructor(private api: ApiService, private router: Router) {}

  ngOnInit(): void {
    this.api.getPreguntas().subscribe({
      next: v => { this.preguntas.set(v); this.loading.set(false); },
      error: () => { this.error.set('No se pudo cargar assessment'); this.loading.set(false); }
    });
  }

  responder(id: number, val: string): void {
    this.respuestas.update(m => ({...m, [id]: val}));
  }

  enviar(): void {
    this.error.set(null);
    const dto = {
      usuarioId: Number(localStorage.getItem('usuarioId') ?? '1'),
      respuestas: Object.entries(this.respuestas()).map(([k,v])=>({ preguntaId: Number(k), respuestaTexto: v }))
    };
    if (dto.respuestas.length < 8) { this.error.set('Responde al menos 8 preguntas'); return; }
    this.loading.set(true);
    this.api.submitAssessment(dto as any).subscribe({
      next: r => { this.resultado.set(r); this.loading.set(false); },
      error: (e) => { this.error.set(e.error?.error?.message ?? 'Error al enviar'); this.loading.set(false); }
    });
  }

  irALecciones(): void {
    this.router.navigateByUrl('/reading');
  }
}
