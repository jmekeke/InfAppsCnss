using CommunicationInterne.Application.Ports;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace CommunicationInterne.Infrastructure.ExternalServices;

/// <summary>
/// Adaptateur ACL — interroge la base SQL Server du bounded context Agent.
/// Connexion configurée via la clé "AgentDb" dans appsettings.
/// </summary>
internal sealed class AgentSqlQueryService(IConfiguration config) : IAgentQueryService
{
    private SqlConnection CreateConnection() =>
        new(config.GetConnectionString("AgentDb")
            ?? throw new InvalidOperationException("Connection string 'AgentDb' not found."));

    public async Task<AgentDto?> GetAgentAsync(Guid agentId, CancellationToken ct = default)
    {
        const string sql = """
            SELECT a.Id, a.Matricule, a.NomComplet, a.Email, a.Telephone,
                   d.Nom AS DirectionNom, g.Libelle AS GradeLibelle, cg.Libelle AS CategorieGradeLibelle
            FROM Agents a
            LEFT JOIN Directions d ON d.Id = a.DirectionId
            LEFT JOIN Grades g ON g.Id = a.GradeId
            LEFT JOIN CategorieGrades cg ON cg.Id = g.CategorieGradeId
            WHERE a.Id = @agentId
            """;

        await using var conn = CreateConnection();
        await conn.OpenAsync(ct);
        await using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@agentId", agentId);

        await using var reader = await cmd.ExecuteReaderAsync(ct);
        if (!await reader.ReadAsync(ct)) return null;

        return ReadAgent(reader);
    }

    public async Task<List<AgentDto>> GetAgentsByDirectionAsync(Guid directionId, CancellationToken ct = default)
    {
        const string sql = """
            SELECT a.Id, a.Matricule, a.NomComplet, a.Email, a.Telephone,
                   d.Nom AS DirectionNom, g.Libelle AS GradeLibelle, cg.Libelle AS CategorieGradeLibelle
            FROM Agents a
            LEFT JOIN Directions d ON d.Id = a.DirectionId
            LEFT JOIN Grades g ON g.Id = a.GradeId
            LEFT JOIN CategorieGrades cg ON cg.Id = g.CategorieGradeId
            WHERE a.DirectionId = @directionId
            ORDER BY a.NomComplet
            """;

        return await QueryListAsync(sql, "@directionId", directionId, ct);
    }

    public async Task<List<AgentDto>> GetAgentsByGradeAsync(Guid gradeId, CancellationToken ct = default)
    {
        const string sql = """
            SELECT a.Id, a.Matricule, a.NomComplet, a.Email, a.Telephone,
                   d.Nom AS DirectionNom, g.Libelle AS GradeLibelle, cg.Libelle AS CategorieGradeLibelle
            FROM Agents a
            LEFT JOIN Directions d ON d.Id = a.DirectionId
            LEFT JOIN Grades g ON g.Id = a.GradeId
            LEFT JOIN CategorieGrades cg ON cg.Id = g.CategorieGradeId
            WHERE a.GradeId = @gradeId
            ORDER BY a.NomComplet
            """;

        return await QueryListAsync(sql, "@gradeId", gradeId, ct);
    }

    private async Task<List<AgentDto>> QueryListAsync(string sql, string paramName, Guid paramValue, CancellationToken ct)
    {
        await using var conn = CreateConnection();
        await conn.OpenAsync(ct);
        await using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue(paramName, paramValue);

        await using var reader = await cmd.ExecuteReaderAsync(ct);
        var result = new List<AgentDto>();
        while (await reader.ReadAsync(ct))
            result.Add(ReadAgent(reader));

        return result;
    }

    private static AgentDto ReadAgent(SqlDataReader r) => new(
        Id: r.GetGuid(r.GetOrdinal("Id")),
        Matricule: r.GetString(r.GetOrdinal("Matricule")),
        NomComplet: r.GetString(r.GetOrdinal("NomComplet")),
        Email: r.IsDBNull(r.GetOrdinal("Email")) ? null : r.GetString(r.GetOrdinal("Email")),
        Telephone: r.IsDBNull(r.GetOrdinal("Telephone")) ? null : r.GetString(r.GetOrdinal("Telephone")),
        DirectionNom: r.IsDBNull(r.GetOrdinal("DirectionNom")) ? null : r.GetString(r.GetOrdinal("DirectionNom")),
        GradeLibelle: r.IsDBNull(r.GetOrdinal("GradeLibelle")) ? null : r.GetString(r.GetOrdinal("GradeLibelle")),
        CategorieGradeLibelle: r.IsDBNull(r.GetOrdinal("CategorieGradeLibelle")) ? null : r.GetString(r.GetOrdinal("CategorieGradeLibelle"))
    );
}
