using Cnss.Metier.CommunicationInterne.Domain.Aggregats;
using Cnss.Metier.Shared.Domain;

namespace Cnss.Metier.CommunicationInterne.Domain.Repositories;

public interface IDossierDiffusionRepository : IRepository<DossierDiffusion>
{
    Task<DossierDiffusion?> GetWithLignesAsync(Guid id, CancellationToken ct = default);
    Task<List<DossierDiffusion>> GetByMessageAsync(Guid messageInterneId, CancellationToken ct = default);
    Task CommitAsync(CancellationToken ct = default);
}
