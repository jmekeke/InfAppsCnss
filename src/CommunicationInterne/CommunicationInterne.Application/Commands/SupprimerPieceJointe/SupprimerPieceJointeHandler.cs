using MDiator;
using Cnss.Metier.CommunicationInterne.Application.Ports;
using Cnss.Metier.CommunicationInterne.Domain.Repositories;
using Cnss.Metier.Shared.Application.Abstractions;
using Cnss.Metier.Shared.Domain.Common;

namespace Cnss.Metier.CommunicationInterne.Application.Commands.SupprimerPieceJointe;

public class SupprimerPieceJointeHandler(
    IMessageInterneRepository messageRepo,
    IUnitOfWork uow,
    ICurrentUserContext currentUser) : IMDiatorHandler<SupprimerPieceJointeCommand, Result>
{
    public async Task<Result> Handle(SupprimerPieceJointeCommand cmd, CancellationToken ct)
    {
        currentUser.SetUser(cmd.UserId, cmd.UserName);

        var message = await messageRepo.GetByIdAsync(cmd.MessageId, ct);
        if (message is null)
            return Result.Failure("Message introuvable.");

        message.SupprimerPieceJointe(cmd.PieceJointeId);
        await uow.SaveChangesAsync(ct);
        return Result.Success();
    }
}
