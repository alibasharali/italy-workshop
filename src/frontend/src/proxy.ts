import { NextRequest, NextResponse } from "next/server";
import { getSession } from "@/lib/session";

// Beskytter alle /admin-ruter ved å kreve aktiv sesjon.
// Ikke-autentiserte brukere omdirigeres til /login.
export async function proxy(request: NextRequest) {
  if (request.nextUrl.pathname.startsWith("/admin")) {
    const session = await getSession(request.headers);

    if (!session) {
      return NextResponse.redirect(new URL("/login", request.url));
    }
  }

  return NextResponse.next();
}

export const config = {
  matcher: ["/admin/:path*"],
};
