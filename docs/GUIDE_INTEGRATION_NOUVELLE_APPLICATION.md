# Guide d'intégration d'une nouvelle application — RubacCore

> **Public visé :** développeurs souhaitant brancher une nouvelle application (frontend SPA, API backend, service interne) sur RubacCore.  
> **Pré-requis :** RubacCore est déployé et accessible. Vous disposez d'un accès au code source pour modifier `OpenIddictSeedWorker.cs` et `Program.cs`.

---

## Sommaire

1. [Architecture et concepts fondamentaux](#1-architecture-et-concepts-fondamentaux)
2. [Les chemins d'authentification disponibles](#2-les-chemins-dauthentification-disponibles)
3. [Cas A — Intégrer un Frontend SPA](#3-cas-a--intégrer-un-frontend-spa)
4. [Cas B — Intégrer une API Backend (Resource Server)](#4-cas-b--intégrer-une-api-backend-resource-server)
5. [Cas C — Communication service-à-service](#5-cas-c--communication-service-à-service)
6. [Gestion des rôles multi-applications](#6-gestion-des-rôles-multi-applications)
7. [Checklist récapitulative par cas](#7-checklist-récapitulative-par-cas)
8. [Pièges fréquents et solutions](#8-pièges-fréquents-et-solutions)
9. [Fichiers de référence](#9-fichiers-de-référence)

---

## 1. Architecture et concepts fondamentaux

RubacCore est le **serveur d'autorité OAuth2 / OpenID Connect** basé sur [OpenIddict](https://documentation.openiddict.com/). Toute application délègue son authentification à RubacCore et reçoit un **JWT signé** en retour.

### Vue d'ensemble

```
┌─────────────────────┐        ┌──────────────────────┐
│   Frontend SPA      │ ─────▶ │      RubacCore       │  ← émet le JWT
│   (client public)   │ login  │  (OpenIddict / IdP)  │
└────────┬────────────┘        └──────────┬───────────┘
         │ JWT                             │
         ▼                                 │
┌─────────────────────┐                   │
│   API Backend       │ ◀─────────────────┘
│   (resource server) │   valide le JWT (audience + signature)
└─────────────────────┘
         │
         ▼ (optionnel)
┌─────────────────────┐
│   Autre API         │  ← appel service-à-service (client_credentials)
│   (resource server) │
└─────────────────────┘
```

### Concepts clés

| Concept | Description |
|---|---|
| **`client_id`** | Identifiant unique de l'application appelante (ex. `grh-frontend`). |
| **`scope`** | Permission d'accès à une ressource. Chaque API dispose de son scope (ex. `grh`, `dashboard`). |
| **`audience` (`aud`)** | Identifiant de l'API destinataire du token. Définie par `Resources` du scope (ex. `grh_api`). |
| **`grant_type`** | Type de flux OAuth2 : `password`, `refresh_token`, `client_credentials`. |
| **`Application` (rôle)** | Champ sur `ApplicationRole` qui restreint un rôle à un `client_id` précis. |
| **Client `Public`** | Application sans secret (SPA, mobile) — utilise `grant_type=password`. |
| **Client `Confidential`** | Application avec secret (API, service) — nécessaire pour l'introspection ou `client_credentials`. |

### Trois types d'applications

| Type | Description | Client OpenIddict |
|---|---|---|
| **Frontend SPA** | Interface navigateur qui ouvre la session utilisateur | `Public` (sans secret) |
| **API Backend** | Resource server qui valide les tokens reçus | Aucun (validation JWT locale) ou `Confidential` si introspection |
| **Service interne** | Backend qui appelle une autre API en son nom propre | `Confidential` + `client_credentials` |

---

## 2. Les chemins d'authentification disponibles

RubacCore supporte **deux chemins d'authentification** qui coexistent sous le même endpoint `/connect/token`. Le frontend ne distingue pas les deux : il envoie toujours le même format de requête.

```
Utilisateurs externes (locaux)      Utilisateurs entreprise (AD/LDAP)
          │                                       │
  ASP.NET Identity                         Bind LDAP (port 389/636)
  (hash en base de données)                (mot de passe validé par AD)
          │                                       │
          └──────────────▶ OpenIddict ◀───────────┘
                           (émetteur JWT)
                                 │
                    Toutes les applications clientes
```

### Chemin 1 — Identité locale (utilisateurs externes)

L'utilisateur est créé dans la base de données de RubacCore avec un hash de mot de passe géré par ASP.NET Core Identity.

**Quand l'utiliser :** comptes non-Active Directory (prestataires, administrateurs externes, comptes de test).

**Détection :** tout username ne se terminant **pas** par le domaine LDAP configuré.

**Exemple de requête :**
```http
POST /connect/token
Content-Type: application/x-www-form-urlencoded

grant_type=password
&client_id=mon-app-front
&username=alice
&password=MonMotDePasse1!
&scope=openid profile email roles mon-app offline_access
```

### Chemin 2 — Active Directory / LDAP (utilisateurs entreprise)

L'utilisateur se connecte avec son UPN Windows (`jean.dupont@corp.local`). RubacCore effectue un **bind LDAP** pour valider le mot de passe. À la première connexion, un « shadow user » est automatiquement créé en base (sans hash de mot de passe).

**Quand l'utiliser :** comptes du domaine Active Directory de l'entreprise.

**Détection :** username se terminant par le domaine configuré dans `Ldap:Domain` (ex. `@corp.local`) **ET** `Ldap:Enabled = true`.

**Configuration dans `appsettings.json` :**
```json
"Ldap": {
  "Enabled": true,
  "Host": "dc01.corp.local",
  "Port": 389,
  "UseSsl": false,
  "Domain": "corp.local",
  "SearchBase": "DC=corp,DC=local",
  "ServiceAccount": "svc-rubac@corp.local",
  "ServicePassword": "SECRET_EN_VARIABLE_ENV",
  "DefaultRole": "User"
}
```

> **Sécurité :** ne jamais stocker `ServicePassword` dans un fichier JSON versionné. Utiliser les variables d'environnement ou `dotnet user-secrets` en développement.

**Exemple de requête :** identique au chemin local, mais avec l'UPN comme username :
```http
POST /connect/token
Content-Type: application/x-www-form-urlencoded

grant_type=password
&client_id=mon-app-front
&username=jean.dupont@corp.local
&password=MotDePasseWindows1!
&scope=openid profile email roles mon-app offline_access
```

**Modèle de données utilisateur selon le chemin :**

| Colonne | Identité locale | LDAP |
|---|---|---|
| `AuthProvider` | `"local"` | `"ldap"` |
| `LdapDn` | `NULL` | `CN=Jean Dupont,OU=Staff,DC=corp,DC=local` |
| `PasswordHash` | Rempli par Identity | `NULL` — jamais stocké |

---

## 3. Cas A — Intégrer un Frontend SPA

### Vue d'ensemble du flux

```
┌────────────┐  1. POST /connect/token (username + password)    ┌──────────────┐
│            │ ─────────────────────────────────────────────▶  │              │
│  Frontend  │  2. { access_token, refresh_token, id_token }   │  RubacCore   │
│            │ ◀───────────────────────────────────────────── │              │
│            │                                                 └──────────────┘
│            │  3. GET /api/xxx  Authorization: Bearer <jwt>    ┌──────────────┐
│            │ ─────────────────────────────────────────────▶  │  Mon API     │
│            │  4. réponse                                      │  (backend)   │
│            │ ◀───────────────────────────────────────────── │              │
└────────────┘                                                 └──────────────┘
```

### Étape 1 — Déclarer le scope et le client dans RubacCore

Éditer [Workers/OpenIddictSeedWorker.cs](../Workers/OpenIddictSeedWorker.cs).

**a) Déclarer le scope** (si la SPA appelle une API qui n'en a pas encore) :

```csharp
await ForceRecreateScopeAsync(manager, new OpenIddictScopeDescriptor
{
    Name        = "mon-app",               // nom du scope demandé par la SPA
    DisplayName = "Mon App — accès API",
    Resources   = { "mon_app_api" }        // → devient l'audience (aud) du JWT
}, cancellationToken);
```

**b) Déclarer le client public :**

```csharp
await RecreateAppAsync(manager, new OpenIddictApplicationDescriptor
{
    ClientId    = "mon-app-front",          // ⚠ identifiant unique, stable
    ClientType  = ClientTypes.Public,       // SPA = pas de secret
    DisplayName = "Mon App — Frontend",
    Permissions =
    {
        Permissions.Endpoints.Token,
        Permissions.Endpoints.Logout,

        // Flux autorisés
        Permissions.GrantTypes.Password,
        Permissions.GrantTypes.RefreshToken,

        // Scopes OIDC standards
        Permissions.Prefixes.Scope + "openid",
        Permissions.Scopes.Profile,
        Permissions.Scopes.Email,
        Permissions.Scopes.Roles,

        // Scope de l'API cible
        Permissions.Prefixes.Scope + "mon-app",   // → audience mon_app_api

        // Si la SPA doit aussi appeler RubacCore (gestion utilisateurs, rôles…)
        Permissions.Prefixes.Scope + "rubac",

        // Active le refresh token
        Permissions.Prefixes.Scope + "offline_access",
    }
}, cancellationToken);
```

> **Règle :** chaque scope envoyé dans `scope=` doit apparaître dans `Permissions`, sinon OpenIddict renvoie `invalid_scope`.

### Étape 2 — Autoriser l'origine CORS dans RubacCore

Dans [Program.cs](../Program.cs), section `AddCors` :

```csharp
.WithOrigins(
    "http://localhost:4200",   // dev
    "https://mon-app.prod"     // prod
)
```

### Étape 3 — Créer les rôles applicatifs (optionnel)

Dans [Workers/DataSeedWorker.cs](../Workers/DataSeedWorker.cs) :

```csharp
new ApplicationRole {
    Name        = "Admin",
    Description = "Administrateur de Mon App",
    Application = "mon-app-front"   // ← rôle visible uniquement dans les tokens de ce client_id
}
```

Un rôle avec `Application = null` est **global** (présent dans tous les tokens).

### Étape 4 — Redémarrer RubacCore

`OpenIddictSeedWorker` recrée les apps et scopes en base au démarrage. Aucune migration EF Core n'est nécessaire.

### Étape 5 — Appeler `/connect/token` depuis la SPA

**Login (`grant_type=password`) :**
```http
POST /connect/token
Content-Type: application/x-www-form-urlencoded

grant_type=password
&client_id=mon-app-front
&username=alice@corp.local
&password=*****
&scope=openid profile email roles mon-app offline_access
```

**Réponse :**
```json
{
  "access_token":  "eyJhbGciOiJSUzI1NiIs...",
  "refresh_token": "CFDJ8...",
  "id_token":      "eyJhbGciOiJSUzI1NiIs...",
  "token_type":    "Bearer",
  "expires_in":    3600
}
```

**Renouvellement silencieux (`grant_type=refresh_token`) :**
```http
POST /connect/token
Content-Type: application/x-www-form-urlencoded

grant_type=refresh_token
&client_id=mon-app-front
&refresh_token=CFDJ8...
```

> Toujours remplacer l'ancien `refresh_token` par le nouveau (rotation activée).

### Étape 6 — Envoyer le token aux APIs

```http
Authorization: Bearer eyJhbGciOiJSUzI1NiIs...
```

### Étape 7 — Décoder l'`id_token` pour afficher l'utilisateur

```typescript
function decodeJwt(jwt: string): any {
  const payload = jwt.split('.')[1];
  return JSON.parse(atob(payload.replace(/-/g, '+').replace(/_/g, '/')));
}

// Exemple de payload décodé
{
  "sub":   "abc-123-def",
  "email": "alice@corp.local",
  "name":  "Alice Dupont",
  "role":  ["Admin", "Operator"],   // tableau OU string selon le nombre de rôles
  "exp":   1745812345
}
```

> **Important :** n'utiliser les claims décodés côté client que pour l'**affichage** (afficher/masquer un menu). La sécurité réelle est imposée par l'API backend.

### Implémentation Angular (référence)

| Fichier | Rôle |
|---|---|
| `src/app/services/auth.service.ts` | Login, refresh, logout, décodage du JWT. Expose des `signal()`. |
| `src/app/interceptors/auth.interceptor.ts` | Ajoute le header `Bearer`, gère le `401` → refresh → retry. |
| `src/app/guards/auth.guard.ts` | Redirige vers `/login` si non authentifié. |
| `proxy.conf.json` | Proxy dev `/connect` et `/api` vers `http://localhost:5262` pour éviter CORS. |

**Squelette `AuthService` :**
```typescript
const ACCESS_TOKEN_KEY  = 'app_access_token';
const REFRESH_TOKEN_KEY = 'app_refresh_token';
const ID_TOKEN_KEY      = 'app_id_token';
const TOKEN_URL         = '/connect/token';
const CLIENT_ID         = 'mon-app-front';
const SCOPES            = 'openid profile email roles mon-app offline_access';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private http = inject(HttpClient);
  readonly currentUser = signal<AuthUser | null>(this.loadFromStorage());
  readonly isAuthenticated = computed(() => this.currentUser() !== null);

  login(username: string, password: string): Observable<void> {
    const body = new HttpParams()
      .set('grant_type', 'password')
      .set('client_id',  CLIENT_ID)
      .set('username',   username)
      .set('password',   password)
      .set('scope',      SCOPES);
    return this.http.post<TokenResponse>(TOKEN_URL, body.toString(), {
      headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
    }).pipe(map(r => this.storeTokens(r)));
  }

  refresh(): Observable<void> {
    const rt = localStorage.getItem(REFRESH_TOKEN_KEY);
    if (!rt) return throwError(() => new Error('no_refresh_token'));
    const body = new HttpParams()
      .set('grant_type',    'refresh_token')
      .set('client_id',     CLIENT_ID)
      .set('refresh_token', rt);
    return this.http.post<TokenResponse>(TOKEN_URL, body.toString(), {
      headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
    }).pipe(map(r => this.storeTokens(r)));
  }

  logout(): void {
    localStorage.removeItem(ACCESS_TOKEN_KEY);
    localStorage.removeItem(REFRESH_TOKEN_KEY);
    localStorage.removeItem(ID_TOKEN_KEY);
    this.currentUser.set(null);
  }
}
```

**Intercepteur HTTP avec refresh automatique :**
```typescript
export const authInterceptor: HttpInterceptorFn = (req, next) => {
  // Ne jamais intercepter /connect/* pour éviter une boucle de refresh
  if (req.url.includes('/connect/')) return next(req);

  const auth = inject(AuthService);
  const withBearer = (r: HttpRequest<unknown>) => {
    const token = auth.getAccessToken();
    return token
      ? r.clone({ setHeaders: { Authorization: `Bearer ${token}` } })
      : r;
  };

  return next(withBearer(req)).pipe(
    catchError((err: HttpErrorResponse) => {
      if (err.status !== 401) return throwError(() => err);
      return auth.refresh().pipe(
        catchError(e => { auth.logout(); return throwError(() => e); }),
        switchMap(() => next(withBearer(req))),
      );
    }),
  );
};
```

**Implémentation React (référence rapide) :**
```typescript
// authClient.ts
const TOKEN_URL = `${import.meta.env.VITE_AUTHORITY}/connect/token`;
const CLIENT_ID = 'mon-app-front';

export async function login(username: string, password: string) {
  const body = new URLSearchParams({
    grant_type: 'password',
    client_id:  CLIENT_ID,
    username,
    password,
    scope: 'openid profile email roles mon-app offline_access',
  });
  const res = await fetch(TOKEN_URL, { method: 'POST', body });
  if (!res.ok) throw new Error('login_failed');
  const tokens = await res.json();
  localStorage.setItem('access_token',  tokens.access_token);
  localStorage.setItem('refresh_token', tokens.refresh_token);
}
```

### Configuration proxy de développement (Angular)

Pour éviter les erreurs CORS en développement local, utiliser un proxy :

```jsonc
// proxy.conf.json
{
  "/connect":  { "target": "http://localhost:5262", "secure": false, "changeOrigin": true },
  "/api":      { "target": "http://localhost:5262", "secure": false, "changeOrigin": true }
}
```

### Erreurs courantes lors du login

| HTTP | `error` | Cause |
|---|---|---|
| 400 | `invalid_grant` | Identifiants incorrects ou compte désactivé. |
| 400 | `invalid_scope` | Scope demandé absent des `Permissions` du client OpenIddict. |
| 400 | `invalid_client` | `client_id` inconnu, ou client `Confidential` appelé sans secret. |

---

## 4. Cas B — Intégrer une API Backend (Resource Server)

L'API valide le JWT reçu en vérifiant la **signature** (clés publiques de RubacCore via JWKS) et l'**audience** (`aud`). Elle n'intervient pas dans le processus de login.

```
┌──────────┐  Bearer JWT   ┌─────────────┐
│ Frontend │ ────────────▶ │   Mon API   │
└──────────┘               │             │
                           │  valide     │
                           │  - sig      │ ← clés publiques (JWKS)
                           │  - aud      │ ← "mon_app_api"
                           │  - exp      │
                           └──────┬──────┘
                                  │ (une fois au démarrage, puis cache)
                                  ▼
                           ┌─────────────┐
                           │  RubacCore  │  /.well-known/openid-configuration
                           │   (JWKS)    │  /.well-known/jwks
                           └─────────────┘
```

> **Validation 100 % offline** après le premier appel — l'API ne contacte RubacCore que pour rafraîchir les clés publiques (rare).

### Pré-requis côté RubacCore

Dans [Workers/OpenIddictSeedWorker.cs](../Workers/OpenIddictSeedWorker.cs) :

```csharp
// Le scope doit exister et déclarer votre audience
await ForceRecreateScopeAsync(manager, new OpenIddictScopeDescriptor
{
    Name        = "mon-app",
    DisplayName = "Mon App — accès API",
    Resources   = { "mon_app_api" }   // ← audience que votre API va valider
}, cancellationToken);

// Le frontend appelant doit avoir ce scope autorisé
Permissions = {
    Permissions.Prefixes.Scope + "mon-app",   // ← doit être présent
    // ...
}
```

> Sans ces deux éléments, le JWT ne contiendra **pas** votre audience et l'API renverra `401`.

### Choisir sa variante d'intégration

| Variante | Quand l'utiliser | Avantage |
|---|---|---|
| **A. JwtBearer** (recommandé .NET) | API .NET dans un process séparé. | Standard, 0 dépendance OpenIddict, validation offline. |
| **B. OpenIddict.Validation** | API .NET dans le même process / DB que RubacCore (rare). | Lecture directe en base, pas de discovery HTTP. |
| **C. Bibliothèque JWT générique** | API Node, Python, Java, Go… | Indépendant de la stack technique. |
| **D. Introspection** | Tokens opaques ou besoin de révocation immédiate. | Vérification en temps réel (aller-retour HTTP à chaque requête). |

---

### Variante A — API .NET avec `JwtBearer` (recommandé)

#### Ajouter le package

```xml
<PackageReference Include="Microsoft.AspNetCore.Authentication.JwtBearer" Version="10.0.0" />
```

#### Configurer dans `Program.cs`

```csharp
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        // Discovery OIDC automatique depuis RubacCore
        options.Authority            = builder.Configuration["Auth:Authority"]
                                       ?? "http://localhost:5262";
        options.RequireHttpsMetadata = !builder.Environment.IsDevelopment();

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer   = true,
            ValidIssuer      = builder.Configuration["Auth:Authority"],
            ValidateAudience = true,
            ValidAudience    = "mon_app_api",   // ← doit correspondre à Resources du scope
            ValidateLifetime = true,
            ClockSkew        = TimeSpan.FromMinutes(1),

            // Nécessaire pour que [Authorize(Roles = "...")] fonctionne
            RoleClaimType    = "role",
            NameClaimType    = "name",
        };

        // En dev : accepter les certificats auto-signés
        if (builder.Environment.IsDevelopment())
        {
            options.BackchannelHttpHandler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback =
                    HttpClientHandler.DangerousAcceptAnyServerCertificateValidator,
            };
        }
    });

builder.Services.AddAuthorization();
builder.Services.AddControllers();

// CORS si appels cross-origin depuis le frontend
builder.Services.AddCors(o => o.AddDefaultPolicy(p => p
    .WithOrigins("http://localhost:4200", "https://mon-frontend.prod")
    .AllowAnyHeader()
    .AllowAnyMethod()));

var app = builder.Build();

app.UseCors();
app.UseAuthentication();    // ⚠ AVANT UseAuthorization
app.UseAuthorization();
app.MapControllers();
app.Run();
```

#### `appsettings.json`

```json
{
  "Auth": {
    "Authority": "http://localhost:5262",
    "Audience":  "mon_app_api"
  }
}
```

#### Protéger les endpoints

```csharp
[ApiController]
[Route("api/[controller]")]
[Authorize]                                       // tout utilisateur authentifié
public class FacturesController : ControllerBase
{
    [HttpGet]
    public IActionResult List() => Ok(/* ... */);

    [HttpPost]
    [Authorize(Roles = "Admin")]                  // rôle spécifique
    public IActionResult Create() => Ok();

    [HttpDelete("{id}")]
    [Authorize(Policy = "CanSupprimerFactures")]  // policy custom
    public IActionResult Delete(string id) => Ok();
}
```

#### Policies d'autorisation avancées

```csharp
builder.Services.AddAuthorization(options =>
{
    // Rôle simple
    options.AddPolicy("AdminOnly", p => p.RequireRole("Admin"));

    // Combinaison de rôles
    options.AddPolicy("CanSupprimerFactures", p => p
        .RequireAuthenticatedUser()
        .RequireRole("Admin", "Comptable"));

    // Vérifier que le token provient d'un frontend spécifique
    options.AddPolicy("DepuisMonFrontend", p => p
        .RequireClaim("client_id", "mon-app-front"));

    // Vérifier un scope spécifique
    options.AddPolicy("RequiertScopeMonApp", p => p
        .RequireAssertion(ctx =>
            ctx.User.FindFirst("scope")?.Value
              ?.Split(' ')
              .Contains("mon-app") == true));
});
```

#### Lire les informations de l'utilisateur courant

```csharp
[HttpGet("profil")]
[Authorize]
public IActionResult MonProfil()
{
    var userId   = User.FindFirst("sub")?.Value;
    var email    = User.FindFirst("email")?.Value;
    var roles    = User.FindAll("role").Select(c => c.Value).ToList();
    var clientId = User.FindFirst("client_id")?.Value;  // frontend appelant

    return Ok(new { userId, email, roles, clientId });
}
```

---

### Variante B — API .NET avec `OpenIddict.Validation`

Utiliser uniquement si l'API tourne dans le **même process** que RubacCore, ou partage la même base de données OpenIddict.

```csharp
builder.Services.AddOpenIddict()
    .AddValidation(options =>
    {
        options.SetIssuer("http://localhost:5262");
        options.AddAudiences("mon_app_api");
        options.UseSystemNetHttp();
        options.UseAspNetCore();
    });

builder.Services.AddAuthentication(o =>
{
    o.DefaultScheme = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme;
});
```

---

### Variante C — API non-.NET

Utiliser n'importe quelle bibliothèque JWT standard en pointant sur le JWKS de RubacCore.

**Endpoints à connaître :**
- Discovery : `https://rubaccore/.well-known/openid-configuration`
- Clés publiques : `https://rubaccore/.well-known/jwks`

#### Node.js (Express + `jose`)

```bash
npm install express jose
```

```javascript
import express from 'express';
import { createRemoteJWKSet, jwtVerify } from 'jose';

const AUTHORITY = 'http://localhost:5262';
const AUDIENCE  = 'mon_app_api';

const JWKS = createRemoteJWKSet(new URL(`${AUTHORITY}/.well-known/jwks`));

async function authMiddleware(req, res, next) {
  try {
    const auth = req.headers.authorization;
    if (!auth?.startsWith('Bearer ')) return res.sendStatus(401);

    const { payload } = await jwtVerify(auth.slice(7), JWKS, {
      issuer:   AUTHORITY,
      audience: AUDIENCE,
    });
    req.user = payload;
    next();
  } catch {
    res.sendStatus(401);
  }
}

const app = express();
app.get('/api/profil', authMiddleware, (req, res) => res.json(req.user));
app.listen(5300);
```

#### Python (FastAPI + `python-jose`)

```python
from fastapi import FastAPI, Depends, HTTPException, Security
from fastapi.security import HTTPBearer, HTTPAuthorizationCredentials
from jose import jwt
import requests

AUTHORITY = "http://localhost:5262"
AUDIENCE  = "mon_app_api"

_jwks = requests.get(f"{AUTHORITY}/.well-known/jwks").json()  # cache au démarrage
bearer = HTTPBearer()

def utilisateur_courant(creds: HTTPAuthorizationCredentials = Security(bearer)):
    try:
        return jwt.decode(
            creds.credentials, _jwks,
            algorithms=["RS256"],
            audience=AUDIENCE,
            issuer=AUTHORITY,
        )
    except Exception:
        raise HTTPException(status_code=401, detail="Token invalide")

app = FastAPI()

@app.get("/api/profil")
def profil(user = Depends(utilisateur_courant)):
    return user
```

#### Java / Spring Boot

```yaml
# application.yml
spring:
  security:
    oauth2:
      resourceserver:
        jwt:
          issuer-uri: http://localhost:5262
          audiences:  [mon_app_api]
```

```java
@RestController
public class ProfilController {
    @GetMapping("/api/profil")
    @PreAuthorize("hasAuthority('SCOPE_mon-app')")
    public Map<String, Object> profil(JwtAuthenticationToken token) {
        return token.getTokenAttributes();
    }
}
```

#### Go (avec `lestrrat-go/jwx`)

```bash
go get github.com/lestrrat-go/jwx/v2
```

```go
package main

import (
    "net/http"
    "github.com/lestrrat-go/jwx/v2/jwk"
    "github.com/lestrrat-go/jwx/v2/jwt"
)

const authority = "http://localhost:5262"
const audience  = "mon_app_api"

func main() {
    cache := jwk.NewCache(context.Background())
    cache.Register(authority + "/.well-known/jwks")

    http.HandleFunc("/api/profil", func(w http.ResponseWriter, r *http.Request) {
        raw := strings.TrimPrefix(r.Header.Get("Authorization"), "Bearer ")
        keySet, _ := cache.Get(context.Background(), authority+"/.well-known/jwks")
        token, err := jwt.Parse([]byte(raw), jwt.WithKeySet(keySet),
            jwt.WithIssuer(authority),
            jwt.WithAudience(audience),
        )
        if err != nil {
            http.Error(w, "Unauthorized", 401)
            return
        }
        // token.Subject(), etc.
    })
    http.ListenAndServe(":5300", nil)
}
```

---

### Variante D — Introspection (tokens opaques / révocation immédiate)

Utiliser lorsqu'il faut **révoquer un token immédiatement** (pas à l'expiration). Coût : 1 aller-retour HTTP par requête API.

#### Côté RubacCore : déclarer l'API comme client confidentiel

```csharp
await RecreateAppAsync(manager, new OpenIddictApplicationDescriptor
{
    ClientId     = "mon-app-api",
    ClientSecret = "SECRET_VIA_ENV_PROD",
    ClientType   = ClientTypes.Confidential,
    Permissions  = {
        Permissions.Endpoints.Introspection
    }
}, cancellationToken);
```

#### Côté API .NET : configurer la validation par introspection

```csharp
builder.Services.AddOpenIddict()
    .AddValidation(options =>
    {
        options.SetIssuer("http://localhost:5262");
        options.AddAudiences("mon_app_api");

        options.UseIntrospection()
               .SetClientId("mon-app-api")
               .SetClientSecret(builder.Configuration["Auth:ClientSecret"]!);

        options.UseSystemNetHttp();
        options.UseAspNetCore();
    });
```

---

## 5. Cas C — Communication service-à-service

Quand un **backend doit appeler un autre backend en son nom propre** (job planifié, worker, webhook…), utiliser `client_credentials`. Il n'y a pas d'utilisateur impliqué.

### Côté RubacCore : déclarer le client confidentiel

```csharp
await RecreateAppAsync(manager, new OpenIddictApplicationDescriptor
{
    ClientId     = "mon-worker",
    ClientSecret = "SECRET_VIA_ENV_PROD",
    ClientType   = ClientTypes.Confidential,
    Permissions  =
    {
        Permissions.Endpoints.Token,
        Permissions.GrantTypes.ClientCredentials,
        Permissions.Prefixes.Scope + "mon-app"    // scope de l'API cible
    }
}, cancellationToken);
```

### Côté worker : récupérer un token

```http
POST /connect/token
Content-Type: application/x-www-form-urlencoded

grant_type=client_credentials
&client_id=mon-worker
&client_secret=SECRET_VIA_ENV_PROD
&scope=mon-app
```

Le JWT obtenu n'a **pas** de claim `sub` utilisateur — il représente l'application elle-même.

### Implémentation .NET avec cache de token

```csharp
public class TokenClient(HttpClient http, IConfiguration cfg)
{
    private string? _token;
    private DateTimeOffset _exp = DateTimeOffset.MinValue;

    public async Task<string> GetTokenAsync(CancellationToken ct = default)
    {
        if (_token is not null && _exp > DateTimeOffset.UtcNow.AddMinutes(1))
            return _token;

        var body = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["grant_type"]    = "client_credentials",
            ["client_id"]     = cfg["Auth:ClientId"]!,
            ["client_secret"] = cfg["Auth:ClientSecret"]!,
            ["scope"]         = "mon-app",
        });

        var r = await http.PostAsync(cfg["Auth:Authority"] + "/connect/token", body, ct);
        r.EnsureSuccessStatusCode();

        var json = await r.Content.ReadFromJsonAsync<TokenResp>(ct);
        _token = json!.access_token;
        _exp   = DateTimeOffset.UtcNow.AddSeconds(json.expires_in - 30);
        return _token;
    }

    private record TokenResp(string access_token, int expires_in);
}
```

### Côté API destination : identifier un appel service-à-service

```csharp
// Policy qui accepte uniquement les appels provenant du worker interne
options.AddPolicy("ServiceInterneUniquement", p => p
    .RequireClaim("client_id", "mon-worker"));
```

---

## 6. Gestion des rôles multi-applications

`ApplicationRole` possède un champ `Application`. Quand il est renseigné, le rôle n'est inclus **que dans les tokens émis pour ce `client_id`**. Quand il est `null`, le rôle est **global** (présent dans tous les tokens).

### Convention de nommage

| Nom du rôle | Champ `Application` | Inclus dans les tokens de |
|---|---|---|
| `SuperAdmin` | `null` | toutes les applications |
| `User` | `null` | toutes les applications |
| `Admin` | `"rubac-admin"` | rubac-admin uniquement |
| `Gestionnaire` | `"mon-app-front"` | mon-app-front uniquement |

### Fonctionnement

`AuthController` appelle `GetRolesForClientAsync(userId, clientId)` à chaque émission de token (password grant **et** refresh token grant). La requête filtre :

```sql
WHERE Application IS NULL OR Application = @clientId
```

Les modifications de rôles prennent effet à la **prochaine émission de token** (sans nécessiter un nouveau login).

### Exemple concret

```
Rôles attribués à jean@corp.local :
  "User"        (Application = null)              → toutes les apps
  "Admin"       (Application = "rubac-admin")     → rubac-admin seulement
  "Gestionnaire"(Application = "mon-app-front")   → mon-app-front seulement

Token émis pour rubac-admin   → role = ["User", "Admin"]
Token émis pour mon-app-front → role = ["User", "Gestionnaire"]
Token émis pour dashboard     → role = ["User"]
```

### Rôle par défaut pour les utilisateurs LDAP

Le `DefaultRole` configuré dans `appsettings.json` est attribué automatiquement à la première connexion. La résolution cherche d'abord un rôle **scopé** au `client_id` appelant, puis un rôle **global** portant ce nom.

---

## 7. Checklist récapitulative par cas

### Cas A — Nouveau frontend SPA
- [ ] Scope créé dans `OpenIddictSeedWorker.SeedScopesAsync` (si nouvelle API associée).
- [ ] Client `Public` créé dans `SeedApplicationsAsync` avec les grants `Password` + `RefreshToken`.
- [ ] Origine ajoutée dans `AddCors` de `Program.cs`.
- [ ] Rôles scopés créés dans `DataSeedWorker` (optionnel).
- [ ] SPA configurée pour appeler `/connect/token` avec son `client_id` et ses scopes.
- [ ] Intercepteur HTTP implémenté pour le refresh automatique.
- [ ] RubacCore redémarré pour appliquer les modifications du seed.

### Cas B — Nouvelle API backend
- [ ] Scope créé dans `SeedScopesAsync` avec `Resources = { "mon_audience_api" }`.
- [ ] Client `Confidential` créé (uniquement si introspection ou `client_credentials` nécessaires).
- [ ] Frontend(s) appelant cette API ont le scope dans leurs `Permissions`.
- [ ] API configurée avec `Authority = RubacCore` et `ValidAudience = mon_audience_api`.
- [ ] `RoleClaimType = "role"` configuré (sinon `[Authorize(Roles=...)]` ne fonctionne pas).
- [ ] `app.UseAuthentication()` placé **avant** `app.UseAuthorization()`.
- [ ] CORS configuré pour les origines autorisées.
- [ ] Endpoints décorés avec `[Authorize]`, policies et/ou rôles.
- [ ] Test end-to-end : token valide → 200 ; token avec mauvaise audience → 401.

### Cas C — Service-à-service
- [ ] Scope cible existant dans RubacCore.
- [ ] Client `Confidential` créé avec `GrantTypes.ClientCredentials`.
- [ ] Secret externalisé en variable d'environnement (pas en clair dans le code).
- [ ] Mécanisme de cache du token implémenté côté worker (évite de redemander un token à chaque appel).
- [ ] API cible configurée pour identifier les appels services (`RequireClaim("client_id", ...)`).

---

## 8. Pièges fréquents et solutions

| Erreur observée | Cause | Solution |
|---|---|---|
| `invalid_scope` au login | Scope demandé par la SPA absent des `Permissions` du client OpenIddict. | Ajouter le scope dans `RecreateAppAsync` et redémarrer RubacCore. |
| `401 Unauthorized` côté API malgré un token valide | L'`audience` dans le token ne correspond pas à `ValidAudience` de l'API. | Aligner `ValidAudience` (côté API) avec la valeur de `Resources` du scope (côté RubacCore). |
| Le token ne contient pas le rôle attendu | Le rôle a `Application = "X"` mais la SPA s'est connectée avec un `client_id` différent. | Créer le rôle comme global (`Application = null`) ou aligner les `client_id`. |
| Erreur CORS dans la console navigateur | URL de la SPA absente de `WithOrigins(...)` dans `Program.cs`. | Ajouter l'URL de la SPA dans la liste CORS de RubacCore. |
| Refresh token rejeté | Normal si les clés de signature ont changé ou si le token a expiré. | Rediriger vers `/login`. |
| `[Authorize(Roles = "Admin")]` toujours refusé | `RoleClaimType` non configuré — le claim `role` n'est pas mappé vers `ClaimTypes.Role`. | Ajouter `RoleClaimType = "role"` dans `TokenValidationParameters`. |
| Secret en clair dans `OpenIddictSeedWorker.cs` | Acceptable en développement uniquement. | En production, lire depuis variables d'environnement ou un vault de secrets. |
| `401` sur endpoint LDAP avec `Ldap:Enabled = false` | LDAP désactivé, les utilisateurs du domaine sont inconnus. | Activer LDAP ou créer des comptes locaux pour les tests. |

---

## 9. Tester rapidement sans implémentation frontend

### Récupérer un token via curl

```bash
curl -X POST http://localhost:5262/connect/token \
  -H "Content-Type: application/x-www-form-urlencoded" \
  -d "grant_type=password&client_id=mon-app-front&username=alice&password=***&scope=openid profile roles mon-app offline_access"
```

### Vérifier le contenu du JWT

Coller l'`access_token` sur [jwt.io](https://jwt.io) et vérifier :

- `aud` contient `mon_app_api`
- `iss` correspond à l'URL de RubacCore
- `role` contient les rôles attendus
- `client_id` = `mon-app-front`

### Appeler l'API avec curl

```bash
curl http://localhost:5300/api/profil \
  -H "Authorization: Bearer eyJhbGciOi..."
```

---

## 10. Fichiers de référence

| Fichier | Contenu |
|---|---|
| [Workers/OpenIddictSeedWorker.cs](../Workers/OpenIddictSeedWorker.cs) | Déclaration de tous les clients et scopes OAuth2. **Point d'entrée principal** pour ajouter une application. |
| [Workers/DataSeedWorker.cs](../Workers/DataSeedWorker.cs) | Création des rôles applicatifs (scoping par `Application`). |
| [Program.cs](../Program.cs) | Configuration OpenIddict serveur, CORS, authentification et autorisation. |
| [Services/AuthService.cs](../Services/AuthService.cs) | Routage Local Identity / LDAP, scoping des rôles par `client_id`. |
| [Controllers/AuthController.cs](../Controllers/AuthController.cs) | Endpoint `/connect/token` (password, refresh, token-exchange). |
| [AUTH.md](../AUTH.md) | Détail des deux chemins d'authentification (Local + LDAP) et du scoping multi-applications. |
| [FRONTEND_INTEGRATION.md](../FRONTEND_INTEGRATION.md) | Guide détaillé d'intégration côté frontend. |
| [API_INTEGRATION.md](../API_INTEGRATION.md) | Guide détaillé d'intégration côté API backend. |

---

**Résumé en 3 étapes** pour brancher n'importe quelle application :

1. **RubacCore** — Ajouter un *scope* et un *client OpenIddict* dans `OpenIddictSeedWorker.cs`, puis ajouter l'origine CORS dans `Program.cs`.
2. **Nouvelle app** — Configurer la validation JWT avec `Authority = RubacCore` et `ValidAudience = mon_audience_api`.
3. **Redémarrer RubacCore** — Le seed worker applique les changements automatiquement.
