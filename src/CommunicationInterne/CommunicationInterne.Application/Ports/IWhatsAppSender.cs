namespace Cnss.Metier.CommunicationInterne.Application.Ports;

/// <summary>
/// Port — abstraction d'envoi WhatsApp.
/// L'implémentation initiale peut être fake/log-based sans bloquer le projet.
/// </summary>
public interface IWhatsAppSender
{
    Task EnvoyerAsync(string numeroWhatsApp, string message, CancellationToken ct = default);
}
