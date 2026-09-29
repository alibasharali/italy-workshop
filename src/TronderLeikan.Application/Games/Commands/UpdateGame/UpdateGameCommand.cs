using TronderLeikan.Application.Common.Interfaces;

namespace TronderLeikan.Application.Games.Commands.UpdateGame;

// Full erstatning av navn, beskrivelse, dato og anledning
public record UpdateGameCommand(Guid GameId, string Name, string? Description, DateOnly? PlayedOn, string? Occasion) : ICommand;
