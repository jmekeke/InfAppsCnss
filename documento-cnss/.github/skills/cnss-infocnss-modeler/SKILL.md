---
name: cnss-infocnss-modeler
description: >
  WORKFLOW SKILL — Crée ou met à jour de manière incrémentale un modèle
  DDD + Clean Architecture pour un module de communications internes CNSS.
  USE FOR: modélisation d'un bounded context CNSS, extraction de use cases
  depuis agent-communicat.md, mise à jour contrôlée d'un modèle existant,
  production de diagrammes Mermaid, structure Clean Architecture suggérée,
  delta explicite entre deux versions de modèle, génération de squelette de
  projet Clean Architecture.
  DO NOT USE FOR: questions générales C#, revue juridique, interprétation
  règlementaire sans source documentaire.
---

# CNSS InfoCnss — DDD Clean Modeler

## Source principale des règles métier
Toujours lire en premier :
`d:/J_Projetsd/AppMetier/Autre/InfoCnss/docs/conventions/agent-communicat.md`

## Ressources
- [Instructions système agent](./references/prompt-Agt.md)
- [Référence DDD Clean Architecture](./references/DDD-CleanArchitecture.md)
- [Script génération structure](./scripts/New-CleanArchModule.ps1)

## Quand utiliser ce skill
- Modéliser un module de communication interne CNSS depuis zéro
- Mettre à jour un modèle existant à partir d'une nouvelle version de `agent-communicat.md`
- Produire un delta explicite entre deux versions de modèle
- Générer la structure de dossiers Clean Architecture d'un nouveau module

## Workflow

### Étape 1 — Lecture des sources
1. Lire `d:/J_Projetsd/AppMetier/Autre/InfoCnss/docs/conventions/agent-communicat.md`
2. Lire le modèle validé existant du module ciblé dans `documento-cnss/` (s'il existe)
3. Lire le glossaire et les décisions d'architecture disponibles

### Étape 2 — Extraction
4. Extraire : acteurs, tâches, règles métier, documents, états, dépendances externes
5. Mapper les tâches → use cases (style `Request / Response / Handler / Validator`)

### Étape 3 — Modélisation
6. Dériver : entities, value objects, aggregates, domain services, repositories, domain events
7. Appliquer les décisions de socle :
   - Base `Cnss.Shared.Domain.Abstractions.ValueObject`
   - Identifiants métier via service de domaine
   - Commit repository explicite
   - Factory d'agrégat uniquement au cas par cas

#### Agrégat externe : `Agent` (SQL Server externe)
L'agrégat `Agent` et ses entités liées (`Direction`, `Grade`, `CategorieGrade`) appartiennent
à un bounded context distinct, persisté dans une base **SQL Server séparée**.
**Règle d'intégration DDD — Anti-Corruption Layer :**

| Couche | Ce qu'on fait |
|---|---|
| **Domain** | Référencer uniquement par `AgentId` (Guid/int scalaire). Jamais d'import de la classe `Agent`. |
| **Application/Ports** | Déclarer un port `IAgentQueryService` pour les besoins de lecture (enrichissement de DTOs). |
| **Infrastructure/ExternalServices** | Implémenter `IAgentQueryService` via HTTP REST ou requête SQL Server directe selon l'accès disponible. |

DTO de référence minimale à créer dans Application :
```csharp
// Application/Ports/IAgentQueryService.cs
public interface IAgentQueryService
{
    Task<AgentDto?> GetAgentAsync(Guid agentId, CancellationToken ct = default);
    Task<List<AgentDto>> GetAgentsByDirectionAsync(Guid directionId, CancellationToken ct = default);
}

public record AgentDto(
    Guid Id,
    string Matricule,
    string NomComplet,
    string? Email,
    string? Telephone,
    string? Direction,
    string? Grade,
    string? CategorieGrade
);
```

Infrastructure (adaptateur SQL Server) :
```csharp
// Infrastructure/ExternalServices/AgentSqlQueryService.cs
internal sealed class AgentSqlQueryService(IConfiguration config) : IAgentQueryService
{
    // Connexion distincte vers la base SQL Server Agent
    private SqlConnection CreateConnection() =>
        new(config.GetConnectionString("AgentDb"));
    // ...
}
```

Enregistrement dans `DependencyInjection.cs` :
```csharp
services.AddScoped<IAgentQueryService, AgentSqlQueryService>();
// ou : services.AddHttpClient<IAgentQueryService, AgentHttpClient>(...);
```

### Étape 4 — Proposition
8. Produire la structure Clean Architecture suggérée
9. Comparer avec le modèle existant (delta explicite)
10. Séparer : **Conventions observées** | **Hypothèses** | **Questions ouvertes** | **Conflits**

### Étape 5 — Sorties
Les fichiers produits doivent être sauvegardés dans `d:/J_Projetsd/AppMetier/Autre/InfoCnss/documento-cnss/`

11. Synthèse métier du module
12. Schéma JSON conforme à `documento-cnss/modeling-output-schema.json`
13. Diagrammes Mermaid (agrégats, flux, états)
14. Documentation de modèle structurée

### Étape 6 — Génération de la structure (optionnel)
Si demandé, exécuter :
```powershell
.\scripts\New-CleanArchModule.ps1 -ModuleName <NomModule> -OutputPath <CheminCible>
```

## Garde-fous
- Ne jamais inventer silencieusement une règle absente de `agent-communicat.md`
- Ne jamais remplacer un modèle existant sans signaler les écarts
- Ne jamais traiter un brouillon comme une vérité officielle
- Marquer explicitement toute déduction non prouvée
- Ne pas produire d'architecture théorique déconnectée du style observé

## Limites
- Ne valide pas juridiquement les textes
- Ne tranche pas seul les ambiguïtés métier majeures
- Ne remplace pas une revue architecture ou métier
