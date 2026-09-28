---
issue: 11
title: Når ble det spilt?
status: approved
branch: feat/11-spilt-dato
---

# Når ble det spilt?

## Problem

Administrators cannot record when a game was played or which occasion (a team trip, a specific Friday beer) it belonged to. Games are listed alphabetically, so nobody can see a tournament's season unfold from the first evening to the last. After this story, every new game has a date and an optional occasion, and games are shown in chronological order to admins and to the public.

## Domain decisions

- **D1 – Date only:** a game has a `PlayedOn` calendar date (no time of day, no time zone). *Why:* "first evening to last" needs only the day; a time adds time-zone handling and a heavier form for no domain value. Rejected: date and time.
- **D2 – Date is required on every save, but may be absent on legacy games:** creating a game and updating a game both require a date. Games that existed before this story keep an empty date until an admin edits them, and editing forces a date to be set. Once set, a date can be changed but never cleared. *Why:* the developer wants all new data dated; forcing it on edit backfills old games gradually. Rejected: optional date, and allowing an empty date on edit.
- **D3 – Any date is allowed:** past and future dates are both valid, and completing a game does not check its date. *Why:* admins often create a game before the evening to add participants. Rejected: no future dates; no completing before the date.
- **D4 – Occasion is optional free text:** a game has an optional `Occasion` of at most 200 characters. Leading and trailing whitespace is trimmed, and text that is empty after trimming is stored as no occasion. There is no occasion entity. *Why:* it covers "which trip or Friday beer" without a new CRUD. Rejected: an `Occasion` entity with its own name and date; free text without suggestions.
- **D5 – Occasion suggestions come from the same tournament:** the occasion input suggests the distinct occasions already used by games in the same tournament (exact-match dedupe after trimming). Occasions from other tournaments are not suggested. Suggestions are derived from the tournament's existing game list, with no new endpoint. *Why:* it cuts typo variants like "Fredagspils uke 12" vs "fredagspils uke12" at no backend cost.
- **D6 – Chronological order everywhere:** every game list (the tournament games API, the admin tournament page, the new public list) is ordered by `PlayedOn` ascending, then by name (Norwegian collation in the UI), with undated games last. This replaces both the API's name ordering and the admin page's "unfinished first" client-side ordering. *Why:* the story is about seeing the season unfold.
- **D7 – No effect on points:** date and occasion do not affect the scoreboard, ranking, ties or domain events. `GameCompletedEvent` is unchanged.
- **D8 – Display format:** dates are shown in Norwegian long form, e.g. `13. mars 2026`, using the platform's `Intl` formatting for `nb-NO`, with no new date library. An undated game shows `Dato ikke satt`.
- **D9 – The create form is pre-filled with today:** the date input on "Legg til nytt spill" defaults to today's date in Norwegian time (Europe/Oslo, computed when the page renders), and the admin can change it before saving. *Why:* most games are registered on the evening they are played.
- **D10 – One edit form for game details:** the admin game page gets a "Rediger spill" form with name, description, date and occasion, backed by the existing update-game endpoint. The update is a full replacement of those four fields. Location stays out (see Out of scope).
- **D11 – The public tournament page lists all games:** a new flat chronological list shows every game in the tournament, finished or not. Each row shows the date, the occasion if any, the name and the status (`Ferdig` / `Ikke ferdig`), and links to the public game page.

## Acceptance criteria

