using Cnss.Metier.CommunicationInterne.Application.Ports;
using Microsoft.Extensions.Logging;

namespace Cnss.Metier.CommunicationInterne.Infrastructure.ExternalServices;

/// <summary>Implémentation log-based — à remplacer par l'adaptateur réel.</summary>
internal sealed class LogSmsSender(ILogger<LogSmsSender> logger) : ISmsSender
{
    public Task EnvoyerAsync(string numeroTelephone, string message, CancellationToken ct = default)
    {
        logger.LogInformation("[SMS] À: {Numero} | Message: {Message}", numeroTelephone, message);
        return Task.CompletedTask;
    }
}
