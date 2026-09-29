---
issue: 1
title: Hvorfor fikk jeg disse poengene?
status: approved
branch: feat/1-hvorfor-disse-poengene
---

# Hvorfor fikk jeg disse poengene?

## Problem

Participants see only a total on the scoreboard and cannot tell where their points came from, per game or overall. A first-timer cannot work out why they got 4 points and not 6. After this story, every finished game shows each person's points split into labelled parts. Each scoreboard row also links to a page that lists that person's points game by game, adds up to the scoreboard total, and explains the tournament's point rules in plain Norwegian.

## Domain decisions

- **D1 – One scoring source:** points are computed in one domain module that turns a finished game plus the tournament's point rules into *point lines* (person, kind, points). The scoreboard total, the per-game table and the person breakdown are all sums of these lines. The scoreboard query stops doing its own arithmetic. *Why:* the explanation must always add up to the scoreboard. Two calculations would drift.
- **D2 – Point line kinds:** `Participation`, `OrganizedWithParticipation`, `OrganizedWithoutParticipation`, `Spectator`, `FirstPlace`, `SecondPlace`, `ThirdPlace`. Each line carries the point value from the tournament's current rules.
- **D3 – No rule changes: the breakdown mirrors today's scoring exactly**, quirks included:
  - An organizer in a game where `IsOrganizersParticipating` is true gets a `Participation` line and an `OrganizedWithParticipation` line. Otherwise they get one `OrganizedWithoutParticipation` line. The flag is per game, not per organizer (story #5 changes that).
  - A person with several roles in one game gets lines for every role. For example, an organizer who plays and is also listed as a participant gets `Participation` twice plus `OrganizedWithParticipation`.
  - A placement line is given to whoever is in the placement list, whether or not they are listed as a participant (possible for Simracing).
  *Why:* the developer chose to show today's behaviour truthfully rather than change totals in this story. Rejected: fixing double roles here.
- **D4 – Line order within a game:** `Participation`, `OrganizedWithParticipation`, `OrganizedWithoutParticipation`, `Spectator`, `FirstPlace`, `SecondPlace`, `ThirdPlace`. The same kind repeats when D3 produces it twice.
- **D5 – Labels (Norwegian UI):** `Participation` → "Deltok", `OrganizedWithParticipation` → "Arrangerte og spilte", `OrganizedWithoutParticipation` → "Arrangerte (spilte ikke)", `Spectator` → "Så på", `FirstPlace` → "1. plass", `SecondPlace` → "2. plass", `ThirdPlace` → "3. plass". The first line of a game is shown as a plain number and later lines are prefixed with `+`, followed by the game sum.
- **D6 – Only finished games give points:** an unfinished game has no point lines. Its game page shows `Poeng deles ut når spillet er ferdig`, and it is absent from the person breakdown. This matches the scoreboard.
- **D7 – Rank and ties on the person page:** the rank is the one the scoreboard shows, using the existing tie rule (equal totals share a rank, and the next rank skips). When others share the rank, the page says `Delt <rank>. plass med <names>`. A person with no points in the tournament has no rank.
- **D8 – Rule text in plain Norwegian:** the person page and the game page's points section show one short paragraph built from the tournament's actual values, for example: `Du får 3 poeng for å delta. 1., 2. og 3. plass gir 3, 2 og 1 poeng ekstra. Arrangerer du og spiller selv, får du 1 poeng ekstra. Arrangerer du uten å spille, får du 3 poeng. Tilskuere får 1 poeng. Bare ferdige spill teller.` *Why:* this is how a reader sees why 4 is not 6. Rejected: per-game what-if hints.
- **D9 – Per-game table:** the public game page gets a "Poeng i dette spillet" section with one row per person who has point lines: name, the lines, and the game sum. Rows are sorted by game sum descending, then last name and first name. Each name links to that person's breakdown page in the tournament.
- **D10 – Person breakdown page:** a new public page, `/tournaments/[slug]/players/[personId]`, shows the person's name, `Totalt <n> poeng · <rank>. plass` (or the tie wording from D7), the rule text (D8), and one block per finished game the person has lines in. A block shows the date, occasion, game name (linking to the game page), the lines and the game sum. Games are in chronological order, using the existing ordering from story #11. Each scoreboard row's name links to this page.
- **D11 – Two read endpoints, no writes:**
  - `GET /api/v1/games/{id}/points` → `{ isDone, entries: [{ personId, firstName, lastName, lines: [{ kind, points }], total }] }`, with entries sorted as in D9. It returns `entries: []` when the game is not done, and 404 for an unknown game.
  - `GET /api/v1/tournaments/{id}/scoreboard/{personId}` → `{ personId, firstName, lastName, totalPoints, rank, tiedWith: [{ personId, firstName, lastName }], games: [{ gameId, name, playedOn, occasion, lines: [{ kind, points }], total }] }`. It returns 404 for an unknown tournament or person. A known person with no lines in the tournament gets `200` with `totalPoints: 0`, `rank: null`, `tiedWith: []`, `games: []`, and the page shows `Ingen poeng i denne turneringen ennå`.
  - `kind` is serialized as a string, like the other enums.
- **D12 – Current rules apply:** lines use the tournament's current point rules, as the scoreboard does today. Story #7 changes this later.

## Acceptance criteria

Shared setup for AC1–AC7: tournament "Testleikan" with default rules (3; 3/2/1; 1; 3; 1) and persons Kari Nordmann, Ola Hansen, Siri Berg, Per Lie and Nora Dahl.
- "Dart", `2026-03-06`, finished: participants Kari, Ola, Siri; 1st Kari.
- "Kubb", `2026-03-13`, finished: participants Ola, Kari; organizer Per without participation; spectator Nora; 1st Ola, 3rd Kari.
- "Boccia", `2026-03-20`, not finished: participants Kari, Ola.

- **AC1 – Scoreboard unchanged:** When the scoreboard is fetched, then it is Kari 10 (rank 1), Ola 9 (rank 2), Per 3 (rank 3), Siri 3 (rank 3), Nora 1 (rank 5), the same as before this story.
- **AC2 – Per-game points API:** When `GET /api/v1/games/{Kubb}/points` is called, then `isDone` is true and the entries in order are Ola `[Participation 3, FirstPlace 3]` total 6, Kari `[Participation 3, ThirdPlace 1]` total 4, Per `[OrganizedWithoutParticipation 3]` total 3, Nora `[Spectator 1]` total 1.
- **AC3 – Unfinished game:** When `GET /api/v1/games/{Boccia}/points` is called, then the response is `200` with `isDone: false` and `entries: []`. The public Boccia page shows `Poeng deles ut når spillet er ferdig` and no points table. For an unknown game id, the endpoint returns `404`.
- **AC4 – Person breakdown API:** When `GET /api/v1/tournaments/{Testleikan}/scoreboard/{Kari}` is called, then `totalPoints` is 10, `rank` is 1, `tiedWith` is empty, and `games` is Dart `[Participation 3, FirstPlace 3]` total 6, then Kubb `[Participation 3, ThirdPlace 1]` total 4. Boccia is absent.
- **AC5 – Ties:** When Per's breakdown is fetched, then `rank` is 3 and `tiedWith` is `[Siri Berg]`. The page says `Delt 3. plass med Siri Berg`.
- **AC6 – Totals always add up:** For every person on the Testleikan scoreboard, their breakdown `totalPoints` equals their scoreboard total and equals the sum of all their `lines`.
- **AC7 – No points yet:** Given a person in no Testleikan game, their breakdown is `200` with `totalPoints: 0`, `rank: null`, `games: []`, and the page shows `Ingen poeng i denne turneringen ennå`. An unknown person id or tournament id returns `404`.
- **AC8 – Organizers and double roles are shown truthfully:** Given a finished game "Quiz" with default rules, where Lise is added as an organizer with participation and also as a participant, and Tor is added as an organizer *without* participation (Lise's addition sets the game-level flag, so Tor is treated as playing too), then Lise's lines are `[Participation 3, Participation 3, OrganizedWithParticipation 1]` total 7, and Tor's are `[Participation 3, OrganizedWithParticipation 1]` total 4. Both totals match the scoreboard.
- **AC9 – Placement without participation:** Given a finished Simracing game where Mia has a result and is 1st but is not a participant, her lines are `[FirstPlace 3]` total 3, and the scoreboard also counts 3.
- **AC10 – Game page table:** On the public Kubb page, "Poeng i dette spillet" shows the rows `Ola Hansen: Deltok 3 + 1. plass 3 = 6`, `Kari Nordmann: Deltok 3 + 3. plass 1 = 4`, `Per Lie: Arrangerte (spilte ikke) 3 = 3`, `Nora Dahl: Så på 1 = 1`, in that order, with the rule text from D8. Each name links to that person's breakdown page.
- **AC11 – Scoreboard links:** On the public Testleikan page, each scoreboard name links to `/tournaments/testleikan/players/<personId>`.
- **AC12 – Person page:** On Kari's breakdown page: `Kari Nordmann`, `Totalt 10 poeng · 1. plass`, the rule text with the tournament's values, then a Dart block (`6. mars 2026 · Dart`: `Deltok 3 + 1. plass 3 = 6`) followed by a Kubb block (`13. mars 2026 · Kubb`: `Deltok 3 + 3. plass 1 = 4`). Game names link to their game pages.
- **AC13 – Custom rules:** Given a tournament with custom rules (participation 2, 1st 5), the lines, the rule text and the totals use 2 and 5.

## Slices

Build in this order. Each slice is vertical and ends with green tests.

1. **S1 – Scoring module:** a domain module that yields point lines for a finished game under given point rules (D1–D4), with the scoreboard query refactored to sum those lines and no change in results. Satisfies AC1, AC8 (scoreboard part), AC9 (scoreboard part), AC13 (totals).
2. **S2 – Points per game:** the `GET /api/v1/games/{id}/points` query and endpoint, plus the "Poeng i dette spillet" section, the unfinished-game note and the rule text on the public game page. Satisfies AC2, AC3, AC8, AC9, AC10, AC13.
3. **S3 – Person breakdown:** the `GET /api/v1/tournaments/{id}/scoreboard/{personId}` query and endpoint with rank and ties, the new public person page, and the scoreboard name links. Satisfies AC4, AC5, AC6, AC7, AC11, AC12.

## Test seams

| AC | Seam | Prior art |
|---|---|---|
| AC8, AC9, AC13, D4 order | domain unit on the scoring module | `Domain.Tests/Games/GameTests.cs` |
| AC1, AC6 | handler test: scoreboard totals before and after the refactor, and equal to the breakdown sums | `Application.Tests/Tournaments/TournamentQueryHandlerTests.cs` |
| AC2, AC3, AC4, AC5, AC7 | handler tests for the two new queries | `Application.Tests/Tournaments/TournamentQueryHandlerTests.cs`, `Application.Tests/Games/GameBannerAndQueryTests.cs` |
| AC2, AC3, AC4, AC7 | API (JSON shape, `kind` as string, 404s; the DB is shared, so use a fresh tournament) | `Api.Tests/Games/GamesApiTests.cs`, `Api.Tests/Tournaments/TournamentsApiTests.cs` |
| AC10–AC12 | Playwright proof against the running app (no e2e suite) | none |

## Proof plan

How each AC is demonstrated against the running app (Playwright screenshot or API call) before the PR.

| AC | Proof |
|---|---|
| setup | create the shared Testleikan setup through the API (tournament, persons, games, roles, completion) |
| AC1 | `GET /api/v1/tournaments/{id}/scoreboard` shows the expected totals and ranks |
| AC2, AC3 | `GET /api/v1/games/{Kubb}/points` and `{Boccia}/points`, plus an unknown id → 404 |
| AC4, AC5, AC6, AC7 | `GET /api/v1/tournaments/{id}/scoreboard/{personId}` for Kari, Per, every scoreboard person (sums compared), a person with no games, and an unknown id |
| AC8, AC9 | a Quiz game and a Simracing game created through the API, then the points endpoint and the scoreboard |
| AC10 | Playwright screenshot of the public Kubb page and of the Boccia page's note |
| AC11, AC12 | Playwright: click Kari on the scoreboard and screenshot her breakdown page, then Per's page with the tie wording |
| AC13 | API: tournament with custom rules, points endpoint and breakdown show 2 and 5 |

## Data and migrations

None. Everything is computed at query time from existing games and point rules.

## Out of scope

- Changing any scoring rule: double roles, the per-game organizer flag (#5), placement without participation.
- Rules that change mid-season or are applied backwards (#7), best-N counting (#2), and team games (#8).
- Cross-tournament history on `/players/[id]` (#13). That page is unchanged.
- What-if hints ("1. plass hadde gitt 3 poeng mer").
- Point explanations in admin pages.
- Showing provisional points for unfinished games.

## Open questions

