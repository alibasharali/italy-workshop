#!/usr/bin/env bash
# Stop: før agenten får si at den er ferdig, må bygg og raske tester være grønne.
# Kjører bare når arbeidstreet har endringer som ikke er sjekket før (hash-cache),
# så vanlige samtaletur ikke koster et bygg. Docker-testene (Api/Infrastructure) tar CI.
input=$(cat)
root="${CLAUDE_PROJECT_DIR:-$(git rev-parse --show-toplevel)}"
cd "$root" || exit 0

changed=$(git status --porcelain --untracked-files=all)
[ -z "$changed" ] && exit 0

hash=$( { git diff HEAD; git ls-files --others --exclude-standard | xargs -I{} sh -c 'echo {}; cat "{}"' 2>/dev/null; } | shasum | cut -d' ' -f1)
cache="${TMPDIR:-/tmp}/claude-stop-checks-$(printf '%s' "$root" | shasum | cut -c1-12)"
[ "$(cat "$cache" 2>/dev/null)" = "$hash" ] && exit 0

log=$(mktemp)
fail=""

if printf '%s\n' "$changed" | grep -Eq '\.(cs|csproj|slnx|props)$'; then
  { dotnet build TronderLeikan.slnx -v q -nologo \
    && dotnet test tests/TronderLeikan.Domain.Tests --no-build -v q -nologo \
    && dotnet test tests/TronderLeikan.Application.Tests --no-build -v q -nologo \
    && dotnet test tests/TronderLeikan.Architecture.Tests --no-build -v q -nologo; } >"$log" 2>&1 \
    || fail="Backend: bygg eller tester (Domain, Application, Architecture) feiler."
fi

if [ -z "$fail" ] && printf '%s\n' "$changed" | grep -Eq 'src/frontend/.*\.(ts|tsx|mjs)$'; then
  (cd src/frontend && npm run lint --silent) >"$log" 2>&1 || fail="Frontend: eslint feiler."
fi

if [ -z "$fail" ]; then
  echo "$hash" >"$cache"
  rm -f "$log"
  exit 0
fi

details=$(grep -E 'error|Failed|✖' "$log" | head -20)
rm -f "$log"

# Andre gang på rad: ikke lås agenten i en løkke — si fra til brukeren i stedet.
if [ "$(printf '%s' "$input" | jq -r '.stop_hook_active // false')" = "true" ]; then
  jq -n --arg m "$fail Sjekkene er fortsatt røde etter ett forsøk på å fikse. $details" '{systemMessage: $m}'
  exit 0
fi

jq -n --arg r "$fail Fiks dette før du sier at du er ferdig. Ikke endre testene for å få dem grønne.
$details" '{decision: "block", reason: $r}'
exit 0
