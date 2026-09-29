---
issue: 13
title: Min historikk
status: approved
branch: feat/13-min-historikk
---

# Min historikk

## Problem

A person's public player page today shows only a name and an avatar. Nobody can see which games they played, where they placed, how their points developed from tournament to tournament, or whether they mostly play, organize or watch. After this story, the player page tells each person's story: a short summary in Norwegian, one card per tournament with points and rank, a bar chart of points across tournaments, a role profile and a timeline of every finished game. It reads as well for someone with two games as for someone with two seasons.

## Domain decisions

- **D1 – Public, on the existing player page:** the history is added to the public page `/players/[id]`, visible to everyone without login. *Why:* `TRONDER_LEIKAN.md` says everyone can see each person's history. Rejected: a separate `/historikk` subpage; a "Meg" page for the logged-in user (needs story 20).
- **D2 – Scoreboard names link to the player page:** every name on the public tournament scoreboard links to `/players/{id}`. *Why:* otherwise the history is hard to find.
- **D3 – Only finished games:** the history counts only games with `IsDone`, as the scoreboard does. Unfinished games are not shown at all. *Why:* only finished games have points and placements. Rejected: listing unfinished games marked "Ikke ferdig".
- **D4 – A person "was in" a game** when their id is in any of the game's participant, organizer, spectator, first-, second- or third-place lists. Deleted persons are not a concern here (their page returns 404).
- **D5 – One shared scoring rule:** the points one person earns in one finished game under a tournament's rules are computed by a single pure domain calculator. Both the scoreboard and the history use it, so they can never disagree. The calculator reproduces today's scoreboard behaviour exactly, quirks included:
  - each participant listing gives `Participation`
  - each organizer gives `OrganizedWithParticipation + Participation` when the game's `IsOrganizersParticipating` is true, else `OrganizedWithoutParticipation`
  - each spectator gives `Spectator`
  - first/second/third place give `FirstPlace`/`SecondPlace`/`ThirdPlace` on top, whether or not the person is also a participant
  - the lists are additive, so a person in several lists gets the sum.

  *Why:* the history shows per-game and per-tournament points that must match the scoreboard. Rejected: a separate calculation in the history. Fixing the quirks is out of scope (story 10).
- **D6 – Tournament rank comes from the full scoreboard:** the person's rank in a tournament is their rank on that tournament's scoreboard, which keeps today's competition ranking (1, 1, 3). `RankedCount` is the number of entries on that scoreboard. `IsRankShared` is true when another scoreboard entry has the same rank.
- **D7 – Tournament order:** a tournament's position in the history is the earliest `PlayedOn` among its finished games that the person was in. Tournaments where all those games are undated come last. Ties are broken by tournament name (ordinal). Tournament cards and bars are shown oldest first.
- **D8 – Role counting:** per finished game, the person gets one count for each role they hold:
  - participant: they are in the participant list, **or** they are an organizer and the game's `IsOrganizersParticipating` is true
  - organizer: they are in the organizer list
  - spectator: they are in the spectator list

  So a playing organizer counts as both participant and organizer, and a game can give a person more than one count. A person who appears only in a placement list gets no role count for that game.
- **D9 – Main role label:** the largest role count gives `Mest deltaker`, `Mest arrangør` or `Mest tilskuer`. On a tie between the largest counts, the label lists them in the fixed order deltaker, arrangør, tilskuer: `Like mye deltaker som arrangør`, `Like mye deltaker som tilskuer`, `Like mye arrangør som tilskuer`, or `Like mye deltaker, arrangør og tilskuer`.
- **D10 – Highlights are computed in the API:**
  - `BestPlacement`: the best (lowest) place the person got in any game, ties broken by the most recent `PlayedOn` (undated counts as oldest), then by game name. It is null with no podium.
  - `PodiumCount`: the number of finished games where the person placed first, second or third.
  - `BestTournamentRank`: the lowest tournament rank, ties broken by the tournament that comes latest in D7 order.

  *Why:* the frontend has no test runner, so the selection logic lives where handler tests can cover it. The frontend only fills in sentence templates.
