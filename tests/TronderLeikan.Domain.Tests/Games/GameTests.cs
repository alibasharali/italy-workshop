using AwesomeAssertions;
using TronderLeikan.Domain.Games;
using TronderLeikan.Domain.Games.Events;

namespace TronderLeikan.Domain.Tests.Games;

public class GameTests
{
    [Fact]
    public void Create_SetsNavnTournamentIdOgStandardverdier()
    {
        var tournamentId = Guid.NewGuid();

        var game = Game.Create("Kubb", tournamentId, new DateOnly(2026, 3, 13));

        game.Id.Should().NotBeEmpty();
        game.Name.Should().Be("Kubb");
        game.TournamentId.Should().Be(tournamentId);
        game.GameType.Should().Be(GameType.Standard);
        game.IsDone.Should().BeFalse();
        game.IsOrganizersParticipating.Should().BeFalse();
        game.HasBanner.Should().BeFalse();
    }

    [Fact]
    public void Create_SetterSpiltDato()
    {
        var game = Game.Create("Dart", Guid.NewGuid(), new DateOnly(2026, 3, 13));

        game.PlayedOn.Should().Be(new DateOnly(2026, 3, 13));
    }

    [Fact]
    public void Complete_MedDatoIFremtiden_FullføresSomVanlig()
    {
        var game = Game.Create("Dart", Guid.NewGuid(), new DateOnly(2027, 1, 1));
        var vinner = Guid.NewGuid();

        game.Complete([vinner], [], []);

        game.IsDone.Should().BeTrue();
        game.PlayedOn.Should().Be(new DateOnly(2027, 1, 1));
    }

    [Fact]
    public void UpdateOccasion_FjernerMellomromRundt()
    {
        var game = Game.Create("Dart", Guid.NewGuid(), new DateOnly(2026, 3, 13));

        game.UpdateOccasion("  Fredagspils uke 11 ");

        game.Occasion.Should().Be("Fredagspils uke 11");
    }

    [Theory]
    [InlineData("   ")]
    [InlineData("")]
    [InlineData(null)]
    public void UpdateOccasion_TomTekst_GirIngenAnledning(string? occasion)
    {
        var game = Game.Create("Dart", Guid.NewGuid(), new DateOnly(2026, 3, 13));
        game.UpdateOccasion("Fredagspils uke 11");

        game.UpdateOccasion(occasion);

        game.Occasion.Should().BeNull();
    }

    [Fact]
    public void Create_MedGameType_SetsGameType()
    {
        var game = Game.Create("Simracing", Guid.NewGuid(), new DateOnly(2026, 3, 13), GameType.Simracing);

        game.GameType.Should().Be(GameType.Simracing);
    }

    [Fact]
    public void AddParticipant_LeggTilPerson()
    {
        var game = Game.Create("Kubb", Guid.NewGuid(), new DateOnly(2026, 3, 13));
        var personId = Guid.NewGuid();

        game.AddParticipant(personId);

        game.Participants.Should().ContainSingle().Which.Should().Be(personId);
    }

    [Fact]
    public void AddParticipant_DuplikatIgnoreres()
    {
        var game = Game.Create("Kubb", Guid.NewGuid(), new DateOnly(2026, 3, 13));
        var personId = Guid.NewGuid();

        game.AddParticipant(personId);
        game.AddParticipant(personId);

        game.Participants.Should().ContainSingle();
    }

    [Fact]
    public void AddOrganizer_LeggTilArrangør()
    {
        var game = Game.Create("Kubb", Guid.NewGuid(), new DateOnly(2026, 3, 13));
        var personId = Guid.NewGuid();

        game.AddOrganizer(personId, withParticipation: false);

        game.Organizers.Should().ContainSingle().Which.Should().Be(personId);
        game.IsOrganizersParticipating.Should().BeFalse();
    }

    [Fact]
    public void AddOrganizer_MedDeltakelse_SetsIsOrganizersParticipating()
    {
        var game = Game.Create("Kubb", Guid.NewGuid(), new DateOnly(2026, 3, 13));

        game.AddOrganizer(Guid.NewGuid(), withParticipation: true);

        game.IsOrganizersParticipating.Should().BeTrue();
    }

    [Fact]
    public void AddSpectator_LeggTilTilskuer()
    {
        var game = Game.Create("Kubb", Guid.NewGuid(), new DateOnly(2026, 3, 13));
        var personId = Guid.NewGuid();

        game.AddSpectator(personId);

        game.Spectators.Should().ContainSingle().Which.Should().Be(personId);
    }

    [Fact]
    public void Complete_SetsIsDoneOgPlasseringer()
    {
        var game = Game.Create("Kubb", Guid.NewGuid(), new DateOnly(2026, 3, 13));
        var alice = Guid.NewGuid();
        var bob = Guid.NewGuid();
        var charlie = Guid.NewGuid();

        game.Complete(
            firstPlace: [alice],
            secondPlace: [bob],
            thirdPlace: [charlie]
        );

        game.IsDone.Should().BeTrue();
        game.FirstPlace.Should().ContainSingle().Which.Should().Be(alice);
        game.SecondPlace.Should().ContainSingle().Which.Should().Be(bob);
        game.ThirdPlace.Should().ContainSingle().Which.Should().Be(charlie);
    }

    [Fact]
    public void Complete_RaiserGameCompletedEvent()
    {
        var game = Game.Create("Kubb", Guid.NewGuid(), new DateOnly(2026, 3, 13));

        game.Complete(firstPlace: [], secondPlace: [], thirdPlace: []);

        game.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<GameCompletedEvent>()
            .Which.GameId.Should().Be(game.Id);
    }

    [Fact]
    public void Complete_SupportsTies()
    {
        var game = Game.Create("Kubb", Guid.NewGuid(), new DateOnly(2026, 3, 13));
        var alice = Guid.NewGuid();
        var bob = Guid.NewGuid();

        game.Complete(firstPlace: [alice, bob], secondPlace: [], thirdPlace: []);

        game.FirstPlace.Should().HaveCount(2).And.Contain(alice).And.Contain(bob);
    }

    [Fact]
    public void SetBanner_SetsHasBannerTilTrue()
    {
        var game = Game.Create("Kubb", Guid.NewGuid(), new DateOnly(2026, 3, 13));

        game.SetBanner();

        game.HasBanner.Should().BeTrue();
    }
}
