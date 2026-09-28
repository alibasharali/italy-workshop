using AwesomeAssertions;
using TronderLeikan.Application.Common.Interfaces;
using TronderLeikan.Application.Games.Commands.UploadGameBanner;
using TronderLeikan.Application.Games.Queries.GetGameById;
using TronderLeikan.Application.Games.Queries.GetGamesByTournament;
using TronderLeikan.Domain.Games;
using TronderLeikan.Domain.Tournaments;

namespace TronderLeikan.Application.Tests.Games;

public sealed class GameBannerAndQueryTests
{
    private sealed class FakeImageProcessor : IImageProcessor
    {
        public Task<byte[]> ProcessPersonImageAsync(Stream input, CancellationToken ct) => Task.FromResult(Array.Empty<byte>());
        public Task<byte[]> ProcessGameBannerAsync(Stream input, CancellationToken ct) => Task.FromResult(new byte[] { 1, 2 });
    }

    [Fact]
    public async Task UploadGameBanner_LagrerBannerOgSetterFlag()
    {
        await using var db = TestAppDbContext.Create();
        var game = Game.Create("Spill", Guid.NewGuid(), new DateOnly(2026, 3, 13));
        db.Games.Add(game);
        await db.SaveChangesAsync();
        using var stream = new MemoryStream([9]);
        var result = await new UploadGameBannerCommandHandler(db, new FakeImageProcessor()).Handle(new UploadGameBannerCommand(game.Id, stream));
        Assert.True(result.IsSuccess);
        var updated = await db.Games.FindAsync(game.Id);
        Assert.True(updated!.HasBanner);
    }

    [Fact]
    public async Task GetGameById_ReturnererSpillDetaljer()
    {
        await using var db = TestAppDbContext.Create();
        var game = Game.Create("Spill", Guid.NewGuid(), new DateOnly(2026, 3, 13));
        db.Games.Add(game);
        await db.SaveChangesAsync();
        var result = await new GetGameByIdQueryHandler(db).Handle(new GetGameByIdQuery(game.Id));
        Assert.True(result.IsSuccess);
        Assert.Equal("Spill", result.Value!.Name);
    }

    [Fact]
    public async Task GetGamesByTournament_SortererKronologiskMedUdaterteSist()
    {
        await using var db = TestAppDbContext.Create();
        var tournament = Tournament.Create("Fredagspils", "fredagspils");
        db.Tournaments.Add(tournament);
        var kubb = Game.Create("Kubb", tournament.Id, new DateOnly(2026, 1, 1));
        db.Games.AddRange(
            Game.Create("Dart", tournament.Id, new DateOnly(2026, 3, 13)),
            Game.Create("Boccia", tournament.Id, new DateOnly(2026, 3, 13)),
            Game.Create("Mario Kart", tournament.Id, new DateOnly(2026, 3, 6)),
            kubb);
        // Kubb er registrert før datoen ble påkrevd
        db.Entry(kubb).Property(g => g.PlayedOn).CurrentValue = null;
        await db.SaveChangesAsync();

        var result = await new GetGamesByTournamentQueryHandler(db).Handle(new GetGamesByTournamentQuery(tournament.Id));

        result.Value!.Select(g => g.Name).Should().Equal("Mario Kart", "Boccia", "Dart", "Kubb");
        result.Value![0].PlayedOn.Should().Be(new DateOnly(2026, 3, 6));
        result.Value![3].PlayedOn.Should().BeNull();
    }

    [Fact]
    public async Task GetGameById_ReturnererSpiltDato()
    {
        await using var db = TestAppDbContext.Create();
        var game = Game.Create("Dart", Guid.NewGuid(), new DateOnly(2026, 3, 13));
        db.Games.Add(game);
        await db.SaveChangesAsync();

        var result = await new GetGameByIdQueryHandler(db).Handle(new GetGameByIdQuery(game.Id));

        result.Value!.PlayedOn.Should().Be(new DateOnly(2026, 3, 13));
    }
}
