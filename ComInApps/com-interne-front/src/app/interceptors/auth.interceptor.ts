import { HttpInterceptorFn, HttpRequest } from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError, switchMap, throwError } from 'rxjs';
import { AuthService } from '../services/auth.service';
import { environment } from '../../environments/environment';

/**
 * Intercepteur HTTP fonctionnel (pattern Angular 15+).
 *
 * RESPONSABILITÉS :
 *   1. Attache l'en-tête Authorization: Bearer <access_token> à chaque requête
 *      vers ComInterne.Api (/api/*).
 *   2. Si le serveur répond 401 (token expiré), tente un refresh silencieux
 *      puis relance la requête originale avec le nouveau token.
 *   3. En cas d'échec du refresh → déconnecte l'utilisateur.
 *
 * EXCLUSION de RubacCore :
 *   Les appels vers le serveur d'identité (/connect/token) sont exclus
 *   pour éviter les boucles infinies lors du refresh.
 *
 * POURQUOI utiliser withInterceptors() dans appConfig ?
 *   Pattern Angular 15+ ; remplace la classe HTTP_INTERCEPTORS.
 *   Les intercepteurs fonctionnels sont plus simples, testables, et composables.
 */
export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const auth = inject(AuthService);

  // Exclure les appels vers RubacCore pour éviter les boucles de refresh
  if (req.url.startsWith(environment.authServerUrl)) {
    return next(req);
  }

  // Ajoute le token Bearer si l'utilisateur est authentifié
  const token = auth.getAccessToken();
  const authReq = token ? attachBearerToken(req, token) : req;

  return next(authReq).pipe(
    catchError(err => {
      // Token expiré (401) → tenter un refresh silencieux
      if (err.status === 401 && token) {
        return auth.refreshToken().pipe(
          switchMap(newToken => next(attachBearerToken(req, newToken))),
          catchError(refreshErr => {
            // Refresh également échoué → déconnexion forcée
            auth.logout();
            return throwError(() => refreshErr);
          }),
        );
      }
      return throwError(() => err);
    }),
  );
};

/** Retourne une copie de la requête avec l'en-tête Authorization. */
function attachBearerToken(
  req: HttpRequest<unknown>,
  token: string,
): HttpRequest<unknown> {
  return req.clone({
    setHeaders: { Authorization: `Bearer ${token}` },
  });
}
