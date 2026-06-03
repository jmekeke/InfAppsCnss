namespace Cnss.Metier.CommunicationInterne.Domain.Enums;

/// <summary>
/// Statut d'une tentative de diffusion vers un destinataire.
/// </summary>
public enum StatutEnvoi
{
    EnAttente = 1,
    Envoye = 2,
    Echec = 3,
    Ignore = 4
}
