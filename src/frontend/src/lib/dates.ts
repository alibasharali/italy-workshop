// Datoer fra API-et er rene kalenderdatoer ("2026-03-13") uten klokkeslett.
// Vi tolker og formaterer dem i UTC, slik at tidssonen aldri flytter datoen en dag.
const langDato = new Intl.DateTimeFormat("nb-NO", {
  day: "numeric",
  month: "long",
  year: "numeric",
  timeZone: "UTC",
});

// "2026-03-13" → "13. mars 2026", null → "Dato ikke satt"
export function formatPlayedOn(playedOn: string | null): string {
  if (!playedOn) return "Dato ikke satt";
  return langDato.format(new Date(`${playedOn}T00:00:00Z`));
}

// Dagens dato i norsk tid som "YYYY-MM-DD", til forhåndsutfylling av datofelt
export function todayInNorway(): string {
  const deler = new Intl.DateTimeFormat("en-GB", {
    day: "2-digit",
    month: "2-digit",
    year: "numeric",
    timeZone: "Europe/Oslo",
  }).formatToParts(new Date());
  const del = (type: string) => deler.find((d) => d.type === type)?.value;
  return `${del("year")}-${del("month")}-${del("day")}`;
}
