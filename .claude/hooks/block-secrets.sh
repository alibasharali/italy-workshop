#!/usr/bin/env bash
# PreToolUse (Bash): stopper kommandoer som leser hemmeligheter.
# Read-deny i settings.json dekker Read/Grep, men ikke `cat`/`grep -r` i Bash — derfor denne.
# Hemmelighetene: zitadel-bootstrap/ (admin-PAT, OIDC-klient), user secrets, .env-filer.
cmd=$(jq -r '.tool_input.command // ""')

pattern='zitadel-bootstrap|usersecrets|UserSecrets|user-secrets[[:space:]]+list|admin\.pat|(^|[[:space:]/"'"'"'=])\.env(\.[A-Za-z0-9_-]+)?([[:space:]"'"'"';|&)]|$)'

if printf '%s' "$cmd" | grep -Eq "$pattern"; then
  jq -n '{
    hookSpecificOutput: {
      hookEventName: "PreToolUse",
      permissionDecision: "deny",
      permissionDecisionReason: "Blokkert av .claude/hooks/block-secrets.sh: kommandoen berører hemmeligheter (zitadel-bootstrap/, user secrets eller .env). Agenten skal aldri lese disse. Trenger du en verdi, be brukeren om den."
    }
  }'
fi
exit 0
