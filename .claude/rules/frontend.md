---
paths:
  - "src/frontend/**"
---

# Frontend conventions

Next.js 16 differs from what you know. `src/frontend/AGENTS.md` says to read the docs in `node_modules/next/dist/docs/` first.

## Data

- Pages are **server components**. Add `"use client"` only for a component that needs browser interactivity, as `LogoutButton` and `login` do.
- A page reads from the API in a small local `async` function:
  - `fetch(\`${process.env.API_BASE_URL ?? "http://localhost:5000"}/api/v1/...\`, { cache: "no-store" })`
  - it returns `null` or `[]` when `!res.ok` or on an exception, and the page renders `notFound()` or an empty state
- Response types are local `type XResponse` definitions that mirror the C# response record of the same name.
- **Every mutation is a server action** in an `actions.ts` next to the page, with `"use server"`:
  1. `fetch` the API
  2. `if (!res.ok) throw new Error("<Norwegian message>")`. A few older actions skip this check. Write new ones with it.
  3. `revalidatePath(...)` for every page that shows the data

## Auth

- The session comes from `getSession()` in `src/lib/session.ts`, and only from there. That's what makes `Auth__Mode=mock` work.
- `/admin` is protected twice: in `src/proxy.ts` and in `(admin)/layout.tsx`. A new admin page goes under `(admin)/admin/` and gets both.

## UI

- All text the user sees is Norwegian.
- Style with Tailwind utility classes inline. Pages sit in `mx-auto max-w-5xl px-4`, using the gray palette. A new UI or CSS library needs an ADR first.
- A route segment that loads data has `loading.tsx` and `error.tsx`, as in `(public)/`.
- Use real `<button>` and `<label>` elements, and put an `aria-label` on navigation, as in the admin layout.
- `npm run lint` allows zero warnings.
