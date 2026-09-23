import { Routes } from '@angular/router';
import { placementGuard } from './core/guards/placement.guard';

export const routes: Routes = [
  { path: 'assessment', loadComponent: () => import('./features/assessment/assessment.component').then(m=>m.AssessmentComponent) },
  { path: 'reading', loadComponent: () => import('./features/reading/reading.component').then(m=>m.ReadingComponent), canActivate: [placementGuard] },
  { path: 'listening', loadComponent: () => import('./features/listening/listening.component').then(m=>m.ListeningComponent), canActivate: [placementGuard] },
  { path: 'writing', loadComponent: () => import('./features/writing/writing.component').then(m=>m.WritingComponent), canActivate: [placementGuard] },
  { path: 'speaking', loadComponent: () => import('./features/speaking/speaking.component').then(m=>m.SpeakingComponent), canActivate: [placementGuard] },
  { path: 'progreso', loadComponent: () => import('./shared/components/progreso.component').then(m=>m.ProgresoComponent) },
  { path: '', redirectTo: 'assessment', pathMatch: 'full' },
];
