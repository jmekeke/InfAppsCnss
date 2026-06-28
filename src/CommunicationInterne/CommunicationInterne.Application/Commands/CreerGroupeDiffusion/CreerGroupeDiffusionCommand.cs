using MDiator;
using Cnss.Metier.CommunicationInterne.Domain.Enums;
using Cnss.Metier.Shared.Domain.Common;

namespace Cnss.Metier.CommunicationInterne.Application.Commands.CreerGroupeDiffusion;

public record CreerGroupeDiffusionCommand(
    Guid CreateurId,
    string Nom,
    string? Description = null,
    TypeGroupe TypeGroupe = TypeGroupe.Manuel,
    string? CritereType = null,
    string? CritereValeur = null,
    string UserId = "",
    string? UserName = null) : IMDiatorRequest<Result<Guid>>;
