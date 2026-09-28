namespace TronderLeikan.Api.Tests.Games;

[Collection(nameof(ApiTestCollection))]
public class GamesApiTests(TronderLeikanApiFactory factory)
{
    private readonly HttpClient _client = factory.CreateClient();

    // Hjelper — oppretter turnering og returnerer id
    private async Task<Guid> OpprettTurnering() =>
        await (await _client.PostAsJsonAsync("/api/v1/tournaments",
            new { name = "Test", slug = $"t-{Guid.NewGuid():N}" }))
            .Content.ReadFromJsonAsync<Guid>();

    // Hjelper — oppretter person og returnerer id
    private async Task<Guid> OpprettPerson(string fornavn, string etternavn) =>
        await (await _client.PostAsJsonAsync("/api/v1/persons",
            new { firstName = fornavn, lastName = etternavn }))
            .Content.ReadFromJsonAsync<Guid>();

    [Fact]
    public async Task POST_games_returnerer_201_med_guid()
    {
        var tournamentId = await OpprettTurnering();

        var response = await _client.PostAsJsonAsync("/api/v1/games", new
        {
            tournamentId,
            playedOn = "2026-03-13",
            name = "Testspill",
            gameType = 0 // Standard
        });

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var id = await response.Content.ReadFromJsonAsync<Guid>();
        id.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GET_game_som_ikke_finnes_returnerer_404()
    {
        var response = await _client.GetAsync($"/api/v1/games/{Guid.NewGuid()}");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);

        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("title").GetString().Should().Be("Game.NotFound");
    }

    [Fact]
    public async Task POST_participants_og_complete_game_happy_path()
    {
        var tournamentId = await OpprettTurnering();
        var personId1 = await OpprettPerson("Per", "Testesen");
        var personId2 = await OpprettPerson("Pål", "Testesen");
        var personId3 = await OpprettPerson("Espen", "Testesen");

        var gameId = await (await _client.PostAsJsonAsync("/api/v1/games", new
        {
            tournamentId,
            playedOn = "2026-03-13",
            name = "Finalespill",
            gameType = 0
        })).Content.ReadFromJsonAsync<Guid>();

        // Legg til deltakere
        (await _client.PostAsJsonAsync($"/api/v1/games/{gameId}/participants",
            new { gameId, personId = personId1 }))
            .StatusCode.Should().Be(HttpStatusCode.NoContent);
        (await _client.PostAsJsonAsync($"/api/v1/games/{gameId}/participants",
            new { gameId, personId = personId2 }))
            .StatusCode.Should().Be(HttpStatusCode.NoContent);
        (await _client.PostAsJsonAsync($"/api/v1/games/{gameId}/participants",
            new { gameId, personId = personId3 }))
            .StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Fullfør spill
        var completeResponse = await _client.PostAsJsonAsync($"/api/v1/games/{gameId}/complete", new
        {
            gameId,
            firstPlace = new[] { personId1 },
            secondPlace = new[] { personId2 },
            thirdPlace = new[] { personId3 }
        });
        completeResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verifiser at spillet er done
        var body = await (await _client.GetAsync($"/api/v1/games/{gameId}"))
            .Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("isDone").GetBoolean().Should().BeTrue();
    }

    [Fact]
    public async Task POST_simracing_results_og_complete_beregner_plasseringer()
    {
        var tournamentId = await OpprettTurnering();
        var p1 = await OpprettPerson("Rask", "Raser");
        var p2 = await OpprettPerson("Midt", "Raser");
        var p3 = await OpprettPerson("Treg", "Raser");

        var gameId = await (await _client.PostAsJsonAsync("/api/v1/games", new
        {
            tournamentId,
            playedOn = "2026-03-13",
            name = "Simracing 1",
            gameType = 1 // Simracing
        })).Content.ReadFromJsonAsync<Guid>();

        // Registrer racetider (lavest er best)
        await _client.PostAsJsonAsync($"/api/v1/games/{gameId}/simracing-results",
            new { gameId, personId = p1, raceTimeMs = 90000L });
        await _client.PostAsJsonAsync($"/api/v1/games/{gameId}/simracing-results",
            new { gameId, personId = p2, raceTimeMs = 95000L });
        await _client.PostAsJsonAsync($"/api/v1/games/{gameId}/simracing-results",
            new { gameId, personId = p3, raceTimeMs = 100000L });

        // Fullfør automatisk
        var completeResponse = await _client.PostAsync(
            $"/api/v1/games/{gameId}/simracing-results/complete", null);
        completeResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verifiser at spillet er done
        var body = await (await _client.GetAsync($"/api/v1/games/{gameId}"))
            .Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("isDone").GetBoolean().Should().BeTrue();

        // Verifiser plasseringer
        var firstPlace = body.GetProperty("firstPlace").EnumerateArray()
            .Select(e => e.GetGuid()).ToList();
        firstPlace.Should().Contain(p1);
    }

