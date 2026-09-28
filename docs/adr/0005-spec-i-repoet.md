# 0005 – A story's spec lives in the repo, and a human approves it

**Status:** accepted (recorded 2026-09-28)

## Context

The team works spec-driven with AI agents (`docs/agents/workflow.md`). The spec is what the implementing agent and the reviewer treat as the truth, so it has to be versioned, reviewable, and approved by a human before any code is written.

## Decision

- The spec is `docs/specs/<issue>-<slug>.md`, in English, made from the template in `.claude/skills/write-spec/`. The GitHub issue stays the Norwegian card, and it links to the spec.
- The spec is written at pick time, by grilling the developer who takes the story, and not in advance. The domain decisions in it belong to the human.
- The front matter `status: draft | approved` is the only gate before the PR. The spec and its code reach `main` in the same PR.

## Consequences

- If code and spec disagree, the spec is corrected first, as its own commit, and the code follows.
- A decision in a spec that will outlive the story becomes an ADR here.
