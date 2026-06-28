using Cnss.Metier.CommunicationInterne.Application.Ports;

namespace Cnss.Metier.CommunicationInterne.Infrastructure;

/// <summary>
/// Implémentation scoped de <see cref="ICurrentUserContext"/>.
/// Alimentée au début de chaque commande ; en production remplacée par un middleware HTTP
/// qui lit l'identité depuis le JWT.
/// </summary>
public sealed class CurrentUserContext : ICurrentUserContext
{
    public string UserId { get; private set; } = "système";
    public string? UserName { get; private set; }

    public void SetUser(string userId, string? userName = null)
    {
        UserId = string.IsNullOrWhiteSpace(userId) ? "système" : userId;
        UserName = userName;
    }
}
