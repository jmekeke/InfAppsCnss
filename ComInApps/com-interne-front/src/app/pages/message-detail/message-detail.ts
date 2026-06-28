import { Component, inject, OnInit, signal, computed } from '@angular/core';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { MessageService } from '../../services/message.service';
import { GroupeDiffusionService } from '../../services/groupe-diffusion.service';
import { AgentService } from '../../services/agent.service';
import { MessageInterne, StatutMessage, DestinataireCibleCmd, TypeDestinataire } from '../../models/message.model';
import { GroupeDiffusionSummary } from '../../models/groupe-diffusion.model';
import { Agent } from '../../models/agent.model';

/**
 * Page DÃ©tail d'un message â€” GET /api/messages/{id}.
 *
 * Affiche toutes les informations du message et expose les actions
 * disponibles selon le statut du cycle de vie :
 *
 *   Brouillon           â†’ Soumettre | Modifier | Supprimer
 *   EnAttenteValidation â†’ Valider | Rejeter
 *   Valide              â†’ Programmer | Lancer diffusion | DÃ©finir destinataires
 *   Programme           â†’ Lancer diffusion
 *   Diffuse             â†’ Voir dossiers de diffusion
 *
 * Les modales de confirmation sont gÃ©rÃ©es localement avec des Signals.
 */
@Component({
  selector: 'app-message-detail',
  imports: [RouterLink, FormsModule],
  templateUrl: './message-detail.html',
  styleUrl: './message-detail.css',
})
export class MessageDetailComponent implements OnInit {
  private readonly route          = inject(ActivatedRoute);
  private readonly router         = inject(Router);
  private readonly messageService = inject(MessageService);
  private readonly groupeService  = inject(GroupeDiffusionService);
  private readonly agentService   = inject(AgentService);

  readonly loading = signal(true);
  readonly error   = signal<string | null>(null);
  readonly message = signal<MessageInterne | null>(null);

  readonly groupes = signal<GroupeDiffusionSummary[]>([]);
  readonly agents  = signal<Agent[]>([]);

  readonly showModalSoumettre       = signal(false);
  readonly showModalValider         = signal(false);
  readonly showModalRejeter         = signal(false);
  readonly showModalDemandCorrection = signal(false);
  readonly showModalProgrammer      = signal(false);
  readonly showModalDiffuser        = signal(false);
  readonly showModalSupprimer       = signal(false);
  readonly showModalDestinataires   = signal(false);
  readonly showModalPJ              = signal(false);
  readonly showPreview              = signal(false);
  readonly actionLoading            = signal(false);
  readonly actionError              = signal<string | null>(null);

  commentaireValidation  = '';
  motifRejet             = '';
  commentaireCorrection  = '';
  dateProgrammee         = '';

  readonly destTab    = signal<TypeDestinataire>('AgentIndividu');
  readonly destCibles = signal<DestinataireCibleCmd[]>([]);
  rechercheAgent  = '';
  rechercheGroupe = '';

  readonly agentsFiltres = computed(() => {
    const t = this.rechercheAgent.toLowerCase();
    if (!t) return this.agents();
    return this.agents().filter(a =>
      [a.nom, a.postnom, a.prenom, a.matricule, a.entiteLibelle]
        .filter(Boolean).join(' ').toLowerCase().includes(t)
    );
  });

  readonly groupesFiltres = computed(() => {
    const t = this.rechercheGroupe.toLowerCase();
    if (!t) return this.groupes();
    return this.groupes().filter(g => g.nom.toLowerCase().includes(t));
  });

  readonly entitesDistinctes = computed(() =>
    [...new Set(this.agents().map(a => a.entiteLibelle ?? '').filter(e => !!e))].sort()
  );

