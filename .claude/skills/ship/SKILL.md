---
name: ship
description: Ship a finished story branch by reviewing it in a fresh context, pushing, and opening the PR that links the issue, spec and proof. Use when a story's implementation and proof are done, or the user says "ship", "open a PR" or "create the PR".
---

# Ship

## 1. Preconditions

- You are on a story branch, never on `main`.
- The working tree is clean.
- `docs/specs/<N>-<slug>/proof/README.md` covers every AC in the spec. If it doesn't, go back to `implement-spec` step 3.

## 2. Checks

Run the full checks from `implement-spec` step 2. Any red result sends you back to fixing, never forward to shipping.

## 3. Review in fresh context

Run the `code-review` skill with fixed point `origin/main` and the spec file as the spec source. Its subagents review the diff without this session's history.

- **Hard findings:** a broken documented standard, a missing or wrong AC. Fix them, commit, and rerun the checks.
- **Judgement calls** (smells): fix them, or list them in the PR under *Kjente avveininger* with one line each on why you left them.

Done when no hard finding is open.

## 4. Open the PR

1. `git push -u origin HEAD`
2. Fill in [pr-template.md](pr-template.md), in Norwegian because humans read it, and create the PR: `gh pr create --title "feat: <issue title> (#<N>)" --body-file <file>`.
   - `Closes #<N>` moves the card to *In review* through the board's automation.
   - Include the Codespaces section only if `.devcontainer/` exists.
3. Check the card. If it is not in *In review* after a minute, run `.claude/scripts/board-status.sh <N> "In review"`.

## 5. Hand over

Watch CI in the background with `gh pr checks <PR> --watch`. When it finishes, give the user:

- the PR link
- the CI result
- the verdict from the Claude review comment, once it has posted

The human tests and squash-merges. You never merge.
