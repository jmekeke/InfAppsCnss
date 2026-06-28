using MDiator;
using Cnss.Metier.CommunicationInterne.Application.Ports;
using Cnss.Metier.Shared.Domain.Common;

namespace Cnss.Metier.CommunicationInterne.Application.Queries.RecupAgent;

// ── Récupérer un agent par son Id ──────────────────────────────────────────

public record RecupAgentQuery(int AgentId) : IMDiatorRequest<Result<RecupAgentDto>>;

public class RecupAgentHandler(IRecupAgentQueryService agentService)
    : IMDiatorHandler<RecupAgentQuery, Result<RecupAgentDto>>
{
    public async Task<Result<RecupAgentDto>> Handle(RecupAgentQuery query, CancellationToken ct)
    {
        var agent = await agentService.GetAgentAsync(query.AgentId, ct);
        if (agent is null)
            return Result.NotFound<RecupAgentDto>();

        return Result.Success(agent);
    }
}

// ── Lister tous les agents ─────────────────────────────────────────────────

public record ListerAgentsQuery() : IMDiatorRequest<Result<List<RecupAgentDto>>>;

public class ListerAgentsHandler(IRecupAgentQueryService agentService)
    : IMDiatorHandler<ListerAgentsQuery, Result<List<RecupAgentDto>>>
{
    public async Task<Result<List<RecupAgentDto>>> Handle(ListerAgentsQuery query, CancellationToken ct)
    {
        var agents = await agentService.ListerAgentsAsync(ct);
        return Result.Success(agents);
    }
}

// ── Rechercher des agents par nom et/ou entité ─────────────────────────────

public record RechercherAgentsQuery(string? Nom, string? EntiteLibelle)
    : IMDiatorRequest<Result<List<RecupAgentDto>>>;

public class RechercherAgentsHandler(IRecupAgentQueryService agentService)
    : IMDiatorHandler<RechercherAgentsQuery, Result<List<RecupAgentDto>>>
{
    public async Task<Result<List<RecupAgentDto>>> Handle(RechercherAgentsQuery query, CancellationToken ct)
    {
        var agents = await agentService.RechercherAgentsAsync(query.Nom, query.EntiteLibelle, ct);
        return Result.Success(agents);
    }
}
