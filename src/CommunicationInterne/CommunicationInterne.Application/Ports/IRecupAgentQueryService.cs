namespace Cnss.Metier.CommunicationInterne.Application.Ports;

/// <summary>
/// Port ACL vers la base RH_DB (SQL Server) — table Agent.
/// </summary>
public interface IRecupAgentQueryService
{
    Task<RecupAgentDto?> GetAgentAsync(int agentId, CancellationToken ct = default);
    Task<List<RecupAgentDto>> ListerAgentsAsync(CancellationToken ct = default);
    Task<List<RecupAgentDto>> RechercherAgentsAsync(string? nom, string? entiteLibelle, CancellationToken ct = default);
}

/// <summary>
/// Projection de la table Agent (RH_DB) pour les besoins de CommunicationInterne.
/// </summary>
public record RecupAgentDto(
    int Id,
    string? Matricule,
    string? Nom,
    string? Postnom,
    string? Prenom,
    string? EmailProfessionnel,
    string? EmailPersonnel,
    string? Telephone,
    string? EntiteLibelle,
    string? GradeLibelle,
    string? FonctionLibelle,
    string Categorie,
    string EtatCivil,
    string Sexe,
    DateTime? DateEngagement,
    DateTime? DateNaissance
);
