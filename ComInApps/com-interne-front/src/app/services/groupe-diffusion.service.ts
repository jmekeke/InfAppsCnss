import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import {
  GroupeDiffusion,
  GroupeDiffusionSummary,
  PagedGroupes,
  CreerGroupeDiffusionCommand,
  ModifierGroupeDiffusionCommand,
  AjouterMembreGroupeCommand,
  MembreEnrichi,
} from '../models/groupe-diffusion.model';

/**
 * GroupeDiffusionService — accès à l'API /api/groupes-diffusion.
 *
 * Endpoints mappés depuis GroupesDiffusionController.cs :
 *   lister()              GET    /api/groupes-diffusion
 *   get()                 GET    /api/groupes-diffusion/{id}
 *   creer()               POST   /api/groupes-diffusion
 *   supprimer()           DELETE /api/groupes-diffusion/{id}
 *   ajouterMembre()       POST   /api/groupes-diffusion/{id}/membres
 *   retirerMembre()       DELETE /api/groupes-diffusion/{id}/membres/{agentId}
 *   ajouterMembreRh()     POST   /api/groupes-diffusion/{id}/membres/{agentIdRh}
 *   listerMembresEnrichis() GET  /api/groupes-diffusion/membres-enrichis
 */
@Injectable({ providedIn: 'root' })
export class GroupeDiffusionService {
  private readonly http = inject(HttpClient);
  private readonly base = '/api/groupes-diffusion';

  // ─── Queries ─────────────────────────────────────────────────────────────

  /** Liste les groupes avec pagination et recherche optionnelle. */
  lister(page = 1, pageSize = 20, search?: string): Observable<PagedGroupes> {
    let params = new HttpParams()
      .set('page', page)
      .set('pageSize', pageSize);
    if (search) params = params.set('search', search);
    return this.http.get<PagedGroupes>(this.base, { params });
  }

  /** Récupère un groupe avec sa liste de membres. */
  get(id: string): Observable<GroupeDiffusion> {
    return this.http.get<any>(`${this.base}/${id}`).pipe(
      map(raw => ({
        id: raw.id,
        nom: raw.nom,
        description: raw.description,
        type: raw.type ?? raw.typeGroupe,
        dateCreation: raw.dateCreation,
        critereType: raw.critereType ?? null,
        critereValeur: raw.critereValeur ?? null,
        membres: raw.membres ?? [],
      }) as GroupeDiffusion),
    );
  }

  /**
   * Récupère tous les groupes avec membres enrichis depuis la base RH.
   * Coûteux — à appeler uniquement sur la page de détail ou à la demande.
   */
  listerMembresEnrichis(): Observable<{ groupes: Array<{ id: string; nom: string; membres: MembreEnrichi[] }> }> {
    return this.http.get<{ groupes: Array<{ id: string; nom: string; membres: MembreEnrichi[] }> }>(
      `${this.base}/membres-enrichis`,
    );
  }

  // ─── Commandes ────────────────────────────────────────────────────────────

  /** Crée un nouveau groupe de diffusion. */
  creer(cmd: CreerGroupeDiffusionCommand): Observable<{ id: string }> {
    return this.http.post<{ id: string }>(this.base, cmd);
  }

  /** Modifie le nom, la description et le type d'un groupe existant. */
  modifier(id: string, cmd: ModifierGroupeDiffusionCommand): Observable<void> {
    return this.http.put<void>(`${this.base}/${id}`, cmd);
  }

  /**
   * Bascule l'état actif/inactif d'un groupe.
   * PATCH /api/groupes-diffusion/{id}/basculer-etat
   */
  basculerEtat(id: string): Observable<void> {
    return this.http.patch<void>(`${this.base}/${id}/basculer-etat`, {});
  }

  /** Supprime définitivement un groupe de diffusion. */
  supprimer(id: string): Observable<void> {
    return this.http.delete<void>(`${this.base}/${id}`);
  }

  // ─── Gestion des membres ──────────────────────────────────────────────────

  /** Ajoute un membre manuellement (sans lien RH). */
  ajouterMembre(groupeId: string, cmd: AjouterMembreGroupeCommand): Observable<void> {
    return this.http.post<void>(`${this.base}/${groupeId}/membres`, cmd);
  }

  /**
   * Ajoute un agent RH comme membre du groupe.
   * L'agent est identifié par son identifiant RH (int, base SQL Server).
   */
  ajouterMembreRh(groupeId: string, agentIdRh: number): Observable<void> {
    return this.http.post<void>(`${this.base}/${groupeId}/membres/${agentIdRh}`, {});
  }

  /** Retire un membre du groupe (par son GUID de membre). */
  retirerMembre(groupeId: string, agentId: string): Observable<void> {
    return this.http.delete<void>(`${this.base}/${groupeId}/membres/${agentId}`);
  }
}