    [Fact]
    public async Task GET_simracing_results_returnerer_sortert_liste()
    {
        var tournamentId = await OpprettTurnering();
        var p1 = await OpprettPerson("Rask2", "Raser");
        var p2 = await OpprettPerson("Treg2", "Raser");

        var gameId = await (await _client.PostAsJsonAsync("/api/v1/games", new
        {
            tournamentId,
            playedOn = "2026-03-13",
            name = "Simracing 2",
            gameType = 1
        })).Content.ReadFromJsonAsync<Guid>();

        await _client.PostAsJsonAsync($"/api/v1/games/{gameId}/simracing-results",
            new { gameId, personId = p2, raceTimeMs = 100000L });
        await _client.PostAsJsonAsync($"/api/v1/games/{gameId}/simracing-results",
            new { gameId, personId = p1, raceTimeMs = 90000L });

        var response = await _client.GetAsync($"/api/v1/games/{gameId}/simracing-results");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        var times = body.EnumerateArray()
            .Select(r => r.GetProperty("raceTimeMs").GetInt64()).ToList();
        times.Should().BeInAscendingOrder();
    }

    [Fact]
    public async Task POST_games_med_dato_lagrer_og_returnerer_playedOn()
    {
        var tournamentId = await OpprettTurnering();

        var response = await _client.PostAsJsonAsync("/api/v1/games",
            new { tournamentId, name = "Dart", gameType = 0, playedOn = "2026-03-13" });

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var id = await response.Content.ReadFromJsonAsync<Guid>();
        var body = await (await _client.GetAsync($"/api/v1/games/{id}")).Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("playedOn").GetString().Should().Be("2026-03-13");
    }

    [Fact]
    public async Task POST_games_uten_dato_returnerer_400_og_lagrer_ingenting()
    {
        var tournamentId = await OpprettTurnering();

        var response = await _client.PostAsJsonAsync("/api/v1/games",
            new { tournamentId, name = "Dart", gameType = 0 });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("title").GetString().Should().Be("Game.PlayedOnMissing");
        var games = await (await _client.GetAsync($"/api/v1/tournaments/{tournamentId}/games"))
            .Content.ReadFromJsonAsync<JsonElement>();
        games.GetArrayLength().Should().Be(0);
    }

    [Fact]
    public async Task POST_games_med_dato_i_fremtiden_kan_fullføres()
    {
        var tournamentId = await OpprettTurnering();
        var vinner = await OpprettPerson("Fremtidig", "Vinner");

        var response = await _client.PostAsJsonAsync("/api/v1/games",
            new { tournamentId, name = "Nyttårsdart", gameType = 0, playedOn = "2027-01-01" });
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var gameId = await response.Content.ReadFromJsonAsync<Guid>();
        var complete = await _client.PostAsJsonAsync($"/api/v1/games/{gameId}/complete",
            new { gameId, firstPlace = new[] { vinner }, secondPlace = Array.Empty<Guid>(), thirdPlace = Array.Empty<Guid>() });

        complete.StatusCode.Should().Be(HttpStatusCode.NoContent);
        var body = await (await _client.GetAsync($"/api/v1/games/{gameId}")).Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("isDone").GetBoolean().Should().BeTrue();
        body.GetProperty("playedOn").GetString().Should().Be("2027-01-01");
    }

