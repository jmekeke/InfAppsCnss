using Cnss.Metier.CommunicationInterne.Domain.Aggregats;
using Cnss.Metier.Shared.Domain;
using Cnss.Metier.Shared.Domain.Common;

namespace Cnss.Metier.CommunicationInterne.Domain.Repositories;

public interface IMessageInterneRepository : IRepository<MessageInterne>
{
    Task<MessageInterne?> GetWithCanauxAsync(Guid id, CancellationToken ct = default);
    Task<PagedResult<MessageInterne>> GetAllAsync(int page = 1, int pageSize = 20, string? search = null, CancellationToken ct = default);
    Task<List<MessageInterne>> GetByAuteurAsync(Guid auteurId, CancellationToken ct = default);
    /// <summary>Retourne les canaux pour un ensemble de messages identifiés par leurs Ids.</summary>
    Task<Dictionary<Guid, List<string>>> GetCanauxForMessagesAsync(IEnumerable<Guid> messageIds, CancellationToken ct = default);
    /// <summary>Retourne les canaux d'un message unique.</summary>
    Task<List<string>> GetCanauxAsync(Guid messageId, CancellationToken ct = default);
    /// <summary>Remplace (supprime + réinsère) les canaux d'un message dans la table de jonction.</summary>
    Task ReplaceCanauxAsync(Guid messageId, IEnumerable<Domain.Enums.TypeCanal> canaux, CancellationToken ct = default);
    /// <summary>Retourne les pièces jointes d'un message.</summary>
    Task<List<PieceJointe>> GetPiecesJointesAsync(Guid messageId, CancellationToken ct = default);
    /// <summary>Retourne les ids de groupes destinataires d'un message (rétrocompat).</summary>
    Task<List<Guid>> GetGroupeIdsAsync(Guid messageId, CancellationToken ct = default);
    /// <summary>Retourne les destinataires cibles d'un message.</summary>
    Task<List<DestinataireCible>> GetDestinatairesAsync(Guid messageId, CancellationToken ct = default);
    /// <summary>Remplace (supprime + réinsère) les destinataires cibles d'un message.</summary>
    Task ReplaceDestinatairesAsync(Guid messageId, IEnumerable<DestinataireCible> destinataires, CancellationToken ct = default);
    Task CommitAsync(CancellationToken ct = default);
}
