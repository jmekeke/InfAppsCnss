using Cnss.Metier.Shared.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cnss.Metier.CommunicationInterne.Infrastructure.Persistence.Configurations;

/// <summary>
/// Mapping EF Core de <see cref="HistoriqueAction"/> dans le schéma communication_interne.
/// Chaque module dispose de sa propre table d'audit locale pour éviter les dépendances
/// cross-DB avec le SharedDbContext.
/// </summary>
public class HistoriqueActionConfiguration : IEntityTypeConfiguration<HistoriqueAction>
{
    public void Configure(EntityTypeBuilder<HistoriqueAction> builder)
    {
        builder.ToTable("historique_actions");
        builder.HasKey(h => h.Id);

        builder.Property(h => h.EntiteType).HasMaxLength(128).IsRequired();
        builder.Property(h => h.EntiteId).IsRequired();
        builder.Property(h => h.Action).HasMaxLength(128).IsRequired();
        builder.Property(h => h.EtatAvant).HasColumnType("text");
        builder.Property(h => h.EtatApres).HasColumnType("text");
        builder.Property(h => h.UtilisateurId).HasMaxLength(64).IsRequired();
        builder.Property(h => h.UtilisateurNom).HasMaxLength(256);
        builder.Property(h => h.UtilisateurRole).HasMaxLength(64);
        builder.Property(h => h.DestinataireId).HasMaxLength(64);
        builder.Property(h => h.DestinataireNom).HasMaxLength(256);
        builder.Property(h => h.Commentaire).HasColumnType("text");

        builder.HasIndex(h => new { h.EntiteType, h.EntiteId });
        builder.HasIndex(h => h.DateAction);
    }
}
