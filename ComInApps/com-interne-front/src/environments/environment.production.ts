// ── Environnement de production ──────────────────────────────────────────────────
//
// Ce fichier remplace automatiquement environment.ts lors du build :
//   ng build --configuration=production
//
// Modifier les URLs selon le déploiement cible (serveur CNSS, cloud, etc.).
export const environment = {
  production: true,

  // URL publique de RubacCore (serveur d'identité)
  authServerUrl: 'https://auth.cnss.dz',

  // URL publique de ComInterne.Api
  apiBaseUrl: 'https://com-interne.cnss.dz',
};