- **D11 – Summary sentences:** the page opens with fixed Norwegian templates (no AI), built from the API data. Sentences, in order:
  1. `{Fornavn} har vært med på {n} spill i {t} {turnering|turneringer}, {oftest som deltaker|oftest som arrangør|oftest som tilskuer|like ofte som deltaker og arrangør|…}.` The role phrase follows D9, e.g. `like ofte som deltaker, arrangør og tilskuer`.
  2. `Beste plassering: {p}. plass i {spill} ({dato}).` The date uses the existing Norwegian long format, or is left out with its parentheses for an undated game.
  3. `{k} {pallplass|pallplasser} totalt.`
  4. `Beste turneringsplassering: {Delt }{r}. plass av {m} i {turnering}.`

  Sentences 2 and 3 are left out when the person has no podium. When all role counts are 0, the role clause is left out of sentence 1.
- **D12 – Tournament cards and bars:** one card per tournament in D7 order, showing the tournament name (linking to the public tournament page), total points and rank as `{r}. plass av {m}`, prefixed `Delt ` when `IsRankShared` (`Delt 3. plass av 10`). Above the cards, a bar chart is drawn with CSS or inline SVG (no chart library), with one bar per tournament. Bar heights are proportional to the person's highest tournament total, and each bar is labelled with the tournament name and points. The bar chart is hidden when the person has fewer than 2 tournaments. *Why:* one bar says nothing, and the summary and timeline carry the story for a short history. Rejected: a cumulative line, which is misleading across different point rules.
- **D13 – Role profile:** a section shows the three role counts (`Deltaker`, `Arrangør`, `Tilskuer`) and the D9 label.
- **D14 – Game timeline:** every finished game the person was in, newest first by `PlayedOn`, then by game name, with undated games last. Each row shows the date (or `Dato ikke satt`), the occasion if any, the tournament name, the game name (linking to the public game page), the role(s) in D8 order (`Deltaker`, `Arrangør`, `Tilskuer`), the placement if any (`1. plass`), and the points earned in that game (`6 poeng`). There is no points breakdown and no pagination: all games are shown.
- **D15 – Empty history:** a person with no finished games gets the text `Ingen ferdige spill ennå` below the name, and no summary, bars, cards, role profile or timeline.
- **D16 – One endpoint:** `GET /api/v1/persons/{id}/history` returns everything the page needs. It returns 404 ProblemDetails for an unknown person, and 200 with zero counts, empty lists and null highlights for a person with no finished games.

Response shape (JSON, camelCase, enums as strings):

```
PersonHistoryResponse
  personId, firstName, lastName
  roleCounts: { participant: int, organizer: int, spectator: int }
  podiumCount: int
  bestPlacement: { place: 1|2|3, gameId, gameName, playedOn: date|null } | null
  bestTournamentRank: { tournamentId, tournamentName, rank, rankedCount, isRankShared } | null
  tournaments: [ { tournamentId, name, slug, totalPoints, rank, rankedCount, isRankShared } ]   // D7 order
  games: [ { gameId, name, playedOn: date|null, occasion: string|null,
             tournamentId, tournamentName, tournamentSlug,
             roles: ("Participant"|"Organizer"|"Spectator")[], placement: 1|2|3|null, points: int } ]  // D14 order
```

## Acceptance criteria

Fixture used by AC1–AC7 and AC9 ("the Kari fixture"):

- Tournament **Lagtur** (slug `lagtur`) with default rules (3, 3/2/1, 1, 3, 1):
  - **Boccia**, `2026-05-20`, finished. Participants Kari and Ola. First place Kari, second place Ola.
  - **Vinquiz**, `2026-05-21`, finished. Kari is organizer with participation, Ola is participant. No placements.
  - **Petanque**, `2026-05-22`, **not** finished. Kari is participant.
- Tournament **Fredagspils** (slug `fredagspils`) with custom rules: participation 2, first 5, second 3, third 1, organized with participation 1, organized without participation 2, spectator 1.
  - **Mario Kart**, `2026-09-04`, finished. Participants Kari and Ola. First place Ola, third place Kari.
  - **Dart**, `2026-09-18`, occasion "Fredagspils uke 38", finished. Participant Ola, spectator Kari. First place Ola.

Expected points for Kari:
- Boccia 3 + 3 = 6
- Vinquiz 1 + 3 = 4
- Mario Kart 2 + 1 = 3
- Dart 1

Totals: Lagtur 10 (Ola has 3 + 2 + 3 = 8), so Kari is rank 1 of 2. Fredagspils 4 (Ola has 2 + 5 + 2 + 5 = 14), so Kari is rank 2 of 2.

