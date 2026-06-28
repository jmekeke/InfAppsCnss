using Cnss.Metier.CommunicationInterne.Domain.Aggregats;
using Cnss.Metier.Shared.Domain;
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
    public DbSet<PieceJointe> PiecesJointes => Set<PieceJointe>();
    public DbSet<DestinataireCible> DestinatairesMessages => Set<DestinataireCible>();
    public DbSet<HistoriqueAction> HistoriqueActions => Set<HistoriqueAction>();
    public DbSet<VoieCommunication>  VoiesCommunication  => Set<VoieCommunication>();
    public DbSet<HistoriqueVoie>     HistoriquesVoie     => Set<HistoriqueVoie>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schema);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CommunicationInterneDbContext).Assembly);
    }
}
