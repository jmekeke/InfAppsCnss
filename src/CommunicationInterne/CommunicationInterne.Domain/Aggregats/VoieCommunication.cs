using Cnss.Metier.CommunicationInterne.Domain.Enums;
using Cnss.Metier.Shared.Domain;

namespace Cnss.Metier.CommunicationInterne.Domain.Aggregats;

/// <summary>
/// Agrégat — Voie de communication préférentielle d'un agent CNSS.
///
/// Relie un agent RH à une ou plusieurs voies :
///   - Téléphonique : numéro + type (Appel / Sms / WhatsApp)
///   - E-mail        : adresse + type (Professionnel / Privé)
///
/// Règles métier :
///   - Un seul enregistrement par (Canal × Type) ; il peut être actif ou inactif.
///   - Toute création, modification, désactivation, réactivation ou suppression
///     est tracée dans <see cref="Historique"/> via l'entité <see cref="HistoriqueVoie"/>.
/// </summary>
public class VoieCommunication : AggregateRoot
{
    public int    AgentIdRh { get; private set; }
    public string Matricule { get; private set; } = default!;

    private readonly List<VoieTelephone>  _telephones = [];
    private readonly List<VoieEmail>      _emails      = [];
    private readonly List<HistoriqueVoie> _historique  = [];

    public IReadOnlyCollection<VoieTelephone>  Telephones => _telephones.AsReadOnly();
    public IReadOnlyCollection<VoieEmail>      Emails      => _emails.AsReadOnly();
    public IReadOnlyCollection<HistoriqueVoie> Historique  => _historique.AsReadOnly();

    private VoieCommunication() { } // EF Core

    // ── Fabrique ──────────────────────────────────────────────────────────────

    public static VoieCommunication Creer(int agentIdRh, string matricule)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(matricule);
        return new VoieCommunication { AgentIdRh = agentIdRh, Matricule = matricule };
    }

    // ── Téléphone ─────────────────────────────────────────────────────────────

    public void DefinirTelephone(TypeVoieTelephone type, string numero, string modifiePar)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(numero);
        var existant = _telephones.FirstOrDefault(t => t.Type == type);
        if (existant is not null)
        {
            Archiver(CanalVoie.Telephone, existant.Type.ToString(), existant.Numero,
                     existant.EstActif, ActionHistorique.Modifie, modifiePar);
            _telephones.Remove(existant);
        }
        _telephones.Add(VoieTelephone.Creer(Id, type, numero.Trim()));
    }

    public void DesactiverTelephone(TypeVoieTelephone type, string modifiePar)
    {
        var v = _telephones.FirstOrDefault(t => t.Type == type)
            ?? throw new InvalidOperationException($"Aucune voie téléphonique de type {type}.");
        if (!v.EstActif)
            throw new InvalidOperationException($"La voie téléphonique {type} est déjà inactive.");
        Archiver(CanalVoie.Telephone, v.Type.ToString(), v.Numero, true, ActionHistorique.Desactive, modifiePar);
        v.Desactiver();
    }

    public void ReactiverTelephone(TypeVoieTelephone type, string modifiePar)
    {
        var v = _telephones.FirstOrDefault(t => t.Type == type)
            ?? throw new InvalidOperationException($"Aucune voie téléphonique de type {type}.");
        if (v.EstActif)
            throw new InvalidOperationException($"La voie téléphonique {type} est déjà active.");
        Archiver(CanalVoie.Telephone, v.Type.ToString(), v.Numero, false, ActionHistorique.Reactive, modifiePar);
        v.Reactiver();
    }

    public void SupprimerTelephone(TypeVoieTelephone type, string modifiePar)
    {
        var v = _telephones.FirstOrDefault(t => t.Type == type)
            ?? throw new InvalidOperationException($"Aucune voie téléphonique de type {type}.");
        Archiver(CanalVoie.Telephone, v.Type.ToString(), v.Numero, v.EstActif, ActionHistorique.Supprime, modifiePar);
        _telephones.Remove(v);
    }

    // ── E-mail ────────────────────────────────────────────────────────────────

    public void DefinirEmail(TypeVoieEmail type, string adresse, string modifiePar)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(adresse);
        var existant = _emails.FirstOrDefault(e => e.Type == type);
        if (existant is not null)
        {
            Archiver(CanalVoie.Email, existant.Type.ToString(), existant.Adresse,
                     existant.EstActif, ActionHistorique.Modifie, modifiePar);
            _emails.Remove(existant);
        }
        _emails.Add(VoieEmail.Creer(Id, type, adresse.Trim().ToLowerInvariant()));
    }

    public void DesactiverEmail(TypeVoieEmail type, string modifiePar)
    {
        var v = _emails.FirstOrDefault(e => e.Type == type)
            ?? throw new InvalidOperationException($"Aucune voie e-mail de type {type}.");
        if (!v.EstActif)
            throw new InvalidOperationException($"La voie e-mail {type} est déjà inactive.");
        Archiver(CanalVoie.Email, v.Type.ToString(), v.Adresse, true, ActionHistorique.Desactive, modifiePar);
        v.Desactiver();
    }

    public void ReactiverEmail(TypeVoieEmail type, string modifiePar)
    {
        var v = _emails.FirstOrDefault(e => e.Type == type)
            ?? throw new InvalidOperationException($"Aucune voie e-mail de type {type}.");
        if (v.EstActif)
            throw new InvalidOperationException($"La voie e-mail {type} est déjà active.");
        Archiver(CanalVoie.Email, v.Type.ToString(), v.Adresse, false, ActionHistorique.Reactive, modifiePar);
        v.Reactiver();
    }

    public void SupprimerEmail(TypeVoieEmail type, string modifiePar)
    {
        var v = _emails.FirstOrDefault(e => e.Type == type)
            ?? throw new InvalidOperationException($"Aucune voie e-mail de type {type}.");
        Archiver(CanalVoie.Email, v.Type.ToString(), v.Adresse, v.EstActif, ActionHistorique.Supprime, modifiePar);
        _emails.Remove(v);
    }

    // ── Archivage (privé) ─────────────────────────────────────────────────────

    private void Archiver(CanalVoie canal, string typeVoie, string valeur,
                          bool estActif, ActionHistorique action, string modifiePar)
    {
        _historique.Add(new HistoriqueVoie
        {
            VoieCommunicationId = Id,
            Canal      = canal,
            TypeVoie   = typeVoie,
            Valeur     = valeur,
            EstActif   = estActif,
            Action     = action,
            ModifiePar = modifiePar,
            DateAction = DateTime.UtcNow,
        });
    }
}

