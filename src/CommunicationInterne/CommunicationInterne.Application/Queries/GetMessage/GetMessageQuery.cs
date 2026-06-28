using MDiator;
using Cnss.Metier.CommunicationInterne.Domain.Enums;
using Cnss.Metier.CommunicationInterne.Domain.Repositories;
using Cnss.Metier.Shared.Domain.Common;

namespace Cnss.Metier.CommunicationInterne.Application.Queries.GetMessage;

public record GetMessageQuery(Guid MessageId) : IMDiatorRequest<Result<MessageInterneDto>>;

public record PieceJointeDto(Guid Id, string NomFichier, string TypeMime, long TailleOctets, DateTime DateAjout);

public record DestinataireCibleDto(
    Guid Id,
    TypeDestinataire Type,
    string? ReferenceId,
    string Libelle);

public record MessageInterneDto(
    Guid Id,
    string Objet,
    string Corps,
    bool EstInstitutionnel,
    Guid AuteurId,
    string AuteurNom,
    string Statut,
    IReadOnlyList<string> Canaux,
    IReadOnlyList<PieceJointeDto> PiecesJointes,
    IReadOnlyList<Guid> GroupeIds,
    IReadOnlyList<DestinataireCibleDto> Destinataires,
    DateTime DateCreation,
    DateTime? DateValidation,
    Guid? ValidateurId,
    string? MotifRejet,
    string? CommentaireRetour,
    DateTime? DateProgrammee,
    DateTime? DateDiffusion,
    bool EstArchive);

public class GetMessageHandler(
    IMessageInterneRepository messageRepo) : IMDiatorHandler<GetMessageQuery, Result<MessageInterneDto>>
{
    public async Task<Result<MessageInterneDto>> Handle(GetMessageQuery query, CancellationToken ct)
    {
        var m = await messageRepo.GetWithCanauxAsync(query.MessageId, ct);
        if (m is null)
            return Result.NotFound<MessageInterneDto>();

        var canaux        = await messageRepo.GetCanauxAsync(m.Id, ct);
        var piecesJointes = await messageRepo.GetPiecesJointesAsync(m.Id, ct);
        var groupeIds     = await messageRepo.GetGroupeIdsAsync(m.Id, ct);
        var destinataires = await messageRepo.GetDestinatairesAsync(m.Id, ct);

        var piecesJointesDto = piecesJointes
            .Select(p => new PieceJointeDto(p.Id, p.NomFichier, p.TypeMime, p.TailleOctets, p.DateAjout))
            .ToList();

        var destinatairesDto = destinataires
            .Select(d => new DestinataireCibleDto(d.Id, d.Type, d.ReferenceId, d.Libelle))
            .ToList();

        return Result.Success(new MessageInterneDto(
            m.Id, m.Objet, m.Corps, m.EstInstitutionnel, m.AuteurId, m.AuteurNom,
            m.Statut.ToString(), canaux, piecesJointesDto, groupeIds, destinatairesDto,
            m.DateCreation, m.DateValidation, m.ValidateurId, m.MotiDeRejet, m.CommentaireRetour,
            m.DateProgrammee, m.DateDiffusion, m.EstArchive));
    }
}
