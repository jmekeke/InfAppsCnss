// ── Modèles d'authentification ───────────────────────────────────────────────────
//
// Ces interfaces décrivent :
//   1. AuthUser       — l'utilisateur connecté extrait du JWT (état en mémoire)
//   2. TokenPayload   — les claims du JWT décodé côté client (base64)
//   3. TokenResponse  — la réponse HTTP de RubacCore (/connect/token)

/** Utilisateur authentifié — état local après décodage du JWT. */
export interface AuthUser {
  /** Identifiant unique (sub claim du JWT, GUID). */
  sub: string;
  /** Nom complet de l'utilisateur. */
  name: string;
  /** Adresse e-mail. */
  email: string;
  /** Liste des rôles accordés. */
  roles: string[];
  /** Raccourcis calculés à la lecture du token. */
  isAdmin: boolean;
  isManager: boolean;
  isConsultant: boolean;
}

/** Claims attendus à l'intérieur du JWT (format RubacCore). */
export interface TokenPayload {
  sub: string;
  name: string;
  email: string;
  /** Peut être un seul rôle ou un tableau selon la configuration RubacCore. */
  role: string | string[];
  /** Tâches/permissions granulaires — claim alternatif selon la config RubacCore. */
  permission?: string | string[];
  permissions?: string | string[];
  /** Timestamp d'expiration (Unix epoch en secondes). */
  exp: number;
  /** Toutes les autres propriétés du payload (claims dynamiques). */
  [key: string]: unknown;
}

/** Réponse de l'endpoint POST /connect/token de RubacCore. */
export interface TokenResponse {
  access_token: string;
  refresh_token: string;
  id_token?: string;
  token_type: string;
  /** Durée de validité en secondes. */
  expires_in: number;
}
