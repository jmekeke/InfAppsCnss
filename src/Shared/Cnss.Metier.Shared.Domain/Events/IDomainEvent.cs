using System;

namespace Cnss.Metier.Shared.Domain.Events;

public interface IDomainEvent
{
    DateTime OccurredOn { get; }
}
