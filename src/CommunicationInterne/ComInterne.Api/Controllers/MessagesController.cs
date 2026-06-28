using Cnss.Metier.CommunicationInterne.Application.Commands.AjouterPieceJointe;
using Cnss.Metier.CommunicationInterne.Application.Commands.CreerMessage;
using Cnss.Metier.CommunicationInterne.Application.Commands.DemanderCorrection;
using Cnss.Metier.CommunicationInterne.Application.Commands.DefinirDestinataires;
using Cnss.Metier.CommunicationInterne.Application.Commands.LancerDiffusion;
using Cnss.Metier.CommunicationInterne.Application.Commands.ModifierMessage;
using Cnss.Metier.CommunicationInterne.Application.Commands.ProgrammerDiffusion;
using Cnss.Metier.CommunicationInterne.Application.Commands.RejeterMessage;
using Cnss.Metier.CommunicationInterne.Application.Commands.SoumettreMessageAValidation;
using Cnss.Metier.CommunicationInterne.Application.Commands.SupprimerMessage;
using Cnss.Metier.CommunicationInterne.Application.Commands.SupprimerPieceJointe;
using Cnss.Metier.CommunicationInterne.Application.Commands.ValiderMessage;
using Cnss.Metier.CommunicationInterne.Application.Queries.GetMessage;
using Cnss.Metier.CommunicationInterne.Application.Queries.ListerMessages;
using MDiator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ComInterne.Api.Controllers;

[Authorize]
[Route("api/messages")]
public class MessagesController(IMediator mediator) : ApiControllerBase
{
    // ─── Queries ────────────────────────────────────────────────────

    /// <summary>Liste les messages internes avec pagination et recherche.</summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Lister(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? search = null,
        CancellationToken ct = default)
        => ToResult(await mediator.Send(new ListerMessagesQuery(page, pageSize, search), ct));

    /// <summary>Récupère un message par son identifiant.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Get(Guid id, CancellationToken ct = default)
        => ToResult(await mediator.Send(new GetMessageQuery(id), ct));

    // ─── Commands ───────────────────────────────────────────────────

    /// <summary>Crée un nouveau message interne (statut Brouillon).</summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Creer([FromBody] CreerMessageCommand cmd, CancellationToken ct = default)
    {
        var (userId, userName, _) = GetUser();
        Guid.TryParse(userId, out var auteurId);
        var result = await mediator.Send(cmd with { UserId = userId, UserName = userName, AuteurId = auteurId }, ct);
        if (!result.IsSuccess)
            return StatusCode(result.StatusCode ?? 400, new { error = result.Error });
        return Ok(new { id = result.Value });
    }

    /// <summary>Modifie le contenu d'un message brouillon.</summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Modifier(Guid id, [FromBody] ModifierMessageCommand cmd, CancellationToken ct = default)
    {
        var (userId, userName, _) = GetUser();
        return ToResult(await mediator.Send(cmd with { MessageId = id, UserId = userId, UserName = userName }, ct));
    }

    /// <summary>Supprime un message brouillon.</summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Supprimer(Guid id, CancellationToken ct = default)
    {
        var (userId, userName, _) = GetUser();
        return ToResult(await mediator.Send(new SupprimerMessageCommand(id, userId, userName), ct));
    }

    /// <summary>Soumet un message à la validation.</summary>
    [HttpPost("{id:guid}/soumettre")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Soumettre(Guid id, CancellationToken ct = default)
    {
        var (userId, userName, _) = GetUser();
        return ToResult(await mediator.Send(new SoumettreMessageAValidationCommand(id, userId, userName), ct));
    }

    /// <summary>Valide un message soumis.</summary>
    [HttpPost("{id:guid}/valider")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Valider(Guid id, [FromBody] ValiderMessageCommand cmd, CancellationToken ct = default)
    {
        var (userId, userName, _) = GetUser();
        return ToResult(await mediator.Send(cmd with { MessageId = id, UserId = userId, UserName = userName }, ct));
    }

    /// <summary>Rejette un message soumis avec un motif.</summary>
    [HttpPost("{id:guid}/rejeter")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Rejeter(Guid id, [FromBody] RejeterMessageCommand cmd, CancellationToken ct = default)
    {
        var (userId, userName, _) = GetUser();
        return ToResult(await mediator.Send(cmd with { MessageId = id, UserId = userId, UserName = userName }, ct));
    }

    /// <summary>Renvoie un message en correction à l'auteur (retour en Brouillon).</summary>
    [HttpPost("{id:guid}/demander-correction")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DemanderCorrection(Guid id, [FromBody] DemanderCorrectionCommand cmd, CancellationToken ct = default)
    {
        var (userId, userName, _) = GetUser();
        Guid.TryParse(userId, out var validateurId);
        return ToResult(await mediator.Send(cmd with { MessageId = id, ValidateurId = validateurId, UserId = userId, UserName = userName }, ct));
    }

    /// <summary>Programme la diffusion d'un message validé.</summary>
    [HttpPost("{id:guid}/programmer")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Programmer(Guid id, [FromBody] ProgrammerDiffusionCommand cmd, CancellationToken ct = default)
    {
        var (userId, userName, _) = GetUser();
        return ToResult(await mediator.Send(cmd with { MessageId = id, UserId = userId, UserName = userName }, ct));
    }

    /// <summary>Lance immédiatement la diffusion d'un message.</summary>
    [HttpPost("{id:guid}/lancer-diffusion")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> LancerDiffusion(Guid id, [FromBody] LancerDiffusionCommand cmd, CancellationToken ct = default)
    {
        var (userId, userName, _) = GetUser();
        return ToResult(await mediator.Send(cmd with { MessageId = id, UserId = userId, UserName = userName }, ct));
    }

    /// <summary>Définit les destinataires cibles d'un message (multi-types).</summary>
    [HttpPost("{id:guid}/destinataires")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DefinirDestinataires(Guid id, [FromBody] DefinirDestinatairesCommand cmd, CancellationToken ct = default)
    {
        var (userId, userName, _) = GetUser();
        return ToResult(await mediator.Send(cmd with { MessageId = id, UserId = userId, UserName = userName }, ct));
    }

    // ─── Pièces jointes ─────────────────────────────────────────────

    /// <summary>Ajoute une pièce jointe à un message brouillon (upload multipart/form-data).</summary>
    [HttpPost("{id:guid}/pieces-jointes")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> AjouterPieceJointe(Guid id, IFormFile file, CancellationToken ct = default)
    {
        if (file is null || file.Length == 0)
            return BadRequest(new { error = "Fichier manquant ou vide." });

        var (userId, userName, _) = GetUser();
        var cmd = new AjouterPieceJointeCommand(id, file.FileName, file.ContentType, file.Length, userId, userName);
        return ToResult(await mediator.Send(cmd, ct));
    }

    /// <summary>Supprime une pièce jointe d'un message brouillon.</summary>
    [HttpDelete("{id:guid}/pieces-jointes/{pieceJointeId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SupprimerPieceJointe(Guid id, Guid pieceJointeId, CancellationToken ct = default)
    {
        var (userId, userName, _) = GetUser();
        return ToResult(await mediator.Send(new SupprimerPieceJointeCommand(id, pieceJointeId, userId, userName), ct));
    }
}