  selectedFiles: File[] = [];
  readonly pjLoading = signal(false);
  readonly pjError   = signal<string | null>(null);

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id')!;
    this.chargerMessage(id);
    this.groupeService.lister(1, 200).subscribe({ next: r => this.groupes.set(r.items) });
    this.agentService.lister().subscribe({ next: a => this.agents.set(a) });
  }

  chargerMessage(id: string): void {
    this.loading.set(true);
    this.messageService.get(id).subscribe({
      next: m => {
        this.message.set(m);
        this.destCibles.set(m.destinataires.map(d => ({
          type: d.type, referenceId: d.referenceId, libelle: d.libelle
        })));
        this.loading.set(false);
      },
      error: err => {
        this.error.set(err?.status === 404 ? 'Message introuvable.' : 'Erreur chargement.');
        this.loading.set(false);
      },
    });
  }

  soumettre(): void {
    const id = this.message()!.id;
    this.actionLoading.set(true);
    this.messageService.soumettre(id).subscribe({
      next: () => { this.showModalSoumettre.set(false); this.chargerMessage(id); },
      error: () => { this.actionError.set('Erreur lors de la soumission.'); this.actionLoading.set(false); },
    });
  }

  valider(): void {
    const id = this.message()!.id;
    this.actionLoading.set(true);
    this.messageService.valider(id, { commentaire: this.commentaireValidation }).subscribe({
      next: () => { this.showModalValider.set(false); this.chargerMessage(id); },
      error: () => { this.actionError.set('Erreur lors de la validation.'); this.actionLoading.set(false); },
    });
  }

  rejeter(): void {
    if (!this.motifRejet.trim()) { this.actionError.set('Le motif est obligatoire.'); return; }
    const id = this.message()!.id;
    this.actionLoading.set(true);
    this.messageService.rejeter(id, { motif: this.motifRejet }).subscribe({
      next: () => { this.showModalRejeter.set(false); this.chargerMessage(id); },
      error: () => { this.actionError.set('Erreur lors du rejet.'); this.actionLoading.set(false); },
    });
  }

  demanderCorrection(): void {
    if (!this.commentaireCorrection.trim()) { this.actionError.set('Le commentaire est obligatoire.'); return; }
    const id = this.message()!.id;
    this.actionLoading.set(true);
    this.messageService.demanderCorrection(id, { commentaire: this.commentaireCorrection }).subscribe({
      next: () => { this.showModalDemandCorrection.set(false); this.commentaireCorrection = ''; this.chargerMessage(id); },
      error: () => { this.actionError.set('Erreur lors de la demande de correction.'); this.actionLoading.set(false); },
    });
  }

  programmer(): void {
    if (!this.dateProgrammee) { this.actionError.set('Veuillez sÃ©lectionner une date.'); return; }
    const id = this.message()!.id;
    this.actionLoading.set(true);
    const groupeIds = this.destCibles()
      .filter(d => d.type === 'GroupeDiffusion' && d.referenceId)
      .map(d => d.referenceId!);
    this.messageService.programmer(id, {
      dateProgrammee: new Date(this.dateProgrammee).toISOString(), groupeIds,
    }).subscribe({
      next: () => { this.showModalProgrammer.set(false); this.chargerMessage(id); },
      error: () => { this.actionError.set('Erreur lors de la programmation.'); this.actionLoading.set(false); },
    });
  }

  lancerDiffusion(): void {
    const id = this.message()!.id;
    this.actionLoading.set(true);
    const groupeIds = this.destCibles()
      .filter(d => d.type === 'GroupeDiffusion' && d.referenceId)
      .map(d => d.referenceId!);
    this.messageService.lancerDiffusion(id, { groupeIds }).subscribe({
      next: () => { this.showModalDiffuser.set(false); this.chargerMessage(id); },
      error: () => { this.actionError.set('Erreur lors du lancement.'); this.actionLoading.set(false); },
    });
  }

  definirDestinataires(): void {
    const id = this.message()!.id;
    this.actionLoading.set(true);
    this.messageService.definirDestinataires(id, { destinataires: this.destCibles() }).subscribe({
      next: () => { this.showModalDestinataires.set(false); this.chargerMessage(id); },
      error: () => { this.actionError.set('Erreur lors de la sauvegarde.'); this.actionLoading.set(false); },
    });
  }

  supprimer(): void {
    const id = this.message()!.id;
    this.actionLoading.set(true);
    this.messageService.supprimer(id).subscribe({
      next: () => this.router.navigate(['/messages']),
      error: () => { this.actionError.set('Erreur lors de la suppression.'); this.actionLoading.set(false); },
    });
  }

  // â”€â”€ Destinataires â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

  ajouterAgent(agent: Agent): void {
    const ref = String(agent.id);
    if (this.destCibles().some(d => d.type === 'AgentIndividu' && d.referenceId === ref)) return;
    const nom = [agent.nom, agent.postnom, agent.prenom].filter(Boolean).join(' ');
    this.destCibles.update(c => [...c, { type: 'AgentIndividu', referenceId: ref, libelle: nom }]);
  }

  ajouterGroupe(groupe: GroupeDiffusionSummary): void {
    if (this.destCibles().some(d => d.type === 'GroupeDiffusion' && d.referenceId === groupe.id)) return;
    this.destCibles.update(c => [...c, { type: 'GroupeDiffusion', referenceId: groupe.id, libelle: groupe.nom }]);
  }

  ajouterEntite(entite: string): void {
    if (this.destCibles().some(d => d.type === 'DirectionService' && d.referenceId === entite)) return;
    this.destCibles.update(c => [...c, { type: 'DirectionService', referenceId: entite, libelle: entite }]);
  }

  ajouterTousLesAgents(): void {
    if (this.destCibles().some(d => d.type === 'TousLesAgents')) return;
    this.destCibles.update(c => [...c, { type: 'TousLesAgents', referenceId: null, libelle: 'Tous les agents actifs' }]);
  }

  retirerDestinataire(index: number): void {
    this.destCibles.update(c => c.filter((_, i) => i !== index));
  }

  isAgentSelectionne(id: number): boolean {
    return this.destCibles().some(d => d.type === 'AgentIndividu' && d.referenceId === String(id));
  }

  isGroupeSelectionne(id: string): boolean {
    return this.destCibles().some(d => d.type === 'GroupeDiffusion' && d.referenceId === id);
  }

  isEntiteSelectionnee(entite: string): boolean {
    return this.destCibles().some(d => d.type === 'DirectionService' && d.referenceId === entite);
  }

  labelType(type: TypeDestinataire): string {
    const map: Record<TypeDestinataire, string> = {
      AgentIndividu: 'Agent', GroupeDiffusion: 'Groupe',
      DirectionService: 'EntitÃ©', TousLesAgents: 'Tous',
    };
    return map[type];
  }

  // â”€â”€ PiÃ¨ces jointes â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

  onFilesSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    if (input.files) this.selectedFiles = Array.from(input.files);
  }

  async uploaderFichiers(): Promise<void> {
    const id = this.message()!.id;
    if (!this.selectedFiles.length) return;
    this.pjLoading.set(true);
    this.pjError.set(null);
    for (const file of this.selectedFiles) {
      await new Promise<void>((resolve, reject) => {
        this.messageService.ajouterPieceJointe(id, file).subscribe({
          next: () => resolve(),
          error: () => reject(new Error(`Erreur upload ${file.name}`)),
        });
      }).catch((e: Error) => this.pjError.set(e.message));
    }
    this.selectedFiles = [];
    this.pjLoading.set(false);
    this.showModalPJ.set(false);
    this.chargerMessage(id);
  }

  supprimerPJ(pieceJointeId: string): void {
    const id = this.message()!.id;
    this.messageService.supprimerPieceJointe(id, pieceJointeId).subscribe({
      next: () => this.chargerMessage(id),
    });
  }

  // â”€â”€ Helpers â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

  fermerModales(): void {
    this.showModalSoumettre.set(false); this.showModalValider.set(false);
    this.showModalRejeter.set(false); this.showModalDemandCorrection.set(false);
    this.showModalProgrammer.set(false);
    this.showModalDiffuser.set(false); this.showModalSupprimer.set(false);
    this.showModalDestinataires.set(false); this.showModalPJ.set(false);
    this.actionError.set(null); this.actionLoading.set(false);
  }

  badgeClass(statut: StatutMessage): string {
    const map: Record<StatutMessage, string> = {
      Brouillon: 'badge badge-brouillon', EnAttenteValidation: 'badge badge-attente',
      Valide: 'badge badge-valide', Rejete: 'badge badge-rejete',
      Programme: 'badge badge-programme', Diffuse: 'badge badge-diffuse',
    };
    return map[statut];
  }

  statutLabel(statut: StatutMessage): string {
    const map: Record<StatutMessage, string> = {
      Brouillon: 'Brouillon', EnAttenteValidation: 'En attente de validation',
      Valide: 'ValidÃ©', Rejete: 'RejetÃ©', Programme: 'ProgrammÃ©', Diffuse: 'DiffusÃ©',
    };
    return map[statut];
  }

  formatDate(d?: string | null): string {
    if (!d) return 'â€”';
    return new Date(d).toLocaleDateString('fr-CD', {
      day: '2-digit', month: 'short', year: 'numeric', hour: '2-digit', minute: '2-digit'
    });
  }

  formatSize(bytes: number): string {
    if (bytes < 1024) return `${bytes} o`;
    if (bytes < 1024 * 1024) return `${(bytes / 1024).toFixed(1)} Ko`;
    return `${(bytes / 1024 / 1024).toFixed(1)} Mo`;
  }

  nomComplet(agent: Agent): string {
    return [agent.nom, agent.postnom, agent.prenom].filter(Boolean).join(' ') || `Agent #${agent.id}`;
  }
}

