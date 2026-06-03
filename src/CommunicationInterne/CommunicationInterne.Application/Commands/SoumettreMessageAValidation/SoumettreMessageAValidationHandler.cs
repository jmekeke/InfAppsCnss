using MDiator;
using Cnss.Metier.CommunicationInterne.Domain.Repositories;
using Cnss.Metier.Shared.Application.Abstractions;
using Cnss.Metier.Shared.Domain.Common;

namespace Cnss.Metier.CommunicationInterne.Application.Commands.SoumettreMessageAValidation;

public class SoumettreMessageAValidationHandler(
    IMessageInterneRepository messageRepo,
    IUnitOfWork uow) : IMDiatorHandler<SoumettreMessageAValidationCommand, Result>
{
    public async Task<Result> Handle(SoumettreMessageAValidationCommand cmd, CancellationToken ct)
    {
        var message = await messageRepo.GetByIdAsync(cmd.MessageId, ct);
        if (message is null)
            return Result.NotFound();

        message.SoumettreAValidation();
        await uow.SaveChangesAsync(ct);
        return Result.Success();
    }
}
