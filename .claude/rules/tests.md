---
paths:
  - "tests/**/*.cs"
---

# Test conventions

## Pick the seam

| Behaviour | Project | Setup |
|---|---|---|
| A rule inside one entity | `Domain.Tests` | plain objects |
| A use case: handler + state | `Application.Tests` | `await using var db = TestAppDbContext.Create();` then `new XHandler(db).Handle(...)` |
| HTTP contract: status codes, ProblemDetails, JSON shape | `Api.Tests` | `[Collection(nameof(ApiTestCollection))]`, class takes `TronderLeikanApiFactory` |
| EF mapping against real Postgres | `Infrastructure.Tests` | Testcontainers |
| Layering | `Architecture.Tests` | extend the rules instead of adding exceptions |

## Writing a test

- **Names are Norwegian.** Unit and handler tests use `<Metode>_<ForventetOppførsel>`, e.g. `AddParticipant_DuplikatIgnoreres` or `CompleteGame_SetterIsDoneOgPlasseringer`. API tests use `<VERB>_<ressurs>_<resultat>` in snake case, e.g. `POST_games_returnerer_201_med_guid`.
- **Assertions use AwesomeAssertions** (`.Should()`). Older Application tests use `Assert.*`. Leave them as they are, and write new tests with `.Should()`.
- Separate arrange, act and assert with blank lines.
- **Api.Tests share one database across the whole run.** Create your own data with unique values (`slug = $"t-{Guid.NewGuid():N}"`), and never assume a table is empty. A failed result's `title` in ProblemDetails is the error code (`Game.NotFound`). Assert on that, not on the Norwegian text.
- A test that fails is information. Fix the code, or ask. Keep the test's expectation as it was written.
