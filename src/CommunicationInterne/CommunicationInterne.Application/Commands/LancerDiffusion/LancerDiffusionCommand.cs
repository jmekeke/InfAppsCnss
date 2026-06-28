using MDiator;
using Cnss.Metier.CommunicationInterne.Application.Ports;
using Cnss.Metier.CommunicationInterne.Domain.Aggregats;
using Cnss.Metier.CommunicationInterne.Domain.Enums;
using Cnss.Metier.CommunicationInterne.Domain.Repositories;
using Cnss.Metier.Shared.Application.Abstractions;
using Cnss.Metier.Shared.Domain.Common;

namespace Cnss.Metier.CommunicationInterne.Application.Commands.LancerDiffusion;

public record LancerDiffusionCommand(
    Guid MessageId,
    Guid GroupeId,
    Guid DeclencheurId,
    string UserId = "",
    string? UserName = null) : IMDiatorRequest<Result<Guid>>;

public class LancerDiffusionHandler(
    IMessageInterneRepository messageRepo,
    IGroupeDiffusionRepository groupeRepo,
    IDossierDiffusionRepository dossierRepo,
    IEmailSender emailSender,
    ISmsSender smsSender,
    IWhatsAppSender whatsAppSender,
    IUnitOfWork uow,
    ICurrentUserContext currentUser) : IMDiatorHandler<LancerDiffusionCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(LancerDiffusionCommand cmd, CancellationToken ct)
    {
        currentUser.SetUser(cmd.UserId, cmd.UserName);

        var message = await messageRepo.GetWithCanauxAsync(cmd.MessageId, ct);
        if (message is null)
            return Result.NotFound<Guid>();

        var groupe = await groupeRepo.GetWithMembresAsync(cmd.GroupeId, ct);
        if (groupe is null)
            return Result.NotFound<Guid>();

        int nbDestinataires = groupe.CompterDestinataires();
        if (nbDestinataires == 0)
            return Result.Failure<Guid>("Le groupe ne contient aucun destinataire actif.");

        // Règle : calcul du nombre de destinataires avant la diffusion
        var dossier = DossierDiffusion.Lancer(message.Id, cmd.DeclencheurId, nbDestinataires);

        foreach (var membre in groupe.Membres)
        {
            foreach (var canal in message.Canaux)
            {
                try
                {
                    await EnvoyerSurCanal(canal, membre.AgentId, message.Objet, message.Corps, ct);
                    dossier.EnregistrerEnvoi(membre.AgentId, canal, StatutEnvoi.Envoye);
                }
                catch (Exception ex)
                {
                    dossier.EnregistrerEnvoi(membre.AgentId, canal, StatutEnvoi.Echec, ex.Message);
                }
            }
        }

        message.MarquerCommeDiffuse();
        dossierRepo.Add(dossier);
        await uow.SaveChangesAsync(ct);
        return Result.Success(dossier.Id);
    }

    private Task EnvoyerSurCanal(TypeCanal canal, Guid agentId, string objet, string corps, CancellationToken ct)
        => canal switch
        {
            TypeCanal.Email => emailSender.EnvoyerAsync(agentId.ToString(), objet, corps, ct),
            TypeCanal.Sms => smsSender.EnvoyerAsync(agentId.ToString(), corps, ct),
            TypeCanal.WhatsApp => whatsAppSender.EnvoyerAsync(agentId.ToString(), corps, ct),
            _ => Task.CompletedTask
        };
}
