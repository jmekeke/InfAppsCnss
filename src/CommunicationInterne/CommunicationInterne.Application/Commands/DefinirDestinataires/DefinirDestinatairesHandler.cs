using MDiator;
using Cnss.Metier.CommunicationInterne.Application.Ports;
using Cnss.Metier.CommunicationInterne.Domain.Aggregats;
using Cnss.Metier.CommunicationInterne.Domain.Enums;
using Cnss.Metier.CommunicationInterne.Domain.Repositories;
using Cnss.Metier.Shared.Application.Abstractions;
using Cnss.Metier.Shared.Domain.Common;

namespace Cnss.Metier.CommunicationInterne.Application.Commands.DefinirDestinataires;

public class DefinirDestinatairesHandler(
    IMessageInterneRepository messageRepo,
    IUnitOfWork uow,
    ICurrentUserContext currentUser) : IMDiatorHandler<DefinirDestinatairesCommand, Result>
{
    public async Task<Result> Handle(DefinirDestinatairesCommand cmd, CancellationToken ct)
    {
        currentUser.SetUser(cmd.UserId, cmd.UserName);

        var message = await messageRepo.GetByIdAsync(cmd.MessageId, ct);
        if (message is null)
            return Result.Failure("Message introuvable.");

        // Construire les entités DestinataireCible
        var cibles = cmd.Destinataires.Select(d => new DestinataireCible
        {
            MessageInterneId = cmd.MessageId,
            Type = d.Type,
            ReferenceId = d.ReferenceId,
            Libelle = d.Libelle,
        }).ToList();

        // Mise à jour dans l'agrégat (synchronise aussi _groupeIds pour rétrocompat)
        message.DefinirDestinataires(cibles);

        // Persistance via table de jonction dédiée
        await messageRepo.ReplaceDestinatairesAsync(cmd.MessageId, cibles, ct);

        await uow.SaveChangesAsync(ct);
        return Result.Success();
    }
}
