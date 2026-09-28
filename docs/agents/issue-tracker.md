# Issue tracker: GitHub

Stories are GitHub issues in this repo (label `story`), and the board is described in `docs/agents/board.md`. Use the `gh` CLI.

**The one exception to the mattpocock skills' defaults:** a story's spec lives in the repo, in `docs/specs/<N>-<slug>.md`, and never in the issue body. The issue is the human-readable card and links to the spec. When a skill says "publish the spec to the issue tracker", write the spec file and link it from the issue.

- **Read a story:** `gh issue view <N> --comments`
- **List stories:** `gh issue list --label story --state open --json number,title,labels`
- **Comment:** `gh issue comment <N> --body "..."`
- **Find the spec for a story or PR:** `docs/specs/<N>-*.md`, where N comes from `Closes #N` in the PR or `feat/<N>-*` in the branch name

**PRs as a request surface: no.**

GitHub shares one number space across issues and PRs. Issue #N is story N from `docs/backlog.md`, and the PRs start at #27.
