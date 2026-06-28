using MDiator;
using Cnss.Metier.CommunicationInterne.Application.Ports;
using Cnss.Metier.CommunicationInterne.Domain.Repositories;
using Cnss.Metier.Shared.Application.Abstractions;
using Cnss.Metier.Shared.Domain.Common;

namespace Cnss.Metier.CommunicationInterne.Application.Commands.AjouterMembreGroupe;

public record AjouterMembreGroupeCommand(
    Guid GroupeId,
    Guid AgentId,
    string UserId = "",
    string? UserName = null) : IMDiatorRequest<Result>;

public class AjouterMembreGroupeHandler(
    IGroupeDiffusionRepository groupeRepo,
    IUnitOfWork uow,
    ICurrentUserContext currentUser) : IMDiatorHandler<AjouterMembreGroupeCommand, Result>
{
    public async Task<Result> Handle(AjouterMembreGroupeCommand cmd, CancellationToken ct)
    {
        currentUser.SetUser(cmd.UserId, cmd.UserName);

        var groupe = await groupeRepo.GetWithMembresAsync(cmd.GroupeId, ct);
        if (groupe is null)
            return Result.NotFound();

        // Règle : la vérification de l'activité de l'agent est déléguée à l'appelant (API/intégration)
        groupe.AjouterMembre(cmd.AgentId);
        await uow.SaveChangesAsync(ct);
        return Result.Success();
    }
}
