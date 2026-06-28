import { Component, computed, inject } from '@angular/core';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { AuthService } from './services/auth.service';

/**
 * Composant racine — App Shell.
 *
 * Responsabilités :
 *   - Afficher le header de navigation uniquement quand l'utilisateur est connecté
 *   - Fournir le <router-outlet> pour les pages enfants
 *
 * Le header est conditionnel (signal isAuthenticated) pour ne pas apparaître
 * sur la page /login.
 *
 * POURQUOI inject() ici ?
 *   Angular 14+ : inject() est disponible dans les constructeurs et les champs.
 *   Plus lisible que l'injection par constructeur pour les composants simples.
 */
@Component({
  selector: 'app-root',
  imports: [RouterOutlet, RouterLink, RouterLinkActive],
  templateUrl: './app.html',
  styleUrl: './app.css',
})
export class App {
  private readonly auth = inject(AuthService);

  /** True quand l'utilisateur est authentifié — pilote l'affichage du header. */
  readonly showHeader = computed(() => this.auth.isAuthenticated());

  /** Nom de l'utilisateur connecté pour l'affichage dans le header. */
  readonly userName = computed(() => this.auth.currentUser()?.name ?? '');

  /** Déconnexion depuis le header. */
  logout(): void {
    this.auth.logout();
  }
}
