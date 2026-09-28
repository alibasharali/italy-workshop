#!/usr/bin/env bash
# Kjøres hver gang Codespacen starter: starter hele stacken i mock-modus i bakgrunnen.
# Logg: /tmp/apphost.log. Frontend kommer på port 3000 når migrasjoner og API er klare (1-2 min første gang).
set -euo pipefail

# Docker-in-Docker bruker noen sekunder på å bli klar etter start
for _ in $(seq 1 30); do docker info >/dev/null 2>&1 && break; sleep 2; done

if pgrep -f "TronderLeikan.AppHost" >/dev/null; then
  echo "AppHost kjører allerede."
  exit 0
fi

nohup dotnet run --project src/TronderLeikan.AppHost --launch-profile http \
  >/tmp/apphost.log 2>&1 &
echo "AppHost startet i mock-modus. Følg med: tail -f /tmp/apphost.log"
