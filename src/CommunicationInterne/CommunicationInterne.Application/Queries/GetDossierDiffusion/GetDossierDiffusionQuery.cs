using MDiator;
using Cnss.Metier.CommunicationInterne.Domain.Repositories;
using Cnss.Metier.Shared.Domain.Common;

namespace Cnss.Metier.CommunicationInterne.Application.Queries.GetDossierDiffusion;

public record GetDossierDiffusionQuery(Guid DossierId) : IMDiatorRequest<Result<DossierDiffusionDto>>;

public record DossierDiffusionDto(
    Guid Id,
    Guid MessageInterneId,
    Guid DeclencheurId,
    DateTime DateLancement,
    int NombreDestinatairesTotaux,
    int EnvoisReussis,
    int EnvoisEchoues,
    IReadOnlyList<LigneDiffusionDto> Lignes);

public record LigneDiffusionDto(
    Guid AgentId,
    string Canal,
    string Statut,
    DateTime DateEnvoi,
    string? MessageErreur);

public class GetDossierDiffusionHandler(
    IDossierDiffusionRepository dossierRepo) : IMDiatorHandler<GetDossierDiffusionQuery, Result<DossierDiffusionDto>>
{
    public async Task<Result<DossierDiffusionDto>> Handle(GetDossierDiffusionQuery query, CancellationToken ct)
    {
        var d = await dossierRepo.GetWithLignesAsync(query.DossierId, ct);
        if (d is null)
            return Result.NotFound<DossierDiffusionDto>();

        return Result.Success(new DossierDiffusionDto(
            d.Id,
            d.MessageInterneId,
            d.DeclencheurId,
            d.DateLancement,
            d.NombreDestinatairesTotaux,
            d.CompterEnvoisReussis(),
            d.CompterEnvoisEchoues(),
            d.Lignes.Select(l => new LigneDiffusionDto(
                l.AgentId,
                l.Canal.ToString(),
                l.Statut.ToString(),
                l.DateEnvoi,
                l.MessageErreur)).ToList()));
    }
}
