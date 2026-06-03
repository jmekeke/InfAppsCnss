using MDiator;
using Cnss.Metier.CommunicationInterne.Domain.Enums;
using Cnss.Metier.Shared.Domain.Common;

namespace Cnss.Metier.CommunicationInterne.Application.Commands.CreerMessage;

public record CreerMessageCommand(
    Guid AuteurId,
    string Objet,
    string Corps,
    bool EstInstitutionnel,
    List<TypeCanal> Canaux,
    string UserId = "",
    string? UserName = null) : IMDiatorRequest<Result<Guid>>;
