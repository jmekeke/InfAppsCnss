namespace Cnss.Metier.CommunicationInterne.Application.Ports;

/// <summary>
/// Port — abstraction d'envoi d'email.
/// L'implémentation initiale peut être fake/log-based sans bloquer le projet.
/// </summary>
public interface IEmailSender
{
    Task EnvoyerAsync(string destinataire, string objet, string corps, CancellationToken ct = default);
}