    [Fact]
    public async Task POST_games_med_anledning_lagrer_den_trimmet()
    {
        var tournamentId = await OpprettTurnering();

        var id = await (await _client.PostAsJsonAsync("/api/v1/games",
            new { tournamentId, name = "Dart", playedOn = "2026-03-13", occasion = "  Fredagspils uke 11 " }))
            .Content.ReadFromJsonAsync<Guid>();

        var body = await (await _client.GetAsync($"/api/v1/games/{id}")).Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("occasion").GetString().Should().Be("Fredagspils uke 11");
    }

    [Fact]
    public async Task POST_games_med_blank_eller_uten_anledning_gir_null()
    {
        var tournamentId = await OpprettTurnering();

        var blank = await (await _client.PostAsJsonAsync("/api/v1/games",
            new { tournamentId, name = "Dart", playedOn = "2026-03-13", occasion = "   " }))
            .Content.ReadFromJsonAsync<Guid>();
        var uten = await (await _client.PostAsJsonAsync("/api/v1/games",
            new { tournamentId, name = "Kubb", playedOn = "2026-03-13" }))
            .Content.ReadFromJsonAsync<Guid>();

        foreach (var id in new[] { blank, uten })
        {
            var body = await (await _client.GetAsync($"/api/v1/games/{id}")).Content.ReadFromJsonAsync<JsonElement>();
            body.GetProperty("occasion").ValueKind.Should().Be(JsonValueKind.Null);
        }
    }

    [Fact]
    public async Task POST_games_med_for_lang_anledning_returnerer_400()
    {
        var tournamentId = await OpprettTurnering();

        var response = await _client.PostAsJsonAsync("/api/v1/games",
            new { tournamentId, name = "Dart", playedOn = "2026-03-13", occasion = new string('a', 201) });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("title").GetString().Should().Be("Game.OccasionTooLong");
    }

    // Hjelper — oppretter et datert spill uten anledning og returnerer id
    private async Task<Guid> OpprettSpill(string navn, string playedOn) =>
        await (await _client.PostAsJsonAsync("/api/v1/games",
            new { tournamentId = await OpprettTurnering(), name = navn, playedOn }))
            .Content.ReadFromJsonAsync<Guid>();

    [Fact]
    public async Task PUT_game_erstatter_navn_beskrivelse_dato_og_anledning()
    {
        var id = await OpprettSpill("Dart", "2026-03-13");

        var response = await _client.PutAsJsonAsync($"/api/v1/games/{id}", new
        {
            name = "Dart 501",
            description = "Dobbel ut",
            playedOn = "2026-03-20",
            occasion = "Fredagspils uke 12"
        });

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        var body = await (await _client.GetAsync($"/api/v1/games/{id}")).Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("name").GetString().Should().Be("Dart 501");
        body.GetProperty("description").GetString().Should().Be("Dobbel ut");
        body.GetProperty("playedOn").GetString().Should().Be("2026-03-20");
        body.GetProperty("occasion").GetString().Should().Be("Fredagspils uke 12");
    }

    [Fact]
    public async Task PUT_game_uten_dato_returnerer_400_og_endrer_ingenting()
    {
        var id = await OpprettSpill("Dart", "2026-03-13");

        var response = await _client.PutAsJsonAsync($"/api/v1/games/{id}", new { name = "Endret" });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var problem = await response.Content.ReadFromJsonAsync<JsonElement>();
        problem.GetProperty("title").GetString().Should().Be("Game.PlayedOnMissing");
        var body = await (await _client.GetAsync($"/api/v1/games/{id}")).Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("name").GetString().Should().Be("Dart");
        body.GetProperty("playedOn").GetString().Should().Be("2026-03-13");
    }

    [Fact]
    public async Task PUT_game_med_for_lang_anledning_returnerer_400()
    {
        var id = await OpprettSpill("Dart", "2026-03-13");

        var response = await _client.PutAsJsonAsync($"/api/v1/games/{id}",
            new { name = "Dart", playedOn = "2026-03-13", occasion = new string('a', 201) });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var problem = await response.Content.ReadFromJsonAsync<JsonElement>();
        problem.GetProperty("title").GetString().Should().Be("Game.OccasionTooLong");
    }
}
