using Cnss.Metier.CommunicationInterne.Domain.Aggregats;
using Cnss.Metier.CommunicationInterne.Domain.Repositories;
using Cnss.Metier.CommunicationInterne.Infrastructure.Persistence;
using Cnss.Metier.Shared.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace Cnss.Metier.CommunicationInterne.Infrastructure.Persistence.Repositories;

internal sealed class MessageInterneRepository(CommunicationInterneDbContext db) : IMessageInterneRepository
{
    public Task<MessageInterne?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => db.Messages.FirstOrDefaultAsync(m => m.Id == id, ct);

    public Task<MessageInterne?> GetWithCanauxAsync(Guid id, CancellationToken ct = default)
        => db.Messages.FirstOrDefaultAsync(m => m.Id == id, ct);

    public async Task<PagedResult<MessageInterne>> GetAllAsync(int page = 1, int pageSize = 20, string? search = null, CancellationToken ct = default)
    {
        if (page < 1) page = 1;
        if (pageSize is < 1 or > 200) pageSize = 20;

        var query = db.Messages.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(m => m.Objet.Contains(search));

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(m => m.DateCreation)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return new PagedResult<MessageInterne>(items, total, page, pageSize);
    }

    public async Task<List<MessageInterne>> GetByAuteurAsync(Guid auteurId, CancellationToken ct = default)
        => await db.Messages.Where(m => m.AuteurId == auteurId).ToListAsync(ct);

    public void Add(MessageInterne entity) => db.Messages.Add(entity);

    public void Remove(MessageInterne entity) => db.Messages.Remove(entity);

    public Task CommitAsync(CancellationToken ct = default) => db.SaveChangesAsync(ct);
}