- **AC1 – Create with date:** Given a tournament, when an admin creates a game "Dart" with `playedOn` = `2026-03-13` via `POST /api/v1/games`, then the response is `201`, and `GET /api/v1/games/{id}` returns `playedOn: "2026-03-13"`.
- **AC2 – Create without date is rejected:** Given a tournament, when a game is created with no `playedOn`, then the response is `400` ProblemDetails with a validation error on `PlayedOn`, and no game is created.
- **AC3 – Future date allowed:** When a game is created with `playedOn` = `2027-01-01`, then the response is `201`, and the game can later be completed normally.
- **AC4 – Occasion is saved trimmed:** When a game is created with `occasion` = `"  Fredagspils uke 11 "`, then `GET` returns `occasion: "Fredagspils uke 11"`. When it is created with `occasion` = `"   "` or with no occasion, then `GET` returns `occasion: null`.
- **AC5 – Occasion too long is rejected:** When a game is created or updated with an occasion of 201 characters, then the response is `400` with a validation error on `Occasion`. 200 characters is accepted.
- **AC6 – Edit game details:** Given a game "Dart" dated `2026-03-13` with no occasion, when an admin updates it via `PUT /api/v1/games/{id}` with name "Dart 501", description "Dobbel ut", `playedOn` `2026-03-20` and occasion "Fredagspils uke 12", then the response is `204`, and `GET` returns all four new values.
- **AC7 – Edit without date is rejected:** Given a legacy game with no date, when it is updated with no `playedOn`, then the response is `400` with a validation error on `PlayedOn`, and the game is unchanged. When it is updated with `playedOn` = `2026-02-27`, then the response is `204` and the game has that date.
- **AC8 – Chronological order:** Given a tournament with games "Dart" (`2026-03-13`), "Boccia" (`2026-03-13`), "Mario Kart" (`2026-03-06`) and a legacy "Kubb" (no date), when `GET /api/v1/tournaments/{id}/games` is called, then the order is Mario Kart, Boccia, Dart, Kubb. The admin tournament page and the public tournament page show the games in the same order.
- **AC9 – Summaries carry the new fields:** The game summaries in `GET /api/v1/tournaments/{id}/games` and the game detail in `GET /api/v1/games/{id}` both include `playedOn` (ISO date or `null`) and `occasion` (string or `null`).
- **AC10 – Create form:** On the admin tournament page, the "Legg til nytt spill" form has a date input pre-filled with today's date and an occasion input. Submitting "Dart" with date `2026-03-13` and occasion "Fredagspils uke 11" creates the game, and it appears in the admin list as `13. mars 2026 · Fredagspils uke 11`.
- **AC11 – Occasion suggestions:** Given tournament A with games whose occasions are "Fredagspils uke 11" (two games) and "Fredagspils uke 12", and tournament B with a game whose occasion is "Lagtur Toscana", when the admin focuses the occasion input on A's create form or on the edit form of a game in A, then exactly "Fredagspils uke 11" and "Fredagspils uke 12" are suggested.
- **AC12 – Edit form:** On the admin game page, the "Rediger spill" form is pre-filled with the game's current name, description, date and occasion (date empty for a legacy game). Saving with a changed date shows the new date on the page. The form cannot be submitted with an empty date.
- **AC13 – Public game list:** On the public tournament page, a game list shows each game as a row like `13. mars 2026 · Fredagspils uke 11 · Dart · Ferdig`, in the AC8 order, and each row links to that game's public page. A legacy game shows `Dato ikke satt` instead of a date, and a game without an occasion shows no occasion segment.
- **AC14 – Game pages show date and occasion:** The public game page and the admin game page both show the date (or `Dato ikke satt`) and the occasion, if any.
- **AC15 – Points unchanged:** Given a completed game, when its date or occasion is changed, then the tournament scoreboard shows identical points and ranks.
- **AC16 – Existing data survives:** After the migration runs on a database with existing games, every existing game still loads, with `playedOn: null` and `occasion: null`, and appears last in the lists.

## Slices

Build in this order. Each slice is vertical and ends with green tests.

1. **S1 – Date on create and in lists:** `Game` gets `PlayedOn` (set by the factory, required), a migration adds the column, create-game requires it, both responses carry it, the tournament games query sorts chronologically, and the admin tournament page gets the pre-filled date input and the chronological list with formatted dates. Satisfies AC1, AC2, AC3, AC8, AC9 (date part), AC10 (date part), AC16 (date part).
2. **S2 – Occasion on create:** `Game` gets `Occasion` (trimmed, empty → none, max 200), in the same or a follow-up migration. Create-game accepts it, both responses carry it, and the admin create form gets the occasion input with suggestions from the tournament. Satisfies AC4, AC5 (create), AC9, AC10, AC11 (create form), AC16.
3. **S3 – Edit game details:** update-game takes name, description, date and occasion, with a validator requiring the date. The admin game page gets the "Rediger spill" form with suggestions and shows the date and occasion. Satisfies AC5 (update), AC6, AC7, AC11 (edit form), AC12, AC14 (admin), AC15.
4. **S4 – Public season view:** the public tournament page gets the chronological game list, and the public game page shows the date and occasion. Satisfies AC13, AC14 (public).
5. **S5 – Demo data:** the demo seeder gives every seeded game a date and, where natural, an occasion (for example "Lagtur Toscana" days, and one occasion per Friday in "Fredagspils 2026"). This only affects empty databases. No AC, but it makes S4 demonstrable on a fresh setup.

