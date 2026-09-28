import { auth } from "@/lib/auth";

// AUTH_MODE=mock settes av AppHost i PR-miljøer (Codespaces), der Zitadel ikke kjører.
// Da later appen som en admin er logget inn, så den som tester slipper innlogging.
export const isMockAuth = process.env.AUTH_MODE === "mock";

// Aldri i et produksjonsbygg: mock er bare for dev-serveren.
if (isMockAuth && process.env.NODE_ENV === "production") {
  throw new Error("AUTH_MODE=mock er ikke tillatt i produksjon.");
}

const mockSession = {
  user: { name: "Demo-admin (PR-miljø)", email: "demo-admin@leikan.local" },
};

// Henter innlogget bruker — det eneste stedet appen spør om sesjon, så mock bare trengs her.
export async function getSession(headers: Headers) {
  if (isMockAuth) return mockSession;
  return auth.api.getSession({ headers });
}
