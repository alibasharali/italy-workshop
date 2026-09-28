# Project board

Stories are GitHub Issues in `alibasharali/italy-workshop`, tracked on the Projects board
https://github.com/users/alibasharali/projects/2 ("Italy Workshop"). Issue number = story number in `docs/backlog.md`.
The `gh` token needs the `project` scope (`gh auth refresh -s project`).

Labels: `story` on every story, plus one theme label: `tema: poeng`, `tema: historikk`, `tema: tilgang`, `tema: opplevelse`.

## Moving a card

```bash
# item id for issue N
ITEM=$(gh project item-list 2 --owner alibasharali --format json --limit 200 \
  --jq ".items[] | select(.content.number == N) | .id")

gh project item-edit --id "$ITEM" \
  --project-id PVT_kwHOCJW5DM4Bk7C4 \
  --field-id PVTSSF_lAHOCJW5DM4Bk7C4zhjpzXc \
  --single-select-option-id <option id>
```

| Status | Option id | Set by |
|---|---|---|
| Backlog | `849fe02f` | default for new stories |
| Spec | `a3697db7` | `/story` when the branch is created |
| In progress | `6280f8d8` | once the spec is `status: approved` |
| In review | `3a8e1899` | `/ship` when the PR is opened |
| Done | `b3e7e3dc` | GitHub's built-in project workflow when the PR is merged |

To add a new story: `gh issue create --label story --label "tema: …"`, then `gh project item-add 2 --owner alibasharali --url <issue url>` and set it to Backlog.
