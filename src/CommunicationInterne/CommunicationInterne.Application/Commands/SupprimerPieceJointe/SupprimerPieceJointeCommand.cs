using MDiator;
using Cnss.Metier.Shared.Domain.Common;

namespace Cnss.Metier.CommunicationInterne.Application.Commands.SupprimerPieceJointe;

public record SupprimerPieceJointeCommand(
    Guid MessageId,
    Guid PieceJointeId,
    string UserId = "",
    string? UserName = null) : IMDiatorRequest<Result>;
