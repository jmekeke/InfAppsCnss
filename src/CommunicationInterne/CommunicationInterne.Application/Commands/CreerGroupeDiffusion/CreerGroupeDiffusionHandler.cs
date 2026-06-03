using MDiator;
using Cnss.Metier.CommunicationInterne.Domain.Aggregats;
using Cnss.Metier.CommunicationInterne.Domain.Repositories;
using Cnss.Metier.Shared.Application.Abstractions;
using Cnss.Metier.Shared.Domain.Common;

namespace Cnss.Metier.CommunicationInterne.Application.Commands.CreerGroupeDiffusion;

public class CreerGroupeDiffusionHandler(
    IGroupeDiffusionRepository groupeRepo,
    IUnitOfWork uow) : IMDiatorHandler<CreerGroupeDiffusionCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreerGroupeDiffusionCommand cmd, CancellationToken ct)
    {
        var groupe = GroupeDiffusion.Creer(cmd.CreateurId, cmd.Nom, cmd.Description, cmd.TypeGroupe);
        groupeRepo.Add(groupe);
        await uow.SaveChangesAsync(ct);
        return Result.Success(groupe.Id);
    }
}
