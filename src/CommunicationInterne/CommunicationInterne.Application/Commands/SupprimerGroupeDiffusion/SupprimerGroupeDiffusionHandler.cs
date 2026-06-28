using MDiator;
using Cnss.Metier.CommunicationInterne.Application.Ports;
using Cnss.Metier.CommunicationInterne.Domain.Repositories;
using Cnss.Metier.Shared.Application.Abstractions;
using Cnss.Metier.Shared.Domain.Common;

namespace Cnss.Metier.CommunicationInterne.Application.Commands.SupprimerGroupeDiffusion;

public class SupprimerGroupeDiffusionHandler(
    IGroupeDiffusionRepository groupeRepo,
    IUnitOfWork uow,
    ICurrentUserContext currentUser) : IMDiatorHandler<SupprimerGroupeDiffusionCommand, Result>
{
    public async Task<Result> Handle(SupprimerGroupeDiffusionCommand cmd, CancellationToken ct)
    {
        currentUser.SetUser(cmd.UserId, cmd.UserName);

        var groupe = await groupeRepo.GetByIdAsync(cmd.GroupeId, ct);
        if (groupe is null)
            return Result.Failure($"Groupe {cmd.GroupeId} introuvable.");

        groupe.Supprimer();
        await uow.SaveChangesAsync(ct);
        return Result.Success();
    }
}
