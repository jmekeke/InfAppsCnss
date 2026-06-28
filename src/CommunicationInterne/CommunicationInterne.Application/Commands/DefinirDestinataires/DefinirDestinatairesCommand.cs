using MDiator;
using Cnss.Metier.CommunicationInterne.Domain.Enums;
using Cnss.Metier.Shared.Domain.Common;

namespace Cnss.Metier.CommunicationInterne.Application.Commands.DefinirDestinataires;

/// <summary>Représente un destinataire cible pour la commande DefinirDestinataires.</summary>
public record DestinataireCibleDto(
    TypeDestinataire Type,
    /// <summary>Id agent RH (AgentIndividu), Guid groupe (GroupeDiffusion), code entité (DirectionService), null (TousLesAgents).</summary>
    string? ReferenceId,
    string Libelle);

public record DefinirDestinatairesCommand(
    Guid MessageId,
    List<DestinataireCibleDto> Destinataires,
    // Conservé pour rétrocompatibilité (Programmer/LancerDiffusion)
    List<Guid>? GroupeIds = null,
    string UserId = "",
    string? UserName = null) : IMDiatorRequest<Result>;
