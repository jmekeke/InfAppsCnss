using System.Text.Json;
using System.Text.Json.Serialization;
using Cnss.Metier.CommunicationInterne.Application.Ports;
using Cnss.Metier.Shared.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Cnss.Metier.CommunicationInterne.Infrastructure.Persistence;

/// <summary>
/// Intercepteur EF Core — capture automatiquement TOUS les INSERT, UPDATE et DELETE
/// sur les entités du module CommunicationInterne et écrit une <see cref="HistoriqueAction"/>
/// dans la même transaction, avant chaque SaveChanges.
/// </summary>
public sealed class AuditSaveChangesInterceptor(ICurrentUserContext currentUser) : SaveChangesInterceptor
{
    /// <summary>Types exclus pour éviter la récursion (l'audit n'audite pas lui-même).</summary>
    private static readonly HashSet<string> NomsTypesExclus = ["HistoriqueAction"];

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = false,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    // ─── Sync ───────────────────────────────────────────────────────────────

    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData, InterceptionResult<int> result)
    {
        AjouterEntriesAudit(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    // ─── Async ──────────────────────────────────────────────────────────────

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        AjouterEntriesAudit(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    // ─── Logique centrale ───────────────────────────────────────────────────

    private void AjouterEntriesAudit(DbContext? ctx)
    {
        if (ctx is null) return;

        var entrees = ctx.ChangeTracker.Entries()
            .Where(e =>
                e.State is EntityState.Added or EntityState.Modified or EntityState.Deleted
                && !NomsTypesExclus.Contains(e.Entity.GetType().Name))
            .ToList();

        foreach (var entree in entrees)
        {
            // On ne trace que les entités avec une PK Guid nommée "Id"
            var idProp = entree.Properties.FirstOrDefault(p => p.Metadata.Name == "Id");
            if (idProp?.CurrentValue is not Guid entiteId || entiteId == Guid.Empty)
                continue;

            var audit = new HistoriqueAction
            {
                EntiteType  = entree.Entity.GetType().Name,
                EntiteId    = entiteId,
                Action      = entree.State switch
                {
                    EntityState.Added    => "Création",
                    EntityState.Modified => "Modification",
                    EntityState.Deleted  => "Suppression",
                    _                   => entree.State.ToString(),
                },
                EtatAvant   = entree.State != EntityState.Added
                    ? SerialiserValeurs(entree.OriginalValues) : null,
                EtatApres   = entree.State != EntityState.Deleted
                    ? SerialiserValeurs(entree.CurrentValues) : null,
                UtilisateurId  = currentUser.UserId,
                UtilisateurNom = currentUser.UserName,
                DateAction     = DateTime.UtcNow,
            };

            ctx.Set<HistoriqueAction>().Add(audit);
        }
    }

    private static string SerialiserValeurs(PropertyValues valeurs)
    {
        var dict = new Dictionary<string, string?>();
        foreach (var prop in valeurs.Properties)
            dict[prop.Name] = valeurs[prop]?.ToString();
        return JsonSerializer.Serialize(dict, JsonOptions);
    }
}
