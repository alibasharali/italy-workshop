# 0003 – Login is enforced only in the frontend, for now

**Status:** accepted, expected to be superseded by story #18 (recorded 2026-09-28)

## Context

The system was built for internal use behind a login-protected admin UI. Zitadel issues identities, and the frontend uses better-auth with stateless cookie sessions.

## Decision

- The frontend guards `/admin` twice: in `proxy.ts` and in the admin layout. It gets the session only through `getSession()` in `src/lib/session.ts`, which also provides `Auth__Mode=mock` for local testing without Zitadel.
- **The API has no authentication.** Anyone who can reach it can call the endpoints that change data.

## Consequences

- Don't assume the API knows who the user is. No handler can read the current user today (`ICurrentUser` exists, but nothing implements it).
- Story #18 ("Logg inn før du endrer") replaces this decision. It needs token validation in the API, a way to send the token from the server actions, and a new ADR that supersedes this one.
