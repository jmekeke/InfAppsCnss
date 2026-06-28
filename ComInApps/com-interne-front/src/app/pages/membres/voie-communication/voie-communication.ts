import { Component, inject, signal, OnInit, computed } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { AgentService } from '../../../services/agent.service';
import { VoieCommunicationService } from '../../../services/voie-communication.service';
import { Agent } from '../../../models/agent.model';
import {
  VoieCommunicationDto,
  VoieTelephoneDto,
  VoieEmailDto,
  HistoriqueVoieDto,
  TypeVoieTelephone,
  TypeVoieEmail,
  CanalVoie,
  TYPES_TELEPHONE,
  TYPES_EMAIL,
} from '../../../models/voie-communication.model';

type Mode = 'view' | 'edit-tel' | 'edit-email';

@Component({
  selector: 'app-voie-communication',
  imports: [RouterLink],
  templateUrl: './voie-communication.html',
  styleUrl:    './voie-communication.css',
})
export class VoieCommunicationComponent implements OnInit {
  private readonly route      = inject(ActivatedRoute);
  private readonly agentSvc   = inject(AgentService);
  private readonly voieSvc    = inject(VoieCommunicationService);

  readonly agent         = signal<Agent | null>(null);
  readonly voie          = signal<VoieCommunicationDto | null>(null);
  readonly historique    = signal<HistoriqueVoieDto[]>([]);
  readonly loading       = signal(true);
  readonly saving        = signal(false);
  readonly errorMsg      = signal<string | null>(null);
  readonly successMsg    = signal<string | null>(null);
  readonly showHistorique = signal(false);
  readonly filtreCanal   = signal<CanalVoie | 'Tous'>('Tous');

  // ── Formulaire téléphone en cours d'édition ───────────────────────────────
  readonly editTelType   = signal<TypeVoieTelephone | null>(null);
  readonly editTelNumero = signal('');

  // ── Formulaire e-mail en cours d'édition ─────────────────────────────────
  readonly editEmailType    = signal<TypeVoieEmail | null>(null);
  readonly editEmailAdresse = signal('');

  // ── Constantes pour le template ───────────────────────────────────────────
  readonly typesTelephone = TYPES_TELEPHONE;
  readonly typesEmail     = TYPES_EMAIL;

  readonly historiqueFiltre = computed(() => {
    const canal = this.filtreCanal();
    return canal === 'Tous'
      ? this.historique()
      : this.historique().filter(h => h.canal === canal);
  });

  agentId = 0;

  ngOnInit(): void {
    this.agentId = Number(this.route.snapshot.paramMap.get('id'));
    this.charger();
  }

  // ── Chargement ────────────────────────────────────────────────────────────

  private charger(): void {
    this.loading.set(true);
    this.agentSvc.get(this.agentId).subscribe({
      next: a => {
        this.agent.set(a);
        this.voieSvc.get(this.agentId).subscribe({
          next:  v => { this.voie.set(v); this.loading.set(false); },
          error: err => {
            // 404 = pas encore de voie — normal
            if (err.status === 404) { this.voie.set(null); }
            else { this.errorMsg.set('Erreur lors du chargement des voies.'); }
            this.loading.set(false);
          },
        });
      },
      error: () => { this.errorMsg.set('Agent introuvable.'); this.loading.set(false); },
    });
  }

  recharger(): void { this.charger(); }

  // ── Téléphone — édition ───────────────────────────────────────────────────

  ouvrirEditTel(type: TypeVoieTelephone): void {
    const existant = this.voie()?.telephones.find(t => t.type === type);
    this.editTelType.set(type);
    this.editTelNumero.set(existant?.numero ?? '');
    this.editEmailType.set(null);
  }

  annulerEditTel(): void { this.editTelType.set(null); this.editTelNumero.set(''); }

  sauvegarderTelephone(): void {
    const type   = this.editTelType();
    const numero = this.editTelNumero().trim();
    if (!type || !numero) return;

    this.saving.set(true);
    const v = this.voie();
    const matricule = this.agent()?.matricule ?? '';

    // On envoie tout le PUT avec le numéro modifié
    const telephones = this.typesTelephone.map(t => ({
      type: t,
      numero: t === type ? numero : (v?.telephones.find(x => x.type === t)?.numero ?? undefined),
    }));
    const emails = (v?.emails ?? []).map(e => ({ type: e.type, adresse: e.adresse }));

    this.voieSvc.mettreAJour(this.agentId, { matricule, telephones, emails }).subscribe({
      next: () => {
        this.flash('Téléphone enregistré.');
        this.annulerEditTel();
        this.charger();
      },
      error: () => { this.saving.set(false); this.errorMsg.set('Erreur lors de la sauvegarde.'); },
    });
  }

  desactiverTelephone(type: TypeVoieTelephone): void {
    if (!confirm(`Désactiver le numéro ${type} ?`)) return;
    this.saving.set(true);
    this.voieSvc.desactiverTelephone(this.agentId, type).subscribe({
      next: () => { this.flash(`Numéro ${type} désactivé.`); this.charger(); },
      error: () => { this.saving.set(false); this.errorMsg.set('Erreur lors de la désactivation.'); },
    });
  }

