---
name: write-spec
description: Write or revise the agent-ready spec for a story in docs/specs/ by grilling the developer on every open decision. Use when a story has no approved spec, or the user asks to write, change or reopen a spec.
---

# Write spec

A spec is written for agents: precise, complete, and in English. The issue stays the human-readable card, in Norwegian. The spec is the **single source of truth** for what gets built. The implementing agent reads nothing else about the story.

## 1. Gather the facts

Finding facts is your job, never the developer's. Read:

- the issue (`gh issue view <N> --comments`)
- the domain: `docs/TRONDER_LEIKAN.md` and any ADRs in `docs/adr/`
- the code the story touches, with an Explore subagent when it spans several layers. Record the existing seams, entities, endpoints and pages you will build on, plus any prior art (Simracing is the example of a game type with its own results).

Done when you can name every module the story will change.

## 2. Grill

Use the `grilling` skill, with a recommendation on every question. Where a question has 2–4 clear options, ask it with AskUserQuestion. Keep grilling until the frontier is empty for all of these:

- **domain rules**: the core domain, which the human owns. Push hardest here: ties, edge cases, what happens to points.
- **user experience**: who does what, on which page, in what order
- **scope boundary**: what this story will not do
- **data**: new entities or fields, migrations, what happens to existing data
- **test seams**: which seam proves each behaviour (domain unit, handler, API, Playwright)

Done when every section of the template can be filled without a guess. Anything the developer chooses to defer goes under *Open questions* as `deferred by <name>`.

## 3. Write

Fill in [spec-template.md](spec-template.md) as `docs/specs/<N>-<slug>.md`, using the same slug as the branch. Rules:

- Number everything (D1, AC1, S1), so the code, commits and review can cite it.
- Every acceptance criterion is observable from outside: Given/When/Then, with concrete values.
- Every slice is vertical: domain → API → UI for one behaviour. List the ACs each slice satisfies. Slice order is build order.
- Name modules and behaviour, not file paths or code. The one exception is a schema or state shape that a decision depends on.

## 4. Publish

1. Commit as `docs(spec): #<N> <title>` and push the branch.
2. In the issue body, replace the `**Spec:**` line with a link to the spec on the branch: `https://github.com/alibasharali/italy-workshop/blob/<branch>/docs/specs/<file>`. Edit the body with `gh issue edit <N> --body-file`.

Done when the pushed spec is `status: draft` and the issue links to it.
