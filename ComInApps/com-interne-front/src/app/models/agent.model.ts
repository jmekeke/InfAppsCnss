// ── Modèle Agent RH ──────────────────────────────────────────────────────────────
//
// Reflète la réponse de GET /api/agents (source : base RH SQL Server).
// L'identifiant est un int (clé primaire SQL Server), pas un GUID.

/** Agent CNSS extrait de la base RH. */
export interface Agent {
  /** Identifiant RH (int — clé primaire SQL Server). */
  id: number;
  matricule?: string;
  nom?: string;
  postnom?: string;
  prenom?: string;
  emailProfessionnel?: string;
  emailPersonnel?: string;
  telephone?: string;
  entiteLibelle?: string;
  gradeLibelle?: string;
  fonctionLibelle?: string;
  categorie: string;
  etatCivil: string;
  sexe: string;
  dateEngagement?: string;
  dateNaissance?: string;
}

/** Paramètres de recherche d'agents. */
export interface RechercheAgentParams {
  nom?: string;
  entiteLibelle?: string;
}

/**
 * Résumé de contact pour un agent, issu des voies de communication enregistrées
 * dans PostgreSQL (source prioritaire sur les données RH SQL Server).
 * Retourné par GET /api/agents/voies-resume.
 */
export interface ResumeVoie {
  agentIdRh: number;
  telephoneActif?: string | null;
  emailProfActif?: string | null;
}
