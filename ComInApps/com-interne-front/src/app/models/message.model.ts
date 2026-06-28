// ── Modèles du domaine MessageInterne ────────────────────────────────────────────
//
// Ces types TypeScript reflètent le domaine C# :
//   Domain/Aggregats/MessageInterne.cs
//   Domain/Enums/StatutMessage.cs
//   Domain/Enums/TypeCanal.cs
//
// Convention : les noms de valeurs correspondent exactement aux enums C# pour
// éviter les conversions côté API (le backend sérialise les enums en string).

// ─── Enums ─────────────────────────────────────────────────────────────────────

/**
 * Cycle de vie complet d'un message interne — reflète StatutMessage.cs.
 *
 * Transitions autorisées :
 *   Brouillon → EnAttenteValidation (soumettre)
 *   EnAttenteValidation → Valide     (valider)
 *   EnAttenteValidation → Rejete     (rejeter)
 *   Valide → Programme              (programmer)
 *   Valide | Programme → Diffuse    (lancerDiffusion)
 */
export type StatutMessage =
  | 'Brouillon'           // 1 — en cours de rédaction, modifiable
  | 'EnAttenteValidation' // 2 — soumis, en attente d'un validateur
  | 'Valide'              // 3 — approuvé, prêt à être diffusé
  | 'Rejete'              // 4 — refusé avec motif obligatoire
  | 'Programme'           // 5 — planifié pour une diffusion future
  | 'Diffuse';            // 6 — envoyé aux destinataires (terminal)

/** Canaux de diffusion disponibles — reflète TypeCanal.cs. */
export type TypeCanal = 'Email' | 'Sms' | 'WhatsApp' | 'CanalInterne';

// ─── Entités ───────────────────────────────────────────────────────────────────

/** Pièce jointe attachée à un message. */
export interface PieceJointe {
  id: string;
  nomFichier: string;
  typeMime: string;
  tailleOctets: number;
  dateAjout: string;
}

/** Types de destinataires — reflète TypeDestinataire.cs */
export type TypeDestinataire = 'AgentIndividu' | 'GroupeDiffusion' | 'DirectionService' | 'TousLesAgents';

/** Destinataire cible d'un message. */
export interface DestinataireCible {
  id: string;
  type: TypeDestinataire;
  referenceId: string | null;
  libelle: string;
}

/** Commande pour définir les destinataires (POST). */
export interface DestinataireCibleCmd {
  type: TypeDestinataire;
  referenceId: string | null;
  libelle: string;
}

/** Représentation complète d'un message interne (lecture). */
export interface MessageInterne {
  id: string;
  objet: string;
  corps: string;
  estInstitutionnel: boolean;
  auteurId: string;
  auteurNom: string;
  statut: StatutMessage;
  canaux: TypeCanal[];
  dateCreation: string;
  dateValidation?: string;
  validateurId?: string;
  motiDeRejet?: string;
  commentaireRetour?: string;
  dateProgrammee?: string;
  dateDiffusion?: string;
  estArchive: boolean;
  piecesJointes: PieceJointe[];
  groupeIds: string[];
  destinataires: DestinataireCible[];
}

/** Résultat paginé — utilisé par GET /api/messages. */
export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
}

// ─── Commandes (corps des requêtes POST/PUT) ──────────────────────────────────

/** POST /api/messages — Créer un nouveau brouillon. */
export interface CreerMessageCommand {
  objet: string;
  corps: string;
  estInstitutionnel: boolean;
  canaux: TypeCanal[];
}

/** PUT /api/messages/{id} — Modifier un brouillon existant. */
export interface ModifierMessageCommand {
  objet: string;
  corps: string;
  estInstitutionnel: boolean;
  canaux: TypeCanal[];
}

/** POST /api/messages/{id}/valider — Approuver le message. */
export interface ValiderMessageCommand {
  commentaire?: string;
}

/** POST /api/messages/{id}/rejeter — Refuser avec motif obligatoire. */
export interface RejeterMessageCommand {
  motif: string;
}

/** POST /api/messages/{id}/demander-correction — Renvoyer en correction à l'auteur. */
export interface DemanderCorrectionCommand {
  commentaire: string;
}

/** POST /api/messages/{id}/programmer — Planifier l'envoi à une date future. */
export interface ProgrammerDiffusionCommand {
  /** Date/heure d'envoi planifiée (ISO 8601). */
  dateProgrammee: string;
  groupeIds: string[];
}

/** POST /api/messages/{id}/lancer-diffusion — Déclencher l'envoi immédiatement. */
export interface LancerDiffusionCommand {
  groupeIds: string[];
}

/** POST /api/messages/{id}/destinataires — Définir les destinataires cibles. */
export interface DefinirDestinatairesCommand {
  destinataires: DestinataireCibleCmd[];
}
