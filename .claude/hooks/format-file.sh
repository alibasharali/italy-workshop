#!/usr/bin/env bash
# PostToolUse (Edit|Write): formaterer filen agenten nettopp endret, så formatering aldri blir et review-tema.
# C#: dotnet format whitespace (samme regler som CI via .editorconfig). Frontend: eslint --fix.
file=$(jq -r '.tool_response.filePath // .tool_input.file_path // ""')
[ -f "$file" ] || exit 0
root="${CLAUDE_PROJECT_DIR:-$(git rev-parse --show-toplevel)}"

case "$file" in
  */Migrations/*) ;;  # generert kode
  *.cs)
    dotnet format whitespace "$root" --folder --include "${file#"$root"/}" >/dev/null 2>&1 ;;
  "$root"/src/frontend/*.ts|"$root"/src/frontend/*.tsx|"$root"/src/frontend/*.mjs)
    (cd "$root/src/frontend" && ./node_modules/.bin/eslint --fix "$file" >/dev/null 2>&1) ;;
esac
exit 0
