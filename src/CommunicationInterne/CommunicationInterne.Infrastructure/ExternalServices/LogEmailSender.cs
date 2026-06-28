using Cnss.Metier.CommunicationInterne.Application.Ports;
using Microsoft.Extensions.Logging;

namespace Cnss.Metier.CommunicationInterne.Infrastructure.ExternalServices;

/// <summary>Implémentation log-based — à remplacer par l'adaptateur réel.</summary>
internal sealed class LogEmailSender(ILogger<LogEmailSender> logger) : IEmailSender
{
    public Task EnvoyerAsync(string destinataire, string objet, string corps, CancellationToken ct = default)
    {
        logger.LogInformation("[Email] À: {Destinataire} | Objet: {Objet}", destinataire, objet);
        return Task.CompletedTask;
    }
}
