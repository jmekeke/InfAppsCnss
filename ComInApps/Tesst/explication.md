src/
├── environments/
│   ├── environment.ts          ← RubacCore :5262 + ComInterne.Api :5100
│   └── environment.production.ts
├── app/
│   ├── app.config.ts           ← provideHttpClient + authInterceptor
│   ├── app.routes.ts           ← 11 routes lazy-loadées + guards
│   ├── app.ts / app-shell.html ← Shell + nav conditionnelle
│   ├── app.css                 ← Layout header + contenu
│   ├── guards/
│   │   ├── auth.guard.ts       ← "Êtes-vous connecté ?"
│   │   └── role.guard.ts       ← "Avez-vous le bon rôle ?"
│   ├── interceptors/
│   │   └── auth.interceptor.ts ← Bearer token + refresh 401
│   ├── models/                 ← 5 modèles typés (message, groupe, agent…)
│   ├── services/               ← 5 services (1 par contrôleur .NET)
│   └── pages/
│       ├── login/              ← Page de connexion RubacCore
│       ├── forbidden/          ← Accès refusé (roleGuard)
│       ├── dashboard/          ← Stats + derniers messages
│       ├── messages/           ← Liste paginée + recherche
│       ├── message-detail/     ← Cycle de vie complet (6 actions)
│       ├── message-form/       ← Créer / Modifier (ReactiveForm)
│       ├── groupes/            ← Liste + création rapide
│       ├── groupe-detail/      ← Membres, ajout RH / manuel, retrait
│       └── dossiers-diffusion/ ← Statuts d'envoi par destinataire
├── styles.css                  ← Thème CNSS + badges statuts
proxy.conf.json                 ← /api → http://localhost:5100
cd ComInApps/com-interne-front
ng serve
# → http://localhost:4200 (proxifié vers :5100)