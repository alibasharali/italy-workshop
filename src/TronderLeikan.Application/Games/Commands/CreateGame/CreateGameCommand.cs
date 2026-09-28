using TronderLeikan.Application.Common.Interfaces;
using TronderLeikan.Domain.Games;

namespace TronderLeikan.Application.Games.Commands.CreateGame;

// PlayedOn er nullable slik at validatoren, ikke modellbindingen, avviser et manglende felt
public record CreateGameCommand(Guid TournamentId, string Name, GameType GameType, DateOnly? PlayedOn, string? Occasion) : ICommand<Guid>;
