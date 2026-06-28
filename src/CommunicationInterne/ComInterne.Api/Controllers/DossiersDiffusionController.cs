using Cnss.Metier.CommunicationInterne.Application.Queries.GetDossierDiffusion;
using Cnss.Metier.CommunicationInterne.Application.Queries.ListerDossiersDiffusion;
using MDiator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ComInterne.Api.Controllers;

[Authorize]
[Route("api/dossiers-diffusion")]
public class DossiersDiffusionController(IMediator mediator) : ApiControllerBase
{
    /// <summary>Récupère un dossier de diffusion avec ses lignes d'envoi.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Get(Guid id, CancellationToken ct = default)
        => ToResult(await mediator.Send(new GetDossierDiffusionQuery(id), ct));

    /// <summary>Liste tous les dossiers de diffusion d'un message.</summary>
    [HttpGet("par-message/{messageId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ListerParMessage(Guid messageId, CancellationToken ct = default)
        => ToResult(await mediator.Send(new ListerDossiersDiffusionQuery(messageId), ct));
}