- **AC1 – Scoreboard unchanged:** Given the Kari fixture, plus a game with a non-playing organizer, a spectator and a shared placement, when `GET /api/v1/tournaments/{id}/scoreboard` is called before and after the calculator refactor, then every person's points and rank are identical. On the Kari fixture: Lagtur is Kari 10 (rank 1) and Ola 8 (rank 2); Fredagspils is Ola 14 (rank 1) and Kari 4 (rank 2).
- **AC2 – History endpoint, games:** Given the Kari fixture, when `GET /api/v1/persons/{kari}/history` is called, then the response is `200` and `games` has exactly four rows, in order:
  1. Dart: roles `["Spectator"]`, placement null, points 1, occasion "Fredagspils uke 38"
  2. Mario Kart: `["Participant"]`, 3, 3
  3. Vinquiz: `["Participant","Organizer"]`, null, 4
  4. Boccia: `["Participant"]`, 1, 6

  Petanque is not included.
- **AC3 – History endpoint, tournaments:** Given the Kari fixture, then `tournaments` is Lagtur (`totalPoints` 10, `rank` 1, `rankedCount` 2, `isRankShared` false) followed by Fredagspils (4, 2, 2, false).
- **AC4 – History endpoint, roles and highlights:** Given the Kari fixture, then:
  - `roleCounts` is `{participant: 3, organizer: 1, spectator: 1}`
  - `podiumCount` is 2
  - `bestPlacement` is `{place: 1, gameName: "Boccia", playedOn: "2026-05-20"}`
  - `bestTournamentRank` is `{tournamentName: "Lagtur", rank: 1, rankedCount: 2, isRankShared: false}`
- **AC5 – Shared rank:** Given a tournament where Kari and Ola both have 6 points and Per has 3, when Kari's history is requested, then that tournament has `rank` 1, `rankedCount` 3, `isRankShared` true. On the player page its card reads `Delt 1. plass av 3`.
- **AC6 – Tie-breaks in highlights:** Given Kari placed first in "Boccia" (`2026-05-20`) and in "Dart" (`2026-09-18`), then `bestPlacement.gameName` is "Dart". Given Kari has rank 1 in both Lagtur and Fredagspils, then `bestTournamentRank.tournamentName` is "Fredagspils".
- **AC7 – Summary on the page:** Given the Kari fixture, when the public page `/players/{kari}` is opened, then it shows:
  - `Kari har vært med på 4 spill i 2 turneringer, oftest som deltaker.`
  - `Beste plassering: 1. plass i Boccia (20. mai 2026).`
  - `2 pallplasser totalt.`
  - `Beste turneringsplassering: 1. plass av 2 i Lagtur.`
- **AC8 – Short history:** Given Mari has two finished games in one tournament, one as participant with no placement and one as non-playing organizer, when her page is opened, then:
  - the summary reads `Mari har vært med på 2 spill i 1 turnering, like ofte som deltaker og arrangør.`, with no placement or podium sentence
  - the role profile shows Deltaker 1, Arrangør 1, Tilskuer 0 and the label `Like mye deltaker som arrangør`
  - one tournament card is shown and the bar chart is **not** shown
  - the timeline has two rows.
- **AC9 – Tournament cards, bars and timeline on the page:** Given the Kari fixture, when her page is opened, then:
  - the bar chart shows two bars, Lagtur (10) then Fredagspils (4), with Fredagspils at 40 % of Lagtur's height
  - the cards read `Lagtur · 10 poeng · 1. plass av 2` and `Fredagspils · 4 poeng · 2. plass av 2`, each linking to its tournament page
  - the role profile shows Deltaker 3, Arrangør 1, Tilskuer 1 and `Mest deltaker`
  - the timeline shows Dart, Mario Kart, Vinquiz, Boccia in that order. The Vinquiz row reads roles `Deltaker, Arrangør` and `4 poeng`, the Boccia row shows `1. plass` and `6 poeng`, and each game name links to its public game page.
- **AC10 – Empty history:** Given a person with no finished games (only an unfinished game, or none), when `GET /api/v1/persons/{id}/history` is called, then the response is `200` with `roleCounts` all 0, `podiumCount` 0, `bestPlacement` null, `bestTournamentRank` null, and empty `tournaments` and `games`. Their page shows `Ingen ferdige spill ennå` and none of the history sections.
- **AC11 – Unknown person:** When `GET /api/v1/persons/{random guid}/history` is called, then the response is `404` ProblemDetails. The page `/players/{random guid}` shows the existing not-found page.
- **AC12 – Undated legacy game:** Given Kari was a participant in a finished legacy game "Kubb" with no `PlayedOn` in Lagtur, then "Kubb" is the last row of `games`, and its timeline row shows `Dato ikke satt`. If it is her best placement, the summary shows `Beste plassering: 1. plass i Kubb.` with no date.
- **AC13 – Scoreboard links:** On the public tournament page, every scoreboard name links to `/players/{personId}`.

