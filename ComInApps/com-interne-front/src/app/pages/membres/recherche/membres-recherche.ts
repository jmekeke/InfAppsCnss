import { Component, inject, signal, computed } from '@angular/core';
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
import { ModalCanal } from '../liste/membres-liste';

const PAGE_SIZE = 20;

/** Recherche d'agents par nom et/ou entité — GET /api/agents/recherche */
@Component({
  selector: 'app-membres-recherche',
  imports: [FormsModule, RouterLink],
  templateUrl: './membres-recherche.html',
  styleUrl: './membres-recherche.css',
})
export class MembreRechercheComponent {
  private readonly agentService = inject(AgentService);
  private readonly authService  = inject(AuthService);
  private readonly voieSvc      = inject(VoieCommunicationService);

  /** Autorisation granulaire : gérer les e-mails des membres. */
  readonly canEmail = computed(() => this.authService.hasTache('cnss-metier-front.Membres.Membres.Email'));

  /** Champs du formulaire de recherche */
  nom           = '';
  entiteLibelle = '';

  readonly agents      = signal<Agent[] | null>(null);
  readonly loading     = signal(false);
  readonly error       = signal<string | null>(null);
  readonly searched    = signal(false);
  readonly page        = signal(1);

  readonly totalPages = computed(() => {
    const total = this.agents()?.length ?? 0;
    return Math.max(1, Math.ceil(total / PAGE_SIZE));
  });

  readonly pageCourante = computed(() => {
    const all = this.agents() ?? [];
    const debut = (this.page() - 1) * PAGE_SIZE;
    return all.slice(debut, debut + PAGE_SIZE);
  });

  readonly pageNumbers = computed(() => {
    const total = this.totalPages();
    const current = this.page();
    const delta = 2;
    const pages: (number | '...')[] = [];
    for (let i = 1; i <= total; i++) {
      if (i === 1 || i === total || (i >= current - delta && i <= current + delta)) {
        pages.push(i);
      } else if (pages[pages.length - 1] !== '...') {
        pages.push('...');
      }
    }
    return pages;
  });

  readonly pageFinActuelle = computed(() => {
    const total = this.agents()?.length ?? 0;
    return Math.min(this.page() * PAGE_SIZE, total);
  });

  /** Map agentId → résumé voie communication (source PostgreSQL, prioritaire sur RH). */
  readonly voiesResume = signal<Map<number, ResumeVoie>>(new Map());

  // ── Modal ─────────────────────────────────────────────────────────────────
  readonly modalVisible = signal(false);
  readonly modalCanal   = signal<ModalCanal>('tel');
  readonly modalAgent   = signal<Agent | null>(null);
  readonly modalVoie    = signal<VoieCommunicationDto | null>(null);
  readonly modalLoading = signal(false);
  readonly modalSaving  = signal(false);
  readonly modalError   = signal<string | null>(null);
  readonly modalSuccess = signal<string | null>(null);

  readonly editType   = signal<string | null>(null);
  readonly editValeur = signal('');

  readonly typesTelephone = TYPES_TELEPHONE;
  readonly typesEmail     = TYPES_EMAIL;

  // ── Formulaire recherche ──────────────────────────────────────────────────

  get formValid(): boolean {
    return this.nom.trim().length > 0 || this.entiteLibelle.trim().length > 0;
  }

  rechercher(): void {
    if (!this.formValid) return;
    this.page.set(1);
    this.loading.set(true);
    this.error.set(null);
    this.searched.set(true);
    this.agentService.rechercher({
      nom:           this.nom.trim()           || undefined,
      entiteLibelle: this.entiteLibelle.trim() || undefined,
    }).subscribe({
      next:  a => { this.agents.set(a); this.loading.set(false); },
      error: () => { this.error.set('Erreur lors de la recherche.'); this.loading.set(false); },
    });

    // Rafraîchissement des voies de communication pour les agents retournés
    this.agentService.getVoiesResume().subscribe({
      next: resumes => this.voiesResume.set(new Map(resumes.map(r => [r.agentIdRh, r]))),
    });
  }

