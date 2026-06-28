import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import {
  VoieCommunicationDto,
  MettreAJourVoieCommunicationRequest,
  TypeVoieTelephone,
  TypeVoieEmail,
  CanalVoie,
  HistoriqueVoieDto,
} from '../models/voie-communication.model';

/**
 * VoieCommunicationService — gestion des coordonnées de contact d'un agent.
 *
 * Endpoints :
 *   GET    /api/agents/{id}/voie-communication
 *   PUT    /api/agents/{id}/voie-communication
 *   PATCH  /api/agents/{id}/voie-communication/telephones/{type}/desactiver
 *   PATCH  /api/agents/{id}/voie-communication/telephones/{type}/reactiver
 *   DELETE /api/agents/{id}/voie-communication/telephones/{type}
 *   PATCH  /api/agents/{id}/voie-communication/emails/{type}/desactiver
 *   PATCH  /api/agents/{id}/voie-communication/emails/{type}/reactiver
 *   DELETE /api/agents/{id}/voie-communication/emails/{type}
 *   GET    /api/agents/{id}/voie-communication/historique[?canal=]
 */
@Injectable({ providedIn: 'root' })
export class VoieCommunicationService {
  private readonly http = inject(HttpClient);

  private base(agentId: number): string {
    return `/api/agents/${agentId}/voie-communication`;
  }

  // ── Lecture ──────────────────────────────────────────────────────────────

  get(agentId: number): Observable<VoieCommunicationDto> {
    return this.http.get<VoieCommunicationDto>(this.base(agentId));
  }

  getHistorique(agentId: number, canal?: CanalVoie): Observable<HistoriqueVoieDto[]> {
    let params = new HttpParams();
    if (canal) params = params.set('canal', canal);
    return this.http.get<HistoriqueVoieDto[]>(`${this.base(agentId)}/historique`, { params });
  }

  // ── Mise à jour globale ──────────────────────────────────────────────────

  mettreAJour(agentId: number, body: MettreAJourVoieCommunicationRequest): Observable<void> {
    return this.http.put<void>(this.base(agentId), body);
  }

  // ── Téléphone ────────────────────────────────────────────────────────────

  desactiverTelephone(agentId: number, type: TypeVoieTelephone): Observable<void> {
    return this.http.patch<void>(`${this.base(agentId)}/telephones/${type}/desactiver`, null);
  }

  reactiverTelephone(agentId: number, type: TypeVoieTelephone): Observable<void> {
    return this.http.patch<void>(`${this.base(agentId)}/telephones/${type}/reactiver`, null);
  }

  supprimerTelephone(agentId: number, type: TypeVoieTelephone): Observable<void> {
    return this.http.delete<void>(`${this.base(agentId)}/telephones/${type}`);
  }

  // ── E-mail ───────────────────────────────────────────────────────────────

  desactiverEmail(agentId: number, type: TypeVoieEmail): Observable<void> {
    return this.http.patch<void>(`${this.base(agentId)}/emails/${type}/desactiver`, null);
  }

  reactiverEmail(agentId: number, type: TypeVoieEmail): Observable<void> {
    return this.http.patch<void>(`${this.base(agentId)}/emails/${type}/reactiver`, null);
  }

  supprimerEmail(agentId: number, type: TypeVoieEmail): Observable<void> {
    return this.http.delete<void>(`${this.base(agentId)}/emails/${type}`);
  }
}
