using MDiator;
using Cnss.Metier.CommunicationInterne.Domain.Enums;
using Cnss.Metier.CommunicationInterne.Domain.Repositories;
using Cnss.Metier.Shared.Domain.Common;

namespace Cnss.Metier.CommunicationInterne.Application.Queries.GetMessage;

public record GetMessageQuery(Guid MessageId) : IMDiatorRequest<Result<MessageInterneDto>>;

public record MessageInterneDto(
    Guid Id,
    string Objet,
    string Corps,
    bool EstInstitutionnel,
    Guid AuteurId,
    string Statut,
    DateTime DateCreation,
    DateTime? DateValidation,
    Guid? ValidateurId,
    string? MotifRejet,
    DateTime? DateProgrammee,
    DateTime? DateDiffusion,
    bool EstArchive,
    IReadOnlyList<string> Canaux);

public class GetMessageHandler(
    IMessageInterneRepository messageRepo) : IMDiatorHandler<GetMessageQuery, Result<MessageInterneDto>>
{
    public async Task<Result<MessageInterneDto>> Handle(GetMessageQuery query, CancellationToken ct)
    {
        var m = await messageRepo.GetWithCanauxAsync(query.MessageId, ct);
        if (m is null)
            return Result.NotFound<MessageInterneDto>();

        return Result.Success(new MessageInterneDto(
            m.Id,
            m.Objet,
            m.Corps,
            m.EstInstitutionnel,
            m.AuteurId,
            m.Statut.ToString(),
            m.DateCreation,
            m.DateValidation,
            m.ValidateurId,
            m.MotiDeRejet,
            m.DateProgrammee,
            m.DateDiffusion,
            m.EstArchive,
            m.Canaux.Select(c => c.ToString()).ToList()));
    }
}