## Slices

Build in this order. Each slice is vertical and ends with green tests.

1. **S1 – Shared scoring calculator:** a pure domain calculator gives one person's points in one game under given rules (D5). The scoreboard query uses it, with no change in behaviour. Scoreboard characterization tests cover the organizer (with and without participation), spectator and shared-placement cases, and are written **before** the refactor and kept green through it. Satisfies AC1.
2. **S2 – History query and endpoint:** a person-history query in Application and `GET /api/v1/persons/{id}/history`, returning the D16 shape. It uses the calculator for per-game points and the tournament scoreboard ranking for ranks (D6), and computes roles (D8), ordering (D7, D14) and highlights (D10). Satisfies AC2, AC3, AC4, AC5, AC6, AC10, AC11, AC12 (API part).
3. **S3 – Player page history:** the public player page fetches the history and renders the summary (D11), the bar chart and tournament cards (D12), the role profile (D9, D13), the timeline (D14) and the empty state (D15). Satisfies AC7, AC8, AC9, AC10 (page), AC11 (page), AC12 (page).
4. **S4 – Scoreboard links:** scoreboard names on the public tournament page link to the player page. Satisfies AC13.

## Test seams

| AC | Seam | Prior art |
|---|---|---|
| AC1 | domain unit tests for the calculator (each role, flags, custom rules, a person in several lists), plus scoreboard handler characterization tests (organizer with and without participation, spectator, shared placement) | `Domain.Tests/Games/GameTests.cs`, `Application.Tests/Tournaments/TournamentQueryHandlerTests.cs` (`GetScoreboard_BeregnerPoengRiktig`) |
| AC2–AC6, AC10, AC12 | handler tests on the history query with the Kari fixture | `Application.Tests/Persons/PersonQueryHandlerTests.cs` |
| AC2, AC10, AC11 | API test: JSON shape (camelCase, string enums, `"2026-05-20"` dates), 200 and 404. The shared DB is not empty, so use fresh persons. | `Api.Tests/Persons/PersonsApiTests.cs` |
| AC7–AC9, AC12 (page), AC13 | Playwright proof against the running app (no frontend test runner) | none |

## Proof plan

How each AC is demonstrated against the running app (Playwright screenshot or API call) before the PR.

| AC | Proof |
|---|---|
| AC1 | `GET /api/v1/tournaments/{id}/scoreboard` for both seeded demo tournaments on the main branch and on the story branch: identical JSON |
| AC2–AC4 | create the Kari fixture through the API, then `GET /api/v1/persons/{kari}/history`: the response shows the expected rows, totals, roles and highlights |
| AC5, AC6 | the same with the shared-rank and tie-break fixtures |
| AC10, AC11 | `GET` history for a person without finished games (200, empty) and for a random guid (404) |
| AC7, AC9 | screenshot of `/players/{kari}` showing the summary, bars, cards, role profile and timeline |
| AC8 | screenshot of the short-history player page (no bars) |
| AC10 (page) | screenshot of a player page with `Ingen ferdige spill ennå` |
| AC12 | screenshot of a timeline row with `Dato ikke satt` (use a game created before the date migration, or a fixture with a null date) |
| AC13 | screenshot of a tournament scoreboard, then a click on a name landing on the player page |
| Demo | screenshot of a seeded person with games in both demo tournaments (e.g. Kari), showing the history on a fresh setup |

## Data and migrations

None. The history is derived at query time from `Games`, `Tournaments` and `Persons`. The person's games are found by checking the game's person-id arrays, loaded and filtered in memory like the scoreboard; Postgres array `Contains` is allowed if it translates. The scoreboard ranking is computed per tournament the person is in. No new entity, column or index. `TestAppDbContext` needs no change.

## Out of scope

- A points breakdown per game ("why did I get these points", story 1).
- Records and streaks (story 25).
- A "Meg" page or linking the history to the logged-in user (story 20).
- Simracing race times or placements beyond the podium.
- Department history or comparing with other people.
- Changing the scoreboard's current scoring quirks: the game-wide organizer flag, placement points without participation, and rank gaps from deleted persons (story 10).
- Unfinished or upcoming games in the history.
- Pagination or filtering of the timeline.
- A chart library or a frontend test runner.

## Open questions

