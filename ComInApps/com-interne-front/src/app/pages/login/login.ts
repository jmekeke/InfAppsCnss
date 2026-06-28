import { Component, inject, signal } from '@angular/core';
import { Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { AuthService } from '../../services/auth.service';

/**
 * Page de connexion — accessible sans authentification.
 *
 * Flux :
 *   1. L'utilisateur saisit identifiant + mot de passe
 *   2. AuthService.login() appelle POST /connect/token (RubacCore)
 *   3. En cas de succès → redirection vers /dashboard
 *   4. En cas d'erreur  → affichage du message d'erreur
 *
 * POURQUOI FormsModule (template-driven) et non ReactiveFormsModule ?
 *   Formulaire simple à 2 champs : template-driven est plus concis ici.
 *   Pour les formulaires complexes (MessageForm), on utilisera ReactiveFormsModule.
 */
@Component({
  selector: 'app-login',
  imports: [FormsModule],
  templateUrl: './login.html',
  styleUrl: './login.css',
})
export class LoginComponent {
  private readonly auth   = inject(AuthService);
  private readonly router = inject(Router);

  /** Champs du formulaire liés par ngModel. */
  username = '';
  password = '';

  /** États de la page. */
  readonly loading = signal(false);
  readonly error   = signal<string | null>(null);

  /** Soumet le formulaire de connexion. */
  onSubmit(): void {
    if (!this.username || !this.password) return;

    this.loading.set(true);
    this.error.set(null);

    this.auth.login(this.username, this.password).subscribe({
      next: () => {
        // Connexion réussie → redirection vers le tableau de bord
        this.router.navigate(['/dashboard']);
      },
      error: (err: Error) => {
        this.error.set(err.message);
        this.loading.set(false);
      },
    });
  }
}
