# AGENTS.md

This file provides guidance to coding agents working with code in this repository.

TrønderLeikan: a points/scoreboard system for informal company competitions. Domain rules (point values, ranking ties) are in `docs/TRONDER_LEIKAN.md`; workshop user stories are in `docs/backlog.md`. Code comments, docs, error messages and test names are written in **Norwegian**. Keep new ones in Norwegian too.

All work follows the story → spec → PR workflow in `docs/agents/workflow.md`. Read it before starting on a story.

## Commands

```bash
./bootstrap.sh                                   # check prerequisites, restore dotnet-ef tool
./reset-local.sh                                 # wipe local Postgres volume + zitadel-bootstrap/ (stop AppHost first)
```

CI (`.github/workflows/ci.yml`) runs restore/build/test in Release plus frontend `npm ci`, lint and build.

## Running locally

Aspire (`src/TronderLeikan.AppHost/AppHost.cs`) starts, in order: Postgres (persistent container, volume `leikan-postgres-data`), Zitadel (Traefik on a fixed port, default 8080), `DbMigrator` (applies migrations and seeds demo data if the DB is empty, then exits), API, and the Next.js frontend. Frontend and API get random ports, so read them from the Aspire dashboard. The Aspire MCP server (`.mcp.json`) can list resources and logs.

- Admin login: `zitadel-admin@zitadel.localhost` / `Password1!`.
- Secrets are generated and kept in AppHost user secrets. The frontend's OIDC client is provisioned in Zitadel automatically and cached in `src/TronderLeikan.AppHost/zitadel-bootstrap/` (gitignored).
- Don't run the AppHost from a git worktree. The Zitadel port and Postgres volume are shared, and `zitadel-bootstrap/` only exists in the main clone. See the troubleshooting table in `README.md`.

## Backend architecture (.NET 10, Clean Architecture)

`Domain` ← `Application` ← `Infrastructure` ← `API` / `DbMigrator`. `ServiceDefaults` holds the Aspire telemetry and health setup.

- **Domain**: entities derive from `Entity` (Guid `Id` + domain events via `AddDomainEvent`). Entities use factory methods (`Game.Create`, `Person.Create`) and private setters. `Game` keeps its participant, organizer, spectator and placement lists in private backing fields (`_participants` and so on). `TournamentPointRules` is an owned value object on `Tournament`.
- **Application**: a custom CQRS mediator. There is **no MediatR**.
  - Each use case is a folder: `<Feature>/Commands|Queries/<UseCase>/` containing the `…Command`/`…Query` record, its `…Handler`, and an optional FluentValidation `…Validator`.
  - Handlers implement `ICommandHandler<T>`, `ICommandHandler<T,TResult>` or `IQueryHandler<T,TResult>` and are auto-registered by Scrutor in `Common/DependencyInjection.cs`.
  - `Sender` dispatches by reflection through the pipeline behaviors `ObservabilityBehavior` (outer) and `ValidationBehavior` (inner).
  - Handlers return `Result`/`Result<T>` and don't throw for expected failures. Errors are static members of `Common/Errors/<Feature>Errors.cs`, and `ErrorType` sets the HTTP status.
  - Handlers depend on `IAppDbContext`, never the concrete `AppDbContext`.
- **Infrastructure**: `AppDbContext` is `internal`. Its `SaveChangesAsync` collects domain events and writes them to `OutboxMessages` and to the append-only `EventStore` in the same transaction. A domain event must have a Guid property ending in `Id`, which becomes the stream id. No outbox processor exists yet: `InMemoryMessagePublisher`, `DomainEventDispatcher` and the handlers in `Application/*/EventHandlers` are placeholders. Entity mappings live in `Persistence/Configurations/`.
- **API**: controllers inherit `ApiControllerBase`, which sets route `api/v{version}/[controller]` and has a `Problem(Error)` overload that returns RFC 9457 ProblemDetails. The pattern is `(await sender.Send(command with { GameId = id }, ct)).Match(onSuccess, Problem)`. Enums are serialized as strings. The API has no authentication. Auth is enforced only in the frontend.
- **Scoring** is computed at query time in `Application/Tournaments/Queries/GetScoreboard/GetScoreboardQueryHandler.cs`, from completed games only.

## Tests (xUnit, AwesomeAssertions)

- `Domain.Tests`: plain unit tests.
- `Application.Tests`: handlers are constructed directly with `TestAppDbContext` (EF InMemory). **When you add an entity or change a mapping, update `TestAppDbContext.OnModelCreating` to mirror the Infrastructure configuration.**
- `Infrastructure.Tests` and `Api.Tests`: real Postgres through Testcontainers. `Api.Tests` shares one `TronderLeikanApiFactory` (and one database) across classes via `[Collection(nameof(ApiTestCollection))]`, so tests must not assume an empty DB.

## Frontend (`src/frontend`, Next.js 16 / React 19 / Tailwind 4)

- **Read `src/frontend/AGENTS.md` first.** This Next.js version has breaking changes, so check the docs in `node_modules/next/dist/docs/` before writing code.
- Route groups: `(public)` holds the scoreboard, players and games pages. `(admin)/admin` holds the CRUD pages.
- Mutations are server actions (`actions.ts`) that `fetch` `${API_BASE_URL}/api/v1/...` and then call `revalidatePath`.
- Auth uses better-auth with Zitadel over generic OAuth and stateless JWE cookie sessions (`src/lib/auth.ts`). `/admin` is guarded in `src/proxy.ts`, which replaces the old `middleware.ts` in this Next.js version.
