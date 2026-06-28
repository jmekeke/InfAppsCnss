import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from '../services/auth.service';

/**
 * Guard d'authentification — protège les routes privées.
 *
 * QUAND utiliser ce guard ?
 *   Toutes les routes qui nécessitent d'être connecté (quel que soit le rôle).
 *   Utiliser SEUL pour les routes accessibles à tout utilisateur authentifié.
 *   Combiner avec roleGuard pour les routes avec restriction de rôle.
 *
 * COMPORTEMENT :
 *   - Token présent et valide  → laisse passer (retourne true)
 *   - Token absent ou expiré   → redirige vers /login
 *
 * POURQUOI une fonction et non une classe ?
 *   Angular 15+ recommande les guards fonctionnels (inject() works here).
 *   Moins de boilerplate, pas besoin d'implémenter une interface.
 */
export const authGuard: CanActivateFn = () => {
  const auth   = inject(AuthService);
  const router = inject(Router);

  if (auth.isAuthenticated()) return true;

  // Redirige vers la page de connexion si non authentifié
  return router.createUrlTree(['/login']);
};
