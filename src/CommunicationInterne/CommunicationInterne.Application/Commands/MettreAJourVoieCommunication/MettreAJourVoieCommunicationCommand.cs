using MDiator;
using Cnss.Metier.CommunicationInterne.Application.Ports;
using Cnss.Metier.CommunicationInterne.Domain.Aggregats;
using Cnss.Metier.CommunicationInterne.Domain.Enums;
using Cnss.Metier.CommunicationInterne.Domain.Repositories;
using Cnss.Metier.Shared.Application.Abstractions;
using Cnss.Metier.Shared.Domain.Common;

namespace Cnss.Metier.CommunicationInterne.Application.Commands.MettreAJourVoieCommunication;

// ── DTOs de voies ─────────────────────────────────────────────────────────────

/// <summary>Numéro de téléphone à associer à un type de voie.</summary>
public record VoieTelephoneDto(TypeVoieTelephone Type, string? Numero);

/// <summary>Adresse e-mail à associer à un type de voie.</summary>
public record VoieEmailDto(TypeVoieEmail Type, string? Adresse);

// ── Commande ──────────────────────────────────────────────────────────────────

/// <summary>
/// Met à jour (ou crée) les voies de communication d'un agent CNSS.
///
/// Pour chaque voie :
///   - Si <c>Numero</c> / <c>Adresse</c> est fourni et non vide → définit la voie.
///   - Si <c>Numero</c> / <c>Adresse</c> est null ou vide         → supprime la voie.
/// </summary>
public record MettreAJourVoieCommunicationCommand(
    int    AgentIdRh,
    string Matricule,
    IReadOnlyList<VoieTelephoneDto> Telephones,
    IReadOnlyList<VoieEmailDto>     Emails,
    string UserId   = "",
    string? UserName = null
) : IMDiatorRequest<Result>;

// ── Handler ───────────────────────────────────────────────────────────────────

public class MettreAJourVoieCommunicationHandler(
    IVoieCommunicationRepository voieRepo,
    ICurrentUserContext          currentUser,
    IUnitOfWork                  uow)
    : IMDiatorHandler<MettreAJourVoieCommunicationCommand, Result>
{
    public async Task<Result> Handle(MettreAJourVoieCommunicationCommand cmd, CancellationToken ct)
    {
        currentUser.SetUser(cmd.UserId, cmd.UserName);

        // Récupère la voie existante ou en crée une nouvelle
        var voie = await voieRepo.GetByAgentIdRhAsync(cmd.AgentIdRh, ct);
        if (voie is null)
        {
            voie = VoieCommunication.Creer(cmd.AgentIdRh, cmd.Matricule);
            voieRepo.Add(voie);
        }

        // ── Téléphones ────────────────────────────────────────────────────────
        foreach (var t in cmd.Telephones)
        {
            if (!string.IsNullOrWhiteSpace(t.Numero))
                voie.DefinirTelephone(t.Type, t.Numero, cmd.UserName ?? cmd.UserId);
            else
            {
                try { voie.SupprimerTelephone(t.Type, cmd.UserName ?? cmd.UserId); }
                catch (InvalidOperationException) { /* déjà absent — idempotent */ }
            }
        }

        // ── E-mails ───────────────────────────────────────────────────────────
        foreach (var e in cmd.Emails)
        {
            if (!string.IsNullOrWhiteSpace(e.Adresse))
                voie.DefinirEmail(e.Type, e.Adresse, cmd.UserName ?? cmd.UserId);
            else
            {
                try { voie.SupprimerEmail(e.Type, cmd.UserName ?? cmd.UserId); }
                catch (InvalidOperationException) { /* déjà absent — idempotent */ }
            }
        }

        await uow.SaveChangesAsync(ct);
        return Result.Success();
    }
}
