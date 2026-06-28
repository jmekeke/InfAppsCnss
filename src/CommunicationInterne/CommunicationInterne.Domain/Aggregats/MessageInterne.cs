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
    public string AuteurNom { get; private set; } = string.Empty;
    public StatutMessage Statut { get; private set; }
    public DateTime DateCreation { get; private set; }
    public DateTime? DateValidation { get; private set; }
    public Guid? ValidateurId { get; private set; }
    public string? MotiDeRejet { get; private set; }
    public string? CommentaireRetour { get; private set; }
    public DateTime? DateProgrammee { get; private set; }
    public DateTime? DateDiffusion { get; private set; }
    public bool EstArchive { get; private set; }

    private readonly List<TypeCanal> _canaux = [];
    public IReadOnlyCollection<TypeCanal> Canaux => _canaux.AsReadOnly();

    private readonly List<PieceJointe> _piecesJointes = [];
    public IReadOnlyCollection<PieceJointe> PiecesJointes => _piecesJointes.AsReadOnly();

    // Conservé pour compatibilité EF (table MessageGroupesDestinataires) — à terme remplacé par _destinataires
    private readonly List<Guid> _groupeIds = [];
    public IReadOnlyCollection<Guid> GroupeIds => _groupeIds.AsReadOnly();

    private readonly List<DestinataireCible> _destinataires = [];
    public IReadOnlyCollection<DestinataireCible> Destinataires => _destinataires.AsReadOnly();

    private MessageInterne() { } // EF Core

    public static MessageInterne Creer(Guid auteurId, string auteurNom, string objet, string corps, bool estInstitutionnel, IEnumerable<TypeCanal> canaux)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(objet);
        ArgumentException.ThrowIfNullOrWhiteSpace(corps);

        var message = new MessageInterne
        {
            AuteurId = auteurId,
            AuteurNom = auteurNom,
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

    /// <summary>
    /// Renvoie le message en correction à l'auteur (retour en Brouillon) avec un commentaire explicatif.
    /// Contrairement à Rejeter, le message peut être corrigé et resoumis.
    /// </summary>
    public void DemanderCorrection(Guid validateurId, string commentaire)
    {
        if (Statut != StatutMessage.EnAttenteValidation)
            throw new InvalidOperationException($"Seul un message EnAttenteValidation peut être retourné en correction. Statut actuel : {Statut}.");

        Statut = StatutMessage.Brouillon;
        CommentaireRetour = commentaire;
        ValidateurId = validateurId;
        AddDomainEvent(new MessageInterneRetourneEnCorrectionEvent(Id, validateurId, commentaire));
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

    /// <summary>Modifie le contenu du message. Uniquement possible en statut Brouillon.</summary>
    public void Modifier(string? objet, string? corps, bool? estInstitutionnel, IEnumerable<TypeCanal>? canaux)
    {
        if (Statut != StatutMessage.Brouillon)
            throw new InvalidOperationException($"Seul un message en Brouillon peut être modifié. Statut actuel : {Statut}.");

        if (objet is not null) { ArgumentException.ThrowIfNullOrWhiteSpace(objet); Objet = objet; }
        if (corps is not null) { ArgumentException.ThrowIfNullOrWhiteSpace(corps); Corps = corps; }
        if (estInstitutionnel is not null) EstInstitutionnel = estInstitutionnel.Value;
        if (canaux is not null)
        {
            _canaux.Clear();
            _canaux.AddRange(canaux.Distinct());
        }
        AddDomainEvent(new MessageInterneModifieEvent(Id));
    }

    /// <summary>Supprime logiquement le message. Uniquement possible en statut Brouillon.</summary>
    public void Supprimer()
    {
        if (Statut != StatutMessage.Brouillon)
            throw new InvalidOperationException($"Seul un message en Brouillon peut être supprimé. Statut actuel : {Statut}.");

        EstArchive = true;
        AddDomainEvent(new MessageInterneSupprimEvent(Id));
    }

    /// <summary>Ajoute une pièce jointe. Uniquement si le canal Email ou WhatsApp est sélectionné.</summary>
    public PieceJointe AjouterPieceJointe(string nomFichier, string typeMime, long tailleOctets)
    {
        if (Statut != StatutMessage.Brouillon)
            throw new InvalidOperationException($"Les pièces jointes ne peuvent être ajoutées qu'en Brouillon. Statut actuel : {Statut}.");

        if (!_canaux.Contains(TypeCanal.Email) && !_canaux.Contains(TypeCanal.WhatsApp))
            throw new InvalidOperationException("Les pièces jointes nécessitent le canal Email ou WhatsApp.");

        ArgumentException.ThrowIfNullOrWhiteSpace(nomFichier);
        var pj = new PieceJointe { MessageInterneId = Id, NomFichier = nomFichier, TypeMime = typeMime, TailleOctets = tailleOctets };
        _piecesJointes.Add(pj);
        AddDomainEvent(new PieceJointeAjouteeEvent(Id, pj.Id, nomFichier));
        return pj;
    }

    /// <summary>Retire une pièce jointe. Uniquement possible en statut Brouillon.</summary>
    public void SupprimerPieceJointe(Guid pieceJointeId)
    {
        if (Statut != StatutMessage.Brouillon)
            throw new InvalidOperationException($"Les pièces jointes ne peuvent être retirées qu'en Brouillon. Statut actuel : {Statut}.");

        var pj = _piecesJointes.FirstOrDefault(p => p.Id == pieceJointeId)
            ?? throw new InvalidOperationException($"Pièce jointe {pieceJointeId} introuvable.");

        _piecesJointes.Remove(pj);
        AddDomainEvent(new PieceJointeSupprimeeEvent(Id, pieceJointeId));
    }

    /// <summary>
    /// Définit les destinataires cibles du message (multi-types).
    /// Uniquement possible en statut Brouillon ou Valide.
    /// </summary>
    public void DefinirDestinataires(IEnumerable<DestinataireCible> destinataires)
    {
        if (Statut is not (StatutMessage.Brouillon or StatutMessage.Valide))
            throw new InvalidOperationException($"Les destinataires ne peuvent être définis qu'en Brouillon ou Valide. Statut actuel : {Statut}.");

        var liste = destinataires.ToList();
        if (liste.Count == 0)
            throw new InvalidOperationException("Au moins un destinataire est requis.");

        _destinataires.Clear();
        _destinataires.AddRange(liste);

        // Synchroniser _groupeIds pour la rétrocompatibilité
        _groupeIds.Clear();
        _groupeIds.AddRange(liste
            .Where(d => d.Type == TypeDestinataire.GroupeDiffusion && d.ReferenceId is not null)
            .Select(d => Guid.Parse(d.ReferenceId!)));

        AddDomainEvent(new DestinatairesDefinisEvent(Id));
    }
}
