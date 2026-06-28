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
        builder.Property(m => m.AuteurNom).HasMaxLength(200).HasDefaultValue(string.Empty);
        builder.Property(m => m.Statut).HasConversion<string>().HasMaxLength(30);
        builder.Property(m => m.MotiDeRejet).HasMaxLength(1000);
        builder.Property(m => m.CommentaireRetour).HasMaxLength(1000);

        builder.HasIndex(m => m.AuteurId);
        builder.HasIndex(m => m.Statut);

        // Canaux stockés dans une table de jonction
        builder.Ignore(m => m.Canaux);
        builder.HasMany<MessageInterneCanalEntry>()
            .WithOne()
            .HasForeignKey("MessageInterneId")
            .OnDelete(DeleteBehavior.Cascade);

        // Pièces jointes
        builder.Ignore(m => m.PiecesJointes);
        builder.HasMany<PieceJointe>()
            .WithOne()
            .HasForeignKey(p => p.MessageInterneId)
            .OnDelete(DeleteBehavior.Cascade);

        // Groupes destinataires (rétrocompatibilité)
        builder.Ignore(m => m.GroupeIds);
        builder.HasMany<MessageGroupeDestinataireEntry>()
            .WithOne()
            .HasForeignKey("MessageInterneId")
            .OnDelete(DeleteBehavior.Cascade);

        // Nouveaux destinataires cibles (multi-types)
        builder.Ignore(m => m.Destinataires);
        builder.HasMany<DestinataireCible>()
            .WithOne()
            .HasForeignKey(d => d.MessageInterneId)
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

/// <summary>Entité de jonction EF — pièce jointe d'un message interne.</summary>
public class PieceJointeConfiguration : IEntityTypeConfiguration<PieceJointe>
{
    public void Configure(EntityTypeBuilder<PieceJointe> builder)
    {
        builder.ToTable("PiecesJointes");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.NomFichier).HasMaxLength(255).IsRequired();
        builder.Property(p => p.TypeMime).HasMaxLength(100).IsRequired();
        builder.HasIndex(p => p.MessageInterneId);
    }
}

/// <summary>Entité de jonction EF — groupe destinataire d'un message interne.</summary>
public class MessageGroupeDestinataireEntry
{
    public Guid MessageInterneId { get; set; }
    public Guid GroupeId { get; set; }
}

public class MessageGroupeDestinataireConfiguration : IEntityTypeConfiguration<MessageGroupeDestinataireEntry>
{
    public void Configure(EntityTypeBuilder<MessageGroupeDestinataireEntry> builder)
    {
        builder.ToTable("MessageGroupesDestinataires");
        builder.HasKey("MessageInterneId", nameof(MessageGroupeDestinataireEntry.GroupeId));
        builder.HasIndex(g => g.GroupeId);
    }
}

public class DestinataireCibleConfiguration : IEntityTypeConfiguration<DestinataireCible>
{
    public void Configure(EntityTypeBuilder<DestinataireCible> builder)
    {
        builder.ToTable("MessageDestinataires");
        builder.HasKey(d => d.Id);
        builder.Property(d => d.Type).HasConversion<string>().HasMaxLength(30);
        builder.Property(d => d.ReferenceId).HasMaxLength(100);
        builder.Property(d => d.Libelle).HasMaxLength(300);
        builder.HasIndex(d => d.MessageInterneId);
    }
}
