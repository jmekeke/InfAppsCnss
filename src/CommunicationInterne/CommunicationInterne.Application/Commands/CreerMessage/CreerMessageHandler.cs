using MDiator;
using Cnss.Metier.CommunicationInterne.Domain.Aggregats;
using Cnss.Metier.CommunicationInterne.Domain.Repositories;
using Cnss.Metier.Shared.Application.Abstractions;
using Cnss.Metier.Shared.Domain.Common;

namespace Cnss.Metier.CommunicationInterne.Application.Commands.CreerMessage;

public class CreerMessageHandler(
    IMessageInterneRepository messageRepo,
    IUnitOfWork uow) : IMDiatorHandler<CreerMessageCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreerMessageCommand cmd, CancellationToken ct)
    {
        if (cmd.Canaux is not { Count: > 0 })
            return Result.Failure<Guid>("Au moins un canal de diffusion est requis.");

        var message = MessageInterne.Creer(
            cmd.AuteurId,
            cmd.Objet,
            cmd.Corps,
            cmd.EstInstitutionnel,
            cmd.Canaux);

        messageRepo.Add(message);
        await uow.SaveChangesAsync(ct);
        return Result.Success(message.Id);
    }
}
