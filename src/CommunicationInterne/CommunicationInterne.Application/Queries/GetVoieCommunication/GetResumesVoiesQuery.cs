using MDiator;
using Cnss.Metier.CommunicationInterne.Domain.Repositories;
using Cnss.Metier.Shared.Domain.Common;

namespace Cnss.Metier.CommunicationInterne.Application.Queries.GetVoieCommunication;

/// <summary>
/// Résumé de contact pour un agent : meilleur téléphone actif + meilleur e-mail actif.
/// Utilisé pour enrichir les colonnes Telephone/Email des listes d'agents.
/// </summary>
public record ResumeVoieDto(int AgentIdRh, string? TelephoneActif, string? EmailProfActif);

/// <summary>
/// Retourne le résumé de voie de communication pour TOUS les agents enregistrés,
/// en une seule requête PostgreSQL.
/// </summary>
public record GetResumesVoiesQuery() : IMDiatorRequest<Result<List<ResumeVoieDto>>>;

public class GetResumesVoiesHandler(IVoieCommunicationRepository voieRepo)
    : IMDiatorHandler<GetResumesVoiesQuery, Result<List<ResumeVoieDto>>>
{
    public async Task<Result<List<ResumeVoieDto>>> Handle(GetResumesVoiesQuery query, CancellationToken ct)
    {
        var resumes = await voieRepo.GetResumesAsync(ct);
        return Result.Success(
            resumes.Select(r => new ResumeVoieDto(r.AgentIdRh, r.TelephoneActif, r.EmailProfActif)).ToList()
        );
    }
}
