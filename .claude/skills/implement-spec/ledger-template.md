# Progress #<N> <title>

Ledger for the orchestrator. Survives compaction and session restarts: `/story <N>` resumes from here. One line per event, newest last. Commit with each slice.

## Slices

| Slice | Status | Commits | Review | Interfaces for later slices |
|---|---|---|---|---|
| S1 | todo / running / review / done | `<base7>..<head7>` | CLEAN / fixed r1 / ruling | <types, endpoints, props> |

## Rulings

Decisions the developer made during implementation, with the date. A ruling that changes the spec also got its own `docs(spec):` commit.

- <date> S<n>: <question> → <decision>

## Proof

- todo / running / <n> failed / done — `<commit>`
