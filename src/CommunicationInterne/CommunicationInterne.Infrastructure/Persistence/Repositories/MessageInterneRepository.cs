using Cnss.Metier.CommunicationInterne.Domain.Aggregats;
using Cnss.Metier.CommunicationInterne.Domain.Repositories;
using Cnss.Metier.CommunicationInterne.Infrastructure.Persistence;
using Cnss.Metier.CommunicationInterne.Infrastructure.Persistence.Configurations;
using Cnss.Metier.Shared.Domain.Common;
using Microsoft.EntityFrameworkCore;
using PieceJointe = Cnss.Metier.CommunicationInterne.Domain.Aggregats.PieceJointe;

namespace Cnss.Metier.CommunicationInterne.Infrastructure.Persistence.Repositories;

internal sealed class MessageInterneRepository(CommunicationInterneDbContext db) : IMessageInterneRepository
{
    public Task<MessageInterne?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => db.Messages.FirstOrDefaultAsync(m => m.Id == id, ct);

    public Task<MessageInterne?> GetWithCanauxAsync(Guid id, CancellationToken ct = default)
        => db.Messages.FirstOrDefaultAsync(m => m.Id == id && !m.EstArchive, ct);

    public async Task<PagedResult<MessageInterne>> GetAllAsync(int page = 1, int pageSize = 20, string? search = null, CancellationToken ct = default)
    {
        if (page < 1) page = 1;
        if (pageSize is < 1 or > 200) pageSize = 20;

        var query = db.Messages.AsNoTracking().Where(m => !m.EstArchive);

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(m => EF.Functions.ILike(m.Objet, $"%{search}%"));

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

    public async Task<Dictionary<Guid, List<string>>> GetCanauxForMessagesAsync(IEnumerable<Guid> messageIds, CancellationToken ct = default)
    {
        var ids = messageIds.ToList();
        var entries = await db.Set<MessageInterneCanalEntry>()
            .AsNoTracking()
            .Where(c => ids.Contains(c.MessageInterneId))
            .ToListAsync(ct);

        return entries
            .GroupBy(c => c.MessageInterneId)
            .ToDictionary(g => g.Key, g => g.Select(c => c.Canal.ToString()).ToList());
    }

    public async Task<List<string>> GetCanauxAsync(Guid messageId, CancellationToken ct = default)
    {
        var entries = await db.Set<MessageInterneCanalEntry>()
            .AsNoTracking()
            .Where(c => c.MessageInterneId == messageId)
            .ToListAsync(ct);
        return entries.Select(c => c.Canal.ToString()).ToList();
    }

    public async Task ReplaceCanauxAsync(Guid messageId, IEnumerable<Domain.Enums.TypeCanal> canaux, CancellationToken ct = default)
    {
        // Supprimer les anciennes entrées
        var existing = await db.Set<MessageInterneCanalEntry>()
            .Where(c => c.MessageInterneId == messageId)
            .ToListAsync(ct);
        db.Set<MessageInterneCanalEntry>().RemoveRange(existing);

        // Ajouter les nouvelles
        foreach (var canal in canaux.Distinct())
            db.Set<MessageInterneCanalEntry>().Add(new MessageInterneCanalEntry
            {
                MessageInterneId = messageId,
                Canal = canal,
            });
    }

    public async Task<List<PieceJointe>> GetPiecesJointesAsync(Guid messageId, CancellationToken ct = default)
        => await db.Set<PieceJointe>()
            .AsNoTracking()
            .Where(p => p.MessageInterneId == messageId)
            .ToListAsync(ct);

    public async Task<List<Guid>> GetGroupeIdsAsync(Guid messageId, CancellationToken ct = default)
    {
        var entries = await db.Set<MessageGroupeDestinataireEntry>()
            .AsNoTracking()
            .Where(g => g.MessageInterneId == messageId)
            .ToListAsync(ct);
        return entries.Select(g => g.GroupeId).ToList();
    }

    public async Task<List<DestinataireCible>> GetDestinatairesAsync(Guid messageId, CancellationToken ct = default)
        => await db.Set<DestinataireCible>()
            .AsNoTracking()
            .Where(d => d.MessageInterneId == messageId)
            .ToListAsync(ct);

    public async Task ReplaceDestinatairesAsync(Guid messageId, IEnumerable<DestinataireCible> destinataires, CancellationToken ct = default)
    {
        var existing = await db.Set<DestinataireCible>()
            .Where(d => d.MessageInterneId == messageId)
            .ToListAsync(ct);
        db.Set<DestinataireCible>().RemoveRange(existing);

        foreach (var dest in destinataires)
            db.Set<DestinataireCible>().Add(new DestinataireCible
            {
                MessageInterneId = messageId,
                Type       = dest.Type,
                ReferenceId = dest.ReferenceId,
                Libelle    = dest.Libelle,
            });
    }

    public void Add(MessageInterne entity) => db.Messages.Add(entity);

    public void Remove(MessageInterne entity) => db.Messages.Remove(entity);

    public Task CommitAsync(CancellationToken ct = default) => db.SaveChangesAsync(ct);
}
