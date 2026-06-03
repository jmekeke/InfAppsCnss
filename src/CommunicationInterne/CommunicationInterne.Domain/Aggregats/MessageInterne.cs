using Cnss.Metier.CommunicationInterne.Domain.Enums;
using Cnss.Metier.CommunicationInterne.Domain.Events;
using Cnss.Metier.Shared.Domain;

namespace Cnss.Metier.CommunicationInterne.Domain.Aggregats;

/// <summary>
/// Agrégat racine — Message interne CNSS.
/// Porte le cycle de vie complet : Brouillon → EnAttenteValidation → Valide/Rejete → Programme → Diffuse.
/// Règle : un message institutionnel ou sensible ne peut être diffusé sans validation préalable.
/// Règle : un message déjà diffusé ne doit pas être supprimé physiquement.
/// </summary>
public class MessageInterne : AggregateRoot
{
    public string Objet { get; private set; } = default!;
    public string Corps { get; private set; } = default!;
    public bool EstInstitutionnel { get; private set; }
    public Guid AuteurId { get; private set; }
    public StatutMessage Statut { get; private set; }
    public DateTime DateCreation { get; private set; }
    public DateTime? DateValidation { get; private set; }
    public Guid? ValidateurId { get; private set; }
    public string? MotiDeRejet { get; private set; }
    public DateTime? DateProgrammee { get; private set; }
    public DateTime? DateDiffusion { get; private set; }
    public bool EstArchive { get; private set; }

    private readonly List<TypeCanal> _canaux = [];
    public IReadOnlyCollection<TypeCanal> Canaux => _canaux.AsReadOnly();

    private MessageInterne() { } // EF Core

    public static MessageInterne Creer(Guid auteurId, string objet, string corps, bool estInstitutionnel, IEnumerable<TypeCanal> canaux)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(objet);
        ArgumentException.ThrowIfNullOrWhiteSpace(corps);

        var message = new MessageInterne
        {
            AuteurId = auteurId,
            Objet = objet,
            Corps = corps,
            EstInstitutionnel = estInstitutionnel,
            Statut = StatutMessage.Brouillon,
            DateCreation = DateTime.UtcNow,
        };
        message._canaux.AddRange(canaux.Distinct());
        message.AddDomainEvent(new MessageInterneCreeeEvent(message.Id, objet));
        return message;
    }

    /// <summary>
    /// Soumet le message à la validation.
    /// Un message institutionnel ou sensible ne peut passer à Valide sans passer par cette étape.
    /// </summary>
    public void SoumettreAValidation()
    {
        if (Statut != StatutMessage.Brouillon)
            throw new InvalidOperationException($"Le message ne peut être soumis qu'à partir du statut Brouillon. Statut actuel : {Statut}.");

        Statut = StatutMessage.EnAttenteValidation;
        AddDomainEvent(new MessageInternesoumisAValidationEvent(Id));
    }

    /// <summary>
    /// Valide le message. Seul un message institutionnel doit impérativement passer par la validation.
    /// </summary>
    public void Valider(Guid validateurId)
    {
        if (Statut != StatutMessage.EnAttenteValidation)
            throw new InvalidOperationException($"Seul un message EnAttenteValidation peut être validé. Statut actuel : {Statut}.");

        Statut = StatutMessage.Valide;
        DateValidation = DateTime.UtcNow;
        ValidateurId = validateurId;
        AddDomainEvent(new MessageInterneValideEvent(Id, validateurId));
    }

    /// <summary>Rejette le message avec un motif obligatoire.</summary>
    public void Rejeter(Guid validateurId, string motif)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(motif);
        if (Statut != StatutMessage.EnAttenteValidation)
            throw new InvalidOperationException($"Seul un message EnAttenteValidation peut être rejeté. Statut actuel : {Statut}.");

        Statut = StatutMessage.Rejete;
        MotiDeRejet = motif;
        ValidateurId = validateurId;
        AddDomainEvent(new MessageInterneRejeteEvent(Id, validateurId, motif));
    }

    /// <summary>Programme la diffusion à une date future.</summary>
    public void Programmer(DateTime dateProgrammee)
    {
        if (Statut is not (StatutMessage.Valide or StatutMessage.Brouillon))
            throw new InvalidOperationException($"Seul un message Valide ou Brouillon (non institutionnel) peut être programmé. Statut actuel : {Statut}.");

        if (EstInstitutionnel && Statut != StatutMessage.Valide)
            throw new InvalidOperationException("Un message institutionnel doit être validé avant d'être programmé.");

        DateProgrammee = dateProgrammee;
        Statut = StatutMessage.Programme;
        AddDomainEvent(new MessageInterneProgrammeEvent(Id, dateProgrammee));
    }

    /// <summary>
    /// Marque le message comme diffusé.
    /// Règle : un message déjà diffusé ne doit pas être supprimé physiquement.
    /// </summary>
    public void MarquerCommeDiffuse()
    {
        if (Statut is not (StatutMessage.Valide or StatutMessage.Programme))
            throw new InvalidOperationException($"Seul un message Valide ou Programmé peut être diffusé. Statut actuel : {Statut}.");

        Statut = StatutMessage.Diffuse;
        DateDiffusion = DateTime.UtcNow;
        AddDomainEvent(new MessageInterneDiffuseEvent(Id, DateDiffusion.Value));
    }

    /// <summary>Archive logiquement le message. Jamais de suppression physique après diffusion.</summary>
    public void Archiver()
    {
        EstArchive = true;
    }
}
