using MDiator;
using Cnss.Metier.CommunicationInterne.Application.Ports;
using Cnss.Metier.CommunicationInterne.Domain.Repositories;
using Cnss.Metier.Shared.Application.Abstractions;
using Cnss.Metier.Shared.Domain.Common;

namespace Cnss.Metier.CommunicationInterne.Application.Commands.AjouterPieceJointe;

public class AjouterPieceJointeHandler(
    IMessageInterneRepository messageRepo,
    IUnitOfWork uow,
    ICurrentUserContext currentUser) : IMDiatorHandler<AjouterPieceJointeCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(AjouterPieceJointeCommand cmd, CancellationToken ct)
    {
        currentUser.SetUser(cmd.UserId, cmd.UserName);

        var message = await messageRepo.GetByIdAsync(cmd.MessageId, ct);
        if (message is null)
            return Result.Failure<Guid>("Message introuvable.");

        var pj = message.AjouterPieceJointe(cmd.NomFichier, cmd.TypeMime, cmd.TailleOctets);
        await uow.SaveChangesAsync(ct);
        return Result.Success(pj.Id);
    }
}
