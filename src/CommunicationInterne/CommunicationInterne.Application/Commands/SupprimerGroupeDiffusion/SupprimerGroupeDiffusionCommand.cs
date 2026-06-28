using MDiator;
using Cnss.Metier.Shared.Domain.Common;

namespace Cnss.Metier.CommunicationInterne.Application.Commands.SupprimerGroupeDiffusion;

public record SupprimerGroupeDiffusionCommand(
    Guid GroupeId,
    string UserId = "",
    string? UserName = null) : IMDiatorRequest<Result>;
