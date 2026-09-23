import { Component, signal } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-listening',
  standalone: true,
  imports: [CommonModule],
  template: `
  <section class="max-w-4xl mx-auto p-6">
    <h1 class="text-xl font-bold">Listening — Audios con transcripción</h1>
    <div class="mt-4 border rounded p-4">
      <audio controls class="w-full" aria-label="Audio ejemplo /θ/ think"></audio>
      <p class="text-sm mt-2">Transcripción: "I think this is thorough" — practica /θ/ vs /ð/.</p>
    </div>
  </section>
  `
})
export class ListeningComponent {}
