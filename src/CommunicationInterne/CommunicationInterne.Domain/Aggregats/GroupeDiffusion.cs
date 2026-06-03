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

    private readonly List<MembreGroupe> _membres = [];
    public IReadOnlyCollection<MembreGroupe> Membres => _membres.AsReadOnly();

    private GroupeDiffusion() { } // EF Core

    public static GroupeDiffusion Creer(Guid createurId, string nom, string? description = null, TypeGroupe typeGroupe = TypeGroupe.Manuel)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(nom);
        var groupe = new GroupeDiffusion
        {
            CreateurId = createurId,
            Nom = nom,
            Description = description,
            TypeGroupe = typeGroupe,
            DateCreation = DateTime.UtcNow,
            EstActif = true,
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
