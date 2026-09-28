---
name: lesson
description: Turn a correction into a lasting team guardrail in the repo, so every developer's agent learns it. Use when the user corrects you ("no, we do X", "you forgot Y", "why did you do Z?"), when a hook, CI or review caught a mistake of yours, or when asked to remember or record something for the team.
---

# Lesson

A lesson is only learned once it's in the repo. That's where it reaches every developer's agent. Personal memory reaches only one agent, so it holds only this person's preferences, never facts about the codebase.

## 1. Name the mistake

Write one sentence on what you did and one on what you should have done. If you can't state both, ask the user.

## 2. Test it

Would a **fresh** agent, with only what's in the repo, make the same mistake? Check `AGENTS.md`, `.claude/rules/`, `docs/adr/`, `docs/agents/` and the skills. The verdict decides the next step:

| Verdict | Next |
|---|---|
| **Not covered anywhere** | step 3 |
| **Covered, but you missed it** | the pointer or wording is weak. Sharpen the existing line (front-load the key word, state it positively), rather than adding a second one |
| **One-off** (specific to this task, won't recur) | no change. Tell the user it's a one-off, and why |
| **Personal preference** (how this user likes to work, not how the codebase works) | it belongs in their `~/.claude/CLAUDE.md`. Suggest the line to them. Keep it out of the repo |

## 3. Pick the home

Choose the strongest home that fits. A check beats a rule, because a check doesn't depend on an agent reading anything.

| The mistake | Home |
|---|---|
| it must never happen | a hook in `.claude/hooks/`, wired in `.claude/settings.json` |
| it can be checked in code | a test. Structure and layering go in `Architecture.Tests`, behaviour at its seam (see `.claude/rules/tests.md`) |
| a convention for one area | a line in `.claude/rules/backend.md`, `tests.md` or `frontend.md` |
| a convention for the whole repo | one line in `AGENTS.md` |
| why a pattern exists, or an alternative the team rejected | an ADR in `docs/adr/` |
| a multi-step procedure that repeats | a skill: edit an existing one, or add a new one |
| mechanical steps | a script in `.claude/scripts/` |

## 4. Make the change and prove it

- Keep one source of truth: edit the existing line where there is one.
- A test must go **red** on the mistake before it goes green. Show both.
- A hook must be triggered once on purpose, and you must see it block.
- Wording: short, positive, and in English (see ADR 0004).

## 5. Land it

- **On a story branch:** commit as `chore(lesson): <what>`. `/ship` lists these commits in the PR under *Lærdom*.
- **Anywhere else:** create a branch `chore/lesson-<slug>` from `main`, commit, and open a small PR that explains the mistake.

Done when the change is committed, and you have told the user where the lesson landed and how it was proven.
