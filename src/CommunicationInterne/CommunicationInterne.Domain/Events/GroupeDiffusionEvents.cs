using Cnss.Metier.Shared.Domain.Events;

namespace Cnss.Metier.CommunicationInterne.Domain.Events;

public sealed record GroupeDiffusionCreeEvent(Guid GroupeId, string Nom) : DomainEvent;

public sealed record MembreAjouteAuGroupeEvent(Guid GroupeId, Guid AgentId) : DomainEvent;

public sealed record MembreRetireDeGroupeEvent(Guid GroupeId, Guid AgentId) : DomainEvent;
