using Cnss.Metier.CommunicationInterne.Domain.Aggregats;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cnss.Metier.CommunicationInterne.Infrastructure.Persistence.Configurations;

public class MessageInterneConfiguration : IEntityTypeConfiguration<MessageInterne>
{
    public void Configure(EntityTypeBuilder<MessageInterne> builder)
    {
        builder.ToTable("Messages");
        builder.HasKey(m => m.Id);

        builder.Property(m => m.Objet).HasMaxLength(500).IsRequired();
        builder.Property(m => m.Corps).IsRequired();
        builder.Property(m => m.Statut).HasConversion<string>().HasMaxLength(30);

        builder.HasIndex(m => m.AuteurId);
        builder.HasIndex(m => m.Statut);

        // Canaux stockés en colonne séparée (table de jonction)
        builder.Ignore(m => m.Canaux);

        builder.HasMany<MessageInterneCanalEntry>()
            .WithOne()
            .HasForeignKey("MessageInterneId")
            .OnDelete(DeleteBehavior.Cascade);
    }
}

/// <summary>Entité de jonction EF — canal associé à un message interne.</summary>
public class MessageInterneCanalEntry
{
    public Guid MessageInterneId { get; set; }
    public Domain.Enums.TypeCanal Canal { get; set; }
}

public class MessageInterneCanalConfiguration : IEntityTypeConfiguration<MessageInterneCanalEntry>
{
    public void Configure(EntityTypeBuilder<MessageInterneCanalEntry> builder)
    {
        builder.ToTable("MessageCanaux");
        builder.HasKey("MessageInterneId", nameof(MessageInterneCanalEntry.Canal));
        builder.Property(c => c.Canal).HasConversion<string>().HasMaxLength(20);
    }
}
