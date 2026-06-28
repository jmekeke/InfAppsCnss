using MDiator;
using Cnss.Metier.CommunicationInterne.Application.Ports;
using Cnss.Metier.CommunicationInterne.Domain.Repositories;
using Cnss.Metier.Shared.Application.Abstractions;
using Cnss.Metier.Shared.Domain.Common;

namespace Cnss.Metier.CommunicationInterne.Application.Commands.RejeterMessage;

public class RejeterMessageHandler(
    IMessageInterneRepository messageRepo,
    IUnitOfWork uow,
    ICurrentUserContext currentUser) : IMDiatorHandler<RejeterMessageCommand, Result>
{
    public async Task<Result> Handle(RejeterMessageCommand cmd, CancellationToken ct)
    {
        currentUser.SetUser(cmd.UserId, cmd.UserName);

        var message = await messageRepo.GetByIdAsync(cmd.MessageId, ct);
        if (message is null)
            return Result.NotFound();

        message.Rejeter(cmd.ValidateurId, cmd.Motif);
        await uow.SaveChangesAsync(ct);
        return Result.Success();
    }
}
