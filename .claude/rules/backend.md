---
paths:
  - "src/TronderLeikan.*/**/*.cs"
---

# Backend conventions

Why the architecture looks like this: `docs/adr/`.

## A use case

- A use case is one folder: `<Feature>/Commands/<UseCase>/` or `<Feature>/Queries/<UseCase>/`. It holds `<UseCase>Command.cs` (a `record` implementing `ICommand` or `ICommand<T>`), `<UseCase>CommandHandler.cs`, and, when the input has a shape to check, `<UseCase>CommandValidator.cs`. One type per file.
- A handler is `sealed`, takes `IAppDbContext` (and other ports) through its primary constructor, and returns `Result`/`Result<T>` through the implicit conversions: `return game.Id;` and `return GameErrors.NotFound;`.
- **Expected failures are returned, never thrown.** Add them as static members of `Common/Errors/<Feature>Errors.cs`, with the code `<Entity>.<Reason>` and a Norwegian description. Pick the `ErrorType` that gives the right HTTP status (NotFound 404, Validation 400, Conflict 409).
- Split the checks by kind:
  - the **validator** checks the input's shape (required, lengths)
  - the **handler** checks preconditions against state (does it exist, is it already done) and returns the error
  - **entity methods** do the change and assume they're called validly
- Queries map to response records in `<Feature>/Responses/` by hand, in the handler. Never return an entity from the API.

## Domain entities

- `sealed`, derived from `Entity`, with a private parameterless constructor for EF and a `static Create(...)` factory. Properties have private setters.
- Collections are private `List<T>` backing fields exposed as `IReadOnlyList<T>`, as in `Game`. Adding the same id twice is ignored, not an error.
- Raise a domain event (`AddDomainEvent`) in the method that causes it. The event needs a `Guid` property ending in `Id`: `AppDbContext` uses it as the event-store stream id and throws without it.

## Persistence

A new entity or mapping touches five places:
1. a `DbSet` in `IAppDbContext`
2. a `DbSet` in `AppDbContext`
3. an `IEntityTypeConfiguration` in `Infrastructure/Persistence/Configurations/`
4. the same mapping mirrored in `tests/TronderLeikan.Application.Tests/TestAppDbContext.cs`
5. a migration

## API

- One controller per aggregate, derived from `ApiControllerBase`, depending only on `ISender`.
- The action shape: `(await sender.Send(command with { GameId = id }, ct)).Match(onSuccess, Problem)`. The id from the route overrides the body through `with`. A create returns `CreatedAtAction(nameof(GetById), new { id }, id)`.
