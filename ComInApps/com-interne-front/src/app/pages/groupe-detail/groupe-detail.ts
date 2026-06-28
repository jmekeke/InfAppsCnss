import { Component, inject, OnInit, signal, computed } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { forkJoin, of } from 'rxjs';
import { GroupeDiffusionService } from '../../services/groupe-diffusion.service';
import { AgentService } from '../../services/agent.service';
import { GroupeDiffusion, MembreGroupe } from '../../models/groupe-diffusion.model';
import { Agent, ResumeVoie } from '../../models/agent.model';

const PAGE_SIZE = 20;

type MembreAffiche = {
  key: string;
  membre: MembreGroupe | null;
  agent?: Agent;
  agentIdRh?: number | null;
  source: 'Base RH' | 'Manuel';
  isDynamic: boolean;
};

/**
 * Page Détail d'un groupe — GET /api/groupes-diffusion/{id}.
 *
 * Fonctionnalités :
 *   - Affichage des membres existants
 *   - Ajout d'un agent RH depuis la liste (POST /api/groupes-diffusion/{id}/membres/{agentIdRh})
 *   - Ajout d'un membre manuel (POST /api/groupes-diffusion/{id}/membres)
 *   - Retrait d'un membre (DELETE /api/groupes-diffusion/{id}/membres/{agentId})
 */
@Component({
  selector: 'app-groupe-detail',
  imports: [RouterLink, FormsModule],
  templateUrl: './groupe-detail.html',
  styleUrl: './groupe-detail.css',
})
export class GroupeDetailComponent implements OnInit {
  private readonly route         = inject(ActivatedRoute);
  private readonly groupeService = inject(GroupeDiffusionService);
  private readonly agentService  = inject(AgentService);

  readonly loading      = signal(true);
  readonly error        = signal<string | null>(null);
  readonly groupe       = signal<GroupeDiffusion | null>(null);
  readonly agents       = signal<Agent[]>([]);
  readonly voiesResume  = signal<Map<number, ResumeVoie>>(new Map());
  readonly page         = signal(1);
  readonly actionLoading = signal(false);
  readonly actionError   = signal<string | null>(null);

  /** Map agentIdRh → Agent pour accès O(1) lors de l'affichage. */
  readonly agentsMap = computed(() => new Map(this.agents().map(a => [a.id, a])));

  /** True si le groupe chargé est dynamique (membres calculés depuis RH). */
  readonly isGroupeDynamique = computed(() => this.groupe()?.type === 'Dynamique');

  /**
   * Lignes de membres affichées dans le tableau.
   * - Groupe Manuel : membres persistés en base.
   * - Groupe Dynamique : membres calculés en temps réel depuis RH (critère grade/entité).
   */
  readonly membresAffiches = computed(() => {
    const g = this.groupe();
    const agentsMap = this.agentsMap();
    if (!g) return [] as MembreAffiche[];

    if (g.type === 'Dynamique') {
      // Priorité aux membres persistés (stockage individuel au moment de la création).
      if (g.membres.length > 0) {
        return g.membres.map(m => ({
          key: m.id,
          membre: m,
          agent: m.agentIdRh != null ? agentsMap.get(m.agentIdRh) : undefined,
          agentIdRh: m.agentIdRh,
          source: m.agentIdRh != null ? ('Base RH' as const) : ('Manuel' as const),
          isDynamic: false,
        }));
      }

      // Fallback legacy : calcul RH à la volée si aucun membre n'est persisté.
      const critere = this.resoudreCritereDynamique(g);
      if (!critere) return [] as MembreAffiche[];

      const correspondants = this.agents().filter(a => {
        if (critere.type === 'grade')
          return this.matchValeurCritere(a.gradeLibelle, critere.valeur);
        if (critere.type === 'entite')
          return this.matchValeurCritere(a.entiteLibelle, critere.valeur);
        return false;
      });

      return correspondants.map(a => ({
        key: `rh-${a.id}`,
        membre: null,
        agent: a,
        agentIdRh: a.id,
        source: 'Base RH' as const,
        isDynamic: true,
      }));
    }

    return g.membres.map(m => ({
      key: m.id,
      membre: m,
      agent: m.agentIdRh != null ? agentsMap.get(m.agentIdRh) : undefined,
      agentIdRh: m.agentIdRh,
      source: m.agentIdRh != null ? ('Base RH' as const) : ('Manuel' as const),
      isDynamic: false,
    }));
  });

  readonly totalPages = computed(() =>
    Math.max(1, Math.ceil(this.membresAffiches().length / PAGE_SIZE))
  );

  readonly currentPage = computed(() =>
    Math.min(Math.max(1, this.page()), this.totalPages())
  );

