using Cnss.Metier.CommunicationInterne.Domain.Repositories;
using Cnss.Metier.Shared.Domain.Common;
using MDiator;

namespace Cnss.Metier.CommunicationInterne.Application.Queries.ListerGroupes;

public record ListerGroupesQuery(
    int Page = 1,
    int PageSize = 20,
    string? Search = null) : IMDiatorRequest<Result<PagedResult<GroupeResumDto>>>;

public record GroupeResumDto(
    Guid Id,
    string Nom,
    string? Description,
    string Type,
    DateTime DateCreation,
    bool EstActif,
    int NombreMembres,
    string? CritereType,
    string? CritereValeur);

public class ListerGroupesHandler(
    IGroupeDiffusionRepository groupeRepo) : IMDiatorHandler<ListerGroupesQuery, Result<PagedResult<GroupeResumDto>>>
{
    public async Task<Result<PagedResult<GroupeResumDto>>> Handle(ListerGroupesQuery query, CancellationToken ct)
    {
        var paged = await groupeRepo.GetAllAsync(query.Page, query.PageSize, query.Search, ct);
        var dtos = paged.Items.Select(g => new GroupeResumDto(
            g.Id,
            g.Nom,
            g.Description,
            g.TypeGroupe.ToString(),
            g.DateCreation,
            g.EstActif,
            g.CompterDestinataires(),
            g.CritereType,
            g.CritereValeur)).ToList();
        return Result.Success(new PagedResult<GroupeResumDto>(dtos, paged.TotalCount, paged.Page, paged.PageSize));
    }
}
