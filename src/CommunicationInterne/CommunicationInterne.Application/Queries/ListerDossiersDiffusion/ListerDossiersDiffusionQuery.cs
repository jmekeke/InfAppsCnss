using Cnss.Metier.CommunicationInterne.Domain.Repositories;
using Cnss.Metier.Shared.Domain.Common;
using MDiator;

namespace Cnss.Metier.CommunicationInterne.Application.Queries.ListerDossiersDiffusion;

public record ListerDossiersDiffusionQuery(
    Guid MessageId) : IMDiatorRequest<Result<List<DossierDiffusionResumDto>>>;

public record DossierDiffusionResumDto(
    Guid Id,
    Guid DeclencheurId,
    DateTime DateLancement,
    int NombreDestinatairesTotaux,
    int EnvoisReussis,
    int EnvoisEchoues);

public class ListerDossiersDiffusionHandler(
    IDossierDiffusionRepository dossierRepo) : IMDiatorHandler<ListerDossiersDiffusionQuery, Result<List<DossierDiffusionResumDto>>>
{
    public async Task<Result<List<DossierDiffusionResumDto>>> Handle(ListerDossiersDiffusionQuery query, CancellationToken ct)
    {
        var dossiers = await dossierRepo.GetByMessageAsync(query.MessageId, ct);
        var dtos = dossiers.Select(d => new DossierDiffusionResumDto(
            d.Id,
            d.DeclencheurId,
            d.DateLancement,
            d.NombreDestinatairesTotaux,
            d.CompterEnvoisReussis(),
            d.CompterEnvoisEchoues())).ToList();
        return Result.Success(dtos);
    }
}
