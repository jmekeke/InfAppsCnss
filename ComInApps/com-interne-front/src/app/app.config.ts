import { ApplicationConfig, provideBrowserGlobalErrorListeners, provideZoneChangeDetection } from '@angular/core';
import { provideRouter } from '@angular/router';
import { provideHttpClient, withFetch, withInterceptors } from '@angular/common/http';
import { routes } from './app.routes';
import { authInterceptor } from './interceptors/auth.interceptor';

/**
 * Configuration globale de l'application.
 *
 * provideHttpClient(withInterceptors([authInterceptor]))
 *   Chaque appel HttpClient passe par authInterceptor qui :
 *     1. Attache `Authorization: Bearer <token>` aux requêtes protégées
 *     2. Tente un refresh silencieux en cas de réponse 401
 *
 * provideAnimationsAsync()
 *   Active les animations Angular pour les transitions de pages.
 *
 * withFetch()
 *   Utilise l'API Fetch native au lieu de XMLHttpRequest (meilleur support SSR).
 */
export const appConfig: ApplicationConfig = {
  providers: [
    provideBrowserGlobalErrorListeners(),
    provideZoneChangeDetection({ eventCoalescing: true }),
    provideRouter(routes),
    provideHttpClient(
      withFetch(),
      withInterceptors([authInterceptor]), // ← attache Bearer token sur chaque requête
    ),
  ],
};
