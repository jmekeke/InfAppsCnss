using CommunicationInterne.Application.Ports;
using CommunicationInterne.Infrastructure.ExternalServices;
using CommunicationInterne.Infrastructure.Persistence;
using CommunicationInterne.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CommunicationInterne.Infrastructure;

/// <summary>
/// Composition Root — module CommunicationInterne (Infrastructure).
/// Enregistre le DbContext PostgreSQL, les repositories et les services externes (ACL).
/// </summary>
public static class DependencyInjection
{
    public const string ConnectionStringName = "CnssMetierDb";

    public static IServiceCollection AddCommunicationInterneInfrastructure(
        this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString(ConnectionStringName)
            ?? throw new InvalidOperationException($"Connection string '{ConnectionStringName}' not found.");

        services.AddDbContext<CommunicationInterneDbContext>(options =>
            options.UseNpgsql(
                connectionString,
                npg => npg.MigrationsHistoryTable("__ef_migrations_history", CommunicationInterneDbContext.Schema)));

        // Repositories
        services.AddScoped<IMessageInterneRepository, MessageInterneRepository>();
        services.AddScoped<IGroupeDiffusionRepository, GroupeDiffusionRepository>();
        services.AddScoped<IDossierDiffusionRepository, DossierDiffusionRepository>();

        // ACL — bounded context Agent (SQL Server externe)
        services.AddScoped<IAgentQueryService, AgentSqlQueryService>();

        return services;
    }
}
