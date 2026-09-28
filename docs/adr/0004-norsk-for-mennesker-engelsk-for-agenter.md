# 0004 – Norwegian for humans, English for identifiers and agent docs

**Status:** accepted (recorded 2026-09-28)

## Context

The users, the product owner and the team are Norwegian, and the domain terms are Norwegian (turnering, arrangør, tilskuer). Code identifiers and the tools' conventions are English. Without one rule, every agent picks a language per file.

## Decision

| Norwegian | English |
|---|---|
| code comments | identifiers: types, methods, properties, routes, JSON |
| test names | specs (`docs/specs/`) |
| `Error` descriptions | skills (`.claude/skills/`) |
| UI text | agent docs (`AGENTS.md`, `.claude/rules/`, `docs/agents/`, ADRs) |
| issues, PR descriptions, review comments | |
| commit messages | |

## Consequences

- Error *codes* (`Game.NotFound`) are identifiers, so they're in English, and tests assert on them. The Norwegian description can change without breaking tests.
- The domain words in `docs/TRONDER_LEIKAN.md` are used as they are in Norwegian text, and identifiers use their established English names (`Game`, `Organizers`).
