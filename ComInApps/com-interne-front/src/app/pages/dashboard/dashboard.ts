import { Component, inject, OnInit, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { MessageService } from '../../services/message.service';
import { GroupeDiffusionService } from '../../services/groupe-diffusion.service';
import { AuthService } from '../../services/auth.service';
import { MessageInterne, StatutMessage } from '../../models/message.model';

/**
 * Page Tableau de bord — vue synthétique de l'activité.
 *
 * Affiche :
 *   - Compteurs par statut (Brouillon, En attente, Validés, Diffusés)
 *   - Liste des 5 derniers messages
 *   - Raccourcis d'actions rapides
 *
 * Les données sont chargées en parallèle au démarrage du composant.
 */
@Component({
  selector: 'app-dashboard',
  imports: [RouterLink],
  templateUrl: './dashboard.html',
  styleUrl: './dashboard.css',
})
export class DashboardComponent implements OnInit {
  private readonly messageService = inject(MessageService);
  private readonly groupeService  = inject(GroupeDiffusionService);
  readonly auth = inject(AuthService);

  /** États de chargement. */
  readonly loading     = signal(true);
  readonly error       = signal<string | null>(null);

  /** Données du tableau de bord. */
  readonly messages         = signal<MessageInterne[]>([]);
  readonly totalMessages    = signal(0);
  readonly totalGroupes     = signal(0);
  readonly countBrouillon   = signal(0);
  readonly countAttente     = signal(0);
  readonly countValide      = signal(0);
  readonly countDiffuse     = signal(0);

  ngOnInit(): void {
    // Chargement des messages (page 1, 10 derniers)
    this.messageService.lister(1, 10).subscribe({
      next: result => {
        this.messages.set(result.items);
        this.totalMessages.set(result.totalCount);
        // Calcul des compteurs par statut
        this.countBrouillon.set(result.items.filter(m => m.statut === 'Brouillon').length);
        this.countAttente.set(result.items.filter(m => m.statut === 'EnAttenteValidation').length);
        this.countValide.set(result.items.filter(m => m.statut === 'Valide').length);
        this.countDiffuse.set(result.items.filter(m => m.statut === 'Diffuse').length);
        this.loading.set(false);
      },
      error: () => {
        this.error.set('Impossible de charger les données du tableau de bord.');
        this.loading.set(false);
      },
    });

    // Chargement du nombre de groupes (en parallèle)
    this.groupeService.lister(1, 1).subscribe({
      next: result => this.totalGroupes.set(result.totalCount),
    });
  }

  /** Retourne la classe CSS du badge selon le statut. */
  badgeClass(statut: StatutMessage): string {
    const map: Record<StatutMessage, string> = {
      Brouillon:           'badge badge-brouillon',
      EnAttenteValidation: 'badge badge-attente',
      Valide:              'badge badge-valide',
      Rejete:              'badge badge-rejete',
      Programme:           'badge badge-programme',
      Diffuse:             'badge badge-diffuse',
    };
    return map[statut];
  }

  /** Libellé lisible du statut. */
  statutLabel(statut: StatutMessage): string {
    const map: Record<StatutMessage, string> = {
      Brouillon:           'Brouillon',
      EnAttenteValidation: 'En attente',
      Valide:              'Validé',
      Rejete:              'Rejeté',
      Programme:           'Programmé',
      Diffuse:             'Diffusé',
    };
    return map[statut];
  }

  /** Formate une date ISO en date locale française. */
  formatDate(iso: string): string {
    return new Date(iso).toLocaleDateString('fr-FR', {
      day: '2-digit', month: 'short', year: 'numeric',
    });
  }
}
