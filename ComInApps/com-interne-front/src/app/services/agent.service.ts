import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Agent, RechercheAgentParams, ResumeVoie } from '../models/agent.model';

/**
 * AgentService — accès à l'API /api/agents.
 *
 * Les agents proviennent de la base RH SQL Server (lecture seule).
 *
 * Endpoints depuis AgentsController.cs :
 *   lister()     GET /api/agents
 *   get()        GET /api/agents/{id}
 *   rechercher() GET /api/agents/recherche?nom=&entiteLibelle=
 */
@Injectable({ providedIn: 'root' })
export class AgentService {
  private readonly http = inject(HttpClient);
  private readonly base = '/api/agents';

  /** Récupère la liste complète des agents RH. */
  lister(): Observable<Agent[]> {
    return this.http.get<Agent[]>(this.base);
  }

  /** Récupère un agent par son identifiant RH (int). */
  get(id: number): Observable<Agent> {
    return this.http.get<Agent>(`${this.base}/${id}`);
  }

  /** Recherche des agents par nom et/ou entité (paramètres optionnels combinés en AND). */
  rechercher(params: RechercheAgentParams): Observable<Agent[]> {
    let httpParams = new HttpParams();
    if (params.nom)           httpParams = httpParams.set('nom', params.nom);
    if (params.entiteLibelle) httpParams = httpParams.set('entiteLibelle', params.entiteLibelle);
    return this.http.get<Agent[]>(`${this.base}/recherche`, { params: httpParams });
  }

  /**
   * Retourne le résumé de contact (téléphone actif + e-mail actif) pour tous les
   * agents ayant une voie de communication enregistrée.
   * GET /api/agents/voies-resume
   */
  getVoiesResume(): Observable<ResumeVoie[]> {
    return this.http.get<ResumeVoie[]>(`${this.base}/voies-resume`);
  }
}
