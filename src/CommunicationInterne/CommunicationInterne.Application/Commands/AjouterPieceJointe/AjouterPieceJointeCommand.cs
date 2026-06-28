using MDiator;
using Cnss.Metier.Shared.Domain.Common;

namespace Cnss.Metier.CommunicationInterne.Application.Commands.AjouterPieceJointe;

public record AjouterPieceJointeCommand(
    Guid MessageId,
    string NomFichier,
    string TypeMime,
    long TailleOctets,
    string UserId = "",
    string? UserName = null) : IMDiatorRequest<Result<Guid>>;
