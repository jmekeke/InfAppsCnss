// ── Modèles du domaine GroupeDiffusion ───────────────────────────────────────────
//
// Reflète :
//   Domain/Aggregats/GroupeDiffusion.cs
//   Domain/Enums/TypeGroupe.cs

/** Mode de constitution d'un groupe — reflète TypeGroupe.cs. */
export type TypeGroupe = 'Manuel' | 'Dynamique';

/** Membre d'un groupe de diffusion. */
export interface MembreGroupe {
  /** GUID interne du membre (= AgentId déterministe pour les agents RH). */
  id: string;
  /** Identifiant RH (int) — présent si membre ajouté via Base RH, null sinon. */
  agentIdRh?: number | null;
  /** Date d'ajout au groupe. */
  dateAjout: string;
}

/** Groupe de diffusion complet avec ses membres — GET /api/groupes-diffusion/{id}. */
export interface GroupeDiffusion {
  id: string;
  nom: string;
  description?: string;
  type: TypeGroupe;
  dateCreation: string;
  critereType?: string | null;
  critereValeur?: string | null;
  membres: MembreGroupe[];
}

/** Résumé de groupe pour la liste — GET /api/groupes-diffusion. */
export interface GroupeDiffusionSummary {
  id: string;
  nom: string;
  description?: string;
  type: TypeGroupe;
  nombreMembres: number;
  dateCreation: string;
  estActif: boolean;
  critereType?: string | null;
  critereValeur?: string | null;
}

/** Résultat paginé des groupes. */
export interface PagedGroupes {
  items: GroupeDiffusionSummary[];
  totalCount: number;
  page: number;
  pageSize: number;
}

/** Membre enrichi avec données RH complètes — GET /api/groupes-diffusion/membres-enrichis. */
export interface MembreEnrichi extends MembreGroupe {
  prenom?: string;
  poste?: string;
  service?: string;
  direction?: string;
  telephone?: string;
}

// ─── Commandes ─────────────────────────────────────────────────────────────────

/** POST /api/groupes-diffusion — Créer un groupe. */
export interface CreerGroupeDiffusionCommand {
  nom: string;
  description?: string;
  typeGroupe: TypeGroupe;
  critereType?: string | null;
  critereValeur?: string | null;
}

/** POST /api/groupes-diffusion/{id}/membres — Ajouter un membre manuellement. */
export interface AjouterMembreGroupeCommand {
  nom: string;
  email?: string;
}

/** PUT /api/groupes-diffusion/{id} — Modifier un groupe existant. */
export interface ModifierGroupeDiffusionCommand {
  nom: string;
  description?: string;
  typeGroupe: TypeGroupe;
}
