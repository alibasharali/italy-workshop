using AwesomeAssertions;
using TronderLeikan.Application.Games.Commands.UpdateGame;
using TronderLeikan.Application.Tournaments.Queries.GetScoreboard;
using TronderLeikan.Application.Tournaments.Queries.GetTournamentBySlug;
using TronderLeikan.Application.Tournaments.Queries.GetTournaments;
using TronderLeikan.Domain.Games;
using TronderLeikan.Domain.Persons;
using TronderLeikan.Domain.Tournaments;

namespace TronderLeikan.Application.Tests.Tournaments;

public sealed class TournamentQueryHandlerTests
{
    [Fact]
    public async Task GetTournaments_ReturnererAlleTurneringer()
    {
        await using var db = TestAppDbContext.Create();
        db.Tournaments.Add(Tournament.Create("NM 2026", "nm-2026"));
        await db.SaveChangesAsync();
        var result = await new GetTournamentsQueryHandler(db).Handle(new GetTournamentsQuery());
        Assert.True(result.IsSuccess);
        Assert.Single(result.Value!);
    }

    [Fact]
    public async Task GetTournamentBySlug_FinnerTurnering()
    {
        await using var db = TestAppDbContext.Create();
        db.Tournaments.Add(Tournament.Create("NM 2026", "nm-2026"));
        await db.SaveChangesAsync();
        var result = await new GetTournamentBySlugQueryHandler(db).Handle(new GetTournamentBySlugQuery("nm-2026"));
        Assert.True(result.IsSuccess);
        Assert.Equal("NM 2026", result.Value!.Name);
    }

    [Fact]
    public async Task GetScoreboard_BeregnerPoengRiktig()
    {
        await using var db = TestAppDbContext.Create();
        var tournament = Tournament.Create("NM", "nm");
        db.Tournaments.Add(tournament);
        var personOla = Person.Create("Ola", "Nordmann");
        var personKari = Person.Create("Kari", "Traa");
        db.Persons.AddRange(personOla, personKari);
        // Ola 1. plass, Kari 2. plass — begge deltakere
        var game = Game.Create("Spill 1", tournament.Id, new DateOnly(2026, 3, 13));
        game.AddParticipant(personOla.Id);
        game.AddParticipant(personKari.Id);
        game.Complete([personOla.Id], [personKari.Id], []);
        db.Games.Add(game);
        await db.SaveChangesAsync();

        var result = await new GetScoreboardQueryHandler(db).Handle(new GetScoreboardQuery(tournament.Id));

        // Ola: participation(3) + firstPlace(3) = 6, Kari: participation(3) + secondPlace(2) = 5
        Assert.True(result.IsSuccess);
        var ola = result.Value!.Single(e => e.PersonId == personOla.Id);
        var kari = result.Value!.Single(e => e.PersonId == personKari.Id);
        Assert.Equal(6, ola.TotalPoints);
        Assert.Equal(5, kari.TotalPoints);
        Assert.Equal(1, ola.Rank);
        Assert.Equal(2, kari.Rank);
    }

    [Fact]
    public async Task GetScoreboard_UendretNårDatoOgAnledningEndres()
    {
        await using var db = TestAppDbContext.Create();
        var tournament = Tournament.Create("NM", "nm");
        db.Tournaments.Add(tournament);
        var ola = Person.Create("Ola", "Nordmann");
        var kari = Person.Create("Kari", "Traa");
        db.Persons.AddRange(ola, kari);
        var game = Game.Create("Dart", tournament.Id, new DateOnly(2026, 3, 13));
        game.AddParticipant(ola.Id);
        game.AddParticipant(kari.Id);
        game.Complete([ola.Id], [kari.Id], []);
        db.Games.Add(game);
        await db.SaveChangesAsync();
        var før = (await new GetScoreboardQueryHandler(db).Handle(new GetScoreboardQuery(tournament.Id))).Value!;

        await new UpdateGameCommandHandler(db).Handle(
            new UpdateGameCommand(game.Id, "Dart", null, new DateOnly(2025, 12, 31), "Julebord"));
        var etter = (await new GetScoreboardQueryHandler(db).Handle(new GetScoreboardQuery(tournament.Id))).Value!;

        etter.Should().BeEquivalentTo(før);
    }
}