## Test seams

| AC | Seam | Prior art |
|---|---|---|
| AC1, AC3 (complete) | domain unit (factory stores date; complete ignores date) | `Domain.Tests/Games/GameTests.cs` |
| AC4 | domain unit (trim / empty → none) | `Domain.Tests/Games/GameTests.cs` |
| AC2, AC5, AC7 | handler + validator tests | `Application.Tests/Games/GameCommandHandlerTests.cs` |
| AC6 | handler test (update-game, no test exists yet) | `Application.Tests/Games/GameCommandHandlerTests.cs` |
| AC8 | handler test (tournament games query ordering, no test exists yet) | `Application.Tests/Games/GameBannerAndQueryTests.cs` |
| AC1, AC2, AC6, AC7, AC9 | API (Testcontainers Postgres, real migration, JSON shape `"2026-03-13"`) | `Api.Tests/Games/GamesApiTests.cs` |
| AC16 | infrastructure persistence round-trip of a game with and without date/occasion | `Infrastructure.Tests/GamePersistenceTests.cs` |
| AC15 | handler test on the scoreboard query | existing scoreboard handler tests in `Application.Tests` |
| AC10–AC14 | Playwright proof against the running app (no e2e suite) | none |

Existing tests that create games without a date must be updated to pass one.

## Proof plan

How each AC is demonstrated against the running app (Playwright screenshot or API call) before the PR.

| AC | Proof |
|---|---|
| AC1, AC3, AC4, AC9 | `POST /api/v1/games` with `playedOn` `2026-03-13` / `2027-01-01` and occasion `"  Fredagspils uke 11 "`, then `GET` shows the stored values |
| AC2, AC5 | `POST` without `playedOn`, and with a 201-character occasion: both `400` ProblemDetails |
| AC6, AC7 | `PUT /api/v1/games/{id}` with and without `playedOn`: `204` / `400`, then `GET` |
| AC8 | `GET /api/v1/tournaments/{id}/games` on the AC8 fixture shows the expected order |
| AC10, AC11 | admin tournament page: screenshot of the pre-filled date, the suggestion list, and the new row in the list |
| AC12, AC14 | admin game page: screenshot of the pre-filled edit form, then after saving a new date |
| AC13, AC14 | public tournament page and public game page: screenshots of the chronological list (including a `Dato ikke satt` row) and of the game header |
| AC15 | scoreboard before and after changing a completed game's date: identical |
| AC16 | migration applied on the existing local database: the legacy games are listed last with `Dato ikke satt` |

## Data and migrations

- `Games.PlayedOn`: `date`, nullable. The domain property is nullable because of legacy rows (D2), but the factory and the update require a value.
- `Games.Occasion`: `character varying(200)`, nullable.
- Migration: the next after `AddGameLocation`. Existing rows get `NULL` in both columns and are not backfilled.
- `TestAppDbContext` needs no change (scalar properties are mapped by convention), but check this when implementing.
- The API contract changes: create-game and update-game now require `playedOn`. The only client is the frontend in this repo, which is updated in the same PR.

## Out of scope

- Time of day and time zones.
- An occasion entity, occasion CRUD, renaming an occasion across games, or grouping games by occasion.
- Season or date-range concepts on tournaments, and filtering or searching by date.
- Making `Location` editable (not in the API or UI today).
- Using the date in a person's history (story 13) or in point development over time.
- A Playwright e2e test suite.
- The frontend's unused `isOrganizersParticipating` field on create, which the backend ignores today.

## Open questions

