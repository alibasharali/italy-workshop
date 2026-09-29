---
name: implement-spec
description: Implement an approved story spec slice by slice through fresh subagents, review each slice, then prove every acceptance criterion against the running app. Use when a spec in docs/specs/ has status approved, or the user asks to implement or continue implementing a story.
---

# Implement spec

You are the **orchestrator**. You write no production code yourself. A fresh `slice-implementer` builds each slice, a `slice-reviewer` checks it, and a `prover` demonstrates the result. You hold the spec, the ledger and the developer's decisions; you are the only one who talks to the developer, and **the only one who commits**. Workers leave their changes in the working tree and report the file list.

Why: one story in one context window degrades toward the end (#11 ran spec, five slices, proof, review and retro in one session). Fresh workers get a precise brief and nothing else; you stay small enough to think.

## 0. Preconditions

- You are on the story branch, `feat/<N>-*`.
- The spec's front matter says `status: approved`. Otherwise stop and hand back to the `write-spec` gate, because approval is the human's call.
- Ledger: `docs/specs/<N>-<slug>/progress.md`. If it exists, resume from it: the first slice not `done` is where you start. If it doesn't, create it from [ledger-template.md](ledger-template.md) with one row per slice, `todo`.

Move the card to **In progress** with `.claude/scripts/board-status.sh <N> "In progress"`.

## 1. Each slice, in spec order, one at a time

Never run two implementers at once: slices build on each other, and there is one AppHost.

1. `BASE=$(git rev-parse HEAD)`. Set the slice to `running` in the ledger.
2. **Write the brief** and dispatch `slice-implementer` (foreground). A brief has four parts, and the worker sees nothing else, so put everything it needs in it:
   - **Objective:** the slice text verbatim, every AC it satisfies verbatim, and the domain decisions those ACs rest on (verbatim, by number).
   - **Sources:** the spec path; the *Test seams* rows for these ACs with their prior art; the *Interfaces* column of the ledger for the slices already done; the *Rulings* that touch this slice.
   - **Boundaries:** the spec's *Out of scope*; the other slices' ACs by number ("not yours"); anything the reviewer flagged as off-limits earlier.
   - **Output:** the report shape from the agent's definition. Tell it not to commit; you do.
3. **On BLOCKED:** don't answer for the developer. Ask them (AskUserQuestion, with the worker's options and recommendation). Record the ruling in the ledger. If the ruling changes the spec, commit `docs(spec): …` first. Then resume the **same** implementer with `SendMessage`, giving the ruling verbatim, so it keeps its context.
4. **On DONE:** check `git status` against the reported *Changed files*: nothing outside the slice, no stray scratch files. Commit as `feat(<area>): S<n> <slice name> (#<N>)`, one slice, one commit. Record the range `BASE..HEAD` and the *Interfaces* in the ledger. Dispatch `slice-reviewer` with the spec path, the slice, its ACs, the seams and `BASE..HEAD`.
5. **Route the review:**
   - `CLEAN` → step 6.
   - `FIX` → resume the same implementer with the findings verbatim. When it reports DONE, commit `fix(review): <what>` and re-review only the new range. Two rounds at most; whatever is left becomes a question for the developer.
   - `HUMAN` → ask the developer with the reviewer's options. Record the ruling. If it needs code, it goes to the implementer as a fix round.
   - Judgement calls the developer leaves in place go to the ledger, and later to the PR's *Kjente avveininger*.
6. Set the slice to `done` with its review outcome, and commit the ledger: `docs(ledger): S<n> ferdig (#<N>)`.

If the spec turns out to be wrong or incomplete at any point, stop and ask the developer. The spec is corrected first, as its own commit, and the code follows. The spec and the code must never disagree.

## 2. Full checks

You run these yourself. Everything must pass together:

- `dotnet format TronderLeikan.slnx --verify-no-changes`
- `dotnet build`
- `dotnet test`, the whole suite including the Docker-based API and Infrastructure tests
- if anything under `src/frontend` changed: `npm run lint` and `npm run build`

A red result goes back to a `slice-implementer` as a fix brief, never to you.

## 3. Prove

Green tests are not proof. Start the stack if it isn't running: `dotnet run --project src/TronderLeikan.AppHost` in the background, from the main clone. Then dispatch `prover` with the spec path, the proof folder and the *Proof plan* table. Set *Proof* to `running` in the ledger.

- `ALL PASS` → read the README it wrote, then commit `test(proof): bevis for #<N>`.
- `FAILED` → each failed AC is a bug. Brief a `slice-implementer` with the AC, the evidence file and the spec line, let the reviewer check the fix, then re-dispatch `prover` for the failed ACs only.
- `BLOCKED` → fix the environment (start or rebuild the stack) and re-dispatch.

Set *Proof* to `done` in the ledger and commit it. Done when every AC has an evidence file. Next is the `ship` skill.
