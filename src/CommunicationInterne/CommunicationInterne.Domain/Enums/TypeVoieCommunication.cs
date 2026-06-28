namespace Cnss.Metier.CommunicationInterne.Domain.Enums;

/// <summary>Type de canal téléphonique pour une voie de communication.</summary>
public enum TypeVoieTelephone
{
    Appel    = 1,
    Sms      = 2,
    WhatsApp = 3,
}

/// <summary>Type d'adresse e-mail pour une voie de communication.</summary>
public enum TypeVoieEmail
{
    Professionnel = 1,
    Prive         = 2,
}

/// <summary>Canal de communication (téléphonique ou e-mail).</summary>
public enum CanalVoie
{
    Telephone = 1,
    Email     = 2,
}

/// <summary>Type d'action enregistré dans l'historique des voies.</summary>
public enum ActionHistorique
{
    Cree      = 1,
    Modifie   = 2,
    Desactive = 3,
    Reactive  = 4,
    Supprime  = 5,
}
