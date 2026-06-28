using Cnss.Metier.CommunicationInterne.Application.Commands.AjouterMembreGroupe;
using Cnss.Metier.CommunicationInterne.Application.Commands.BasculerEtatGroupe;
using Cnss.Metier.CommunicationInterne.Application.Commands.CreerGroupeDiffusion;
using Cnss.Metier.CommunicationInterne.Application.Commands.ModifierGroupeDiffusion;
using Cnss.Metier.CommunicationInterne.Application.Commands.RetirerMembreGroupe;
using Cnss.Metier.CommunicationInterne.Application.Commands.SupprimerGroupeDiffusion;
using Cnss.Metier.CommunicationInterne.Application.Queries.GetGroupe;
using Cnss.Metier.CommunicationInterne.Application.Queries.ListerGroupes;
using Cnss.Metier.CommunicationInterne.Application.Queries.ListerGroupesMembresEnrichis;
using MDiator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ComInterne.Api.Controllers;

[Authorize]
[Route("api/groupes-diffusion")]
public class GroupesDiffusionController(IMediator mediator) : ApiControllerBase
{
    // ─── Queries ────────────────────────────────────────────────────

    /// <summary>Liste les groupes de diffusion avec pagination et recherche.</summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Lister(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? search = null,
        CancellationToken ct = default)
        => ToResult(await mediator.Send(new ListerGroupesQuery(page, pageSize, search), ct));

    /// <summary>Récupère un groupe de diffusion avec ses membres.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Get(Guid id, CancellationToken ct = default)
        => ToResult(await mediator.Send(new GetGroupeQuery(id), ct));

    // ─── Commands ───────────────────────────────────────────────────

    /// <summary>Crée un nouveau groupe de diffusion.</summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Creer([FromBody] CreerGroupeDiffusionCommand cmd, CancellationToken ct = default)
    {
        var (userId, userName, _) = GetUser();
        Guid.TryParse(userId, out var createurId);
        return ToResult(await mediator.Send(cmd with { UserId = userId, UserName = userName, CreateurId = createurId }, ct));
    }

    /// <summary>Supprime un groupe de diffusion.</summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Supprimer(Guid id, CancellationToken ct = default)
    {
        var (userId, userName, _) = GetUser();
        return ToResult(await mediator.Send(new SupprimerGroupeDiffusionCommand(id, userId, userName), ct));
    }

    /// <summary>Modifie le nom, la description et le type d'un groupe existant.</summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Modifier(Guid id, [FromBody] ModifierGroupeDiffusionCommand cmd, CancellationToken ct = default)
        => ToResult(await mediator.Send(cmd with { GroupeId = id }, ct));

    /// <summary>
    /// Bascule l'état actif/inactif du groupe.
    /// Désactive si actif, réactive si inactif.
    /// </summary>
    [HttpPatch("{id:guid}/basculer-etat")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> BasculerEtat(Guid id, CancellationToken ct = default)
        => ToResult(await mediator.Send(new BasculerEtatGroupeCommand(id), ct));

    // ─── Membres ────────────────────────────────────────────────────

    /// <summary>Ajoute un agent comme membre du groupe.</summary>
    [HttpPost("{id:guid}/membres")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AjouterMembre(Guid id, [FromBody] AjouterMembreGroupeCommand cmd, CancellationToken ct = default)
    {
        var (userId, userName, _) = GetUser();
        return ToResult(await mediator.Send(cmd with { GroupeId = id, UserId = userId, UserName = userName }, ct));
    }

    /// <summary>Retire un agent du groupe.</summary>
    [HttpDelete("{id:guid}/membres/{agentId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RetirerMembre(Guid id, Guid agentId, CancellationToken ct = default)
    {
        var (userId, userName, _) = GetUser();
        return ToResult(await mediator.Send(new RetirerMembreGroupeCommand(id, agentId, userId, userName), ct));
    }

    /// <summary>Ajoute un agent depuis la base RH (identifiant int) comme membre du groupe.</summary>
    [HttpPost("{id:guid}/membres/{agentIdRh:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AjouterMembreRh(Guid id, int agentIdRh, CancellationToken ct = default)
    {
        var (userId, userName, _) = GetUser();
        return ToResult(await mediator.Send(new AjouterMembreRhCommand(id, agentIdRh, userId, userName), ct));
    }

    // ─── Membres enrichis (données RH) ──────────────────────────────

    /// <summary>Liste tous les groupes avec leurs membres enrichis depuis la base RH.</summary>
    [HttpGet("membres-enrichis")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ListerMembresEnrichis(CancellationToken ct = default)
        => ToResult(await mediator.Send(new ListerGroupesMembresEnrichisQuery(), ct));

    /// <summary>Récupère un agent spécifique dans un groupe avec ses données RH.</summary>
    [HttpGet("{id:guid}/membres/{agentIdRh:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetMembreEnrichi(Guid id, int agentIdRh, CancellationToken ct = default)
        => ToResult(await mediator.Send(new GetMembreGroupeQuery(id, agentIdRh), ct));
}
