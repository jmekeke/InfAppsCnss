using Cnss.Metier.CommunicationInterne.Domain.Aggregats;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cnss.Metier.CommunicationInterne.Infrastructure.Persistence.Configurations;

public class DossierDiffusionConfiguration : IEntityTypeConfiguration<DossierDiffusion>
{
    public void Configure(EntityTypeBuilder<DossierDiffusion> builder)
    {
        builder.ToTable("DossiersDiffusion");
        builder.HasKey(d => d.Id);

        builder.HasIndex(d => d.MessageInterneId);
        builder.HasIndex(d => d.DateLancement);

        // Règle : aucune suppression physique — pas de DeleteBehavior.Cascade sur les dossiers
        builder.HasMany(d => d.Lignes)
            .WithOne()
            .HasForeignKey(l => l.DossierDiffusionId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class LigneDiffusionConfiguration : IEntityTypeConfiguration<LigneDiffusion>
{
    public void Configure(EntityTypeBuilder<LigneDiffusion> builder)
    {
        builder.ToTable("LignesDiffusion");
        builder.HasKey(l => new { l.DossierDiffusionId, l.AgentId, l.Canal });

        builder.Property(l => l.Canal).HasConversion<string>().HasMaxLength(20);
        builder.Property(l => l.Statut).HasConversion<string>().HasMaxLength(20);
        builder.Property(l => l.MessageErreur).HasMaxLength(2000);

        builder.HasIndex(l => l.AgentId);
        builder.HasIndex(l => l.Statut);
    }
}
