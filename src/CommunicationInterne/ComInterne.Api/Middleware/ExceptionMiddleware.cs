using System.Net;
using System.Text.Json;
using Cnss.Metier.Shared.Domain.Common;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace ComInterne.Api.Middleware;

public class ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger, IHostEnvironment env)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (DomainException ex)
        {
            logger.LogWarning(ex, "Domain rule violation on {Path}", context.Request.Path);
            await WriteProblem(context, HttpStatusCode.BadRequest, "Regle metier violee", ex.Message);
        }
        catch (ValidationException ex)
        {
            logger.LogWarning(ex, "Validation failed on {Path}", context.Request.Path);
            var errors = ex.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());
            await WriteValidationProblem(context, errors);
        }
        catch (UnauthorizedAccessException ex)
        {
            logger.LogWarning("Unauthorized on {Path}: {Reason}", context.Request.Path, ex.Message);
            await WriteProblem(context, HttpStatusCode.Unauthorized, "Authentification requise", ex.Message);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled exception on {Path}", context.Request.Path);
            var detail = env.IsDevelopment()
                ? $"{ex.GetType().Name}: {ex.Message}"
                : "Une erreur interne est survenue. Veuillez reessayer plus tard.";
            await WriteProblem(context, HttpStatusCode.InternalServerError, "Erreur serveur", detail);
        }
    }

    private static Task WriteProblem(HttpContext ctx, HttpStatusCode status, string title, string detail)
    {
        var problem = new ProblemDetails
        {
            Status   = (int)status,
            Title    = title,
            Detail   = detail,
            Instance = ctx.Request.Path,
            Type     = $"https://httpstatuses.io/{(int)status}"
        };
        ctx.Response.StatusCode  = (int)status;
        ctx.Response.ContentType = "application/problem+json";
        return ctx.Response.WriteAsync(JsonSerializer.Serialize(problem));
    }

    private static Task WriteValidationProblem(HttpContext ctx, IDictionary<string, string[]> errors)
    {
        var problem = new ValidationProblemDetails(errors)
        {
            Status   = (int)HttpStatusCode.UnprocessableEntity,
            Title    = "Validation echouee",
            Instance = ctx.Request.Path,
            Type     = "https://httpstatuses.io/422"
        };
        ctx.Response.StatusCode  = (int)HttpStatusCode.UnprocessableEntity;
        ctx.Response.ContentType = "application/problem+json";
        return ctx.Response.WriteAsync(JsonSerializer.Serialize(problem));
    }
}