// ── Entités enfants ───────────────────────────────────────────────────────────

public class VoieTelephone
{
    public Guid              VoieCommunicationId { get; init; }
    public TypeVoieTelephone Type                { get; init; }
    public string            Numero              { get; private set; } = default!;
    public bool              EstActif            { get; private set; } = true;
    public DateTime          DateModification    { get; private set; } = DateTime.UtcNow;

    private VoieTelephone() { }

    internal static VoieTelephone Creer(Guid voieId, TypeVoieTelephone type, string numero) =>
        new() { VoieCommunicationId = voieId, Type = type, Numero = numero,
                EstActif = true, DateModification = DateTime.UtcNow };

    internal void Desactiver() { EstActif = false; DateModification = DateTime.UtcNow; }
    internal void Reactiver()  { EstActif = true;  DateModification = DateTime.UtcNow; }
}

public class VoieEmail
{
    public Guid          VoieCommunicationId { get; init; }
    public TypeVoieEmail Type                { get; init; }
    public string        Adresse             { get; private set; } = default!;
    public bool          EstActif            { get; private set; } = true;
    public DateTime      DateModification    { get; private set; } = DateTime.UtcNow;

    private VoieEmail() { }

    internal static VoieEmail Creer(Guid voieId, TypeVoieEmail type, string adresse) =>
        new() { VoieCommunicationId = voieId, Type = type, Adresse = adresse,
                EstActif = true, DateModification = DateTime.UtcNow };

    internal void Desactiver() { EstActif = false; DateModification = DateTime.UtcNow; }
    internal void Reactiver()  { EstActif = true;  DateModification = DateTime.UtcNow; }
}

/// <summary>
/// Trace unifiée de chaque modification de voie de communication (téléphone ou e-mail).
/// <see cref="Canal"/> indique le type de canal ;
/// <see cref="TypeVoie"/> stocke le nom de l'enum (ex. "Appel", "Professionnel") ;
/// <see cref="Valeur"/> est le numéro ou l'adresse concernés.
/// </summary>
public class HistoriqueVoie
{
    public long             Id                  { get; init; }
    public Guid             VoieCommunicationId { get; init; }
    public CanalVoie        Canal               { get; init; }
    public string           TypeVoie            { get; init; } = default!;
    public string           Valeur              { get; init; } = default!;
    public bool             EstActif            { get; init; }
    public ActionHistorique Action              { get; init; }
    public string           ModifiePar          { get; init; } = default!;
    public DateTime         DateAction          { get; init; }
}


