using Cnss.Metier.CommunicationInterne.Domain.Aggregats;

namespace Cnss.Metier.CommunicationInterne.Domain.Repositories;

/// <summary>
/// Repository pour l'agrégat VoieCommunication.
/// Une voie est créée la première fois et mise à jour via l'agrégat.
/// </summary>
public interface IVoieCommunicationRepository
{
    /// <summary>
    /// Retourne la voie de communication d'un agent RH, ou null si elle n'existe pas encore.
    /// </summary>
    Task<VoieCommunication?> GetByAgentIdRhAsync(int agentIdRh, CancellationToken ct = default);

    void Add(VoieCommunication voie);

    /// <summary>
    /// Retourne pour chaque agent enregistré son téléphone actif prioritaire (Appel > Sms > WhatsApp)
    /// et son e-mail actif prioritaire (Professionnel > Prive).
    /// </summary>
    Task<List<(int AgentIdRh, string? TelephoneActif, string? EmailProfActif)>> GetResumesAsync(CancellationToken ct = default);
}
