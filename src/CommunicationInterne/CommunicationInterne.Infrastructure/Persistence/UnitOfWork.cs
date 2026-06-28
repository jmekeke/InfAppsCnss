using Cnss.Metier.Shared.Application.Abstractions;

namespace Cnss.Metier.CommunicationInterne.Infrastructure.Persistence;

internal sealed class UnitOfWork(CommunicationInterneDbContext context) : IUnitOfWork
{
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => context.SaveChangesAsync(cancellationToken);
}
