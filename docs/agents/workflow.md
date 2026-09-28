# Workflow: from story to main

How every change in this repo gets made. It applies to humans and agents alike.
The skills in `.claude/skills/` automate the steps. This file is the contract they follow.

## Overview

```
Backlog ──/story N──▶ Spec ──human approves──▶ In progress ──▶ In review ──human merges──▶ Done
            (grill + write spec)   (implement slices, prove)   (PR + test steps) 
```

| Board column | Meaning | Who moves the card |
|---|---|---|
| Backlog | Story exists as an issue, with no spec yet | — |
| Spec | Branch created; spec is being written via grilling | agent (`/story`) |
| In progress | Spec has `status: approved`; agent is implementing | agent |
| In review | PR is open, CI is running, local test steps in the PR | agent (`/ship`) |
| Done | PR squash-merged to `main` | GitHub automation |

Board: https://github.com/users/alibasharali/projects/2

## Artifacts

- **Issue** (GitHub Issues, Norwegian): the human-readable story, as the product owner wrote it. It links to the spec.
- **Spec** (`docs/specs/<issue>-<slug>.md`, English): written for agents. It is the single source of truth for what gets built. Front matter holds `issue`, `status: draft | approved` and `branch`.
- **Branch**: `feat/<issue>-<slug>`, in the main clone. One story, one branch, one PR.

## Steps

1. **Pick.** `/story <issue>` creates the branch and moves the card to *Spec*.
2. **Spec** (`write-spec`). The agent reads the issue and the relevant code, then grills the developer on every open decision, giving its recommendation each time. It writes the spec, including:
   - the domain decisions
   - acceptance criteria
   - vertical slices
   - test points
   - how each criterion will be proven
   - what is out of scope

   The spec is committed on the branch.
3. **Gate.** The developer reads the spec and sets `status: approved`. **No production code is written before this.** This is the only human gate before the PR, so it's where core-domain judgement happens.
4. **Implement** (`implement-spec`). The card moves to *In progress*. The agent implements one slice at a time with TDD (red, then green), and commits per slice. It uses the conventions in `AGENTS.md` and `.claude/rules/`.
5. **Prove.** Before shipping:
   - build, all tests, lint and format are green
   - every acceptance criterion is demonstrated against the running app, with Playwright or API calls
   - evidence is saved in `docs/specs/<N>-<slug>/proof/` and linked from the PR

   Green tests alone are not done.
6. **Ship.** `/ship`:
   1. runs `/code-review` in a fresh subagent and fixes its findings
   2. runs a **retro**: every correction during the story goes through the `lesson` skill and becomes a rule, test, hook, ADR or skill in the same PR (*Lærdom*), or is judged a one-off
   3. pushes
   4. opens the PR, which links the issue and spec and includes the proof and the steps to test it locally
   5. moves the card to *In review*
7. **Review.** CI runs the checks and the Claude Code review action. The review follows `docs/agents/review-policy.md`: unambiguous findings are fixed and pushed as `fix(review): …` commits, and everything else becomes an unresolved thread that blocks the merge. Reply `@claude <what you want>` in a thread to have Claude fix it on the branch; a plain reply triggers nothing. Only a human resolves the thread. The human checks out the PR (`gh pr checkout <PR>`) in the main clone, runs the AppHost, tests it, and squash-merges. GitHub moves the card to *Done*.

## Learning loop

A mistake becomes a guardrail through the `lesson` skill. It runs when the developer corrects the agent, and on every story in the `/ship` retro. The lesson lands in the repo (rule, test, hook, ADR, skill or script), never in personal memory, so every developer's agent learns it.

## Guardrails

| Layer | What it enforces |
|---|---|
| Code | tests, `dotnet format`, eslint, architecture tests (layer dependencies), build |
| Harness (`.claude/settings.json`, shared) | deny reading secrets (`zitadel-bootstrap/`, user secrets, `.env`); Stop hook runs build + tests; format after every edit |
| Process (GitHub) | ruleset "main: kun via PR": PR required, CI jobs `Backend (.NET)` and `Frontend (Node)` must be green, all review threads resolved, squash-merge only, no force-push/deletion; merged branches are deleted |

CI is the enforcement that applies to everyone. Claude hooks catch problems earlier, inside agent sessions.
