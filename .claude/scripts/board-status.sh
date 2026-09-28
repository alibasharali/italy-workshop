#!/usr/bin/env bash
# Flytter en story på prosjekttavla. Bruk: board-status.sh <issue> "<Backlog|Spec|In progress|In review|Done>"
# ID-ene står i docs/agents/board.md.
set -euo pipefail
issue="${1:?issue-nummer mangler}"
status="${2:?status mangler}"

case "$status" in
  Backlog) option=849fe02f ;;
  Spec) option=a3697db7 ;;
  "In progress") option=6280f8d8 ;;
  "In review") option=3a8e1899 ;;
  Done) option=b3e7e3dc ;;
  *) echo "Ukjent status: $status" >&2; exit 1 ;;
esac

item=$(gh project item-list 2 --owner alibasharali --format json --limit 200 \
  --jq ".items[] | select(.content.number == $issue) | .id")
if [ -z "$item" ]; then
  item=$(gh project item-add 2 --owner alibasharali \
    --url "https://github.com/alibasharali/italy-workshop/issues/$issue" --format json --jq .id)
fi

gh project item-edit --id "$item" --project-id PVT_kwHOCJW5DM4Bk7C4 \
  --field-id PVTSSF_lAHOCJW5DM4Bk7C4zhjpzXc --single-select-option-id "$option" >/dev/null
echo "#$issue → $status"
