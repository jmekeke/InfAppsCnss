using MDiator;
using Cnss.Metier.Shared.Domain.Common;

namespace Cnss.Metier.CommunicationInterne.Application.Commands.DemanderCorrection;

public record DemanderCorrectionCommand(
    Guid MessageId,
    Guid ValidateurId,
    string Commentaire,
    string UserId = "",
    string? UserName = null) : IMDiatorRequest<Result>;
