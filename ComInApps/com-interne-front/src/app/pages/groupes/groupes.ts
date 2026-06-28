import { Component, inject, OnInit, signal, computed } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { forkJoin } from 'rxjs';
import { GroupeDiffusionService } from '../../services/groupe-diffusion.service';
import { AgentService } from '../../services/agent.service';
import { GroupeDiffusionSummary, TypeGroupe } from '../../models/groupe-diffusion.model';
import { Agent } from '../../models/agent.model';

/**
 * Page Liste des groupes de diffusion — GET /api/groupes-diffusion.
 *
 * Fonctionnalités :
 *   - Recherche + pagination
 *   - Création rapide d'un groupe (modale inline)
 *   - Suppression avec confirmation
 *   - Navigation vers le détail (membres)
 */
@Component({
  selector: 'app-groupes',
  imports: [RouterLink, FormsModule],
  templateUrl: './groupes.html',
  styleUrl: './groupes.css',
})
export class GroupesComponent implements OnInit {
  private readonly groupeService = inject(GroupeDiffusionService);
  private readonly agentService  = inject(AgentService);

  readonly loading    = signal(false);
  readonly error      = signal<string | null>(null);
  readonly groupes    = signal<GroupeDiffusionSummary[]>([]);
  readonly totalCount = signal(0);

  /** Agents RH chargés pour alimenter les critères dynamiques. */
  readonly agents = signal<Agent[]>([]);
  readonly agentsLoading = signal(false);

  readonly gradesDistincts = computed(() =>
    [...new Set(this.agents().map(a => a.gradeLibelle ?? '').filter(Boolean))].sort()
  );
  readonly entitesDistinctes = computed(() =>
    [...new Set(this.agents().map(a => a.entiteLibelle ?? '').filter(Boolean))].sort()
  );

  /**
   * Critère unique pour les groupes Dynamiques.
   * L'utilisateur sélectionne UN seul grade OU une seule entité.
   * Le nom du groupe est automatiquement dérivé de ce critère.
   */
  critereSelectionne: { type: 'grade' | 'entite'; valeur: string } | null = null;

  readonly agentsCorrespondants = computed(() => {
    const c = this.critereSelectionne;
    if (!c) return [];
    const all = this.agents();
    if (c.type === 'grade') return all.filter(a => a.gradeLibelle === c.valeur);
    return all.filter(a => a.entiteLibelle === c.valeur);
  });

  readonly appliquerCriteresLoading = signal(false); // conservé pour compatibilité template

  /** Map groupeId → effectif calculé en temps réel (reactive). */
  readonly membresEffectifsMap = computed(() => {
    const agents  = this.agents();
    const groupes = this.groupes();
    return new Map<string, number>(groupes.map(g => {
      if (g.type !== 'Dynamique' || !g.critereType || !g.critereValeur)
        return [g.id, g.nombreMembres] as [string, number];
      let count = 0;
      if (g.critereType === 'grade')  count = agents.filter(a => a.gradeLibelle  === g.critereValeur).length;
      if (g.critereType === 'entite') count = agents.filter(a => a.entiteLibelle === g.critereValeur).length;
      return [g.id, count] as [string, number];
    }));
  });

  page     = 1;
  pageSize = 20;
  search   = '';

  get totalPages(): number { return Math.ceil(this.totalCount() / this.pageSize); }

  /** Modale de création. */
  readonly showModalCreer  = signal(false);
  readonly actionLoading   = signal(false);
  readonly actionError     = signal<string | null>(null);
  nomGroupe         = '';
  descriptionGroupe = '';
  typeGroupe: TypeGroupe = 'Manuel';

  /** Modale de confirmation de suppression. */
  groupeASupprimer: GroupeDiffusionSummary | null = null;
  readonly toggleLoading = signal<string | null>(null); // id du groupe en cours de bascule

  /** Modale de visualisation / modification. */
  readonly showModalVoir  = signal(false);
  groupeEnEdition: GroupeDiffusionSummary | null = null;
  editNom         = '';
  editDescription = '';
  editType: TypeGroupe = 'Manuel';

  ngOnInit(): void {
    this.charger();
    this.agentsLoading.set(true);
    this.agentService.lister().subscribe({
      next: a => { this.agents.set(a); this.agentsLoading.set(false); },
      error: () => this.agentsLoading.set(false),
    });
  }

  charger(): void {
    this.loading.set(true);
    this.groupeService.lister(this.page, this.pageSize, this.search || undefined).subscribe({
      next: r => {
        this.groupes.set(r.items);
        this.totalCount.set(r.totalCount);
        this.loading.set(false);
      },
      error: () => { this.error.set('Erreur de chargement.'); this.loading.set(false); },
    });
  }

  onSearch(): void { this.page = 1; this.charger(); }
  pagePrecedente(): void { if (this.page > 1) { this.page--; this.charger(); } }
  pageSuivante(): void { if (this.page < this.totalPages) { this.page++; this.charger(); } }

