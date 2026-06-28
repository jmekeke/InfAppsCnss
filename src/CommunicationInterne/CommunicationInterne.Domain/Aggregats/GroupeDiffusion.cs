using Cnss.Metier.CommunicationInterne.Domain.Enums;
using Cnss.Metier.CommunicationInterne.Domain.Events;
using Cnss.Metier.Shared.Domain;

namespace Cnss.Metier.CommunicationInterne.Domain.Aggregats;

/// <summary>
/// Agrégat — Groupe de diffusion.
/// Un groupe est un ensemble nommé d'agents destinataires (manuel en v1, dynamique en v2).
/// Règle : un agent inactif ne doit jamais être ciblé par une diffusion.
/// </summary>
public class GroupeDiffusion : AggregateRoot
{
    public string Nom { get; private set; } = default!;
    public string? Description { get; private set; }
    public TypeGroupe TypeGroupe { get; private set; }
    public Guid CreateurId { get; private set; }
    public DateTime DateCreation { get; private set; }
    public bool EstActif { get; private set; }

    /// <summary>
    /// Pour les groupes Dynamiques : type de critère RH ("Grade" ou "Entite").
    /// Null pour les groupes Manuels.
    /// </summary>
    public string? CritereType { get; private set; }

    /// <summary>
    /// Pour les groupes Dynamiques : valeur du critère (libellé grade ou libellé entité).
    /// Null pour les groupes Manuels.
    /// </summary>
    public string? CritereValeur { get; private set; }

    private readonly List<MembreGroupe> _membres = [];
    public IReadOnlyCollection<MembreGroupe> Membres => _membres.AsReadOnly();

    private GroupeDiffusion() { } // EF Core

    public static GroupeDiffusion Creer(Guid createurId, string nom, string? description = null,
        TypeGroupe typeGroupe = TypeGroupe.Manuel, string? critereType = null, string? critereValeur = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(nom);
        var groupe = new GroupeDiffusion
        {
            CreateurId   = createurId,
            Nom          = nom,
            Description  = description,
            TypeGroupe   = typeGroupe,
            DateCreation = DateTime.UtcNow,
            EstActif     = true,
            CritereType  = typeGroupe == TypeGroupe.Dynamique ? critereType : null,
            CritereValeur = typeGroupe == TypeGroupe.Dynamique ? critereValeur : null,
        };
        groupe.AddDomainEvent(new GroupeDiffusionCreeEvent(groupe.Id, nom));
        return groupe;
    }

    /// <summary>
    /// Ajoute un agent au groupe.
    /// La vérification de l'activité de l'agent est effectuée par la couche Application.
    /// </summary>
    public void AjouterMembre(Guid agentId)
    {
        if (_membres.Any(m => m.AgentId == agentId))
            throw new InvalidOperationException($"L'agent {agentId} est déjà membre de ce groupe.");

        _membres.Add(new MembreGroupe { GroupeDiffusionId = Id, AgentId = agentId, DateAjout = DateTime.UtcNow });
        AddDomainEvent(new MembreAjouteAuGroupeEvent(Id, agentId));
    }

    /// <summary>Retire un agent du groupe.</summary>
    public void RetirerMembre(Guid agentId)
    {
        var membre = _membres.FirstOrDefault(m => m.AgentId == agentId)
            ?? throw new InvalidOperationException($"L'agent {agentId} n'est pas membre de ce groupe.");

        _membres.Remove(membre);
        AddDomainEvent(new MembreRetireDeGroupeEvent(Id, agentId));
    }

    /// <summary>Modifie le nom, la description et le type du groupe.</summary>
    public void Modifier(string nom, string? description, TypeGroupe typeGroupe)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(nom);
        Nom         = nom.Trim();
        Description = description?.Trim();
        TypeGroupe  = typeGroupe;
    }

    /// <summary>Supprime logiquement le groupe (désactivation).</summary>
    public void Supprimer()
    {
        if (!EstActif)
            throw new InvalidOperationException($"Le groupe {Id} est déjà supprimé.");

        EstActif = false;
        AddDomainEvent(new GroupeDiffusionSupprimeEvent(Id));
    }

    /// <summary>Réactive un groupe précédemment désactivé.</summary>
    public void Reactiver()
    {
        if (EstActif)
            throw new InvalidOperationException($"Le groupe {Id} est déjà actif.");

        EstActif = true;
    }

    /// <summary>Retourne le nombre de membres actifs ciblés (calculé avant diffusion).</summary>
    public int CompterDestinataires() => _membres.Count;
}

/// <summary>Entité de jonction — appartenance d'un agent à un groupe.</summary>
public class MembreGroupe
{
    public Guid GroupeDiffusionId { get; init; }
    public Guid AgentId { get; init; }
    public DateTime DateAjout { get; init; }
}
