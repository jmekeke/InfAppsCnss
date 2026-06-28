using MDiator;
using Cnss.Metier.Shared.Domain.Common;

namespace Cnss.Metier.CommunicationInterne.Application.Commands.SupprimerMessage;

public record SupprimerMessageCommand(
    Guid MessageId,
    string UserId = "",
    string? UserName = null) : IMDiatorRequest<Result>;
