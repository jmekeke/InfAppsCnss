namespace Cnss.Metier.CommunicationInterne.Application.Ports;

/// <summary>
/// Contexte de l'utilisateur courant, accessible par l'intercepteur d'audit.
/// Doit être alimenté au début de chaque commande (ou via un middleware HTTP en production).
/// </summary>
public interface ICurrentUserContext
{
    /// <summary>Identifiant de l'utilisateur courant (défaut : "système").</summary>
    string UserId { get; }

    /// <summary>Nom affiché de l'utilisateur courant.</summary>
    string? UserName { get; }

    /// <summary>Initialise le contexte utilisateur depuis la commande entrante.</summary>
    void SetUser(string userId, string? userName = null);
}
