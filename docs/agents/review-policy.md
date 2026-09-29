# Review policy

Every reviewer follows this file: the `slice-reviewer` after each slice, the whole-branch review in `/ship`, and the Claude review in CI. Every finding gets exactly one of two classes.

## Auto-fix

The fix is **unambiguous** and **changes no behaviour that a spec describes**. The reviewer fixes it itself.

- docs, comments, READMEs, skills or templates that are wrong, missing a variant (for example PowerShell next to bash), have a broken link, or disagree with the code
- a convention from `AGENTS.md` that has one correct form: a handler that isn't `sealed`, a `TestAppDbContext` mapping missing for a new entity, `auth.api.getSession` used instead of `getSession()`
- an obvious guard with one reasonable fix, where the spec says nothing either way, such as a null check before a lookup
- a test name or test that contradicts the spec's own wording, if the code is right

Before committing, build and the fast tests must be green: Domain, Application and Architecture, plus `npm run lint` if frontend files changed. If they aren't, the finding becomes *Needs a human*. Commit as `fix(review): <what>`, one commit per finding.

## Needs a human

Everything else. In particular:

- domain rules, points and ranking: the core domain
- an acceptance criterion that is missing, partial or wrong, or anything built outside the spec's scope
- security design: auth, secrets, input that reaches the database
- any finding with two or more reasonable fixes, or where you are unsure which class it belongs to

Each one becomes an inline comment that leaves an unresolved thread, so the merge stays blocked until a human answers it. Say what is wrong and give your recommended fix.

## Never

- Change a test to make it pass. A failing test is a *Needs a human* finding.
- Edit the spec, `.github/workflows/`, `.claude/settings.json` or `.claude/hooks/`.
