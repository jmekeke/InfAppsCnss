using FluentValidation;

namespace Cnss.Metier.CommunicationInterne.Application.Commands.DefinirDestinataires;

public class DefinirDestinatairesValidator : AbstractValidator<DefinirDestinatairesCommand>
{
    public DefinirDestinatairesValidator()
    {
        RuleFor(x => x.MessageId).NotEmpty();
        RuleFor(x => x.GroupeIds).NotEmpty().WithMessage("Au moins un groupe destinataire est requis.");
        RuleForEach(x => x.GroupeIds).NotEmpty();
        RuleFor(x => x.UserId).NotEmpty();
    }
}
