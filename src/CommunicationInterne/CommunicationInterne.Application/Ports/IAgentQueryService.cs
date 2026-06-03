namespace CommunicationInterne.Application.Ports;

/// <summary>
/// Port ACL vers le bounded context Agent (base SQL Server externe).
/// Ne jamais importer la classe Agent dans ce module — référencer uniquement par AgentId.
/// </summary>
public interface IAgentQueryService
{
    Task<AgentDto?> GetAgentAsync(Guid agentId, CancellationToken ct = default);
    Task<List<AgentDto>> GetAgentsByDirectionAsync(Guid directionId, CancellationToken ct = default);
    Task<List<AgentDto>> GetAgentsByGradeAsync(Guid gradeId, CancellationToken ct = default);
}

/// <summary>
/// Projection minimale de l'agrégat Agent pour les besoins de CommunicationInterne.
/// </summary>
public record AgentDto(
    Guid Id,
    string Matricule,
    string NomComplet,
    string? Email,
    string? Telephone,
    string? DirectionNom,
    string? GradeLibelle,
    string? CategorieGradeLibelle
);
