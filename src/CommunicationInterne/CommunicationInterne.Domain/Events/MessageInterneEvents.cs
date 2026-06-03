using Cnss.Metier.Shared.Domain.Events;

namespace Cnss.Metier.CommunicationInterne.Domain.Events;

public sealed record MessageInterneCreeeEvent(Guid MessageId, string Objet) : DomainEvent;

public sealed record MessageInternesoumisAValidationEvent(Guid MessageId) : DomainEvent;

public sealed record MessageInterneValideEvent(Guid MessageId, Guid ValidateurId) : DomainEvent;

public sealed record MessageInterneRejeteEvent(Guid MessageId, Guid ValidateurId, string Motif) : DomainEvent;

public sealed record MessageInterneProgrammeEvent(Guid MessageId, DateTime DateProgrammee) : DomainEvent;

public sealed record MessageInterneDiffuseEvent(Guid MessageId, DateTime DateDiffusion) : DomainEvent;
