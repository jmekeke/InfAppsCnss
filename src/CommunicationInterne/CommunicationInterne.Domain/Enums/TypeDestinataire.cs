namespace Cnss.Metier.CommunicationInterne.Domain.Enums;

/// <summary>
/// Catégories de destinataires d'un message interne.
/// </summary>
public enum TypeDestinataire
{
    /// <summary>Agent individuel identifié par son Id RH.</summary>
    AgentIndividu = 1,

    /// <summary>Groupe de diffusion prédéfini (GroupeDiffusion.Id).</summary>
    GroupeDiffusion = 2,

    /// <summary>Tous les agents d'une entité (direction, service…) identifiée par son code.</summary>
    DirectionService = 3,

    /// <summary>Tous les agents actifs — pas de référenceId requis.</summary>
    TousLesAgents = 4,
}
