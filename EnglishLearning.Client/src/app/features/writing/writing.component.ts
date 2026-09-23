import { Component, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-writing',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
  <section class="max-w-3xl mx-auto p-6">
    <h1 class="text-xl font-bold">Writing — Redacción con validación léxica</h1>
    <textarea [(ngModel)]="texto" class="w-full border rounded p-3 mt-4" rows="5" placeholder="Escribe aquí... evita 'I am agree'"></textarea>
    <p *ngIf="aviso()" class="text-amber-600 text-sm mt-2" role="alert">{{aviso()}}</p>
    <button (click)="validar()" class="mt-3 bg-black text-white px-4 py-2 rounded">Validar</button>
  </section>
  `
})
export class WritingComponent {
  texto = '';
  aviso = signal<string | null>(null);
  validar(): void {
    if (/I am agree/i.test(this.texto)) this.aviso.set("Transferencia detectada: 'I am agree' → 'I agree'");
    else if (/I have \d+ years/i.test(this.texto)) this.aviso.set("'I have 30 years' → 'I am 30 years old'");
    else this.aviso.set("¡Bien! Sigue practicando conectores B1/B2.");
  }
}
