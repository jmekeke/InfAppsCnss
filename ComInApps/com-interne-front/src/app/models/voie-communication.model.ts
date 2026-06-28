// ── Enums (miroir du domaine .NET) ────────────────────────────────────────────

export type TypeVoieTelephone = 'Appel' | 'Sms' | 'WhatsApp';
export type TypeVoieEmail     = 'Professionnel' | 'Prive';
export type CanalVoie         = 'Telephone' | 'Email';
export type ActionHistorique  = 'Cree' | 'Modifie' | 'Desactive' | 'Reactive' | 'Supprime';

export const TYPES_TELEPHONE: TypeVoieTelephone[] = ['Appel', 'Sms', 'WhatsApp'];
export const TYPES_EMAIL:     TypeVoieEmail[]     = ['Professionnel', 'Prive'];

// ── Réponses API ─────────────────────────────────────────────────────────────

export interface VoieTelephoneDto {
  type:             TypeVoieTelephone;
  numero:           string;
  estActif:         boolean;
  dateModification: string;
}

export interface VoieEmailDto {
  type:             TypeVoieEmail;
  adresse:          string;
  estActif:         boolean;
  dateModification: string;
}

export interface HistoriqueVoieDto {
  canal:      CanalVoie;
  typeVoie:   string;
  valeur:     string;
  estActif:   boolean;
  action:     ActionHistorique;
  modifiePar: string;
  dateAction: string;
}

export interface VoieCommunicationDto {
  id:         string;
  agentIdRh:  number;
  matricule:  string;
  telephones: VoieTelephoneDto[];
  emails:     VoieEmailDto[];
  historique: HistoriqueVoieDto[];
}

// ── Requêtes PUT ─────────────────────────────────────────────────────────────

export interface VoieTelephoneRequest {
  type:    TypeVoieTelephone;
  numero?: string;
}

export interface VoieEmailRequest {
  type:     TypeVoieEmail;
  adresse?: string;
}

export interface MettreAJourVoieCommunicationRequest {
  matricule:  string;
  telephones: VoieTelephoneRequest[];
  emails:     VoieEmailRequest[];
}
