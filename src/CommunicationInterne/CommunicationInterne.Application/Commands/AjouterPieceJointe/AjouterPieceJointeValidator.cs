using FluentValidation;

namespace Cnss.Metier.CommunicationInterne.Application.Commands.AjouterPieceJointe;

public class AjouterPieceJointeValidator : AbstractValidator<AjouterPieceJointeCommand>
{
    private static readonly string[] TypesMimeAutorises =
    [
        "application/pdf", "image/jpeg", "image/png", "image/gif",
        "application/msword",
        "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
        "application/vnd.ms-excel",
        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"
    ];

    public AjouterPieceJointeValidator()
    {
        RuleFor(x => x.MessageId).NotEmpty();
        RuleFor(x => x.NomFichier).NotEmpty().MaximumLength(255);
        RuleFor(x => x.TypeMime).NotEmpty().Must(t => TypesMimeAutorises.Contains(t))
            .WithMessage("Type de fichier non autorisé.");
        RuleFor(x => x.TailleOctets).GreaterThan(0).LessThanOrEqualTo(10 * 1024 * 1024)
            .WithMessage("La pièce jointe ne doit pas dépasser 10 Mo.");
        RuleFor(x => x.UserId).NotEmpty();
    }
}
