using MDiator;
using Cnss.Metier.CommunicationInterne.Application.Ports;
using Cnss.Metier.CommunicationInterne.Domain.Repositories;
using Cnss.Metier.Shared.Application.Abstractions;
using Cnss.Metier.Shared.Domain.Common;

namespace Cnss.Metier.CommunicationInterne.Application.Commands.DemanderCorrection;

public class DemanderCorrectionHandler(
    IMessageInterneRepository messageRepo,
    IUnitOfWork uow,
    ICurrentUserContext currentUser) : IMDiatorHandler<DemanderCorrectionCommand, Result>
{
    public async Task<Result> Handle(DemanderCorrectionCommand cmd, CancellationToken ct)
    {
        currentUser.SetUser(cmd.UserId, cmd.UserName);

        var message = await messageRepo.GetByIdAsync(cmd.MessageId, ct);
        if (message is null)
            return Result.NotFound();

        message.DemanderCorrection(cmd.ValidateurId, cmd.Commentaire);
        await uow.SaveChangesAsync(ct);
        return Result.Success();
    }
}
