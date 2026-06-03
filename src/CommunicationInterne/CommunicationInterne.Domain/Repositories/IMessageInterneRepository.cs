using Cnss.Metier.CommunicationInterne.Domain.Aggregats;
using Cnss.Metier.Shared.Domain;
using Cnss.Metier.Shared.Domain.Common;

namespace Cnss.Metier.CommunicationInterne.Domain.Repositories;

public interface IMessageInterneRepository : IRepository<MessageInterne>
{
    Task<MessageInterne?> GetWithCanauxAsync(Guid id, CancellationToken ct = default);
    Task<PagedResult<MessageInterne>> GetAllAsync(int page = 1, int pageSize = 20, string? search = null, CancellationToken ct = default);
    Task<List<MessageInterne>> GetByAuteurAsync(Guid auteurId, CancellationToken ct = default);
    Task CommitAsync(CancellationToken ct = default);
}
