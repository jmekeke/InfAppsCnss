using Cnss.Metier.CommunicationInterne.Application.Ports;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace Cnss.Metier.CommunicationInterne.Infrastructure.ExternalServices;

/// <summary>
/// Adaptateur ACL — interroge la table Agent de la base RH_DB (SQL Server).
/// Connexion configurée via la clé "RhDb" dans appsettings.
/// </summary>
internal sealed class RecupAgentSqlService(IConfiguration config) : IRecupAgentQueryService
{
    private const string ConnKey = "RhDb";

    private SqlConnection CreateConnection() =>
        new(config.GetConnectionString(ConnKey)
            ?? throw new InvalidOperationException($"Connection string '{ConnKey}' not found."));

    private const string SelectBase = """
        SELECT a.Id, a.Matricule, a.Nom, a.Postnom, a.Prenom,
               a.EmailProfessionnel, a.EmailPersonnel, a.Telephoneagent,
               e.Libelle          AS EntiteLibelle,
               g.Descriptiongrade AS GradeLibelle,
               f.Libellefonction  AS FonctionLibelle,
               a.Categorie, a.EtatCivil, a.Sexe,
               a.Dateengagement, a.Datenaissance
        FROM Agent a
        LEFT JOIN Entite   e ON e.Id         = a.EntiteId
        LEFT JOIN Grade    g ON g.IdGrade    = a.GradeId
        LEFT JOIN Fonction f ON f.IdFonction = a.FonctionId
        """;

    public async Task<RecupAgentDto?> GetAgentAsync(int agentId, CancellationToken ct = default)
    {
        var sql = SelectBase + " WHERE a.Id = @agentId";

        await using var conn = CreateConnection();
        await conn.OpenAsync(ct);
        await using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@agentId", agentId);

        await using var reader = await cmd.ExecuteReaderAsync(ct);
        if (!await reader.ReadAsync(ct)) return null;

        return ReadAgent(reader);
    }

    public async Task<List<RecupAgentDto>> ListerAgentsAsync(CancellationToken ct = default)
    {
        var sql = SelectBase + " ORDER BY a.Nom, a.Prenom";

        await using var conn = CreateConnection();
        await conn.OpenAsync(ct);
        await using var cmd = new SqlCommand(sql, conn);

        await using var reader = await cmd.ExecuteReaderAsync(ct);
        var result = new List<RecupAgentDto>();
        while (await reader.ReadAsync(ct))
            result.Add(ReadAgent(reader));

        return result;
    }

    public async Task<List<RecupAgentDto>> RechercherAgentsAsync(
        string? nom, string? entiteLibelle, CancellationToken ct = default)
    {
        var conditions = new List<string>();
        if (!string.IsNullOrWhiteSpace(nom))
            conditions.Add("(a.Nom LIKE @nom OR a.Postnom LIKE @nom OR a.Prenom LIKE @nom)");
        if (!string.IsNullOrWhiteSpace(entiteLibelle))
            conditions.Add("e.Libelle LIKE @entiteLibelle");

        var where  = conditions.Count > 0 ? " WHERE " + string.Join(" AND ", conditions) : "";
        var sql    = SelectBase + where + " ORDER BY a.Nom, a.Prenom";

        await using var conn = CreateConnection();
        await conn.OpenAsync(ct);
        await using var cmd = new SqlCommand(sql, conn);

        if (!string.IsNullOrWhiteSpace(nom))
            cmd.Parameters.AddWithValue("@nom", $"%{nom}%");
        if (!string.IsNullOrWhiteSpace(entiteLibelle))
            cmd.Parameters.AddWithValue("@entiteLibelle", $"%{entiteLibelle}%");

        await using var reader = await cmd.ExecuteReaderAsync(ct);
        var result = new List<RecupAgentDto>();
        while (await reader.ReadAsync(ct))
            result.Add(ReadAgent(reader));

        return result;
    }

    private static RecupAgentDto ReadAgent(SqlDataReader r)
    {
        string? GetStr(string col)
        {
            var ord = r.GetOrdinal(col);
            return r.IsDBNull(ord) ? null : r.GetString(ord);
        }

        DateTime? GetDate(string col)
        {
            var ord = r.GetOrdinal(col);
            return r.IsDBNull(ord) ? null : r.GetDateTime(ord);
        }

        return new RecupAgentDto(
            Id:                 r.GetInt32(r.GetOrdinal("Id")),
            Matricule:          GetStr("Matricule"),
            Nom:                GetStr("Nom"),
            Postnom:            GetStr("Postnom"),
            Prenom:             GetStr("Prenom"),
            EmailProfessionnel: GetStr("EmailProfessionnel"),
            EmailPersonnel:     GetStr("EmailPersonnel"),
            Telephone:          GetStr("Telephoneagent"),
            EntiteLibelle:      GetStr("EntiteLibelle"),
            GradeLibelle:       GetStr("GradeLibelle"),
            FonctionLibelle:    GetStr("FonctionLibelle"),
            Categorie:          r.GetString(r.GetOrdinal("Categorie")),
            EtatCivil:          r.GetString(r.GetOrdinal("EtatCivil")),
            Sexe:               r.GetString(r.GetOrdinal("Sexe")),
            DateEngagement:     GetDate("Dateengagement"),
            DateNaissance:      GetDate("Datenaissance")
        );
    }
}
