namespace Cnss.Metier.CommunicationInterne.Domain.Aggregats;

/// <summary>
/// Entité — Pièce jointe attachée à un message interne.
/// Règle métier : uniquement autorisée pour les canaux Email ou WhatsApp.
/// </summary>
public class PieceJointe
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public Guid MessageInterneId { get; init; }
    public string NomFichier { get; init; } = default!;
    public string TypeMime { get; init; } = default!;
    public long TailleOctets { get; init; }
    public DateTime DateAjout { get; init; } = DateTime.UtcNow;
}
