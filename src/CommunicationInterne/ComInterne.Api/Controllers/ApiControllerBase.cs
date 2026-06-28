using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using DomainCommonResult = Cnss.Metier.Shared.Domain.Common.Result;

namespace ComInterne.Api.Controllers;

/// <summary>
/// Classe de base commune à tous les contrôleurs :
/// versioning, extraction des claims JWT, conversion Result → IActionResult.
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/[controller]")]
[Produces("application/json")]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
public abstract class ApiControllerBase : ControllerBase
{
    /// <summary>Extrait (UserId, UserName, UserRole) depuis les claims JWT.</summary>
    protected (string UserId, string UserName, string? UserRole) GetUser()
    {
        var userId = User.FindFirst("sub")?.Value
                     ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                     ?? throw new UnauthorizedAccessException("Claim 'sub' manquant.");
        var userName = User.FindFirst("name")?.Value
                       ?? User.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value ?? "Inconnu";
        var userRole = User.FindFirst("role")?.Value
                       ?? User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
        return (userId, userName, userRole);
    }

    // ─── Conversion Result → IActionResult ─────────────────────────

    protected IActionResult ToResult(DomainCommonResult result)
        => result.IsSuccess
            ? Ok(new { Success = true })
            : StatusCode(result.StatusCode ?? 400, new { error = result.Error });

    protected IActionResult ToResult<T>(Cnss.Metier.Shared.Domain.Common.Result<T> result)
        => result.IsSuccess
            ? Ok(result.Value)
            : StatusCode(result.StatusCode ?? 400, new { error = result.Error });
}
