using MDiator;
using Cnss.Metier.CommunicationInterne.Domain.Enums;
using Cnss.Metier.Shared.Domain.Common;

namespace Cnss.Metier.CommunicationInterne.Application.Commands.ModifierMessage;

public record ModifierMessageCommand(
    Guid MessageId,
    string? Objet,
    string? Corps,
    bool? EstInstitutionnel,
    List<TypeCanal>? Canaux,
    string UserId = "",
    string? UserName = null) : IMDiatorRequest<Result>;
