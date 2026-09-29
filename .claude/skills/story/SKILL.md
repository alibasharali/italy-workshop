---
name: story
description: Start or resume work on a backlog story (a GitHub issue) and drive it from branch to spec to PR. Use for "/story N", "work on story N", "pick a story", or "what's next on the board".
argument-hint: <issue number>
---

# Story

The entry point to the workflow in `docs/agents/workflow.md`. This skill owns the order of the steps and the **gate**. The step skills own the work.

Move cards with `.claude/scripts/board-status.sh <issue> "<status>"`.

## 1. Pick

With an issue number, read it: `gh issue view <N> --comments`. Without one, list the stories in *Backlog* (`gh issue list --label story --state open`), show them as `#N title (tema)`, and ask which one to take.

## 2. Find where the story is

Resume instead of restarting. Check, in this order, and jump to the first step that is not done:

- an open PR for `feat/<N>-*` (`gh pr list --head`) → step 7, only to report status
- a ledger `docs/specs/<N>-*/progress.md` → step 6; `implement-spec` resumes from the first slice that isn't `done`
- a spec in `docs/specs/<N>-*.md` with `status: approved` → step 6
- a spec with `status: draft` → step 5
- a branch `feat/<N>-*` → switch to it, then step 4
- nothing → step 3

## 3. Branch

1. The working tree must be clean. If it is dirty, stop and ask the user what to do with the changes.
2. `git switch main && git pull`, then `git switch -c feat/<N>-<slug>`. The slug is 2–4 ASCII kebab-case words from the issue title (`feat/26-utslagsturnering`).
3. `gh issue edit <N> --add-assignee @me`, then move the card to **Spec**.

## 4. Spec

Invoke the `write-spec` skill for issue N. Done when the spec file is committed and pushed with `status: draft`.

## 5. Gate

Stop here and hand over to the human. Show the spec path and a five-line summary of the domain decisions, then ask them to read the spec and approve it. Tell them how: reply "godkjent" in the chat, or set `status: approved` in the front matter themselves and push. Their approval is the only permission to write production code.

- If they request changes, go back into `write-spec` with their feedback.
- If they approve, set `status: approved` in the front matter and commit it as `docs(spec): godkjent #<N>`.

## 6. Implement

Invoke the `implement-spec` skill. Done when every acceptance criterion has proof.

## 7. Ship

Invoke the `ship` skill. Done when the PR is open, CI is running, and the user has the link. The human merges.
