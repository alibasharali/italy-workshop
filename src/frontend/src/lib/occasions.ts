// Forslag til anledning: de ulike anledningene som alt er brukt i turneringen,
// i den rekkefølgen de først dukker opp i den kronologiske spillisten
export function occasionSuggestions(
  games: { occasion: string | null }[]
): string[] {
  return [
    ...new Set(
      games
        .map((g) => g.occasion)
        .filter((o): o is string => o !== null)
    ),
  ];
}
