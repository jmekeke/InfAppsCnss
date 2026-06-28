using Cnss.Metier.CommunicationInterne.Domain.Aggregats;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cnss.Metier.CommunicationInterne.Infrastructure.Persistence.Configurations;

public class VoieCommunicationConfiguration : IEntityTypeConfiguration<VoieCommunication>
{
    public void Configure(EntityTypeBuilder<VoieCommunication> builder)
    {
        builder.ToTable("VoiesCommunication");
        builder.HasKey(v => v.Id);
        builder.Property(v => v.AgentIdRh).IsRequired();
        builder.Property(v => v.Matricule).HasMaxLength(50).IsRequired();
        builder.HasIndex(v => v.AgentIdRh).IsUnique();
        builder.HasIndex(v => v.Matricule);

        builder.HasMany(v => v.Telephones)
            .WithOne().HasForeignKey(t => t.VoieCommunicationId).OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(v => v.Emails)
            .WithOne().HasForeignKey(e => e.VoieCommunicationId).OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(v => v.Historique)
            .WithOne().HasForeignKey(h => h.VoieCommunicationId).OnDelete(DeleteBehavior.Cascade);
    }
}

public class VoieTelephoneConfiguration : IEntityTypeConfiguration<VoieTelephone>
{
    public void Configure(EntityTypeBuilder<VoieTelephone> builder)
    {
        builder.ToTable("VoiesTelephone");
        builder.HasKey(t => new { t.VoieCommunicationId, t.Type });
        builder.Property(t => t.Type).HasConversion<string>().HasMaxLength(20);
        builder.Property(t => t.Numero).HasMaxLength(30).IsRequired();
        builder.Property(t => t.EstActif).IsRequired().HasDefaultValue(true);
        builder.Property(t => t.DateModification).IsRequired();
    }
}

public class VoieEmailConfiguration : IEntityTypeConfiguration<VoieEmail>
{
    public void Configure(EntityTypeBuilder<VoieEmail> builder)
    {
        builder.ToTable("VoiesEmail");
        builder.HasKey(e => new { e.VoieCommunicationId, e.Type });
        builder.Property(e => e.Type).HasConversion<string>().HasMaxLength(20);
        builder.Property(e => e.Adresse).HasMaxLength(200).IsRequired();
        builder.Property(e => e.EstActif).IsRequired().HasDefaultValue(true);
        builder.Property(e => e.DateModification).IsRequired();
    }
}

public class HistoriqueVoieConfiguration : IEntityTypeConfiguration<HistoriqueVoie>
{
    public void Configure(EntityTypeBuilder<HistoriqueVoie> builder)
    {
        builder.ToTable("HistoriquesVoie");
        builder.HasKey(h => h.Id);
        builder.Property(h => h.Id).UseIdentityAlwaysColumn();

        // Canal : enum stocké comme string (Telephone | Email)
        builder.Property(h => h.Canal).HasConversion<string>().HasMaxLength(15).IsRequired();
        // TypeVoie : nom de l'enum source ("Appel", "Sms", "WhatsApp", "Professionnel", "Prive")
        builder.Property(h => h.TypeVoie).HasMaxLength(30).IsRequired();
        // Valeur : numéro ou adresse
        builder.Property(h => h.Valeur).HasMaxLength(200).IsRequired();
        builder.Property(h => h.EstActif).IsRequired();
        builder.Property(h => h.Action).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(h => h.ModifiePar).HasMaxLength(100).IsRequired();
        builder.Property(h => h.DateAction).IsRequired();

        builder.HasIndex(h => new { h.VoieCommunicationId, h.DateAction });
        builder.HasIndex(h => h.Canal);
    }
}
