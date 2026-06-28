// ── Environnement de développement ───────────────────────────────────────────────
//
// Ce fichier est actif lors de `ng serve` (build de développement).
// Il est automatiquement remplacé par environment.production.ts lors du build
// --configuration=production (voir angular.json → fileReplacements).
//
// PORTS — d'après Properties/launchSettings.json des projets .NET :
//
//   RubacCore  (serveur d'identité / tokens)
//     HTTP  → http://localhost:5262
//
//   ComInterne.Api  (API ressource — CommunicationInterne)
//     HTTP  → http://localhost:5100
//     Note : les appels /api/* sont proxifiés via proxy.conf.json lors de ng serve.
//
// POURQUOI HTTP et non HTTPS ?
//   `dotnet watch run` sans profil démarre les APIs en HTTP uniquement.
//   Les trois couches (Angular, RubacCore, ComInterne.Api) doivent utiliser
//   le même schéma+host+port car le claim `iss` du JWT doit correspondre
//   à l'URL réelle du serveur — toute divergence entraîne un rejet du token.
export const environment = {
  production: false,

  // URL de base de RubacCore — proxifié via /auth/* → localhost:5262
  authServerUrl: '/auth',

  // URL de base de ComInterne.Api — proxifié via /api/* → localhost:5500
  apiBaseUrl: '/api',
};
