import { Injectable, inject, signal, computed } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Router } from '@angular/router';
import { catchError, finalize, map, shareReplay, throwError } from 'rxjs';
import type { Observable } from 'rxjs';
import { AuthUser, TokenPayload, TokenResponse } from '../models/auth.model';
import { environment } from '../../environments/environment';

/**
 * AuthService — source de vérité unique pour l'état d'authentification.
 *
 * RESPONSABILITÉS :
 *   1. Login   : POST /connect/token → stocker les tokens en localStorage
 *   2. Logout  : vider le stockage, rediriger vers /login
 *   3. Refresh : POST /connect/token (grant refresh_token) → nouveaux tokens
 *   4. Exposer : l'utilisateur courant via un Signal Angular (réactif)
 *
 * POURQUOI localStorage ?
 *   Survit au rechargement de page — l'utilisateur reste connecté.
 *   En production, préférer des cookies httpOnly pour limiter l'exposition XSS
 *   (trade-off : nécessite un BFF ou proxy de token côté serveur).
 *
 * POURQUOI Signals au lieu de BehaviorSubject ?
 *   Angular 17+ : les Signals sont le primitif réactif idiomatique.
 *   Pas de fuite de souscription dans les composants.
 *   computed() dérive automatiquement isAuthenticated, isAdmin, etc.
 */

const ACCESS_TOKEN_KEY  = 'comin_access_token';
const REFRESH_TOKEN_KEY = 'comin_refresh_token';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly http   = inject(HttpClient);
  private readonly router = inject(Router);

  // La requête de refresh en cours — évite les appels parallèles multiples
  private refreshInFlight$: Observable<string> | null = null;

  // ── Endpoint token (RubacCore) ─────────────────────────────────────────────
  // L'URL est injectée depuis environment.ts pour être facilement configurable.
  private readonly tokenUrl = `${environment.authServerUrl}/connect/token`;

  // ── État réactif (Signals) ────────────────────────────────────────────────

  /** Utilisateur authentifié courant, null si déconnecté. */
  readonly currentUser = signal<AuthUser | null>(this.loadUserFromStorage());

  /** True si un access token valide est présent. */
  readonly isAuthenticated = computed(() => this.currentUser() !== null);

  /** Vérifications de rôle — utilisées par les guards et les templates. */
  readonly isAdmin      = computed(() => this.currentUser()?.isAdmin      ?? false);
  readonly isManager    = computed(() => this.currentUser()?.isManager    ?? false);
  readonly isConsultant = computed(() => this.currentUser()?.isConsultant ?? false);

  /**
   * Vérifie si l'utilisateur connecté possède une tâche (rôle granulaire) donnée.
   * Les admins ont accès à toutes les tâches par convention.
   */
  hasTache(tache: string): boolean {
    const user = this.currentUser();
    if (!user) return false;
    if (user.isAdmin) return true;
    return user.roles.includes(tache);
  }

  // ── Authentification ──────────────────────────────────────────────────────

  /**
   * Authentifie l'utilisateur (resource owner password grant — ROPC).
   * En production OIDC, préférer le code flow avec PKCE.
   */
  login(username: string, password: string): Observable<void> {
    const body = new HttpParams()
      .set('grant_type', 'password')
      .set('username', username)
      .set('password', password)
      .set('client_id', 'cnss-metier-front')
      .set('scope', 'openid profile email roles cnss-metier offline_access');

    return this.http.post<TokenResponse>(this.tokenUrl, body).pipe(
      map(resp => {
        this.storeTokens(resp);
        this.currentUser.set(this.parseJwt(resp.access_token));
      }),
      catchError(err =>
        throwError(() =>
          new Error(err?.error?.error_description ?? 'Identifiants incorrects'),
        ),
      ),
    );
  }

  /** Déconnecte l'utilisateur, vide le stockage et redirige vers /login. */
  logout(): void {
    localStorage.removeItem(ACCESS_TOKEN_KEY);
    localStorage.removeItem(REFRESH_TOKEN_KEY);
    this.currentUser.set(null);
    this.router.navigate(['/login']);
  }

  /** Retourne l'access token brut (utilisé par l'intercepteur HTTP). */
  getAccessToken(): string | null {
    return localStorage.getItem(ACCESS_TOKEN_KEY);
  }

  /**
   * Rafraîchit le token silencieusement.
   * Partage la requête en cours si plusieurs intercepteurs appellent en parallèle
   * (évite plusieurs POST /connect/token simultanés).
   */
  refreshToken(): Observable<string> {
    if (this.refreshInFlight$) return this.refreshInFlight$;

    const refreshToken = localStorage.getItem(REFRESH_TOKEN_KEY);
    if (!refreshToken) {
      this.logout();
      return throwError(() => new Error('Aucun refresh token disponible'));
    }

    const body = new HttpParams()
      .set('grant_type', 'refresh_token')
      .set('refresh_token', refreshToken)
      .set('client_id', 'cnss-metier-front');

    this.refreshInFlight$ = this.http.post<TokenResponse>(this.tokenUrl, body).pipe(
      map(resp => {
        this.storeTokens(resp);
        this.currentUser.set(this.parseJwt(resp.access_token));
        return resp.access_token;
      }),
      finalize(() => (this.refreshInFlight$ = null)),
      shareReplay(1),
    );

    return this.refreshInFlight$;
  }

  // ── Helpers privés ────────────────────────────────────────────────────────

  private storeTokens(resp: TokenResponse): void {
    localStorage.setItem(ACCESS_TOKEN_KEY, resp.access_token);
    if (resp.refresh_token) {
      localStorage.setItem(REFRESH_TOKEN_KEY, resp.refresh_token);
    }
  }

  /** Charge l'utilisateur depuis le localStorage au démarrage de l'application. */
  private loadUserFromStorage(): AuthUser | null {
    const token = localStorage.getItem(ACCESS_TOKEN_KEY);
    if (!token) return null;
    try {
      const payload = this.decodeJwt(token);
      // Vérifier que le token n'est pas expiré (exp est en secondes)
      if (payload.exp * 1000 < Date.now()) {
        localStorage.removeItem(ACCESS_TOKEN_KEY);
        return null;
      }
      return this.buildAuthUser(payload);
    } catch {
      return null;
    }
  }

  private parseJwt(token: string): AuthUser {
    return this.buildAuthUser(this.decodeJwt(token));
  }

  /**
   * Décode le payload Base64URL du JWT sans bibliothèque externe.
   * Le JWT est structuré : header.payload.signature (séparés par des points).
   */
  private decodeJwt(token: string): TokenPayload {
    const base64 = token.split('.')[1].replace(/-/g, '+').replace(/_/g, '/');
    return JSON.parse(atob(base64)) as TokenPayload;
  }

  private buildAuthUser(payload: TokenPayload): AuthUser {
    // Le claim `role` peut être une string ou un tableau selon la config RubacCore
    const toArray = (v: unknown): string[] =>
      Array.isArray(v) ? v : (v ? [v as string] : []);

    // Fusionner role + permission + permissions pour une vérification uniforme
    const roles = [
      ...toArray(payload.role),
      ...toArray(payload.permission),
      ...toArray(payload.permissions),
    ];

    return {
      sub:          payload.sub,
      name:         payload.name,
      email:        payload.email,
      roles,
      isAdmin:      roles.includes('Admin'),
      isManager:    roles.includes('Manager'),
      isConsultant: roles.includes('Consultant'),
    };
  }
}
