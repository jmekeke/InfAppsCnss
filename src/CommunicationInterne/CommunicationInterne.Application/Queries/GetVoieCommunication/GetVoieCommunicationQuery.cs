using MDiator;
using Cnss.Metier.CommunicationInterne.Domain.Aggregats;
using Cnss.Metier.CommunicationInterne.Domain.Enums;
using Cnss.Metier.CommunicationInterne.Domain.Repositories;
using Cnss.Metier.Shared.Domain.Common;

namespace Cnss.Metier.CommunicationInterne.Application.Queries.GetVoieCommunication;

// ── DTOs ─────────────────────────────────────────────────────────────────────

public record VoieTelephoneResultDto(TypeVoieTelephone Type, string Numero, bool EstActif, DateTime DateModification);

public record VoieEmailResultDto(TypeVoieEmail Type, string Adresse, bool EstActif, DateTime DateModification);

/// <summary>
/// Entrée d'historique unifiée pour téléphone et e-mail.
/// <see cref="Canal"/> = Telephone | Email.
/// <see cref="TypeVoie"/> = nom de l'enum (ex. "Appel", "Professionnel").
/// <see cref="Valeur"/> = numéro ou adresse.
/// </summary>
public record HistoriqueVoieDto(
    CanalVoie Canal,
    string TypeVoie,
    string Valeur,
    bool EstActif,
    ActionHistorique Action,
    string ModifiePar,
    DateTime DateAction);

public record VoieCommunicationDto(
    Guid   Id,
    int    AgentIdRh,
    string Matricule,
    IReadOnlyList<VoieTelephoneResultDto> Telephones,
    IReadOnlyList<VoieEmailResultDto>     Emails,
    IReadOnlyList<HistoriqueVoieDto>      Historique
);

// ── Query voie complète ───────────────────────────────────────────────────────

public record GetVoieCommunicationQuery(int AgentIdRh)
    : IMDiatorRequest<Result<VoieCommunicationDto>>;

public class GetVoieCommunicationHandler(IVoieCommunicationRepository voieRepo)
    : IMDiatorHandler<GetVoieCommunicationQuery, Result<VoieCommunicationDto>>
{
    public async Task<Result<VoieCommunicationDto>> Handle(GetVoieCommunicationQuery query, CancellationToken ct)
    {
        var voie = await voieRepo.GetByAgentIdRhAsync(query.AgentIdRh, ct);
        if (voie is null) return Result.NotFound<VoieCommunicationDto>();

        return Result.Success(new VoieCommunicationDto(
            Id:        voie.Id,
            AgentIdRh: voie.AgentIdRh,
            Matricule: voie.Matricule,
            Telephones: voie.Telephones
                .Select(t => new VoieTelephoneResultDto(t.Type, t.Numero, t.EstActif, t.DateModification))
                .ToList(),
            Emails: voie.Emails
                .Select(e => new VoieEmailResultDto(e.Type, e.Adresse, e.EstActif, e.DateModification))
                .ToList(),
            Historique: voie.Historique
                .OrderByDescending(h => h.DateAction)
                .Select(h => new HistoriqueVoieDto(
                    h.Canal, h.TypeVoie, h.Valeur, h.EstActif, h.Action, h.ModifiePar, h.DateAction))
                .ToList()
        ));
    }
}

// ── Query historique seul ─────────────────────────────────────────────────────

public record GetHistoriqueVoieQuery(int AgentIdRh, CanalVoie? Canal = null)
    : IMDiatorRequest<Result<List<HistoriqueVoieDto>>>;

public class GetHistoriqueVoieHandler(IVoieCommunicationRepository voieRepo)
    : IMDiatorHandler<GetHistoriqueVoieQuery, Result<List<HistoriqueVoieDto>>>
{
    public async Task<Result<List<HistoriqueVoieDto>>> Handle(GetHistoriqueVoieQuery query, CancellationToken ct)
    {
        var voie = await voieRepo.GetByAgentIdRhAsync(query.AgentIdRh, ct);
        if (voie is null) return Result.NotFound<List<HistoriqueVoieDto>>();

        var historique = voie.Historique
            .Where(h => query.Canal is null || h.Canal == query.Canal)
            .OrderByDescending(h => h.DateAction)
            .Select(h => new HistoriqueVoieDto(
                h.Canal, h.TypeVoie, h.Valeur, h.EstActif, h.Action, h.ModifiePar, h.DateAction))
            .ToList();

        return Result.Success(historique);
    }
}

