import { Component, inject, signal, computed, OnInit } from '@angular/core';
import { RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { AgentService } from '../../../services/agent.service';
import { AuthService } from '../../../services/auth.service';
import { VoieCommunicationService } from '../../../services/voie-communication.service';
import { Agent, ResumeVoie } from '../../../models/agent.model';
import {
  VoieCommunicationDto,
  VoieTelephoneDto,
  VoieEmailDto,
  TypeVoieTelephone,
  TypeVoieEmail,
  TYPES_TELEPHONE,
  TYPES_EMAIL,
} from '../../../models/voie-communication.model';

const PAGE_SIZE = 20;

export type ModalCanal = 'tel' | 'email';

/** Liste des agents actifs — GET /api/agents (filtrés sur categorie = 'Actif') */
@Component({
  selector: 'app-membres-liste',
  imports: [RouterLink, FormsModule],
  templateUrl: './membres-liste.html',
  styleUrl: './membres-liste.css',
})
export class MembreListeComponent implements OnInit {
  private readonly agentService = inject(AgentService);
  private readonly authService  = inject(AuthService);
  private readonly voieSvc      = inject(VoieCommunicationService);

  /** Autorisation granulaire : gérer les e-mails des membres. */
  readonly canEmail = computed(() => this.authService.hasTache('cnss-metier-front.Membres.Membres.Email'));

  readonly agents  = signal<Agent[]>([]);
  readonly loading = signal(true);
  readonly error   = signal<string | null>(null);
  readonly page    = signal(1);

  /** Terme de recherche côté client — signal pour que agentsFiltres se recalcule. */
  readonly recherche = signal('');

  /** Map agentId → résumé voie communication (source PostgreSQL, prioritaire sur RH). */
  readonly voiesResume = signal<Map<number, ResumeVoie>>(new Map());

  // ── Modal ─────────────────────────────────────────────────────────────────
  readonly modalVisible  = signal(false);
  readonly modalCanal    = signal<ModalCanal>('tel');
  readonly modalAgent    = signal<Agent | null>(null);
  readonly modalVoie     = signal<VoieCommunicationDto | null>(null);
  readonly modalLoading  = signal(false);
  readonly modalSaving   = signal(false);
  readonly modalError    = signal<string | null>(null);
  readonly modalSuccess  = signal<string | null>(null);

  // Formulaire d'édition
  readonly editType      = signal<string | null>(null);
  readonly editValeur    = signal('');

  // Constantes pour le template
  readonly typesTelephone = TYPES_TELEPHONE;
  readonly typesEmail     = TYPES_EMAIL;

  // ── Pagination ────────────────────────────────────────────────────────────
  readonly agentsActifs = computed(() =>
    this.agents().filter(a => a.categorie?.toLowerCase() === 'actif')
  );

  readonly agentsFiltres = computed(() => {
    const terme = this.recherche().trim().toLowerCase();
    if (!terme) return this.agentsActifs();
    return this.agentsActifs().filter(a =>
      this.nomComplet(a).toLowerCase().includes(terme) ||
      (a.matricule ?? '').toLowerCase().includes(terme) ||
      (a.entiteLibelle ?? '').toLowerCase().includes(terme) ||
      (a.fonctionLibelle ?? '').toLowerCase().includes(terme)
    );
  });

  readonly totalPages = computed(() =>
    Math.max(1, Math.ceil(this.agentsFiltres().length / PAGE_SIZE))
  );
  readonly pageCourante = computed(() => {
    const debut = (this.page() - 1) * PAGE_SIZE;
    return this.agentsFiltres().slice(debut, debut + PAGE_SIZE);
  });

  readonly pageNumbers = computed(() => {
    const total = this.totalPages();
    const current = this.page();
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
    Math.min(this.page() * PAGE_SIZE, this.agentsFiltres().length)
  );

  ngOnInit(): void {
    this.agentService.lister().subscribe({
      next:  a => { this.agents.set(a); this.loading.set(false); },
      error: () => { this.error.set('Impossible de charger les agents.'); this.loading.set(false); },
    });

    // Chargement en parallèle des voies de communication pour enrichir les colonnes Telephone/Email
    this.agentService.getVoiesResume().subscribe({
      next: resumes => this.voiesResume.set(new Map(resumes.map(r => [r.agentIdRh, r]))),
    });
  }

  aller(p: number | '…'): void {
    if (p === '…') return;
    if (p >= 1 && p <= this.totalPages()) this.page.set(p);
  }

  onRecherche(): void { this.page.set(1); }

  effacerRecherche(): void { this.recherche.set(''); this.page.set(1); }

  nomComplet(a: Agent): string {
    return [a.nom, a.postnom, a.prenom].filter(Boolean).join(' ');
  }

  // ── Ouverture modal ───────────────────────────────────────────────────────

  ouvrirModal(agent: Agent, canal: ModalCanal): void {
    this.modalAgent.set(agent);
    this.modalCanal.set(canal);
    this.modalVoie.set(null);
    this.modalError.set(null);
    this.modalSuccess.set(null);
    this.editType.set(null);
    this.editValeur.set('');
    this.modalVisible.set(true);
    this.modalLoading.set(true);

    this.voieSvc.get(agent.id).subscribe({
      next:  v => { this.modalVoie.set(v); this.modalLoading.set(false); },
      error: err => {
        if (err.status === 404) this.modalVoie.set(null);
        else this.modalError.set('Erreur lors du chargement.');
        this.modalLoading.set(false);
      },
    });
  }

  fermerModal(): void {
    this.modalVisible.set(false);
    this.editType.set(null);
    this.editValeur.set('');
  }

  // ── Helpers de lecture ────────────────────────────────────────────────────

  telPour(type: TypeVoieTelephone): VoieTelephoneDto | undefined {
    return this.modalVoie()?.telephones.find(t => t.type === type);
  }

  emailPour(type: TypeVoieEmail): VoieEmailDto | undefined {
    return this.modalVoie()?.emails.find(e => e.type === type);
  }

  ouvrirEdit(type: string, valeurActuelle: string): void {
    this.editType.set(type);
    this.editValeur.set(valeurActuelle);
  }

  annulerEdit(): void { this.editType.set(null); this.editValeur.set(''); }

  // ── Sauvegarde (PUT global) ───────────────────────────────────────────────

  sauvegarder(): void {
    const agent = this.modalAgent();
    const type  = this.editType();
    const val   = this.editValeur().trim();
    if (!agent || !type || !val) return;

    this.modalSaving.set(true);
    const v          = this.modalVoie();
    const matricule  = agent.matricule ?? '';
    const canal      = this.modalCanal();

    const telephones = this.typesTelephone.map(t => ({
      type: t,
      numero: canal === 'tel' && t === type
        ? val
        : (v?.telephones.find(x => x.type === t)?.numero ?? undefined),
    }));

    const emails = this.typesEmail.map(t => ({
      type: t,
      adresse: canal === 'email' && t === type
        ? val
        : (v?.emails.find(x => x.type === t)?.adresse ?? undefined),
    }));

    this.voieSvc.mettreAJour(agent.id, { matricule, telephones, emails }).subscribe({
      next: () => {
        this.flash('Enregistré.');
        this.annulerEdit();
        this.rechargerVoie(agent.id);
      },
      error: () => { this.modalSaving.set(false); this.modalError.set('Erreur lors de la sauvegarde.'); },
    });
  }

  // ── Actions téléphone ─────────────────────────────────────────────────────

  desactiverTel(type: TypeVoieTelephone): void {
    if (!confirm(`Désactiver le numéro ${type} ?`)) return;
    this.modalSaving.set(true);
    this.voieSvc.desactiverTelephone(this.modalAgent()!.id, type).subscribe({
      next: () => { this.flash(`${type} désactivé.`); this.rechargerVoie(this.modalAgent()!.id); },
      error: () => { this.modalSaving.set(false); this.modalError.set('Erreur.'); },
    });
  }

  reactiverTel(type: TypeVoieTelephone): void {
    this.modalSaving.set(true);
    this.voieSvc.reactiverTelephone(this.modalAgent()!.id, type).subscribe({
      next: () => { this.flash(`${type} réactivé.`); this.rechargerVoie(this.modalAgent()!.id); },
      error: () => { this.modalSaving.set(false); this.modalError.set('Erreur.'); },
    });
  }

  supprimerTel(type: TypeVoieTelephone): void {
    if (!confirm(`Supprimer définitivement le numéro ${type} ?`)) return;
    this.modalSaving.set(true);
    this.voieSvc.supprimerTelephone(this.modalAgent()!.id, type).subscribe({
      next: () => { this.flash(`${type} supprimé.`); this.rechargerVoie(this.modalAgent()!.id); },
      error: () => { this.modalSaving.set(false); this.modalError.set('Erreur.'); },
    });
  }

  // ── Actions e-mail ────────────────────────────────────────────────────────

  desactiverEmail(type: TypeVoieEmail): void {
    if (!confirm(`Désactiver l'e-mail ${type} ?`)) return;
    this.modalSaving.set(true);
    this.voieSvc.desactiverEmail(this.modalAgent()!.id, type).subscribe({
      next: () => { this.flash(`${type} désactivé.`); this.rechargerVoie(this.modalAgent()!.id); },
      error: () => { this.modalSaving.set(false); this.modalError.set('Erreur.'); },
    });
  }

  reactiverEmail(type: TypeVoieEmail): void {
    this.modalSaving.set(true);
    this.voieSvc.reactiverEmail(this.modalAgent()!.id, type).subscribe({
      next: () => { this.flash(`${type} réactivé.`); this.rechargerVoie(this.modalAgent()!.id); },
      error: () => { this.modalSaving.set(false); this.modalError.set('Erreur.'); },
    });
  }

  supprimerEmail(type: TypeVoieEmail): void {
    if (!confirm(`Supprimer définitivement l'adresse ${type} ?`)) return;
    this.modalSaving.set(true);
    this.voieSvc.supprimerEmail(this.modalAgent()!.id, type).subscribe({
      next: () => { this.flash(`${type} supprimé.`); this.rechargerVoie(this.modalAgent()!.id); },
      error: () => { this.modalSaving.set(false); this.modalError.set('Erreur.'); },
    });
  }

  // ── Helpers privés ────────────────────────────────────────────────────────

  private rechargerVoie(agentId: number): void {
    this.voieSvc.get(agentId).subscribe({
      next: v => {
        this.modalVoie.set(v);
        this.modalSaving.set(false);
        // Mise à jour immédiate de la colonne dans le tableau
        this.rafraichirResume(agentId);
      },
      error: () => { this.modalSaving.set(false); },
    });
  }

  private rafraichirResume(agentId: number): void {
    this.agentService.getVoiesResume().subscribe({
      next: resumes => {
        const nouvelleMap = new Map(this.voiesResume());
        const resume = resumes.find(r => r.agentIdRh === agentId);
        if (resume) nouvelleMap.set(agentId, resume);
        else nouvelleMap.delete(agentId);
        this.voiesResume.set(nouvelleMap);
      },
    });
  }

  private flash(msg: string): void {
    this.modalError.set(null);
    this.modalSuccess.set(msg);
    setTimeout(() => this.modalSuccess.set(null), 3000);
  }

  labelType(t: string): string {
    const map: Record<string, string> = {
      Appel: 'Appel', Sms: 'SMS', WhatsApp: 'WhatsApp',
      Professionnel: 'Professionnel', Prive: 'Privé',
    };
    return map[t] ?? t;
  }

  /**
   * Téléphone à afficher dans le tableau : valeur de la voie communication (PostgreSQL)
   * en priorité, sinon la valeur RH.
   */
  telephoneAffiche(a: Agent): string {
    return this.voiesResume().get(a.id)?.telephoneActif ?? a.telephone ?? '—';
  }

  /**
   * E-mail professionnel à afficher : valeur de la voie communication (PostgreSQL)
   * en priorité, sinon la valeur RH.
   */
  emailAffiche(a: Agent): string {
    return this.voiesResume().get(a.id)?.emailProfActif ?? a.emailProfessionnel ?? '—';
  }
}

