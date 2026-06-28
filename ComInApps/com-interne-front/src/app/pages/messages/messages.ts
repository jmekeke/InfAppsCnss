import { Component, inject, OnInit, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { MessageService } from '../../services/message.service';
import { MessageInterne, StatutMessage } from '../../models/message.model';

/**
 * Page Liste des messages — GET /api/messages avec pagination et recherche.
 *
 * Fonctionnalités :
 *   - Recherche par objet/contenu (debounce natif sur input)
 *   - Pagination (page précédente / suivante)
 *   - Affichage du statut avec badge coloré
 *   - Navigation vers le détail ou le formulaire d'édition
 *   - Bouton "Nouveau message"
 */
@Component({
  selector: 'app-messages',
  imports: [RouterLink, FormsModule],
  templateUrl: './messages.html',
  styleUrl: './messages.css',
})
export class MessagesComponent implements OnInit {
  private readonly messageService = inject(MessageService);

  /** État de la liste. */
  readonly loading    = signal(false);
  readonly error      = signal<string | null>(null);
  readonly messages   = signal<MessageInterne[]>([]);
  readonly totalCount = signal(0);

  /** Paramètres de pagination et de recherche. */
  page     = 1;
  pageSize = 20;
  search   = '';

  /** Nombre total de pages calculé. */
  get totalPages(): number {
    return Math.ceil(this.totalCount() / this.pageSize);
  }

  ngOnInit(): void {
    this.charger();
  }

  /** Charge les messages selon les paramètres courants. */
  charger(): void {
    this.loading.set(true);
    this.error.set(null);
    this.messageService.lister(this.page, this.pageSize, this.search || undefined).subscribe({
      next: result => {
        this.messages.set(result.items);
        this.totalCount.set(result.totalCount);
        this.loading.set(false);
      },
      error: () => {
        this.error.set('Erreur lors du chargement des messages.');
        this.loading.set(false);
      },
    });
  }

  /** Déclenche une nouvelle recherche depuis le début. */
  onSearch(): void {
    this.page = 1;
    this.charger();
  }

  /** Navigation entre pages. */
  pagePrecedente(): void {
    if (this.page > 1) { this.page--; this.charger(); }
  }
  pageSuivante(): void {
    if (this.page < this.totalPages) { this.page++; this.charger(); }
  }

  /** Classe CSS du badge de statut. */
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
    const labels: Record<StatutMessage, string> = {
      Brouillon:           'Brouillon',
      EnAttenteValidation: 'En attente',
      Valide:              'Validé',
      Rejete:              'Rejeté',
      Programme:           'Programmé',
      Diffuse:             'Diffusé',
    };
    return labels[statut];
  }

  /** Formate une date ISO en locale française. */
  formatDate(iso: string): string {
    return new Date(iso).toLocaleDateString('fr-FR', {
      day: '2-digit', month: 'short', year: 'numeric',
    });
  }
}
