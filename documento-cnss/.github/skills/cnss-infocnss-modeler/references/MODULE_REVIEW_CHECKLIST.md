# Checklist de revue d'un module

Utiliser cette checklist pour vérifier qu'un module nouveau ou mis à jour reste conforme au style réellement observé dans le repo d'exemple.

## 1. Structure
- Le module est séparé en `Domain`, `Application`, `Infrastructure` et point d'entrée éventuel.
- Les dossiers métier suivent les conventions observées du repo (`Aggregats`, `Entities`, `ValuesObject`, `Events`, `Repositories`, `Services`, `Factories`).
- Le module conserve les conventions de référence retenues pour le socle : `Aggregats` et `ValueObject`.
- Les variations de structure sont documentées au lieu d'être implicitement introduites.

## 2. Domaine
- Les agrégats racines héritent de `AggregateRoot<TId>`.
- Les entités non racines héritent de `Entity<TId>`.
- Les identifiants sont cohérents avec le style observé du module.
- Les identifiants métier sont générés par un service de domaine lorsqu'ils existent.
- Les identifiants techniques non métier sont laissés à la base de données lorsqu'un auto-incrément est prévu.
- Les setters publics inutiles n'existent pas.
- Les invariants métier sont dans le domaine, pas dans l'API.
- Les méthodes de création/restauration sont explicites lorsque nécessaire.
- Les domain events sont ajoutés depuis l'agrégat.

## 3. Value Objects
- Un value object n'est introduit que lorsqu'il porte un sens métier réel.
- L'égalité est structurelle.
- Le constructeur protège les invariants.
- La base `ValueObject` utilisée est cohérente avec la convention retenue pour le repo.
- La base officielle retenue est `Cnss.Shared.Domain.Abstractions.ValueObject`.

## 4. Application
- Chaque use case est isolé dans un dossier dédié.
- Le dossier contient au minimum `Request`, `Response`, `Handler`.
- Un `RequestValidator` existe lorsque des règles d'entrée sont nécessaires.
- Le handler valide la requête avant d'orchestrer le métier.
- Le handler délègue la logique métier au domaine au lieu de la réimplémenter.
- Les échanges inter-modules passent par des contrats applicatifs explicites.

## 5. Repositories
- Les interfaces de repository vivent dans le domaine.
- Les implémentations vivent dans l'infrastructure.
- Les signatures sont asynchrones et acceptent `CancellationToken`.
- Les responsabilités repository restent centrées sur la persistance, pas sur l'orchestration métier.
- Le repository expose une mécanique de commit explicite (`Commit` ou `CommitAsync(bool flush = true)`) lorsque le module cible suit la convention du projet métier.
- Si plusieurs sauvegardes sont nécessaires dans un même use case, la stratégie transactionnelle est identifiée.

## 6. Infrastructure
- Le module possède un `DbContext` dédié si persistance relationnelle.
- Le `DbContext` utilise un schéma propre au module.
- Les mappings ignorent `DomainEvents` et autres propriétés dérivées.
- Les value objects persistés sont mappés explicitement.
- Les migrations sont localisées dans l'infrastructure du module.

## 7. Messaging / intégration
- Les domain events utiles à l'intégration sont capturés par l'outbox.
- Les records d'outbox sont spécifiques au module.
- La routing key est cohérente avec le nom du boundary.
- Le processor et le publisher sont alignés avec les abstractions partagées.

## 8. Point d'entrée
- L'API ou le Portal convertit les entrées externes vers les requêtes applicatives.
- Le point d'entrée ne contient pas de logique métier centrale.
- Les erreurs applicatives sont au moins gérées de manière cohérente.

## 9. Tests
- Les invariants critiques du domaine sont couverts.
- Les handlers importants sont testés sur leur orchestration.
- Les fakes de test restent simples et ciblés.

## 10. Documentation du modèle
- Les conventions observées sont distinguées des hypothèses.
- Les écarts par rapport à un modèle existant sont signalés.
- Les questions ouvertes sont listées explicitement.
- Le module documente ses bounded contexts, use cases, règles métier et dépendances externes.
