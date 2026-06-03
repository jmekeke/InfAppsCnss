using Cnss.Metier.CommunicationInterne.Domain.Enums;
using Cnss.Metier.CommunicationInterne.Domain.Events;
using Cnss.Metier.Shared.Domain;

namespace Cnss.Metier.CommunicationInterne.Domain.Aggregats;

/// <summary>
/// Agrégat — Dossier de diffusion.
/// Représente une exécution de diffusion d'un message vers des destinataires sur un ou plusieurs canaux.
/// Règle : toute diffusion doit être historisée. Les suppressions physiques sont interdites.
/// </summary>
public class DossierDiffusion : AggregateRoot
{
    public Guid MessageInterneId { get; private set; }
    public Guid DeclencheurId { get; private set; }
    public DateTime DateLancement { get; private set; }
    public int NombreDestinatairesTotaux { get; private set; }

    private readonly List<LigneDiffusion> _lignes = [];
    public IReadOnlyCollection<LigneDiffusion> Lignes => _lignes.AsReadOnly();

    private DossierDiffusion() { } // EF Core

    public static DossierDiffusion Lancer(Guid messageInterneId, Guid declencheurId, int nombreDestinataires)
    {
        if (nombreDestinataires <= 0)
            throw new InvalidOperationException("Impossible de lancer une diffusion sans destinataires.");

        var dossier = new DossierDiffusion
        {
            MessageInterneId = messageInterneId,
            DeclencheurId = declencheurId,
            DateLancement = DateTime.UtcNow,
            NombreDestinatairesTotaux = nombreDestinataires,
        };
        dossier.AddDomainEvent(new DossierDiffusionLanceEvent(dossier.Id, messageInterneId, nombreDestinataires));
        return dossier;
    }

    /// <summary>
    /// Enregistre le résultat d'un envoi vers un destinataire sur un canal donné.
    /// Règle : toute diffusion doit être historisée — aucune suppression physique.
    /// </summary>
    public void EnregistrerEnvoi(Guid agentId, TypeCanal canal, StatutEnvoi statut, string? messageErreur = null)
    {
        _lignes.Add(new LigneDiffusion
        {
            DossierDiffusionId = Id,
            AgentId = agentId,
            Canal = canal,
            Statut = statut,
            DateEnvoi = DateTime.UtcNow,
            MessageErreur = messageErreur,
        });
        AddDomainEvent(new EnvoiEnregistreEvent(Id, agentId, canal, statut));
    }

    public int CompterEnvoisReussis() => _lignes.Count(l => l.Statut == StatutEnvoi.Envoye);
    public int CompterEnvoisEchoues() => _lignes.Count(l => l.Statut == StatutEnvoi.Echec);
}

/// <summary>Entité — Ligne de suivi d'un envoi individuel.</summary>
public class LigneDiffusion
{
    public Guid DossierDiffusionId { get; init; }
    public Guid AgentId { get; init; }
    public TypeCanal Canal { get; init; }
    public StatutEnvoi Statut { get; init; }
    public DateTime DateEnvoi { get; init; }
    public string? MessageErreur { get; init; }
}
