using Cnss.Metier.CommunicationInterne.Domain.Aggregats;
using Cnss.Metier.Shared.Domain;
using Cnss.Metier.Shared.Domain.Common;

namespace Cnss.Metier.CommunicationInterne.Domain.Repositories;

public interface IGroupeDiffusionRepository : IRepository<GroupeDiffusion>
{
    Task<GroupeDiffusion?> GetWithMembresAsync(Guid id, CancellationToken ct = default);
    Task<PagedResult<GroupeDiffusion>> GetAllAsync(int page = 1, int pageSize = 20, string? search = null, CancellationToken ct = default);
    Task CommitAsync(CancellationToken ct = default);
}
