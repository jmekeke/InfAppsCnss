using Cnss.Metier.Shared.Domain.Events;

namespace Cnss.Metier.CommunicationInterne.Domain.Events;

public sealed record DossierDiffusionLanceEvent(Guid DossierId, Guid MessageId, int NombreDestinataires) : DomainEvent;

public sealed record EnvoiEnregistreEvent(Guid DossierId, Guid AgentId, Enums.TypeCanal Canal, Enums.StatutEnvoi Statut) : DomainEvent;
