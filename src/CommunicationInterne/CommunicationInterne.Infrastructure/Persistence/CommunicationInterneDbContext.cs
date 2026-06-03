using Cnss.Metier.CommunicationInterne.Domain.Aggregats;
using Microsoft.EntityFrameworkCore;

namespace Cnss.Metier.CommunicationInterne.Infrastructure.Persistence;

/// <summary>
/// DbContext PostgreSQL du module CommunicationInterne — schéma <c>communication_interne</c>.
/// Agrégats : MessageInterne, GroupeDiffusion, DossierDiffusion.
/// </summary>
public sealed class CommunicationInterneDbContext : DbContext
{
    public const string Schema = "communication_interne";

    public CommunicationInterneDbContext(DbContextOptions<CommunicationInterneDbContext> options) : base(options) { }

    public DbSet<MessageInterne> Messages => Set<MessageInterne>();
    public DbSet<GroupeDiffusion> Groupes => Set<GroupeDiffusion>();
    public DbSet<MembreGroupe> MembresGroupe => Set<MembreGroupe>();
    public DbSet<DossierDiffusion> DossiersDiffusion => Set<DossierDiffusion>();
    public DbSet<LigneDiffusion> LignesDiffusion => Set<LigneDiffusion>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schema);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CommunicationInterneDbContext).Assembly);
    }
}
