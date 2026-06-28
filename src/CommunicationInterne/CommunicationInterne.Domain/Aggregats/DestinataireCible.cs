using Cnss.Metier.CommunicationInterne.Domain.Enums;

namespace Cnss.Metier.CommunicationInterne.Domain.Aggregats;

/// <summary>
/// Entité — Destinataire cible d'un message interne.
/// Un message peut avoir plusieurs cibles de différents types.
/// </summary>
public class DestinataireCible
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public Guid MessageInterneId { get; init; }

    /// <summary>Type de destinataire (agent individu, groupe, direction, tous).</summary>
    public TypeDestinataire Type { get; init; }

    /// <summary>
    /// Identifiant de référence selon le type :
    /// - AgentIndividu : Id RH de l'agent (int converti en string)
    /// - GroupeDiffusion : Guid du groupe de diffusion
    /// - DirectionService : code entité RH
    /// - TousLesAgents : null (non requis)
    /// </summary>
    public string? ReferenceId { get; init; }

    /// <summary>Libellé affiché pour l'utilisateur (nom agent, nom groupe, libellé entité…).</summary>
    public string Libelle { get; init; } = string.Empty;
}
