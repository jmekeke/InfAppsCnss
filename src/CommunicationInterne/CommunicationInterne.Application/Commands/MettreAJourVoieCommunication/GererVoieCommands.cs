using MDiator;
using Cnss.Metier.CommunicationInterne.Application.Ports;
using Cnss.Metier.CommunicationInterne.Domain.Enums;
using Cnss.Metier.CommunicationInterne.Domain.Repositories;
using Cnss.Metier.Shared.Application.Abstractions;
using Cnss.Metier.Shared.Domain.Common;

namespace Cnss.Metier.CommunicationInterne.Application.Commands.MettreAJourVoieCommunication;

// ═══════════════════════════════════════════════════════════════════════════════
// TÉLÉPHONE
// ═══════════════════════════════════════════════════════════════════════════════

public record DesactiverVoieTelephoneCommand(
    int AgentIdRh, TypeVoieTelephone Type,
    string UserId = "", string? UserName = null) : IMDiatorRequest<Result>;

public class DesactiverVoieTelephoneHandler(
    IVoieCommunicationRepository voieRepo, ICurrentUserContext currentUser, IUnitOfWork uow)
    : IMDiatorHandler<DesactiverVoieTelephoneCommand, Result>
{
    public async Task<Result> Handle(DesactiverVoieTelephoneCommand cmd, CancellationToken ct)
    {
        currentUser.SetUser(cmd.UserId, cmd.UserName);
        var voie = await voieRepo.GetByAgentIdRhAsync(cmd.AgentIdRh, ct);
        if (voie is null) return Result.NotFound();
        voie.DesactiverTelephone(cmd.Type, cmd.UserName ?? cmd.UserId);
        await uow.SaveChangesAsync(ct);
        return Result.Success();
    }
}

public record ReactiverVoieTelephoneCommand(
    int AgentIdRh, TypeVoieTelephone Type,
    string UserId = "", string? UserName = null) : IMDiatorRequest<Result>;

public class ReactiverVoieTelephoneHandler(
    IVoieCommunicationRepository voieRepo, ICurrentUserContext currentUser, IUnitOfWork uow)
    : IMDiatorHandler<ReactiverVoieTelephoneCommand, Result>
{
    public async Task<Result> Handle(ReactiverVoieTelephoneCommand cmd, CancellationToken ct)
    {
        currentUser.SetUser(cmd.UserId, cmd.UserName);
        var voie = await voieRepo.GetByAgentIdRhAsync(cmd.AgentIdRh, ct);
        if (voie is null) return Result.NotFound();
        voie.ReactiverTelephone(cmd.Type, cmd.UserName ?? cmd.UserId);
        await uow.SaveChangesAsync(ct);
        return Result.Success();
    }
}

public record SupprimerVoieTelephoneCommand(
    int AgentIdRh, TypeVoieTelephone Type,
    string UserId = "", string? UserName = null) : IMDiatorRequest<Result>;

public class SupprimerVoieTelephoneHandler(
    IVoieCommunicationRepository voieRepo, ICurrentUserContext currentUser, IUnitOfWork uow)
    : IMDiatorHandler<SupprimerVoieTelephoneCommand, Result>
{
    public async Task<Result> Handle(SupprimerVoieTelephoneCommand cmd, CancellationToken ct)
    {
        currentUser.SetUser(cmd.UserId, cmd.UserName);
        var voie = await voieRepo.GetByAgentIdRhAsync(cmd.AgentIdRh, ct);
        if (voie is null) return Result.NotFound();
        voie.SupprimerTelephone(cmd.Type, cmd.UserName ?? cmd.UserId);
        await uow.SaveChangesAsync(ct);
        return Result.Success();
    }
}

// ═══════════════════════════════════════════════════════════════════════════════
// E-MAIL
// ═══════════════════════════════════════════════════════════════════════════════

public record DesactiverVoieEmailCommand(
    int AgentIdRh, TypeVoieEmail Type,
    string UserId = "", string? UserName = null) : IMDiatorRequest<Result>;

public class DesactiverVoieEmailHandler(
    IVoieCommunicationRepository voieRepo, ICurrentUserContext currentUser, IUnitOfWork uow)
    : IMDiatorHandler<DesactiverVoieEmailCommand, Result>
{
    public async Task<Result> Handle(DesactiverVoieEmailCommand cmd, CancellationToken ct)
    {
        currentUser.SetUser(cmd.UserId, cmd.UserName);
        var voie = await voieRepo.GetByAgentIdRhAsync(cmd.AgentIdRh, ct);
        if (voie is null) return Result.NotFound();
        voie.DesactiverEmail(cmd.Type, cmd.UserName ?? cmd.UserId);
        await uow.SaveChangesAsync(ct);
        return Result.Success();
    }
}

public record ReactiverVoieEmailCommand(
    int AgentIdRh, TypeVoieEmail Type,
    string UserId = "", string? UserName = null) : IMDiatorRequest<Result>;

public class ReactiverVoieEmailHandler(
    IVoieCommunicationRepository voieRepo, ICurrentUserContext currentUser, IUnitOfWork uow)
    : IMDiatorHandler<ReactiverVoieEmailCommand, Result>
{
    public async Task<Result> Handle(ReactiverVoieEmailCommand cmd, CancellationToken ct)
    {
        currentUser.SetUser(cmd.UserId, cmd.UserName);
        var voie = await voieRepo.GetByAgentIdRhAsync(cmd.AgentIdRh, ct);
        if (voie is null) return Result.NotFound();
        voie.ReactiverEmail(cmd.Type, cmd.UserName ?? cmd.UserId);
        await uow.SaveChangesAsync(ct);
        return Result.Success();
    }
}

public record SupprimerVoieEmailCommand(
    int AgentIdRh, TypeVoieEmail Type,
    string UserId = "", string? UserName = null) : IMDiatorRequest<Result>;

public class SupprimerVoieEmailHandler(
    IVoieCommunicationRepository voieRepo, ICurrentUserContext currentUser, IUnitOfWork uow)
    : IMDiatorHandler<SupprimerVoieEmailCommand, Result>
{
    public async Task<Result> Handle(SupprimerVoieEmailCommand cmd, CancellationToken ct)
    {
        currentUser.SetUser(cmd.UserId, cmd.UserName);
        var voie = await voieRepo.GetByAgentIdRhAsync(cmd.AgentIdRh, ct);
        if (voie is null) return Result.NotFound();
        voie.SupprimerEmail(cmd.Type, cmd.UserName ?? cmd.UserId);
        await uow.SaveChangesAsync(ct);
        return Result.Success();
    }
}


