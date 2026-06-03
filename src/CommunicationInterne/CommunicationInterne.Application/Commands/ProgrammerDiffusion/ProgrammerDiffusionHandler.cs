using MDiator;
using Cnss.Metier.CommunicationInterne.Domain.Repositories;
using Cnss.Metier.Shared.Application.Abstractions;
using Cnss.Metier.Shared.Domain.Common;

namespace Cnss.Metier.CommunicationInterne.Application.Commands.ProgrammerDiffusion;

public class ProgrammerDiffusionHandler(
    IMessageInterneRepository messageRepo,
    IUnitOfWork uow) : IMDiatorHandler<ProgrammerDiffusionCommand, Result>
{
    public async Task<Result> Handle(ProgrammerDiffusionCommand cmd, CancellationToken ct)
    {
        if (cmd.DateProgrammee <= DateTime.UtcNow)
            return Result.Failure("La date programmée doit être dans le futur.");

        var message = await messageRepo.GetByIdAsync(cmd.MessageId, ct);
        if (message is null)
            return Result.NotFound();

        message.Programmer(cmd.DateProgrammee);
        await uow.SaveChangesAsync(ct);
        return Result.Success();
    }
}
