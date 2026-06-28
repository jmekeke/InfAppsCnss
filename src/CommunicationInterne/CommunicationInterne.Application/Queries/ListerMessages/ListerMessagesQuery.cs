using MDiator;
using Cnss.Metier.CommunicationInterne.Domain.Repositories;
using Cnss.Metier.Shared.Domain.Common;

namespace Cnss.Metier.CommunicationInterne.Application.Queries.ListerMessages;

public record ListerMessagesQuery(
    int Page = 1,
    int PageSize = 20,
    string? Search = null) : IMDiatorRequest<Result<PagedResult<MessageResumDto>>>;

public record MessageResumDto(
    Guid Id,
    string Objet,
    bool EstInstitutionnel,
    string Statut,
    string AuteurNom,
    IReadOnlyList<string> Canaux,
    DateTime DateCreation,
    DateTime? DateDiffusion);

public class ListerMessagesHandler(
    IMessageInterneRepository messageRepo) : IMDiatorHandler<ListerMessagesQuery, Result<PagedResult<MessageResumDto>>>
{
    public async Task<Result<PagedResult<MessageResumDto>>> Handle(ListerMessagesQuery query, CancellationToken ct)
    {
        var paged = await messageRepo.GetAllAsync(query.Page, query.PageSize, query.Search, ct);

        // Chargement des canaux en une seule requête pour tous les messages de la page
        var canauxParMessage = await messageRepo.GetCanauxForMessagesAsync(
            paged.Items.Select(m => m.Id), ct);

        var dtos = paged.Items.Select(m => new MessageResumDto(
            m.Id,
            m.Objet,
            m.EstInstitutionnel,
            m.Statut.ToString(),
            m.AuteurNom,
            canauxParMessage.TryGetValue(m.Id, out var canaux) ? canaux : [],
            m.DateCreation,
            m.DateDiffusion)).ToList();

        return Result.Success(new PagedResult<MessageResumDto>(dtos, paged.TotalCount, paged.Page, paged.PageSize));
    }
}
