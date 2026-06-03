using MDiator;
using Cnss.Metier.Shared.Domain.Common;

namespace Cnss.Metier.CommunicationInterne.Application.Commands.RejeterMessage;

public record RejeterMessageCommand(
    Guid MessageId,
    Guid ValidateurId,
    string Motif,
    string UserId = "",
    string? UserName = null) : IMDiatorRequest<Result>;
