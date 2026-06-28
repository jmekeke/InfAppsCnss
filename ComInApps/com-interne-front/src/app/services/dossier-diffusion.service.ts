import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { DossierDiffusion } from '../models/dossier-diffusion.model';

/**
 * DossierDiffusionService — accès à l'API /api/dossiers-diffusion.
 *
 * Un dossier de diffusion est créé automatiquement lors du lancement ou
 * de la programmation d'un message. Il est en lecture seule côté Angular.
 *
 * Endpoints depuis DossiersDiffusionController.cs :
 *   get()              GET /api/dossiers-diffusion/{id}
 *   listerParMessage() GET /api/dossiers-diffusion/par-message/{messageId}
 */
@Injectable({ providedIn: 'root' })
export class DossierDiffusionService {
  private readonly http = inject(HttpClient);
  private readonly base = '/api/dossiers-diffusion';

  /** Récupère un dossier de diffusion avec ses lignes d'envoi. */
  get(id: string): Observable<DossierDiffusion> {
    return this.http.get<DossierDiffusion>(`${this.base}/${id}`);
  }

  /** Liste tous les dossiers de diffusion d'un message donné. */
  listerParMessage(messageId: string): Observable<DossierDiffusion[]> {
    return this.http.get<DossierDiffusion[]>(`${this.base}/par-message/${messageId}`);
  }
}
