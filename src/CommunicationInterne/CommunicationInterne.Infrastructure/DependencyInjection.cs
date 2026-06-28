using Cnss.Metier.CommunicationInterne.Application.Ports;
using Cnss.Metier.CommunicationInterne.Domain.Repositories;
using Cnss.Metier.CommunicationInterne.Infrastructure.ExternalServices;
using Cnss.Metier.CommunicationInterne.Infrastructure.Persistence;
using Cnss.Metier.CommunicationInterne.Infrastructure.Persistence.Repositories;
using Cnss.Metier.Shared.Application.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Cnss.Metier.CommunicationInterne.Infrastructure;

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

        // Contexte utilisateur courant (scoped, alimenté par chaque commande ou middleware HTTP)
        services.AddScoped<ICurrentUserContext, CurrentUserContext>();
        services.AddScoped<AuditSaveChangesInterceptor>();

        services.AddDbContext<CommunicationInterneDbContext>((sp, options) =>
        {
            options.UseNpgsql(
                connectionString,
                npg => npg.MigrationsHistoryTable("__ef_migrations_history", CommunicationInterneDbContext.Schema));
            options.AddInterceptors(sp.GetRequiredService<AuditSaveChangesInterceptor>());
            options.ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));
        });

        // Unit of Work
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Repositories
        services.AddScoped<IMessageInterneRepository, MessageInterneRepository>();
        services.AddScoped<IGroupeDiffusionRepository, GroupeDiffusionRepository>();
        services.AddScoped<IDossierDiffusionRepository, DossierDiffusionRepository>();
        services.AddScoped<IVoieCommunicationRepository, VoieCommunicationRepository>();

        // ACL — bounded context Agent (SQL Server externe)
        services.AddScoped<IAgentQueryService, AgentSqlQueryService>();

        // ACL — RH_DB : table Agent
        services.AddScoped<IRecupAgentQueryService, RecupAgentSqlService>();

        // Senders externes (implémentations log-based — à remplacer par les vrais adaptateurs)
        services.AddScoped<IEmailSender, LogEmailSender>();
        services.AddScoped<ISmsSender, LogSmsSender>();
        services.AddScoped<IWhatsAppSender, LogWhatsAppSender>();

        return services;
    }
}
