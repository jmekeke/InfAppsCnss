import { Component, inject, OnInit, signal } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { DossierDiffusionService } from '../../services/dossier-diffusion.service';
import { DossierDiffusion, StatutEnvoi } from '../../models/dossier-diffusion.model';

/**
 * Page Dossiers de diffusion — GET /api/dossiers-diffusion/par-message/{messageId}.
 *
 * Affiche la liste des dossiers d'un message diffusé (un dossier = un groupe cible)
 * et le détail des lignes d'envoi (statut par destinataire).
 *
 * Cette page est accessible depuis le détail d'un message Diffusé.
 */
@Component({
  selector: 'app-dossiers-diffusion',
  imports: [RouterLink],
  templateUrl: './dossiers-diffusion.html',
  styleUrl: './dossiers-diffusion.css',
})
export class DossiersDiffusionComponent implements OnInit {
  private readonly route   = inject(ActivatedRoute);
  private readonly service = inject(DossierDiffusionService);

  readonly loading  = signal(true);
  readonly error    = signal<string | null>(null);
  readonly dossiers = signal<DossierDiffusion[]>([]);

  /** Dossier sélectionné pour afficher le détail des lignes. */
  readonly dossierOuvert = signal<DossierDiffusion | null>(null);

  ngOnInit(): void {
    const messageId = this.route.snapshot.paramMap.get('messageId')!;
    this.service.listerParMessage(messageId).subscribe({
      next: d => { this.dossiers.set(d); this.loading.set(false); },
      error: () => { this.error.set('Impossible de charger les dossiers.'); this.loading.set(false); },
    });
  }

  /** Ouvre/ferme le détail des lignes d'un dossier. */
  toggleDossier(d: DossierDiffusion): void {
    this.dossierOuvert.set(this.dossierOuvert()?.id === d.id ? null : d);
  }

  /** Classe CSS du badge de statut d'envoi. */
  envoBadge(statut: StatutEnvoi): string {
    const map: Record<StatutEnvoi, string> = {
      EnAttente: 'badge badge-attente',
      Envoye:    'badge badge-valide',
      Echoue:    'badge badge-rejete',
      Annule:    'badge badge-brouillon',
    };
    return map[statut];
  }

  /** Libellé du statut d'envoi. */
  envoLabel(statut: StatutEnvoi): string {
    const map: Record<StatutEnvoi, string> = {
      EnAttente: 'En attente',
      Envoye:    'Envoyé',
      Echoue:    'Échoué',
      Annule:    'Annulé',
    };
    return map[statut];
  }

  formatDate(iso?: string): string {
    if (!iso) return '—';
    return new Date(iso).toLocaleDateString('fr-FR', {
      day: '2-digit', month: 'short', year: 'numeric', hour: '2-digit', minute: '2-digit',
    });
  }

  /** Calcul du taux de succès (%). */
  tauxSucces(d: DossierDiffusion): number {
    if (d.nombreTotal === 0) return 0;
    return Math.round((d.nombreEnvoyes / d.nombreTotal) * 100);
  }
}
