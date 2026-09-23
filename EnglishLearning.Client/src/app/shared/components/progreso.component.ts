import { Component, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ApiService } from '../../core/services/api.service';
import { ReporteProgreso } from '../../models/leccion.model';

@Component({
  selector: 'app-progreso',
  standalone: true,
  imports: [CommonModule],
  template: `
  <section class="max-w-3xl mx-auto p-6">
    <h1 class="text-xl font-bold">Mi Progreso — Métricas por habilidad</h1>
    <div *ngIf="reporte()" class="mt-4 space-y-4">
      <p>Nivel actual: <strong>{{reporte()!.nivelActual}}</strong></p>
      <div class="grid grid-cols-2 gap-3">
        <div *ngFor="let kv of metricas()" class="border rounded p-3">
          <h3 class="font-medium">{{kv[0]}}</h3>
          <p>{{kv[1].aciertos}}/{{kv[1].total}} ({{kv[1].porcentaje}}%)</p>
        </div>
      </div>
      <ul class="list-disc pl-6">
        <li *ngFor="let r of reporte()!.recomendaciones">{{r}}</li>
      </ul>
    </div>
    <p *ngIf="!reporte()" class="py-8">Cargando...</p>
  </section>
  `
})
export class ProgresoComponent implements OnInit {
  reporte = signal<ReporteProgreso | null>(null);
  metricas = () => Object.entries(this.reporte()?.metricasPorHabilidad ?? {});
  constructor(private api: ApiService) {}
  ngOnInit(): void {
    this.api.getProgreso(Number(localStorage.getItem('usuarioId') ?? '1')).subscribe(v=>this.reporte.set(v));
  }
}
