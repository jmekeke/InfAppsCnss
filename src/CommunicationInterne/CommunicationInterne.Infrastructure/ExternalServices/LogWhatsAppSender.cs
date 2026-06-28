using Cnss.Metier.CommunicationInterne.Application.Ports;
using Microsoft.Extensions.Logging;

namespace Cnss.Metier.CommunicationInterne.Infrastructure.ExternalServices;

/// <summary>Implémentation log-based — à remplacer par l'adaptateur réel.</summary>
internal sealed class LogWhatsAppSender(ILogger<LogWhatsAppSender> logger) : IWhatsAppSender
{
    public Task EnvoyerAsync(string numeroWhatsApp, string message, CancellationToken ct = default)
    {
        logger.LogInformation("[WhatsApp] À: {Numero} | Message: {Message}", numeroWhatsApp, message);
        return Task.CompletedTask;
    }
}
