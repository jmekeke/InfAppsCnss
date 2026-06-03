using Cnss.Metier.CommunicationInterne.Domain.Aggregats;
using Cnss.Metier.CommunicationInterne.Domain.Repositories;
using Cnss.Metier.CommunicationInterne.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Cnss.Metier.CommunicationInterne.Infrastructure.Persistence.Repositories;

internal sealed class DossierDiffusionRepository(CommunicationInterneDbContext db) : IDossierDiffusionRepository
{
    public Task<DossierDiffusion?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => db.DossiersDiffusion.FirstOrDefaultAsync(d => d.Id == id, ct);

    public Task<DossierDiffusion?> GetWithLignesAsync(Guid id, CancellationToken ct = default)
        => db.DossiersDiffusion
            .Include(d => d.Lignes)
            .FirstOrDefaultAsync(d => d.Id == id, ct);

    public async Task<List<DossierDiffusion>> GetByMessageAsync(Guid messageInterneId, CancellationToken ct = default)
        => await db.DossiersDiffusion
            .Where(d => d.MessageInterneId == messageInterneId)
            .OrderByDescending(d => d.DateLancement)
            .ToListAsync(ct);

    public void Add(DossierDiffusion entity) => db.DossiersDiffusion.Add(entity);

    public void Remove(DossierDiffusion entity) => db.DossiersDiffusion.Remove(entity);

    public Task CommitAsync(CancellationToken ct = default) => db.SaveChangesAsync(ct);
}
