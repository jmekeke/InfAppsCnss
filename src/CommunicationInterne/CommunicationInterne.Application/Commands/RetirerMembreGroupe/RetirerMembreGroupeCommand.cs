using MDiator;
using Cnss.Metier.CommunicationInterne.Domain.Repositories;
using Cnss.Metier.Shared.Application.Abstractions;
using Cnss.Metier.Shared.Domain.Common;

namespace Cnss.Metier.CommunicationInterne.Application.Commands.RetirerMembreGroupe;

public record RetirerMembreGroupeCommand(
    Guid GroupeId,
    Guid AgentId,
    string UserId = "",
    string? UserName = null) : IMDiatorRequest<Result>;

public class RetirerMembreGroupeHandler(
    IGroupeDiffusionRepository groupeRepo,
    IUnitOfWork uow) : IMDiatorHandler<RetirerMembreGroupeCommand, Result>
{
    public async Task<Result> Handle(RetirerMembreGroupeCommand cmd, CancellationToken ct)
    {
        var groupe = await groupeRepo.GetWithMembresAsync(cmd.GroupeId, ct);
        if (groupe is null)
            return Result.NotFound();

        groupe.RetirerMembre(cmd.AgentId);
        await uow.SaveChangesAsync(ct);
        return Result.Success();
    }
}
