using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Cnss.Metier.CommunicationInterne.Infrastructure.Persistence;

/// <summary>
/// Factory utilisée par <c>dotnet ef</c> (migrations) en l'absence d'un Host.
/// La chaîne de connexion est lue depuis la variable d'environnement <c>CNSS_METIER_DB</c>.
/// </summary>
public sealed class CommunicationInterneDbContextDesignTimeFactory
    : IDesignTimeDbContextFactory<CommunicationInterneDbContext>
{
    public CommunicationInterneDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("CNSS_METIER_DB")
            ?? "Host=localhost;Port=5432;Database=BDDInfoCnss;Username=postgres;Password=treso@123456";

        var options = new DbContextOptionsBuilder<CommunicationInterneDbContext>()
            .UseNpgsql(
                connectionString,
                npg => npg.MigrationsHistoryTable("__ef_migrations_history", CommunicationInterneDbContext.Schema))
            .ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning))
            .Options;

        return new CommunicationInterneDbContext(options);
    }
}
