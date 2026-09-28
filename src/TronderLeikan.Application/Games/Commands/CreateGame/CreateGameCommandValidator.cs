using FluentValidation;
namespace TronderLeikan.Application.Games.Commands.CreateGame;

public sealed class CreateGameCommandValidator : AbstractValidator<CreateGameCommand>
{
    public CreateGameCommandValidator()
    {
        RuleFor(c => c.Name).NotEmpty().MaximumLength(500);
        RuleFor(c => c.TournamentId).NotEmpty();
        RuleFor(c => c.PlayedOn).NotNull()
            .WithErrorCode("Game.PlayedOnMissing")
            .WithMessage("Spillet må ha en dato.");
    }
}
