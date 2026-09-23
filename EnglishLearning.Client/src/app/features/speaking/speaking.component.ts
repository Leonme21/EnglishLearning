import { Component, signal } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-speaking',
  standalone: true,
  imports: [CommonModule],
  template: `
  <section class="max-w-3xl mx-auto p-6">
    <h1 class="text-xl font-bold">Speaking — Pronunciación y fluidez</h1>
    <p class="text-sm">Practica /v/ very, /θ/ think, y minimal pairs ship/sheep.</p>
    <button class="mt-4 border rounded px-4 py-2" (click)="grabando.set(!grabando())">
      {{grabando() ? 'Detener' : 'Grabar'}} 🎙️
    </button>
    <p *ngIf="grabando()" class="text-sm mt-2" aria-live="polite">Escuchando... di "vegetable" (3 sílabas).</p>
  </section>
  `
})
export class SpeakingComponent { grabando = signal(false); }
