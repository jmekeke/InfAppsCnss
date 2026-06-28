import { inject } from '@angular/core';
import { ActivatedRouteSnapshot, CanActivateFn, Router } from '@angular/router';
import { AuthService } from '../services/auth.service';

/**
 * Guard d'autorisation — vérifie la présence d'un rôle spécifique.
 *
 * USAGE dans app.routes.ts :
 *   canActivate: [authGuard, roleGuard],
 *   data: { roles: ['Admin', 'Manager'] }
 *
 * POURQUOI séparer authGuard et roleGuard ?
 *   authGuard  = "êtes-vous connecté ?"
 *   roleGuard  = "avez-vous l'autorisation ?"
 *   Séparer les deux permet des comportements de redirection différents :
 *   - Pas authentifié            → /login
 *   - Authentifié, mauvais rôle → /forbidden
 */
export const roleGuard: CanActivateFn = (route: ActivatedRouteSnapshot) => {
  const auth   = inject(AuthService);
  const router = inject(Router);

  // Les rôles requis sont déclarés dans data: { roles: [...] }
  const requiredRoles: string[] = route.data['roles'] ?? [];

  // Aucun rôle requis → authGuard seul suffit, on laisse passer
  if (requiredRoles.length === 0) return true;

  const user = auth.currentUser();
  if (!user) return router.createUrlTree(['/login']);

  // Vérifie qu'au moins un des rôles requis est présent
  const hasRole = requiredRoles.some(r => user.roles.includes(r));
  if (hasRole) return true;

  // Authentifié mais rôle insuffisant → page Forbidden
  return router.createUrlTree(['/forbidden']);
};