  reinitialiser(): void {
    this.nom = ''; this.entiteLibelle = '';
    this.agents.set(null); this.searched.set(false); this.error.set(null); this.page.set(1);
  }

  aller(p: number | '...'): void {
    if (p === '...') return;
    if (p >= 1 && p <= this.totalPages()) this.page.set(p);
  }

  nomComplet(a: Agent): string {
    return [a.nom, a.postnom, a.prenom].filter(Boolean).join(' ');
  }

  // ── Ouverture / fermeture modal ───────────────────────────────────────────

  ouvrirModal(agent: Agent, canal: ModalCanal): void {
    this.modalAgent.set(agent); this.modalCanal.set(canal);
    this.modalVoie.set(null); this.modalError.set(null); this.modalSuccess.set(null);
    this.editType.set(null); this.editValeur.set('');
    this.modalVisible.set(true); this.modalLoading.set(true);
    this.voieSvc.get(agent.id).subscribe({
      next:  v => { this.modalVoie.set(v); this.modalLoading.set(false); },
      error: err => {
        if (err.status === 404) this.modalVoie.set(null);
        else this.modalError.set('Erreur lors du chargement.');
        this.modalLoading.set(false);
      },
    });
  }

  fermerModal(): void { this.modalVisible.set(false); this.editType.set(null); this.editValeur.set(''); }

  // ── Helpers de lecture ────────────────────────────────────────────────────

  telPour(type: TypeVoieTelephone): VoieTelephoneDto | undefined {
    return this.modalVoie()?.telephones.find(t => t.type === type);
  }

  emailPour(type: TypeVoieEmail): VoieEmailDto | undefined {
    return this.modalVoie()?.emails.find(e => e.type === type);
  }

  ouvrirEdit(type: string, valeur: string): void { this.editType.set(type); this.editValeur.set(valeur); }
  annulerEdit(): void { this.editType.set(null); this.editValeur.set(''); }

  // ── Sauvegarde ────────────────────────────────────────────────────────────

  sauvegarder(): void {
    const agent = this.modalAgent(), type = this.editType(), val = this.editValeur().trim();
    if (!agent || !type || !val) return;
    this.modalSaving.set(true);
    const v = this.modalVoie(), canal = this.modalCanal(), matricule = agent.matricule ?? '';
    const telephones = this.typesTelephone.map(t => ({
      type: t,
      numero: canal === 'tel' && t === type ? val : (v?.telephones.find(x => x.type === t)?.numero ?? undefined),
    }));
    const emails = this.typesEmail.map(t => ({
      type: t,
      adresse: canal === 'email' && t === type ? val : (v?.emails.find(x => x.type === t)?.adresse ?? undefined),
    }));
    this.voieSvc.mettreAJour(agent.id, { matricule, telephones, emails }).subscribe({
      next: () => { this.flash('Enregistré.'); this.annulerEdit(); this.rechargerVoie(agent.id); },
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
    this.modalError.set(null); this.modalSuccess.set(msg);
    setTimeout(() => this.modalSuccess.set(null), 3000);
  }

  labelType(t: string): string {
    const map: Record<string, string> = {
      Appel: 'Appel', Sms: 'SMS', WhatsApp: 'WhatsApp',
      Professionnel: 'Professionnel', Prive: 'Privé',
    };
    return map[t] ?? t;
  }

  /** Téléphone à afficher : voie communication (PostgreSQL) en priorité, sinon RH. */
  telephoneAffiche(a: Agent): string {
    return this.voiesResume().get(a.id)?.telephoneActif ?? a.telephone ?? '—';
  }

  /** E-mail professionnel à afficher : voie communication (PostgreSQL) en priorité, sinon RH. */
  emailAffiche(a: Agent): string {
    return this.voiesResume().get(a.id)?.emailProfActif ?? a.emailProfessionnel ?? '—';
  }
}
