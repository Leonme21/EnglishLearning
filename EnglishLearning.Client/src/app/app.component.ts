import { Component } from '@angular/core';
import { RouterOutlet, RouterLink } from '@angular/router';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [CommonModule, RouterOutlet, RouterLink],
  template: `
  <nav class="flex gap-4 p-4 border-b" aria-label="Principal">
    <a routerLink="/assessment" class="underline">Assessment</a>
    <a routerLink="/reading">Reading</a>
    <a routerLink="/listening">Listening</a>
    <a routerLink="/writing">Writing</a>
    <a routerLink="/speaking">Speaking</a>
    <a routerLink="/progreso" class="ml-auto">Mi Progreso</a>
  </nav>
  <router-outlet />
  `
})
export class AppComponent {}
