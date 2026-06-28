using MDiator;
using Cnss.Metier.CommunicationInterne.Application.Ports;
using Cnss.Metier.CommunicationInterne.Domain.Repositories;
using Cnss.Metier.Shared.Application.Abstractions;
using Cnss.Metier.Shared.Domain.Common;

namespace Cnss.Metier.CommunicationInterne.Application.Commands.AjouterMembreGroupe;

/// <summary>
/// Variante de AjouterMembreGroupeCommand qui accepte l'identifiant int de la table Agent (RH_DB).
/// L'int est converti en Guid déterministe : ex. 3467 → 00000000-0000-0000-0000-000000003467.
/// </summary>
public record AjouterMembreRhCommand(
    Guid GroupeId,
    int AgentIdRh,
    string UserId = "",
    string? UserName = null) : IMDiatorRequest<Result>;

public class AjouterMembreRhHandler(
    IGroupeDiffusionRepository groupeRepo,
    IUnitOfWork uow,
    ICurrentUserContext currentUser) : IMDiatorHandler<AjouterMembreRhCommand, Result>
{
    /// <summary>Convertit un int RH en Guid déterministe reproductible.</summary>
    public static Guid ToGuid(int id) => new($"00000000-0000-0000-0000-{id:D12}");

    public async Task<Result> Handle(AjouterMembreRhCommand cmd, CancellationToken ct)
    {
        currentUser.SetUser(cmd.UserId, cmd.UserName);

        var groupe = await groupeRepo.GetWithMembresAsync(cmd.GroupeId, ct);
        if (groupe is null)
            return Result.NotFound();

        groupe.AjouterMembre(ToGuid(cmd.AgentIdRh));
        await uow.SaveChangesAsync(ct);
        return Result.Success();
    }
}
