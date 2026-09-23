import { CanActivateFn, Router } from '@angular/router';
import { inject } from '@angular/core';

/**
 * Guarda obligatorio: bloquea /reading,/listening,/writing,/speaking si no hay Placement Test.
 * Usa localStorage nivelActual seteado por ApiService tras submit.
 * Si SinAsignar/null → redirect a /assessment.
 */
export const placementGuard: CanActivateFn = () => {
  const router = inject(Router);
  const nivel = localStorage.getItem('nivelActual');
  if (!nivel || nivel === 'SinAsignar') {
    router.navigateByUrl('/assessment');
    return false;
  }
  return true;
};
