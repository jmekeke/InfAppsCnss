using MDiator;
using Cnss.Metier.Shared.Domain.Common;

namespace Cnss.Metier.CommunicationInterne.Application.Commands.SoumettreMessageAValidation;

public record SoumettreMessageAValidationCommand(
    Guid MessageId,
    string UserId = "",
    string? UserName = null) : IMDiatorRequest<Result>;
