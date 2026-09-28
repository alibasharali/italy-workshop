using AwesomeAssertions;
using TronderLeikan.Application.Games.Commands.AddParticipant;
using TronderLeikan.Application.Games.Commands.CompleteGame;
using TronderLeikan.Application.Games.Commands.CreateGame;
using TronderLeikan.Application.Games.Commands.UpdateGame;
using TronderLeikan.Domain.Games;
using TronderLeikan.Domain.Persons;
using TronderLeikan.Domain.Tournaments;

namespace TronderLeikan.Application.Tests.Games;

public sealed class GameCommandHandlerTests
{
    [Fact]
    public async Task CreateGame_LagrerSpill()
    {
        await using var db = TestAppDbContext.Create();
        var tournament = Tournament.Create("NM", "nm");
        db.Tournaments.Add(tournament);
        await db.SaveChangesAsync();
        var result = await new CreateGameCommandHandler(db).Handle(new CreateGameCommand(tournament.Id, "Dartspill", GameType.Standard, new DateOnly(2026, 3, 13), null));
        Assert.True(result.IsSuccess);
        Assert.Single(db.Games.ToList());
    }

    [Fact]
    public async Task AddParticipant_LeggerTilDeltaker()
    {
        await using var db = TestAppDbContext.Create();
        var game = Game.Create("Spill", Guid.NewGuid(), new DateOnly(2026, 3, 13));
        db.Games.Add(game);
        var person = Person.Create("Ola", "Nordmann");
        db.Persons.Add(person);
        await db.SaveChangesAsync();
        var result = await new AddParticipantCommandHandler(db).Handle(new AddParticipantCommand(game.Id, person.Id));
        Assert.True(result.IsSuccess);
        var updated = await db.Games.FindAsync(game.Id);
        Assert.Contains(person.Id, updated!.Participants);
    }

    [Fact]
    public async Task CompleteGame_SetterIsDoneOgPlasseringer()
    {
        await using var db = TestAppDbContext.Create();
        var personA = Person.Create("A", "A");
        var personB = Person.Create("B", "B");
        db.Persons.AddRange(personA, personB);
        var game = Game.Create("Spill", Guid.NewGuid(), new DateOnly(2026, 3, 13));
        db.Games.Add(game);
        await db.SaveChangesAsync();
        var result = await new CompleteGameCommandHandler(db).Handle(new CompleteGameCommand(game.Id, [personA.Id], [personB.Id], []));
        Assert.True(result.IsSuccess);
        var updated = await db.Games.FindAsync(game.Id);
        Assert.True(updated!.IsDone);
        Assert.Contains(personA.Id, updated.FirstPlace);
    }

    [Fact]
    public async Task CreateGame_LagrerSpiltDato()
    {
        await using var db = TestAppDbContext.Create();
        var tournament = Tournament.Create("NM", "nm");
        db.Tournaments.Add(tournament);
        await db.SaveChangesAsync();

        var result = await new CreateGameCommandHandler(db).Handle(
            new CreateGameCommand(tournament.Id, "Dart", GameType.Standard, new DateOnly(2026, 3, 13), null));

        result.IsSuccess.Should().BeTrue();
        var game = await db.Games.FindAsync(result.Value);
        game!.PlayedOn.Should().Be(new DateOnly(2026, 3, 13));
    }

