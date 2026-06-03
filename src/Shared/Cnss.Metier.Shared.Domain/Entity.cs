namespace Cnss.Metier.Shared.Domain;

/// <summary>
/// Classe de base pour toutes les entités du domaine.
/// Fournit un identifiant Guid et l'égalité par identité.
/// </summary>
public abstract class Entity
{
    public Guid Id { get; init; }

    protected Entity() => Id = Guid.NewGuid();

    protected Entity(Guid id) => Id = id;

    public override bool Equals(object? obj)
    {
        if (obj is not Entity other) return false;
        if (ReferenceEquals(this, other)) return true;
        if (GetType() != other.GetType()) return false;
        return Id == other.Id;
    }

    public override int GetHashCode() => Id.GetHashCode();

    public static bool operator ==(Entity? a, Entity? b)
    {
        if (a is null && b is null) return true;
        if (a is null || b is null) return false;
        return a.Equals(b);
    }

    public static bool operator !=(Entity? a, Entity? b) => !(a == b);
}
