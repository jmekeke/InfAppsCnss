using MDiator;
using Cnss.Metier.CommunicationInterne.Domain.Enums;
using Cnss.Metier.CommunicationInterne.Domain.Repositories;
using Cnss.Metier.Shared.Domain.Common;

namespace Cnss.Metier.CommunicationInterne.Application.Commands.ModifierGroupeDiffusion;

public record ModifierGroupeDiffusionCommand(
    Guid GroupeId,
    string Nom,
    string? Description,
    TypeGroupe TypeGroupe) : IMDiatorRequest<Result>;

public class ModifierGroupeDiffusionHandler(IGroupeDiffusionRepository groupeRepo)
    : IMDiatorHandler<ModifierGroupeDiffusionCommand, Result>
{
    public async Task<Result> Handle(ModifierGroupeDiffusionCommand cmd, CancellationToken ct)
    {
        var groupe = await groupeRepo.GetByIdAsync(cmd.GroupeId, ct);
        if (groupe is null)
            return Result.NotFound();

        groupe.Modifier(cmd.Nom, cmd.Description, cmd.TypeGroupe);
        await groupeRepo.CommitAsync(ct);
        return Result.Success();
    }
}
