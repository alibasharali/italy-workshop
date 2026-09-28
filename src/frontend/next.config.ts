import type { NextConfig } from "next";

// I GitHub Codespaces nås appen via <navn>-3000.app.github.dev. Next.js blokkerer da
// dev-ressurser og server actions fra «fremmed» origin, så vi tillater den — kun i Codespaces.
const codespaceOrigins =
  process.env.CODESPACES === "true" ? ["*.app.github.dev"] : [];

const nextConfig: NextConfig = {
  /* config options here */
  reactCompiler: true,
  allowedDevOrigins: codespaceOrigins,
  experimental: {
    serverActions: { allowedOrigins: codespaceOrigins },
  },
};

export default nextConfig;
