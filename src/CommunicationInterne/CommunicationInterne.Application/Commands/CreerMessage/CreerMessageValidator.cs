using FluentValidation;
using Cnss.Metier.CommunicationInterne.Domain.Enums;

namespace Cnss.Metier.CommunicationInterne.Application.Commands.CreerMessage;

public class CreerMessageValidator : AbstractValidator<CreerMessageCommand>
{
    public CreerMessageValidator()
    {
        RuleFor(x => x.AuteurId).NotEmpty();
        RuleFor(x => x.Objet).NotEmpty().MaximumLength(500);
        RuleFor(x => x.Corps).NotEmpty();
        RuleFor(x => x.Canaux).NotEmpty().WithMessage("Au moins un canal de diffusion est requis.");
        RuleForEach(x => x.Canaux).IsInEnum();
        RuleFor(x => x.UserId).NotEmpty();
    }
}
