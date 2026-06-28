# SPEC — Workflow Communication Interne (InfoCnss)

> Document de référence du cycle de vie d'un message interne CNSS.
> À garder à jour : toute évolution du workflow doit être reflétée ici **avant** le code.
> Stack : .NET 10 (DDD / Clean Architecture / CQRS / MDiator / FluentValidation / EF Core), Angular 21.

---

## 1. Acteurs et rôles

| Acteur | Description |
|--------|-------------|
| **Rédacteur** | Crée et corrige les messages (« l'orateur »). |
| **Lecteur/Correcteur** | Participe aux discussions et corrige. Possède un **sous-type** parmi : |
| → *Responsable* | Lit, commente, corrige pendant la discussion ainsi que Clôture la discussion, rejette, rouvre, programme et déclenche la diffusion directe. |
| → *Simple* | Lit, commente, corrige pendant la discussion. |
| → *Diffuseur* | À l'étape de diffusion, voit les messages et déclenche l'envoi effectif. |

Les rôles sont portés par le **JWT RubacCore** (claims `role` / `permission(s)`)

---

## 2. Types de message

`TypeMessage` : `Article`, `RevueDePresse`, `PageMagazine`, … (enum extensible).
Le type est **obligatoire** à la création et fixé dans la factory `MessageInterne.Creer`.

---

## 3. États (`StatutMessage`)

| État | Sens | Qui agit |
|------|------|----------|
| `Brouillon` | Création/édition libre. | Rédacteur |
| `EnDiscussion` | Visible par le groupe Lecteurs/Correcteurs ; corrections + historique. | Rédacteur + Correcteurs |
| `BrouillonFinal` | Version retenue + instructions, renvoyée au rédacteur. | Rédacteur (corrige) |
| `Rejete` | Archivé, motif obligatoire ; visible du seul Responsable. | Responsable |
| `EnAttenteProgrammation` | Soumis pour programmation ; vu par **tous** les Responsables. | Responsable |
| `Programmee` | Diffusion **manuelle** planifiée (date/heure). | Scheduler |
| `EnDiffusion` | Disponible aux **Diffuseurs** pour envoi. | Diffuseur |
| `Diffuse` | Envoyé aux groupes. État final. | — |

---

## 4. Transitions

| Depuis | Vers | Déclencheur | Acteur |
|--------|------|-------------|--------|
| *(création)* | `Brouillon` | `Creer(type, …)` | Rédacteur |
| `Brouillon` | `Brouillon` | modifications (autant de fois que voulu) | Rédacteur |
| `Brouillon` | `EnDiscussion` | `SoumettreADiscussion` | Rédacteur |
| `EnDiscussion` | `EnDiscussion` | corrections contenu / canaux / groupes (historisées) | Rédacteur + Correcteurs |
| `EnDiscussion` | `BrouillonFinal` | `CloturerEnBrouillonFinal(instructions)` | Responsable |
| `EnDiscussion` | `Rejete` | `Rejeter(motif)` | Responsable |
| `Rejete` | `EnDiscussion` | `Rouvrir(redacteurId)` — réattache l'historique | Responsable |
| `BrouillonFinal` | `BrouillonFinal` | corrections selon instructions | Rédacteur |
| `BrouillonFinal` | `EnAttenteProgrammation` | `SoumettrePourProgrammation` | Rédacteur |
| `EnAttenteProgrammation` | `Programmee` | `ProgrammerDiffusion(date)` — **manuelle** | Responsable |
| `EnAttenteProgrammation` | `EnDiffusion` | `DiffuserDirectement` — **directe** | Responsable |
| `Programmee` | `EnDiffusion` | `OuvrirDiffusion` — à l'échéance (scheduler) | Système |
| `EnDiffusion` | `Diffuse` | `MarquerCommeDiffuse` | Diffuseur |

---

## 5. Règles métier / invariants

- Toute transition invalide **lève une exception de domaine** (statut courant vérifié).
- **Validation et programmation sont fusionnées** : pas d'état `Valide` distinct.
- En `EnDiscussion`, le contenu, les **canaux** et les **groupes de diffusion** peuvent être corrigés
  par le rédacteur **et** les correcteurs.
- **Tout l'historique de discussion est conservé** (append-only, immuable) : auteur, rôle, date,
  type d'intervention, ancienne → nouvelle valeur.
- À la **réouverture** d'un message rejeté, l'historique précédent est **conservé et réattaché**.
- Le **rejet** exige un motif/commentaire ; le message est **archivé** et visible du seul Responsable.
- `EnAttenteProgrammation` est visible par **tous** les Responsables.
- **Jamais de suppression physique** après diffusion (archivage logique uniquement).
- Modification du contenu interdite hors `Brouillon` / `EnDiscussion` / `BrouillonFinal`.

---

## 6. Décisions verrouillées

1. Validation + programmation **fusionnées** en une seule décision du Responsable.
2. En `EnDiscussion`, modifient : les Lecteurs/Correcteurs **et** le rédacteur.
3. `Diffuseur` est un **sous-type** de Lecteur/Correcteur (Responsable, Simple, Diffuseur).
4. `EnAttenteProgrammation` est visible par **tous** les Responsables (rôle/type donné).
5. À la réouverture d'un rejet, l'historique est **conservé et réattaché**.

---

## 7. Point encore ouvert

- **Diffusion directe** : décision actuelle → passe par `EnDiffusion` (un Diffuseur déclenche
  quand même l'envoi). Alternative possible : envoi immédiat `EnAttenteProgrammation → Diffuse`
  sans intervention d'un Diffuseur. À confirmer.

---

## 8. Autorisation (résumé)

| Action | Rôle requis |
|--------|-------------|
| Créer / modifier en brouillon / soumettre | Rédacteur |
| Corriger en discussion | Rédacteur + Correcteurs |
| Clôturer / rejeter / rouvrir / programmer / diffusion directe | Responsable |
| Déclencher l'envoi (`MarquerCommeDiffuse`) | Diffuseur |
| Voir les messages rejetés | Responsable |
