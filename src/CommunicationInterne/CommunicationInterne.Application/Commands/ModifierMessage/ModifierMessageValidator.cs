using FluentValidation;

namespace Cnss.Metier.CommunicationInterne.Application.Commands.ModifierMessage;

public class ModifierMessageValidator : AbstractValidator<ModifierMessageCommand>
{
    public ModifierMessageValidator()
    {
        RuleFor(x => x.MessageId).NotEmpty();
        RuleFor(x => x.UserId).NotEmpty();

        When(x => x.Objet is not null, () =>
            RuleFor(x => x.Objet).NotEmpty().MaximumLength(500));

        When(x => x.Corps is not null, () =>
            RuleFor(x => x.Corps).NotEmpty());

        When(x => x.Canaux is not null, () =>
        {
            RuleFor(x => x.Canaux).NotEmpty().WithMessage("La liste des canaux ne peut pas être vide.");
            RuleForEach(x => x.Canaux).IsInEnum();
        });
    }
}
