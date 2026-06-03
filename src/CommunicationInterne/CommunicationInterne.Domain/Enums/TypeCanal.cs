namespace Cnss.Metier.CommunicationInterne.Domain.Enums;

/// <summary>
/// Canal de diffusion d'un message interne.
/// Règle métier : une diffusion peut utiliser un ou plusieurs canaux.
/// </summary>
public enum TypeCanal
{
    Email = 1,
    Sms = 2,
    WhatsApp = 3,
    CanalInterne = 4
}
