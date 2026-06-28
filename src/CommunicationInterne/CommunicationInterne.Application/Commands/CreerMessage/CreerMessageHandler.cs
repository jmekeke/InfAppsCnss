using MDiator;
using Cnss.Metier.CommunicationInterne.Application.Ports;
using Cnss.Metier.CommunicationInterne.Domain.Aggregats;
using Cnss.Metier.CommunicationInterne.Domain.Repositories;
using Cnss.Metier.Shared.Application.Abstractions;
using Cnss.Metier.Shared.Domain.Common;

namespace Cnss.Metier.CommunicationInterne.Application.Commands.CreerMessage;

public class CreerMessageHandler(
    IMessageInterneRepository messageRepo,
    IUnitOfWork uow,
    ICurrentUserContext currentUser) : IMDiatorHandler<CreerMessageCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreerMessageCommand cmd, CancellationToken ct)
    {
        currentUser.SetUser(cmd.UserId, cmd.UserName);

        if (cmd.Canaux is not { Count: > 0 })
            return Result.Failure<Guid>("Au moins un canal de diffusion est requis.");

        var message = MessageInterne.Creer(
            cmd.AuteurId,
            cmd.UserName ?? cmd.AuteurId.ToString(),
            cmd.Objet,
            cmd.Corps,
            cmd.EstInstitutionnel,
            cmd.Canaux);

        messageRepo.Add(message);
        // Canaux ignorés par EF → persistance explicite dans la table de jonction
        await messageRepo.ReplaceCanauxAsync(message.Id, cmd.Canaux, ct);
        await uow.SaveChangesAsync(ct);
        return Result.Success(message.Id);
    }
}
