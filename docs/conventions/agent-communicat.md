## Objectif du projet
Construire le module **Communication interne** de la **CNSS RDC** selon une approche :
Le module doit permettre :
- la gestion des agents destinataires ;
- la gestion des groupes de diffusion ;
- la création et la validation des messages internes ;
- la diffusion multicanale (Email, SMS, WhatsApp, canaux internes futurs) ;
- le suivi des diffusions ;
- l’archivage ;
- le reporting simple.

---

## Portée fonctionnelle
Le périmètre initial couvre les fonctionnalités suivantes :

1. Gestion des agents destinataires
2. Gestion des groupes de diffusion
3. Création de message interne
4. Soumission à validation
5. Validation / rejet
6. Planification et lancement de diffusion
7. Suivi des statuts de diffusion
8. Archivage logique
9. Reporting et tableau de bord simple

---
## Règles métier obligatoires
Les règles suivantes doivent être respectées dans le domaine ou l’application :

1. Un message peut être :
   - Brouillon
   - EnAttenteValidation
   - Valide
   - Rejete
   - Programme
   - Diffuse

2. Un message institutionnel ou sensible ne doit pas être diffusé sans validation préalable.

3. Un agent inactif ne doit jamais être ciblé par une diffusion.

4. Toute diffusion doit être historisée.

5. Un message déjà diffusé ne doit pas être supprimé physiquement.

6. Avant toute diffusion, le système doit pouvoir calculer le nombre de destinataires ciblés.

7. Une diffusion peut utiliser un ou plusieurs canaux.

8. Les groupes peuvent être manuels ou dynamiques, mais l’implémentation initiale peut commencer par les groupes manuels.

9. Toute action critique doit être traçable.

10. Les suppressions physiques doivent être évitées dès lors qu’un élément a un impact métier, fonctionnel, d’audit ou de reporting.

---
## Couche API

### Responsabilités
- exposition REST
- mapping HTTP vers Application
- validation d’entrée simple
- gestion des codes de retour
- sécurité
- OpenAPI

### Règles
- Les controllers ou endpoints doivent rester fins
- Toute logique métier doit être déléguée à Application
- Les réponses HTTP doivent être cohérentes
- Les erreurs doivent être gérées de manière centralisée
- OpenAPI doit être activé

### Endpoints minimums attendus
Pour l'objet Groupes, nous pouvons avoir par exemple :
- `POST /api/groupes`
- `POST /api/groupes/{id}/members`
- `DELETE /api/groupes/{id}/members/{agentId}`
- `GET /api/groupes/{id}`
- `GET /api/groupes`

---

## Intégrations externes

### Email
Prévoir une abstraction `IEmailSender`.

### SMS
Prévoir une abstraction `ISmsSender`.

### WhatsApp
Prévoir une abstraction `IWhatsAppSender`.

### Stratégie initiale
Commencer avec :
- une implémentation fake ou mockable pour les tests ;
- éventuellement une implémentation de démonstration basée sur logs.

Ne pas bloquer le projet sur l’intégration réelle SMTP ou passerelle SMS dans la première itération.



---

## Priorités métier CNSS
Toujours privilégier :
- la traçabilité ;
- la fiabilité ;
- la sécurité ;
- la maintenabilité ;
- la séparation claire des responsabilités ;
- la possibilité d’audit ;
- l’évolutivité vers d’autres canaux et d’autres modules.
