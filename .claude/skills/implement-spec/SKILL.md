---
name: implement-spec
description: Implement an approved story spec slice by slice, test-first, then prove every acceptance criterion against the running app. Use when a spec in docs/specs/ has status approved, or the user asks to implement or continue implementing a story.
---

# Implement spec

## 0. Preconditions

- You are on the story branch, `feat/<N>-*`.
- The spec's front matter says `status: approved`. Otherwise stop and hand back to the `write-spec` gate, because approval is the human's call.

Move the card to **In progress** with `.claude/scripts/board-status.sh <N> "In progress"`.

## 1. Build each slice, in spec order

Use the `tdd` skill. The spec's *Test seams* table lists the pre-agreed seams, so use them as they are.

For each slice:

1. **Red:** write the test for the slice's next AC at its seam. Run it and watch it fail for the right reason.
2. **Green:** write the least code that passes it. Follow `AGENTS.md`. In particular, when you add an entity or mapping, update `TestAppDbContext`. For a schema change, add a migration with the `dotnet ef` command in `AGENTS.md`.
3. Repeat until every AC in the slice has a passing test, then run the whole affected test project.
4. Commit as `feat(<area>): S<n> <slice name> (#<N>)`.

A slice is done when its ACs have passing tests and the commit is in.

If the spec turns out to be wrong or incomplete, stop and ask the developer. Then update the spec as its own commit, `docs(spec): …`, and continue from the corrected spec. The spec and the code must never disagree.

## 2. Full checks

Everything here must pass together:

- `dotnet format TronderLeikan.slnx --verify-no-changes`
- `dotnet build`
- `dotnet test`, the whole suite including the Docker-based API and Infrastructure tests
- if anything under `src/frontend` changed: `npm run lint` and `npm run build`

## 3. Prove

Green tests are not proof. Show each AC working in the running app:

1. Start the stack if it is not running: `dotnet run --project src/TronderLeikan.AppHost`, in the background, from the main clone. Get the frontend and API URLs from the Aspire MCP (`list_resources`).
2. Follow the spec's *Proof plan* for each AC:
   - UI: use the Playwright MCP and save a screenshot as `docs/specs/<N>-<slug>/proof/AC<n>.png`
   - API: save the request and response as `AC<n>.txt`
3. Write `docs/specs/<N>-<slug>/proof/README.md`, a table with AC → evidence file → one line on what it shows.
4. Commit as `test(proof): bevis for #<N>`.

Done when every AC in the spec has an evidence file. Next is the `ship` skill.
