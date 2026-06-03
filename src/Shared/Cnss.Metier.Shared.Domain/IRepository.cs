namespace Cnss.Metier.Shared.Domain;

/// <summary>
/// Interface de base pour les repositories.
/// Chaque agrégat définit son propre repository typé.
/// </summary>
public interface IRepository<T> where T : AggregateRoot
{
    Task<T?> GetByIdAsync(Guid id, CancellationToken ct = default);
    void Add(T entity);
    void Remove(T entity);
}