  readonly pageCourante = computed(() => {
    const debut = (this.currentPage() - 1) * PAGE_SIZE;
    return this.membresAffiches().slice(debut, debut + PAGE_SIZE);
  });

  readonly pageNumbers = computed(() => {
    const total = this.totalPages();
    const current = this.currentPage();
    const delta = 2;
    const pages: (number | '…')[] = [];

    for (let i = 1; i <= total; i++) {
      if (i === 1 || i === total || (i >= current - delta && i <= current + delta)) {
        pages.push(i);
      } else if (pages[pages.length - 1] !== '…') {
        pages.push('…');
      }
    }

    return pages;
  });

  readonly pageFinActuelle = computed(() =>
    Math.min(this.currentPage() * PAGE_SIZE, this.membresAffiches().length)
  );

  private normaliser(v?: string | null): string {
    return (v ?? '')
      .normalize('NFD')
      .replace(/[\u0300-\u036f]/g, '')
      .replace(/[\-_/|:;,]+/g, ' ')
      .replace(/\s+/g, ' ')
      .trim()
      .toLowerCase();
  }

  private matchValeurCritere(label?: string | null, critereValeur?: string | null): boolean {
    const l = this.normaliser(label);
    const c = this.normaliser(critereValeur);
    if (!l || !c) return false;
    return l === c || l.includes(c) || c.includes(l);
  }

  /**
   * Résout le critère dynamique du groupe.
   * Priorité : critère stocké (API) ; fallback : nom du groupe (match grade/entité RH).
   */
  private resoudreCritereDynamique(g: GroupeDiffusion): { type: 'grade' | 'entite'; valeur: string } | null {
    if (g.critereType && g.critereValeur) {
      const type = this.normaliser(g.critereType);
      if (type === 'grade' || type === 'entite') {
        return { type, valeur: g.critereValeur };
      }
    }

    const cible = this.normaliser(g.nom);
    if (!cible) return null;

    const grades = [...new Set(this.agents().map(a => a.gradeLibelle).filter(Boolean))] as string[];
    const entites = [...new Set(this.agents().map(a => a.entiteLibelle).filter(Boolean))] as string[];

    const gradeExact = grades.find(x => this.normaliser(x) === cible);
    if (gradeExact) return { type: 'grade', valeur: gradeExact };

    const entiteExact = entites.find(x => this.normaliser(x) === cible);
    if (entiteExact) return { type: 'entite', valeur: entiteExact };

    const gradePartiel = grades.find(x => {
      const n = this.normaliser(x);
      return n.includes(cible) || cible.includes(n);
    });
    if (gradePartiel) return { type: 'grade', valeur: gradePartiel };

    const entitePartielle = entites.find(x => {
      const n = this.normaliser(x);
      return n.includes(cible) || cible.includes(n);
    });
    if (entitePartielle) return { type: 'entite', valeur: entitePartielle };

    return null;
  }

  /** Modale : ajouter un agent RH. */
  readonly showModalRh      = signal(false);
  /** Signal contenant le Set des agentIdRh sélectionnés — Signal obligatoire pour la détection de changement. */
  readonly selectedAgentIds = signal<Set<number>>(new Set());
  readonly searchAgent      = signal('');

  /** Agents RH filtrés par la recherche ET sans les membres déjà dans le groupe. */
  readonly agentsFiltres = computed(() => {
    const q       = this.searchAgent().toLowerCase();
    const membres = new Set(this.groupe()?.membres.map(m => m.agentIdRh).filter(Boolean) ?? []);
    return this.agents().filter(a => {
      if (membres.has(a.id)) return false;
      if (!q) return true;
      return this.nomCompletAgent(a).toLowerCase().includes(q) ||
             (a.emailProfessionnel ?? '').toLowerCase().includes(q);
    });
  });

  nomCompletAgent(a: Agent): string {
    return [a.nom, a.postnom, a.prenom].filter(Boolean).join(' ');
  }

  /** Modale : ajouter un membre manuel. */
  readonly showModalManuel = signal(false);
  nomManuel   = '';
  emailManuel = '';

