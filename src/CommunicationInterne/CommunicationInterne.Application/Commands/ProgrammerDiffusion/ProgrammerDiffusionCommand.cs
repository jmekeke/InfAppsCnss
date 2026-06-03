using MDiator;
using Cnss.Metier.Shared.Domain.Common;

namespace Cnss.Metier.CommunicationInterne.Application.Commands.ProgrammerDiffusion;

public record ProgrammerDiffusionCommand(
    Guid MessageId,
    DateTime DateProgrammee,
    string UserId = "",
    string? UserName = null) : IMDiatorRequest<Result>;
