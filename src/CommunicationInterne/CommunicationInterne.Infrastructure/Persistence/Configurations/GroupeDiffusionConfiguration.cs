using Cnss.Metier.CommunicationInterne.Domain.Aggregats;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cnss.Metier.CommunicationInterne.Infrastructure.Persistence.Configurations;

public class GroupeDiffusionConfiguration : IEntityTypeConfiguration<GroupeDiffusion>
{
    public void Configure(EntityTypeBuilder<GroupeDiffusion> builder)
    {
        builder.ToTable("Groupes");
        builder.HasKey(g => g.Id);

        builder.Property(g => g.Nom).HasMaxLength(200).IsRequired();
        builder.Property(g => g.Description).HasMaxLength(1000);
        builder.Property(g => g.TypeGroupe).HasConversion<string>().HasMaxLength(20);

        builder.HasIndex(g => g.Nom);
        builder.HasIndex(g => g.CreateurId);

        builder.HasMany(g => g.Membres)
            .WithOne()
            .HasForeignKey(m => m.GroupeDiffusionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class MembreGroupeConfiguration : IEntityTypeConfiguration<MembreGroupe>
{
    public void Configure(EntityTypeBuilder<MembreGroupe> builder)
    {
        builder.ToTable("MembresGroupe");
        builder.HasKey(m => new { m.GroupeDiffusionId, m.AgentId });
        builder.HasIndex(m => m.AgentId);
    }
}
