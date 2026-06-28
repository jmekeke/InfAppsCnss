using MDiator;
using Cnss.Metier.CommunicationInterne.Application.Ports;
using Cnss.Metier.CommunicationInterne.Domain.Repositories;
using Cnss.Metier.Shared.Domain.Common;

namespace Cnss.Metier.CommunicationInterne.Application.Queries.ListerGroupesMembresEnrichis;

// ─── DTOs ─────────────────────────────────────────────────────────────────────

public record MembreEnrichiDto(
    int     AgentId,
    Guid    AgentGuid,
    string? Nom,
    string? Postnom,
    string? Prenom,
    string  Sexe,
    string? GradeLibelle,
    string? EntiteLibelle,
    string? EmailPersonnel,
    string? EmailProfessionnel,
    DateTime? DateNaissance,
    DateTime  DateAjout);

public record GroupeAvecMembresEnrichisDto(
    Guid   GroupeId,
    string GroupeLibelle,
    int    NombreMembres,
    IReadOnlyList<MembreEnrichiDto> Membres);

// ─── Query 1 : tous les groupes avec tous leurs membres enrichis ──────────────

public record ListerGroupesMembresEnrichisQuery() : IMDiatorRequest<Result<List<GroupeAvecMembresEnrichisDto>>>;

public class ListerGroupesMembresEnrichisHandler(
    IGroupeDiffusionRepository groupeRepo,
    IRecupAgentQueryService    rhService)
    : IMDiatorHandler<ListerGroupesMembresEnrichisQuery, Result<List<GroupeAvecMembresEnrichisDto>>>
{
    public async Task<Result<List<GroupeAvecMembresEnrichisDto>>> Handle(
        ListerGroupesMembresEnrichisQuery query, CancellationToken ct)
    {
        // Charger tous les groupes actifs avec leurs membres
        var paged = await groupeRepo.GetAllAsync(1, 200, null, ct);

        var result = new List<GroupeAvecMembresEnrichisDto>();

        foreach (var groupe in paged.Items)
        {
            var groupeAvecMembres = await groupeRepo.GetWithMembresAsync(groupe.Id, ct);
            if (groupeAvecMembres is null) continue;

            var membres = new List<MembreEnrichiDto>();
            foreach (var m in groupeAvecMembres.Membres)
            {
                var agentId = GuidRhHelper.GuidToRhId(m.AgentId);
                var rh = await rhService.GetAgentAsync(agentId, ct);
                if (rh is null) continue;

                membres.Add(new MembreEnrichiDto(
                    AgentId:             rh.Id,
                    AgentGuid:           m.AgentId,
                    Nom:                 rh.Nom,
                    Postnom:             rh.Postnom,
                    Prenom:              rh.Prenom,
                    Sexe:                rh.Sexe,
                    GradeLibelle:        rh.GradeLibelle,
                    EntiteLibelle:       rh.EntiteLibelle,
                    EmailPersonnel:      rh.EmailPersonnel,
                    EmailProfessionnel:  rh.EmailProfessionnel,
                    DateNaissance:       rh.DateNaissance,
                    DateAjout:           m.DateAjout));
            }

            result.Add(new GroupeAvecMembresEnrichisDto(
                GroupeId:      groupe.Id,
                GroupeLibelle: groupe.Nom,
                NombreMembres: membres.Count,
                Membres:       membres));
        }

        return Result.Success(result);
    }
}

// ─── Query 2 : un seul agent dans un seul groupe ───────────────────────────────

public record GetMembreGroupeQuery(Guid GroupeId, int AgentIdRh)
    : IMDiatorRequest<Result<MembreEnrichiDto>>;

public class GetMembreGroupeHandler(
    IGroupeDiffusionRepository groupeRepo,
    IRecupAgentQueryService    rhService)
    : IMDiatorHandler<GetMembreGroupeQuery, Result<MembreEnrichiDto>>
{
    public async Task<Result<MembreEnrichiDto>> Handle(GetMembreGroupeQuery query, CancellationToken ct)
    {
        var groupe = await groupeRepo.GetWithMembresAsync(query.GroupeId, ct);
        if (groupe is null)
            return Result.NotFound<MembreEnrichiDto>();

        var agentGuid = GuidRhHelper.RhIdToGuid(query.AgentIdRh);
        var membre = groupe.Membres.FirstOrDefault(m => m.AgentId == agentGuid);
        if (membre is null)
            return Result.NotFound<MembreEnrichiDto>();

        var rh = await rhService.GetAgentAsync(query.AgentIdRh, ct);
        if (rh is null)
            return Result.NotFound<MembreEnrichiDto>();

        return Result.Success(new MembreEnrichiDto(
            AgentId:             rh.Id,
            AgentGuid:           agentGuid,
            Nom:                 rh.Nom,
            Postnom:             rh.Postnom,
            Prenom:              rh.Prenom,
            Sexe:                rh.Sexe,
            GradeLibelle:        rh.GradeLibelle,
            EntiteLibelle:       rh.EntiteLibelle,
            EmailPersonnel:      rh.EmailPersonnel,
            EmailProfessionnel:  rh.EmailProfessionnel,
            DateNaissance:       rh.DateNaissance,
            DateAjout:           membre.DateAjout));
    }
}

// ─── Helpers de conversion Guid ↔ int RH ──────────────────────────────────────

file static class GuidRhHelper
{
    /// <summary>00000000-0000-0000-0000-{id:D12}  →  id (int)</summary>
    public static int GuidToRhId(Guid guid)
        => int.Parse(guid.ToString("N")[20..]);

    /// <summary>id (int)  →  00000000-0000-0000-0000-{id:D12}</summary>
    public static Guid RhIdToGuid(int id)
        => new($"00000000-0000-0000-0000-{id:D12}");
}
