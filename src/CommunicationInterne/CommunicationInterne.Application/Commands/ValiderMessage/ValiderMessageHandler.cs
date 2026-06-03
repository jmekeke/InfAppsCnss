using MDiator;
using Cnss.Metier.CommunicationInterne.Domain.Repositories;
using Cnss.Metier.Shared.Application.Abstractions;
using Cnss.Metier.Shared.Domain.Common;

namespace Cnss.Metier.CommunicationInterne.Application.Commands.ValiderMessage;

public class ValiderMessageHandler(
    IMessageInterneRepository messageRepo,
    IUnitOfWork uow) : IMDiatorHandler<ValiderMessageCommand, Result>
{
    public async Task<Result> Handle(ValiderMessageCommand cmd, CancellationToken ct)
    {
        var message = await messageRepo.GetByIdAsync(cmd.MessageId, ct);
        if (message is null)
            return Result.NotFound();

        message.Valider(cmd.ValidateurId);
        await uow.SaveChangesAsync(ct);
        return Result.Success();
    }
}