  /** Crée un nouveau groupe. Si Dynamique, le critère est envoyé puis les membres RH correspondants sont stockés individuellement. */
  creer(): void {
    if (this.typeGroupe === 'Dynamique') {
      if (!this.critereSelectionne) { this.actionError.set('Veuillez sélectionner un grade ou une entité.'); return; }
      this.nomGroupe = this.critereSelectionne.valeur;
    }
    if (!this.nomGroupe.trim()) { this.actionError.set('Le nom est obligatoire.'); return; }
    this.actionLoading.set(true);
    this.groupeService.creer({
      nom:           this.nomGroupe,
      description:   this.descriptionGroupe || undefined,
      typeGroupe:    this.typeGroupe,
      critereType:   this.critereSelectionne?.type ?? null,
      critereValeur: this.critereSelectionne?.valeur ?? null,
    }).subscribe({
      next: ({ id }) => {
        const agentsDyn = this.typeGroupe === 'Dynamique' ? this.agentsCorrespondants().map(a => a.id) : [];
        if (agentsDyn.length === 0) {
          this.finaliserCreation();
          return;
        }

        forkJoin(agentsDyn.map(agentId => this.groupeService.ajouterMembreRh(id, agentId))).subscribe({
          next: () => this.finaliserCreation(),
          error: () => {
            this.actionError.set('Groupe créé, mais erreur lors de l’ajout des membres RH.');
            this.charger();
            this.actionLoading.set(false);
          },
        });
      },
      error: () => { this.actionError.set('Erreur lors de la création.'); this.actionLoading.set(false); },
    });
  }

  private finaliserCreation(): void {
    this.showModalCreer.set(false);
    this.nomGroupe          = '';
    this.descriptionGroupe  = '';
    this.critereSelectionne = null;
    this.charger();
    this.actionLoading.set(false);
  }

  /** Sélectionne un critère unique (radio) — désélectionne le précédent. */
  selectionnerCritere(type: 'grade' | 'entite', valeur: string): void {
    if (this.critereSelectionne?.type === type && this.critereSelectionne?.valeur === valeur) {
      this.critereSelectionne = null;
      this.nomGroupe = '';
    } else {
      this.critereSelectionne = { type, valeur };
      this.nomGroupe = valeur;
    }
  }

  isCritereActif(type: 'grade' | 'entite', valeur: string): boolean {
    return this.critereSelectionne?.type === type && this.critereSelectionne?.valeur === valeur;
  }

  /** Retourne le nombre de membres effectif.
   * - Groupes Dynamiques : compté en temps réel depuis la liste RH.
   * - Groupes Manuels : valeur stockée.
   */
  nombreMembresEffectif(g: GroupeDiffusionSummary): number {
    if (g.type !== 'Dynamique' || !g.critereType || !g.critereValeur) return g.nombreMembres;
    const all = this.agents();
    if (g.critereType === 'grade')  return all.filter(a => a.gradeLibelle  === g.critereValeur).length;
    if (g.critereType === 'entite') return all.filter(a => a.entiteLibelle === g.critereValeur).length;
    return g.nombreMembres;
  }

  /** Supprime le groupe sélectionné. */
  confirmerSuppression(): void {
    if (!this.groupeASupprimer) return;
    this.actionLoading.set(true);
    this.groupeService.supprimer(this.groupeASupprimer.id).subscribe({
      next: () => {
        this.groupeASupprimer = null;
        this.charger();
        this.actionLoading.set(false);
      },
      error: () => { this.actionError.set('Erreur lors de la suppression.'); this.actionLoading.set(false); },
    });
  }

  /** Bascule l'état actif/inactif du groupe directement depuis le tableau. */
  basculerEtat(g: GroupeDiffusionSummary): void {
    const libelle = g.estActif ? 'Désactiver' : 'Activer';
    if (!confirm(`${libelle} le groupe « ${g.nom} » ?`)) return;
    this.toggleLoading.set(g.id);
    this.groupeService.basculerEtat(g.id).subscribe({
      next:  () => { this.toggleLoading.set(null); this.charger(); },
      error: () => { this.toggleLoading.set(null); this.error.set('Erreur lors du changement d’état.'); },
    });
  }

  fermerModales(): void {
    this.showModalCreer.set(false);
    this.showModalVoir.set(false);
    this.groupeASupprimer  = null;
    this.groupeEnEdition   = null;
    this.critereSelectionne = null;
    this.nomGroupe          = '';
    this.actionError.set(null);
    this.actionLoading.set(false);
  }

  /** Ouvre la modale Voir / Modifier avec les valeurs actuelles du groupe. */
  ouvrirVoir(g: GroupeDiffusionSummary): void {
    this.groupeEnEdition   = g;
    this.editNom           = g.nom;
    this.editDescription   = g.description ?? '';
    this.editType          = g.type;
    this.critereSelectionne = null;
    this.actionError.set(null);
    this.showModalVoir.set(true);
  }

  /** Enregistre les modifications du groupe en édition. */
  enregistrerModification(): void {
    if (!this.groupeEnEdition || !this.editNom.trim()) {
      this.actionError.set('Le nom est obligatoire.');
      return;
    }
    this.actionLoading.set(true);
    this.groupeService.modifier(this.groupeEnEdition.id, {
      nom:         this.editNom.trim(),
      description: this.editDescription.trim() || undefined,
      typeGroupe:  this.editType,
    }).subscribe({
      next: () => {
        this.showModalVoir.set(false);
        this.groupeEnEdition = null;
        this.charger();
        this.actionLoading.set(false);
      },
      error: () => { this.actionError.set('Erreur lors de la modification.'); this.actionLoading.set(false); },
    });
  }

  formatDate(iso: string): string {
    return new Date(iso).toLocaleDateString('fr-FR', { day: '2-digit', month: 'short', year: 'numeric' });
  }
}
