using MDiator;
using Cnss.Metier.Shared.Domain.Common;

namespace Cnss.Metier.CommunicationInterne.Application.Commands.ValiderMessage;

public record ValiderMessageCommand(
    Guid MessageId,
    Guid ValidateurId,
    string UserId = "",
    string? UserName = null) : IMDiatorRequest<Result>;
