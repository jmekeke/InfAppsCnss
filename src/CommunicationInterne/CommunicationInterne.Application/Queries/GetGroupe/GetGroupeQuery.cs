using MDiator;
using Cnss.Metier.CommunicationInterne.Domain.Repositories;
using Cnss.Metier.Shared.Domain.Common;

namespace Cnss.Metier.CommunicationInterne.Application.Queries.GetGroupe;

public record GetGroupeQuery(Guid GroupeId) : IMDiatorRequest<Result<GroupeDiffusionDto>>;

public record GroupeDiffusionDto(
    Guid Id,
    string Nom,
    string? Description,
    string TypeGroupe,
    Guid CreateurId,
    bool EstActif,
    int NombreMembres,
    IReadOnlyList<MembreGroupeDto> Membres);

public record MembreGroupeDto(Guid AgentId, DateTime DateAjout);

public class GetGroupeHandler(
    IGroupeDiffusionRepository groupeRepo) : IMDiatorHandler<GetGroupeQuery, Result<GroupeDiffusionDto>>
{
    public async Task<Result<GroupeDiffusionDto>> Handle(GetGroupeQuery query, CancellationToken ct)
    {
        var g = await groupeRepo.GetWithMembresAsync(query.GroupeId, ct);
        if (g is null)
            return Result.NotFound<GroupeDiffusionDto>();

        return Result.Success(new GroupeDiffusionDto(
            g.Id,
            g.Nom,
            g.Description,
            g.TypeGroupe.ToString(),
            g.CreateurId,
            g.EstActif,
            g.CompterDestinataires(),
            g.Membres.Select(m => new MembreGroupeDto(m.AgentId, m.DateAjout)).ToList()));
    }
}
