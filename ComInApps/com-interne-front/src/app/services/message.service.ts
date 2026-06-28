import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import {
  MessageInterne,
  PagedResult,
  CreerMessageCommand,
  ModifierMessageCommand,
  ValiderMessageCommand,
  RejeterMessageCommand,
  DemanderCorrectionCommand,
  ProgrammerDiffusionCommand,
  LancerDiffusionCommand,
  DefinirDestinatairesCommand,
} from '../models/message.model';

/**
 * MessageService — accès à l'API /api/messages.
 *
 * Chaque méthode correspond à un endpoint du MessagesController.cs :
 *   lister()                  GET    /api/messages
 *   get()                     GET    /api/messages/{id}
 *   creer()                   POST   /api/messages
 *   modifier()                PUT    /api/messages/{id}
 *   supprimer()               DELETE /api/messages/{id}
 *   soumettre()               POST   /api/messages/{id}/soumettre
 *   valider()                 POST   /api/messages/{id}/valider
 *   rejeter()                 POST   /api/messages/{id}/rejeter
 *   programmer()              POST   /api/messages/{id}/programmer
 *   lancerDiffusion()         POST   /api/messages/{id}/lancer-diffusion
 *   definirDestinataires()    POST   /api/messages/{id}/destinataires
 *   ajouterPieceJointe()      POST   /api/messages/{id}/pieces-jointes
 *   supprimerPieceJointe()    DELETE /api/messages/{id}/pieces-jointes/{pjId}
 *
 * PROXY : proxy.conf.json redirige /api/* → ComInterne.Api (port 5100) en dev.
 */
@Injectable({ providedIn: 'root' })
export class MessageService {
  private readonly http = inject(HttpClient);
  private readonly base = '/api/messages';

  // ─── Queries ─────────────────────────────────────────────────────────────

  /** Liste les messages avec pagination et recherche optionnelle. */
  lister(page = 1, pageSize = 20, search?: string): Observable<PagedResult<MessageInterne>> {
    let params = new HttpParams()
      .set('page', page)
      .set('pageSize', pageSize);
    if (search) params = params.set('search', search);
    return this.http.get<PagedResult<MessageInterne>>(this.base, { params });
  }

  /** Récupère un message complet par son identifiant (GUID). */
  get(id: string): Observable<MessageInterne> {
    return this.http.get<MessageInterne>(`${this.base}/${id}`);
  }

  // ─── Commandes ────────────────────────────────────────────────────────────

  /** Crée un nouveau message en statut Brouillon. */
  creer(cmd: CreerMessageCommand): Observable<{ id: string }> {
    return this.http.post<{ id: string }>(this.base, cmd);
  }

  /** Modifie le contenu d'un brouillon (objet, corps, canaux). */
  modifier(id: string, cmd: ModifierMessageCommand): Observable<void> {
    return this.http.put<void>(`${this.base}/${id}`, cmd);
  }

  /** Supprime définitivement un message brouillon. */
  supprimer(id: string): Observable<void> {
    return this.http.delete<void>(`${this.base}/${id}`);
  }

  /** Soumet le message à validation → statut EnAttenteValidation. */
  soumettre(id: string): Observable<void> {
    return this.http.post<void>(`${this.base}/${id}/soumettre`, {});
  }

  /** Valide le message → statut Valide. */
  valider(id: string, cmd: ValiderMessageCommand): Observable<void> {
    return this.http.post<void>(`${this.base}/${id}/valider`, cmd);
  }

  /** Rejette le message avec motif → statut Rejete. */
  rejeter(id: string, cmd: RejeterMessageCommand): Observable<void> {
    return this.http.post<void>(`${this.base}/${id}/rejeter`, cmd);
  }

  /** Renvoie le message en correction à l'auteur → retour au statut Brouillon. */
  demanderCorrection(id: string, cmd: DemanderCorrectionCommand): Observable<void> {
    return this.http.post<void>(`${this.base}/${id}/demander-correction`, cmd);
  }

  /** Programme la diffusion à une date future → statut Programme. */
  programmer(id: string, cmd: ProgrammerDiffusionCommand): Observable<void> {
    return this.http.post<void>(`${this.base}/${id}/programmer`, cmd);
  }

  /** Lance la diffusion immédiatement → statut Diffuse. */
  lancerDiffusion(id: string, cmd: LancerDiffusionCommand): Observable<void> {
    return this.http.post<void>(`${this.base}/${id}/lancer-diffusion`, cmd);
  }

  /** Définit les destinataires cibles du message (multi-types). */
  definirDestinataires(id: string, cmd: DefinirDestinatairesCommand): Observable<void> {
    return this.http.post<void>(`${this.base}/${id}/destinataires`, cmd);
  }

  /** Ajoute une pièce jointe (upload multipart/form-data). */
  ajouterPieceJointe(id: string, file: File): Observable<void> {
    const form = new FormData();
    form.append('file', file, file.name);
    return this.http.post<void>(`${this.base}/${id}/pieces-jointes`, form);
  }

  /** Supprime une pièce jointe par son identifiant. */
  supprimerPieceJointe(messageId: string, pieceJointeId: string): Observable<void> {
    return this.http.delete<void>(`${this.base}/${messageId}/pieces-jointes/${pieceJointeId}`);
  }
}
