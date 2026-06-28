// ── Modèles du domaine DossierDiffusion ──────────────────────────────────────────
//
// Reflète :
//   Domain/Aggregats/DossierDiffusion.cs
//   Domain/Enums/StatutEnvoi.cs
//
// Un DossierDiffusion est créé automatiquement lors du lancement/programmation
// d'un message. Il contient une LigneEnvoi par destinataire du groupe ciblé.

/** Statut d'envoi individuel — reflète StatutEnvoi.cs. */
export type StatutEnvoi = 'EnAttente' | 'Envoye' | 'Echoue' | 'Annule';

/** Ligne d'envoi : un destinataire, un statut. */
export interface LigneEnvoi {
  id: string;
  destinataireNom: string;
  destinataireEmail?: string;
  /** Canal utilisé pour cet envoi. */
  canal: string;
  statut: StatutEnvoi;
  /** Date d'envoi effectif (si Envoye). */
  dateEnvoi?: string;
  /** Message d'erreur technique (si Echoue). */
  messageErreur?: string;
}

/** Dossier de diffusion complet avec ses lignes — GET /api/dossiers-diffusion/{id}. */
export interface DossierDiffusion {
  id: string;
  messageId: string;
  groupeId: string;
  groupeNom: string;
  dateCreation: string;
  nombreTotal: number;
  nombreEnvoyes: number;
  nombreEchecs: number;
  lignesEnvoi: LigneEnvoi[];
}
