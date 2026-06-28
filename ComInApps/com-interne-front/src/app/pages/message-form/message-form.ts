import { Component, inject, OnInit, signal } from '@angular/core';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { MessageService } from '../../services/message.service';
import { TypeCanal } from '../../models/message.model';

/**
 * Page Formulaire de message — création (POST) ou modification (PUT).
 *
 * MODE CRÉATION  : path = /messages/nouveau     → id absent dans la route
 * MODE ÉDITION   : path = /messages/:id/modifier → id présent dans la route
 *
 * Champs :
 *   objet             — string, requis
 *   corps             — string, requis
 *   estInstitutionnel — boolean (checkbox)
 *   canaux            — au moins 1 canal requis (checkboxes multiples)
 *
 * POURQUOI ReactiveFormsModule ici ?
 *   Formulaire avec validation complexe (groupe de checkboxes canaux).
 *   ReactiveFormsModule offre un contrôle programmatique précis.
 *   Pour les formulaires simples (login), on utilise template-driven.
 */
@Component({
  selector: 'app-message-form',
  imports: [RouterLink, ReactiveFormsModule],
  templateUrl: './message-form.html',
  styleUrl: './message-form.css',
})
export class MessageFormComponent implements OnInit {
  private readonly route          = inject(ActivatedRoute);
  private readonly router         = inject(Router);
  private readonly fb             = inject(FormBuilder);
  private readonly messageService = inject(MessageService);

  /** True si on est en mode édition (id présent dans la route). */
  readonly isEdit  = signal(false);
  readonly loading = signal(false);
  readonly saving  = signal(false);
  readonly error   = signal<string | null>(null);

  /** ID du message en édition (null si création). */
  messageId: string | null = null;

  /** Canaux disponibles (reflète TypeCanal). */
  readonly tousLesCanaux: TypeCanal[] = ['Email', 'Sms', 'WhatsApp', 'CanalInterne'];

  /** Libellés affichage pour les canaux. */
  readonly canalLabels: Record<TypeCanal, string> = {
    Email: '📧 Email',
    Sms: '📱 SMS',
    WhatsApp: '💬 WhatsApp',
    CanalInterne: '🏢 Canal interne',
  };

  /** Reactive form. */
  form!: FormGroup;

  ngOnInit(): void {
    this.form = this.fb.group({
      objet:             ['', [Validators.required, Validators.minLength(3)]],
      corps:             ['', [Validators.required, Validators.minLength(10)]],
      estInstitutionnel: [false],
      canaux:            [['Email']], // au moins Email par défaut
    });

    // Déterminer le mode (création ou édition)
    // La route /messages/nouveau n'a pas d'id, la route /messages/:id/modifier en a un.
    const id = this.route.snapshot.paramMap.get('id');
    if (id && id !== 'nouveau') {
      this.isEdit.set(true);
      this.messageId = id;
      this.chargerMessage(id);
    }
  }

  /** Charge les données du message pour pré-remplir le formulaire. */
  chargerMessage(id: string): void {
    this.loading.set(true);
    this.messageService.get(id).subscribe({
      next: m => {
        this.form.patchValue({
          objet:             m.objet,
          corps:             m.corps,
          estInstitutionnel: m.estInstitutionnel,
          canaux:            m.canaux,
        });
        this.loading.set(false);
      },
      error: () => {
        this.error.set('Impossible de charger le message.');
        this.loading.set(false);
      },
    });
  }

  /** Gère la sélection des canaux (checkbox multiple). */
  toggleCanal(canal: TypeCanal): void {
    const current: TypeCanal[] = this.form.get('canaux')!.value ?? [];
    const idx = current.indexOf(canal);
    if (idx >= 0) {
      this.form.patchValue({ canaux: current.filter(c => c !== canal) });
    } else {
      this.form.patchValue({ canaux: [...current, canal] });
    }
  }

  isCanalSelected(canal: TypeCanal): boolean {
    return (this.form.get('canaux')!.value ?? []).includes(canal);
  }

  /** Soumet le formulaire. */
  onSubmit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const canaux: TypeCanal[] = this.form.value.canaux ?? [];
    if (canaux.length === 0) {
      this.error.set('Veuillez sélectionner au moins un canal de diffusion.');
      return;
    }

    this.saving.set(true);
    this.error.set(null);

    const payload = {
      objet:             this.form.value.objet,
      corps:             this.form.value.corps,
      estInstitutionnel: this.form.value.estInstitutionnel,
      canaux,
    };

    if (this.isEdit() && this.messageId) {
      // Mode édition → PUT /api/messages/{id}
      this.messageService.modifier(this.messageId, payload).subscribe({
        next: () => this.router.navigate(['/messages', this.messageId]),
        error: () => {
          this.error.set('Erreur lors de la modification.');
          this.saving.set(false);
        },
      });
    } else {
      // Mode création → POST /api/messages
      this.messageService.creer(payload).subscribe({
        next: res => this.router.navigate(['/messages', res.id]),
        error: () => {
          this.error.set('Erreur lors de la création.');
          this.saving.set(false);
        },
      });
    }
  }

  /** Raccourci pour accéder aux contrôles du formulaire dans le template. */
  get f() { return this.form.controls; }

  /** True si un champ est invalide et a été touché. */
  isInvalid(name: string): boolean {
    const ctrl = this.form.get(name);
    return !!(ctrl && ctrl.invalid && ctrl.touched);
  }
}