  reactiverTelephone(type: TypeVoieTelephone): void {
    this.saving.set(true);
    this.voieSvc.reactiverTelephone(this.agentId, type).subscribe({
      next: () => { this.flash(`Numéro ${type} réactivé.`); this.charger(); },
      error: () => { this.saving.set(false); this.errorMsg.set('Erreur lors de la réactivation.'); },
    });
  }

  supprimerTelephone(type: TypeVoieTelephone): void {
    if (!confirm(`Supprimer définitivement le numéro ${type} ?`)) return;
    this.saving.set(true);
    this.voieSvc.supprimerTelephone(this.agentId, type).subscribe({
      next: () => { this.flash(`Numéro ${type} supprimé.`); this.charger(); },
      error: () => { this.saving.set(false); this.errorMsg.set('Erreur lors de la suppression.'); },
    });
  }

  // ── E-mail — édition ─────────────────────────────────────────────────────

  ouvrirEditEmail(type: TypeVoieEmail): void {
    const existant = this.voie()?.emails.find(e => e.type === type);
    this.editEmailType.set(type);
    this.editEmailAdresse.set(existant?.adresse ?? '');
    this.editTelType.set(null);
  }

  annulerEditEmail(): void { this.editEmailType.set(null); this.editEmailAdresse.set(''); }

  sauvegarderEmail(): void {
    const type    = this.editEmailType();
    const adresse = this.editEmailAdresse().trim();
    if (!type || !adresse) return;

    this.saving.set(true);
    const v = this.voie();
    const matricule = this.agent()?.matricule ?? '';

    const telephones = (v?.telephones ?? []).map(t => ({ type: t.type, numero: t.numero }));
    const emails = this.typesEmail.map(t => ({
      type: t,
      adresse: t === type ? adresse : (v?.emails.find(x => x.type === t)?.adresse ?? undefined),
    }));

    this.voieSvc.mettreAJour(this.agentId, { matricule, telephones, emails }).subscribe({
      next: () => {
        this.flash('Adresse e-mail enregistrée.');
        this.annulerEditEmail();
        this.charger();
      },
      error: () => { this.saving.set(false); this.errorMsg.set('Erreur lors de la sauvegarde.'); },
    });
  }

  desactiverEmail(type: TypeVoieEmail): void {
    if (!confirm(`Désactiver l'e-mail ${type} ?`)) return;
    this.saving.set(true);
    this.voieSvc.desactiverEmail(this.agentId, type).subscribe({
      next: () => { this.flash(`E-mail ${type} désactivé.`); this.charger(); },
      error: () => { this.saving.set(false); this.errorMsg.set('Erreur lors de la désactivation.'); },
    });
  }

  reactiverEmail(type: TypeVoieEmail): void {
    this.saving.set(true);
    this.voieSvc.reactiverEmail(this.agentId, type).subscribe({
      next: () => { this.flash(`E-mail ${type} réactivé.`); this.charger(); },
      error: () => { this.saving.set(false); this.errorMsg.set('Erreur lors de la réactivation.'); },
    });
  }

  supprimerEmail(type: TypeVoieEmail): void {
    if (!confirm(`Supprimer définitivement l'adresse e-mail ${type} ?`)) return;
    this.saving.set(true);
    this.voieSvc.supprimerEmail(this.agentId, type).subscribe({
      next: () => { this.flash(`E-mail ${type} supprimé.`); this.charger(); },
      error: () => { this.saving.set(false); this.errorMsg.set('Erreur lors de la suppression.'); },
    });
  }

  // ── Historique ────────────────────────────────────────────────────────────

  toggleHistorique(): void {
    if (!this.showHistorique()) {
      this.voieSvc.getHistorique(this.agentId).subscribe({
        next: h => { this.historique.set(h); this.showHistorique.set(true); },
        error: () => this.errorMsg.set('Erreur lors du chargement de l\'historique.'),
      });
    } else {
      this.showHistorique.set(false);
    }
  }

  filtrerCanal(canal: CanalVoie | 'Tous'): void { this.filtreCanal.set(canal); }

  // ── Helpers template ──────────────────────────────────────────────────────

  telPour(type: TypeVoieTelephone): VoieTelephoneDto | undefined {
    return this.voie()?.telephones.find(t => t.type === type);
  }

  emailPour(type: TypeVoieEmail): VoieEmailDto | undefined {
    return this.voie()?.emails.find(e => e.type === type);
  }

  nomComplet(a: Agent): string {
    return [a.nom, a.postnom, a.prenom].filter(Boolean).join(' ');
  }

  labelAction(a: string): string {
    const map: Record<string, string> = {
      Cree: 'Créé', Modifie: 'Modifié', Desactive: 'Désactivé',
      Reactive: 'Réactivé', Supprime: 'Supprimé',
    };
    return map[a] ?? a;
  }

  labelType(t: string): string {
    const map: Record<string, string> = {
      Appel: 'Appel', Sms: 'SMS', WhatsApp: 'WhatsApp',
      Professionnel: 'Professionnel', Prive: 'Privé',
    };
    return map[t] ?? t;
  }

  formatDate(d: string): string {
    return new Date(d).toLocaleString('fr-FR', { dateStyle: 'short', timeStyle: 'short' });
  }

  private flash(msg: string): void {
    this.saving.set(false);
    this.errorMsg.set(null);
    this.successMsg.set(msg);
    setTimeout(() => this.successMsg.set(null), 3500);
  }
}
