using FluentValidation;
namespace TronderLeikan.Application.Games.Commands.UpdateGame;

public sealed class UpdateGameCommandValidator : AbstractValidator<UpdateGameCommand>
{
    public UpdateGameCommandValidator()
    {
        RuleFor(c => c.Name).NotEmpty().MaximumLength(500);
        RuleFor(c => c.PlayedOn).NotNull()
            .WithErrorCode("Game.PlayedOnMissing")
            .WithMessage("Spillet må ha en dato.");
        RuleFor(c => c.Occasion)
            .Must(o => o is null || o.Trim().Length <= 200)
            .WithErrorCode("Game.OccasionTooLong")
            .WithMessage("Anledningen kan ha maks 200 tegn.");
    }
}
