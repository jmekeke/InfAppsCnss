import { Routes } from '@angular/router';
import { authGuard } from './guards/auth.guard';
import { roleGuard } from './guards/role.guard';

/**
 * Table de routage de l'application Communication Interne CNSS.
 *
 * PATTERN :
 *   - Routes publiques (login, forbidden) : AUCUN guard.
 *   - Routes protégées : canActivate: [authGuard]  → doit être connecté.
 *   - Routes Admin/Manager : canActivate: [authGuard, roleGuard]
 *     avec data: { roles: ['Admin', 'Manager'] }
 *
 * LAZY LOADING :
 *   Chaque page est chargée à la demande (loadComponent) pour réduire
 *   le bundle initial et accélérer le premier affichage.
 */
export const routes: Routes = [

  // ── Redirection racine ────────────────────────────────────────────────────
  {
    path: '',
    redirectTo: 'dashboard',
    pathMatch: 'full',
  },

  // ── Routes publiques ─────────────────────────────────────────────────────
  {
    path: 'login',
    loadComponent: () => import('./pages/login/login').then(m => m.LoginComponent),
  },
  {
    // Affiché quand roleGuard bloque (authentifié mais rôle insuffisant)
    path: 'forbidden',
    loadComponent: () => import('./pages/forbidden/forbidden').then(m => m.ForbiddenComponent),
  },

  // ── Routes protégées (tout utilisateur authentifié) ───────────────────────
  {
    path: 'dashboard',
    canActivate: [authGuard],
    loadComponent: () => import('./pages/dashboard/dashboard').then(m => m.DashboardComponent),
  },

  // ── Gestion des messages internes ─────────────────────────────────────────
  {
    // Liste des messages avec pagination et recherche
    path: 'messages',
    canActivate: [authGuard],
    loadComponent: () => import('./pages/messages/messages').then(m => m.MessagesComponent),
  },
  {
    // Créer un nouveau message (brouillon) — DOIT être avant messages/:id
    path: 'messages/nouveau',
    canActivate: [authGuard],
    loadComponent: () => import('./pages/message-form/message-form').then(m => m.MessageFormComponent),
  },
  {
    // Modifier un message brouillon existant — DOIT être avant messages/:id
    path: 'messages/:id/modifier',
    canActivate: [authGuard],
    loadComponent: () => import('./pages/message-form/message-form').then(m => m.MessageFormComponent),
  },
  {
    // Détail d'un message : statut, workflow, pièces jointes, dossiers de diffusion
    path: 'messages/:id',
    canActivate: [authGuard],
    loadComponent: () => import('./pages/message-detail/message-detail').then(m => m.MessageDetailComponent),
  },

  // ── Groupes de diffusion ──────────────────────────────────────────────────
  {
    // Liste des groupes de diffusion
    path: 'groupes',
    canActivate: [authGuard],
    loadComponent: () => import('./pages/groupes/groupes').then(m => m.GroupesComponent),
  },
  {
    // Détail d'un groupe : membres, actions d'ajout/retrait
    path: 'groupes/:id',
    canActivate: [authGuard],
    loadComponent: () => import('./pages/groupe-detail/groupe-detail').then(m => m.GroupeDetailComponent),
  },

  // ── Dossiers de diffusion (lecture seule) ────────────────────────────────
  {
    // Dossiers d'un message spécifique (statuts d'envoi par destinataire)
    path: 'dossiers/:messageId',
    canActivate: [authGuard],
    loadComponent: () => import('./pages/dossiers-diffusion/dossiers-diffusion').then(m => m.DossiersDiffusionComponent),
  },

  // ── Membres / Agents RH ───────────────────────────────────────────────────
  {
    path: 'membres',
    canActivate: [authGuard],
    loadComponent: () => import('./pages/membres/membres').then(m => m.MembresComponent),
    children: [
      {
        path: '',
        redirectTo: 'liste',
        pathMatch: 'full',
      },
      {
        // Liste complète — GET /api/agents
        path: 'liste',
        loadComponent: () => import('./pages/membres/liste/membres-liste').then(m => m.MembreListeComponent),
      },
      {
        // Recherche par nom / entité — GET /api/agents/recherche
        path: 'recherche',
        loadComponent: () => import('./pages/membres/recherche/membres-recherche').then(m => m.MembreRechercheComponent),
      },
    ],
  },
  {
    // Fiche détaillée — GET /api/agents/:id (hors shell sous-nav)
    path: 'membres/:id',
    canActivate: [authGuard],
    loadComponent: () => import('./pages/membres/detail/membre-detail').then(m => m.MembreDetailComponent),
  },
  {
    // Gestion des voies de communication (téléphone + e-mail) d'un agent
    path: 'membres/:id/voie-communication',
    canActivate: [authGuard],
    loadComponent: () =>
      import('./pages/membres/voie-communication/voie-communication')
        .then(m => m.VoieCommunicationComponent),
  },

  // ── Fallback ─────────────────────────────────────────────────────────────
  {
    path: '**',
    redirectTo: 'dashboard',
  },
];
