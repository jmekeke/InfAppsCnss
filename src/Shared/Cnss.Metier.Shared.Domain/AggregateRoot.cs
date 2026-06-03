using Cnss.Metier.Shared.Domain.Events;

namespace Cnss.Metier.Shared.Domain;

/// <summary>
/// Racine d'agrégat : entité principale qui délimite une frontière de cohérence.
/// Collecte les événements de domaine pour dispatch après persistance.
/// </summary>
public abstract class AggregateRoot : Entity
{
    private readonly List<IDomainEvent> _domainEvents = [];

    protected AggregateRoot() { }

    protected AggregateRoot(Guid id) : base(id) { }

    /// <summary>Événements de domaine en attente de dispatch.</summary>
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    /// <summary>Ajoute un événement de domaine.</summary>
    protected void AddDomainEvent(IDomainEvent domainEvent) => _domainEvents.Add(domainEvent);

    /// <summary>Vide la liste des événements (appelé après dispatch).</summary>
    public void ClearDomainEvents() => _domainEvents.Clear();
}
