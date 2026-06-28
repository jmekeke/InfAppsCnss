using Cnss.Metier.CommunicationInterne.Application.Queries.RecupAgent;
using Cnss.Metier.CommunicationInterne.Application.Queries.GetVoieCommunication;
using MDiator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ComInterne.Api.Controllers;

[Authorize]
[Route("api/agents")]
public class AgentsController(IMediator mediator) : ApiControllerBase
{
    /// <summary>Liste tous les agents de la base RH.</summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Lister(CancellationToken ct = default)
        => ToResult(await mediator.Send(new ListerAgentsQuery(), ct));

    /// <summary>Récupère un agent par son identifiant.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Get(int id, CancellationToken ct = default)
        => ToResult(await mediator.Send(new RecupAgentQuery(id), ct));

    /// <summary>
    /// Recherche des agents par nom (Nom / Postnom / Prenom) et/ou libellé d'entité.
    /// Les deux paramètres sont optionnels et combinés en AND.
    /// Exemples :
    ///   GET /api/agents/recherche?nom=dupont
    ///   GET /api/agents/recherche?entiteLibelle=direction
    ///   GET /api/agents/recherche?nom=dupont&amp;entiteLibelle=direction
    /// </summary>
    [HttpGet("recherche")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Rechercher(
        [FromQuery] string? nom,
        [FromQuery] string? entiteLibelle,
        CancellationToken ct = default)
        => ToResult(await mediator.Send(new RechercherAgentsQuery(nom, entiteLibelle), ct));

    /// <summary>
    /// Retourne le résumé de contact (téléphone actif + e-mail actif) pour tous les agents
    /// qui ont une voie de communication enregistrée, en une seule requête.
    /// Utilisé par les listes d'agents pour enrichir les colonnes Telephone / Email.
    /// </summary>
    [HttpGet("voies-resume")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetVoiesResume(CancellationToken ct = default)
        => ToResult(await mediator.Send(new GetResumesVoiesQuery(), ct));
}
