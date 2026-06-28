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
    string? CritereType,
    string? CritereValeur,
    IReadOnlyList<MembreGroupeDto> Membres);

public record MembreGroupeDto(Guid Id, int? AgentIdRh, DateTime DateAjout);

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
            g.CritereType,
            g.CritereValeur,
            g.Membres.Select(m => new MembreGroupeDto(m.AgentId, TryExtractAgentIdRh(m.AgentId), m.DateAjout)).ToList()));
    }

    /// <summary>
    /// Tente d'extraire l'identifiant RH (int) depuis un GUID déterministe.
    /// Convention : int 3467 → 00000000-0000-0000-0000-000000003467.
    /// Retourne null si le GUID ne suit pas ce pattern.
    /// </summary>
    private static int? TryExtractAgentIdRh(Guid guid)
    {
        var s = guid.ToString(); // xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx
        if (!s.StartsWith("00000000-0000-0000-0000-")) return null;
        var last = s[24..]; // 12 derniers caractères décimaux
        return int.TryParse(last, out var id) ? id : null;
    }
}
