using MDiator;
using Cnss.Metier.CommunicationInterne.Domain.Repositories;
using Cnss.Metier.Shared.Domain.Common;

namespace Cnss.Metier.CommunicationInterne.Application.Commands.BasculerEtatGroupe;

/// <summary>
/// Bascule l'état actif/inactif d'un groupe de diffusion.
/// Si le groupe est actif, il est désactivé (soft-delete).
/// S'il est inactif, il est réactivé.
/// </summary>
public record BasculerEtatGroupeCommand(Guid GroupeId) : IMDiatorRequest<Result>;

public class BasculerEtatGroupeHandler(IGroupeDiffusionRepository groupeRepo)
    : IMDiatorHandler<BasculerEtatGroupeCommand, Result>
{
    public async Task<Result> Handle(BasculerEtatGroupeCommand cmd, CancellationToken ct)
    {
        var groupe = await groupeRepo.GetByIdAsync(cmd.GroupeId, ct);
        if (groupe is null)
            return Result.NotFound();

        if (groupe.EstActif)
            groupe.Supprimer();
        else
            groupe.Reactiver();

        await groupeRepo.CommitAsync(ct);
        return Result.Success();
    }
}
