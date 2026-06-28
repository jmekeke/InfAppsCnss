using Cnss.Metier.CommunicationInterne.Domain.Aggregats;
using Cnss.Metier.CommunicationInterne.Domain.Repositories;
using Cnss.Metier.CommunicationInterne.Infrastructure.Persistence;
using Cnss.Metier.Shared.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace Cnss.Metier.CommunicationInterne.Infrastructure.Persistence.Repositories;

internal sealed class GroupeDiffusionRepository(CommunicationInterneDbContext db) : IGroupeDiffusionRepository
{
    public Task<GroupeDiffusion?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => db.Groupes.FirstOrDefaultAsync(g => g.Id == id, ct);

    public Task<GroupeDiffusion?> GetWithMembresAsync(Guid id, CancellationToken ct = default)
        => db.Groupes
            .Include(g => g.Membres)
            .FirstOrDefaultAsync(g => g.Id == id, ct);

    public async Task<PagedResult<GroupeDiffusion>> GetAllAsync(int page = 1, int pageSize = 20, string? search = null, CancellationToken ct = default)
    {
        if (page < 1) page = 1;
        if (pageSize is < 1 or > 200) pageSize = 20;

        var query = db.Groupes.Include(g => g.Membres).AsNoTracking();

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(g => EF.Functions.ILike(g.Nom, $"%{search}%"));

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderBy(g => g.Nom)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return new PagedResult<GroupeDiffusion>(items, total, page, pageSize);
    }

    public void Add(GroupeDiffusion entity) => db.Groupes.Add(entity);

    public void Remove(GroupeDiffusion entity) => db.Groupes.Remove(entity);

    public Task CommitAsync(CancellationToken ct = default) => db.SaveChangesAsync(ct);
}
