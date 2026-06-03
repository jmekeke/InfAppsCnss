namespace Cnss.Metier.CommunicationInterne.Application.Ports;

/// <summary>
/// Port — abstraction d'envoi de SMS.
/// L'implémentation initiale peut être fake/log-based sans bloquer le projet.
/// </summary>
public interface ISmsSender
{
    Task EnvoyerAsync(string numeroTelephone, string message, CancellationToken ct = default);
}
