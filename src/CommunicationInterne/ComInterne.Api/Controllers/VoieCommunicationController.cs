using Cnss.Metier.CommunicationInterne.Application.Commands.MettreAJourVoieCommunication;
using Cnss.Metier.CommunicationInterne.Application.Queries.GetVoieCommunication;
using Cnss.Metier.CommunicationInterne.Domain.Enums;
using MDiator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ComInterne.Api.Controllers;

/// <summary>
/// Gestion des voies de communication des agents CNSS.
///
/// GET    /api/agents/{id}/voie-communication                               → voies + historique
/// PUT    /api/agents/{id}/voie-communication                               → créer / mettre à jour
///
/// PATCH  /api/agents/{id}/voie-communication/telephones/{type}/desactiver  → soft-delete téléphone
/// PATCH  /api/agents/{id}/voie-communication/telephones/{type}/reactiver   → réactiver téléphone
/// DELETE /api/agents/{id}/voie-communication/telephones/{type}             → suppression physique téléphone
///
/// PATCH  /api/agents/{id}/voie-communication/emails/{type}/desactiver      → soft-delete e-mail
/// PATCH  /api/agents/{id}/voie-communication/emails/{type}/reactiver       → réactiver e-mail
/// DELETE /api/agents/{id}/voie-communication/emails/{type}                 → suppression physique e-mail
///
/// GET    /api/agents/{id}/voie-communication/historique                    → journal complet
/// GET    /api/agents/{id}/voie-communication/historique?canal=Telephone    → filtré par canal
/// </summary>
[Authorize]
[Route("api/agents/{agentIdRh:int}/voie-communication")]
public class VoieCommunicationController(IMediator mediator) : ApiControllerBase
{
    // ── Lecture ───────────────────────────────────────────────────────────────

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Get(int agentIdRh, CancellationToken ct = default)
        => ToResult(await mediator.Send(new GetVoieCommunicationQuery(agentIdRh), ct));

    [HttpGet("historique")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetHistorique(
        int agentIdRh,
        [FromQuery] CanalVoie? canal = null,
        CancellationToken ct = default)
        => ToResult(await mediator.Send(new GetHistoriqueVoieQuery(agentIdRh, canal), ct));

    // ── Mise à jour globale ───────────────────────────────────────────────────

    [HttpPut]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> MettreAJour(
        int agentIdRh,
        [FromBody] MettreAJourVoieCommunicationRequest request,
        CancellationToken ct = default)
    {
        var cmd = new MettreAJourVoieCommunicationCommand(
            AgentIdRh:  agentIdRh,
            Matricule:  request.Matricule,
            Telephones: request.Telephones,
            Emails:     request.Emails,
            UserId:     User.FindFirst("sub")?.Value ?? "",
            UserName:   User.Identity?.Name
        );
        return ToResult(await mediator.Send(cmd, ct));
    }

    // ── Téléphone ─────────────────────────────────────────────────────────────

    [HttpPatch("telephones/{type}/desactiver")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DesactiverTelephone(
        int agentIdRh, TypeVoieTelephone type, CancellationToken ct = default)
        => ToResult(await mediator.Send(
            new DesactiverVoieTelephoneCommand(agentIdRh, type,
                User.FindFirst("sub")?.Value ?? "", User.Identity?.Name), ct));

    [HttpPatch("telephones/{type}/reactiver")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ReactiverTelephone(
        int agentIdRh, TypeVoieTelephone type, CancellationToken ct = default)
        => ToResult(await mediator.Send(
            new ReactiverVoieTelephoneCommand(agentIdRh, type,
                User.FindFirst("sub")?.Value ?? "", User.Identity?.Name), ct));

    [HttpDelete("telephones/{type}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SupprimerTelephone(
        int agentIdRh, TypeVoieTelephone type, CancellationToken ct = default)
        => ToResult(await mediator.Send(
            new SupprimerVoieTelephoneCommand(agentIdRh, type,
                User.FindFirst("sub")?.Value ?? "", User.Identity?.Name), ct));

    // ── E-mail ────────────────────────────────────────────────────────────────

    [HttpPatch("emails/{type}/desactiver")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DesactiverEmail(
        int agentIdRh, TypeVoieEmail type, CancellationToken ct = default)
        => ToResult(await mediator.Send(
            new DesactiverVoieEmailCommand(agentIdRh, type,
                User.FindFirst("sub")?.Value ?? "", User.Identity?.Name), ct));

    [HttpPatch("emails/{type}/reactiver")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ReactiverEmail(
        int agentIdRh, TypeVoieEmail type, CancellationToken ct = default)
        => ToResult(await mediator.Send(
            new ReactiverVoieEmailCommand(agentIdRh, type,
                User.FindFirst("sub")?.Value ?? "", User.Identity?.Name), ct));

    [HttpDelete("emails/{type}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SupprimerEmail(
        int agentIdRh, TypeVoieEmail type, CancellationToken ct = default)
        => ToResult(await mediator.Send(
            new SupprimerVoieEmailCommand(agentIdRh, type,
                User.FindFirst("sub")?.Value ?? "", User.Identity?.Name), ct));
}

public record MettreAJourVoieCommunicationRequest(
    string Matricule,
    IReadOnlyList<VoieTelephoneDto> Telephones,
    IReadOnlyList<VoieEmailDto>     Emails
);

