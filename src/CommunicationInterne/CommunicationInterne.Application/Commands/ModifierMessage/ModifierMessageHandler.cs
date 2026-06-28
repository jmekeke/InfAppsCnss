using MDiator;
using Cnss.Metier.CommunicationInterne.Application.Ports;
using Cnss.Metier.CommunicationInterne.Domain.Repositories;
using Cnss.Metier.Shared.Application.Abstractions;
using Cnss.Metier.Shared.Domain.Common;

namespace Cnss.Metier.CommunicationInterne.Application.Commands.ModifierMessage;

public class ModifierMessageHandler(
    IMessageInterneRepository messageRepo,
    IUnitOfWork uow,
    ICurrentUserContext currentUser) : IMDiatorHandler<ModifierMessageCommand, Result>
{
    public async Task<Result> Handle(ModifierMessageCommand cmd, CancellationToken ct)
    {
        currentUser.SetUser(cmd.UserId, cmd.UserName);

        var message = await messageRepo.GetByIdAsync(cmd.MessageId, ct);
        if (message is null)
            return Result.Failure("Message introuvable.");

        message.Modifier(cmd.Objet, cmd.Corps, cmd.EstInstitutionnel, cmd.Canaux);
        // Canaux ignorés par EF → mise à jour explicite de la table de jonction
        if (cmd.Canaux is { Count: > 0 })
            await messageRepo.ReplaceCanauxAsync(message.Id, cmd.Canaux, ct);
        await uow.SaveChangesAsync(ct);
        return Result.Success();
    }
}
