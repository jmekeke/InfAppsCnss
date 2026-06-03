namespace Cnss.Metier.CommunicationInterne.Domain.Enums;

/// <summary>
/// Cycle de vie d'un message interne.
/// Règle métier : un message institutionnel ou sensible ne peut être diffusé sans validation.
/// </summary>
public enum StatutMessage
{
    Brouillon = 1,
    EnAttenteValidation = 2,
    Valide = 3,
    Rejete = 4,
    Programme = 5,
    Diffuse = 6
}
