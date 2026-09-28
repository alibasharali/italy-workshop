#!/usr/bin/env bash
# Kjøres én gang når Codespacen opprettes: henter avhengigheter og bygger, så oppstarten går raskt.
set -euo pipefail
dotnet restore TronderLeikan.slnx
dotnet tool restore
(cd src/frontend && npm ci)
dotnet build src/TronderLeikan.AppHost --no-restore -v q