  /** Membre à retirer (null = modale fermée). */
  membreARetirer: MembreGroupe | null = null;

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id')!;
    this.charger(id);
    this.agentService.lister().subscribe({
      next: agents => this.agents.set(agents),
    });
    this.agentService.getVoiesResume().subscribe({
      next: resumes => this.voiesResume.set(new Map(resumes.map(r => [r.agentIdRh, r]))),
    });
  }

  charger(id: string): void {
    this.loading.set(true);
    this.page.set(1);
    this.groupeService.get(id).subscribe({
      next: g => { this.groupe.set(g); this.loading.set(false); },
      error: () => { this.error.set('Groupe introuvable.'); this.loading.set(false); },
    });
  }

  aller(p: number | '…'): void {
    if (p === '…') return;
    if (p >= 1 && p <= this.totalPages()) this.page.set(p);
  }

  toggleAgent(agentId: number): void {
    const current = this.selectedAgentIds();
    const next = new Set(current);
    if (next.has(agentId)) next.delete(agentId);
    else next.add(agentId);
    this.selectedAgentIds.set(next);
  }

  /** Ajoute tous les agents sélectionnés comme membres (appels parallèles). */
  ajouterAgentRh(): void {
    const ids = Array.from(this.selectedAgentIds());
    if (ids.length === 0) { this.actionError.set('Sélectionnez au moins un agent.'); return; }
    const groupeId = this.groupe()!.id;
    this.actionLoading.set(true);
    forkJoin(ids.map(id => this.groupeService.ajouterMembreRh(groupeId, id))).subscribe({
      next: () => {
        this.showModalRh.set(false);
        this.selectedAgentIds.set(new Set());
        this.searchAgent.set('');
        this.charger(groupeId);
        this.actionLoading.set(false);
      },
      error: () => { this.actionError.set('Erreur lors de l’ajout d’un ou plusieurs agents.'); this.actionLoading.set(false); },
    });
  }

  /** Ajoute un membre manuellement. */
  ajouterManuel(): void {
    if (!this.nomManuel.trim()) { this.actionError.set('Le nom est obligatoire.'); return; }
    const id = this.groupe()!.id;
    this.actionLoading.set(true);
    this.groupeService.ajouterMembre(id, { nom: this.nomManuel, email: this.emailManuel || undefined }).subscribe({
      next: () => {
        this.showModalManuel.set(false);
        this.nomManuel = ''; this.emailManuel = '';
        this.charger(id); this.actionLoading.set(false);
      },
      error: () => { this.actionError.set('Erreur lors de l\'ajout.'); this.actionLoading.set(false); },
    });
  }

  /** Retire un membre directement avec confirmation. */
  retirerMembre(m: MembreGroupe): void {
    const label = this.nomAffiche(m);
    if (!confirm(`Retirer ${label} de ce groupe ?`)) return;
    const id = this.groupe()!.id;
    this.actionLoading.set(true);
    this.groupeService.retirerMembre(id, m.id).subscribe({
      next: () => { this.charger(id); this.actionLoading.set(false); },
      error: () => { this.actionError.set('Erreur lors du retrait.'); this.actionLoading.set(false); },
    });
  }

  // ── Helpers d'affichage pour la liste des membres ─────────────────────────

  agentPour(row: { agent?: Agent; agentIdRh?: number | null }): Agent | undefined {
    if (row.agent) return row.agent;
    return row.agentIdRh != null ? this.agentsMap().get(row.agentIdRh) : undefined;
  }

  matriculeAffiche(row: { agent?: Agent; agentIdRh?: number | null }): string {
    return this.agentPour(row)?.matricule ?? '—';
  }

  nomAffiche(row: { agent?: Agent; agentIdRh?: number | null }): string {
    const a = this.agentPour(row);
    if (a) return [a.nom, a.postnom, a.prenom].filter(Boolean).join(' ') || '—';
    return row.agentIdRh != null ? `Agent RH #${row.agentIdRh}` : '—';
  }

  entiteAffiche(row: { agent?: Agent; agentIdRh?: number | null }): string {
    return this.agentPour(row)?.entiteLibelle ?? '—';
  }

  gradeAffiche(row: { agent?: Agent; agentIdRh?: number | null }): string {
    return this.agentPour(row)?.gradeLibelle ?? '—';
  }

  fonctionAffiche(row: { agent?: Agent; agentIdRh?: number | null }): string {
    return this.agentPour(row)?.fonctionLibelle ?? '—';
  }

  emailAffiche(row: { agent?: Agent; agentIdRh?: number | null }): string {
    if (row.agentIdRh != null) {
      const resume = this.voiesResume().get(row.agentIdRh);
      if (resume?.emailProfActif) return resume.emailProfActif;
      return this.agentPour(row)?.emailProfessionnel ?? '—';
    }
    return '—';
  }

  telephoneAffiche(row: { agent?: Agent; agentIdRh?: number | null }): string {
    if (row.agentIdRh != null) {
      const resume = this.voiesResume().get(row.agentIdRh);
      if (resume?.telephoneActif) return resume.telephoneActif;
      return this.agentPour(row)?.telephone ?? '—';
    }
    return '—';
  }

  fermerModales(): void {
    this.showModalRh.set(false);
    this.showModalManuel.set(false);
    this.selectedAgentIds.set(new Set());
    this.searchAgent.set('');
    this.actionError.set(null);
    this.actionLoading.set(false);
  }
}