    [Fact]
    public void CreateGameValidator_UtenDato_GirValideringsfeilPåPlayedOn()
    {
        var command = new CreateGameCommand(Guid.NewGuid(), "Dart", GameType.Standard, null, null);

        var result = new CreateGameCommandValidator().Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.PropertyName == "PlayedOn")
            .Which.ErrorCode.Should().Be("Game.PlayedOnMissing");
    }

    [Fact]
    public void CreateGameValidator_MedDatoIFremtiden_ErGyldig()
    {
        var command = new CreateGameCommand(Guid.NewGuid(), "Dart", GameType.Standard, new DateOnly(2027, 1, 1), null);

        var result = new CreateGameCommandValidator().Validate(command);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task CreateGame_LagrerAnledningTrimmet()
    {
        await using var db = TestAppDbContext.Create();
        var tournament = Tournament.Create("NM", "nm");
        db.Tournaments.Add(tournament);
        await db.SaveChangesAsync();

        var result = await new CreateGameCommandHandler(db).Handle(
            new CreateGameCommand(tournament.Id, "Dart", GameType.Standard, new DateOnly(2026, 3, 13), "  Fredagspils uke 11 "));

        var game = await db.Games.FindAsync(result.Value);
        game!.Occasion.Should().Be("Fredagspils uke 11");
    }

    [Theory]
    [InlineData(200, true)]
    [InlineData(201, false)]
    public void CreateGameValidator_AnledningHarMaks200Tegn(int lengde, bool gyldig)
    {
        var command = new CreateGameCommand(Guid.NewGuid(), "Dart", GameType.Standard, new DateOnly(2026, 3, 13), new string('a', lengde));

        var result = new CreateGameCommandValidator().Validate(command);

        result.IsValid.Should().Be(gyldig);
        if (!gyldig)
            result.Errors.Should().ContainSingle(e => e.PropertyName == "Occasion")
                .Which.ErrorCode.Should().Be("Game.OccasionTooLong");
    }

    [Fact]
    public async Task UpdateGame_ErstatterNavnBeskrivelseDatoOgAnledning()
    {
        await using var db = TestAppDbContext.Create();
        var game = Game.Create("Dart", Guid.NewGuid(), new DateOnly(2026, 3, 13));
        db.Games.Add(game);
        await db.SaveChangesAsync();

        var result = await new UpdateGameCommandHandler(db).Handle(
            new UpdateGameCommand(game.Id, "Dart 501", "Dobbel ut", new DateOnly(2026, 3, 20), " Fredagspils uke 12 "));

        result.IsSuccess.Should().BeTrue();
        var updated = await db.Games.FindAsync(game.Id);
        updated!.Name.Should().Be("Dart 501");
        updated.Description.Should().Be("Dobbel ut");
        updated.PlayedOn.Should().Be(new DateOnly(2026, 3, 20));
        updated.Occasion.Should().Be("Fredagspils uke 12");
    }

    [Fact]
    public async Task UpdateGame_SpillUtenDato_FårDato()
    {
        await using var db = TestAppDbContext.Create();
        var game = Game.Create("Kubb", Guid.NewGuid(), new DateOnly(2026, 1, 1));
        db.Games.Add(game);
        // Kubb er registrert før datoen ble påkrevd
        db.Entry(game).Property(g => g.PlayedOn).CurrentValue = null;
        await db.SaveChangesAsync();

        var result = await new UpdateGameCommandHandler(db).Handle(
            new UpdateGameCommand(game.Id, "Kubb", null, new DateOnly(2026, 2, 27), null));

        result.IsSuccess.Should().BeTrue();
        (await db.Games.FindAsync(game.Id))!.PlayedOn.Should().Be(new DateOnly(2026, 2, 27));
    }

    [Fact]
    public async Task UpdateGame_SpillSomIkkeFinnes_GirNotFound()
    {
        await using var db = TestAppDbContext.Create();

        var result = await new UpdateGameCommandHandler(db).Handle(
            new UpdateGameCommand(Guid.NewGuid(), "Kubb", null, new DateOnly(2026, 2, 27), null));

        result.Error!.Code.Should().Be("Game.NotFound");
    }

    [Fact]
    public void UpdateGameValidator_UtenDato_GirValideringsfeilPåPlayedOn()
    {
        var command = new UpdateGameCommand(Guid.NewGuid(), "Kubb", null, null, null);

        var result = new UpdateGameCommandValidator().Validate(command);

        result.Errors.Should().ContainSingle(e => e.PropertyName == "PlayedOn")
            .Which.ErrorCode.Should().Be("Game.PlayedOnMissing");
    }

    [Fact]
    public void UpdateGameValidator_ForLangAnledning_GirValideringsfeilPåOccasion()
    {
        var command = new UpdateGameCommand(Guid.NewGuid(), "Kubb", null, new DateOnly(2026, 2, 27), new string('a', 201));

        var result = new UpdateGameCommandValidator().Validate(command);

        result.Errors.Should().ContainSingle(e => e.PropertyName == "Occasion")
            .Which.ErrorCode.Should().Be("Game.OccasionTooLong");
    }
}
