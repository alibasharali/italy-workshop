# Bevis for #11 Når ble det spilt?

Kjørt 28.09.2026 mot den lokale Aspire-stacken (API på `http://localhost:5024`, frontend på `http://localhost:52930`), etter at migreringene `AddGamePlayedOn` og `AddGameOccasion` var kjørt på den eksisterende lokale databasen.
API-filene inneholder hele forespørselen (`>`) og svaret (`<`).

| AC | Bevis | Hva det viser |
|---|---|---|
| AC1 | [AC1.txt](AC1.txt) | `POST /games` med `playedOn` 2026-03-13 gir 201, og `GET` returnerer `"playedOn": "2026-03-13"`. |
| AC2 | [AC2.txt](AC2.txt) | `POST /games` uten `playedOn` gir 400 med `title: Game.PlayedOnMissing`. |
| AC3 | [AC3.txt](AC3.txt) | Et spill med dato 2027-01-01 opprettes (201) og fullføres (204), og `isDone` blir `true`. |
| AC4 | [AC4.txt](AC4.txt) | `"  Fredagspils uke 11 "` lagres som `"Fredagspils uke 11"`, og `"   "` lagres som `null`. |
| AC5 | [AC5.txt](AC5.txt) | 201 tegn gir 400 `Game.OccasionTooLong` ved både POST og PUT. 200 tegn gir 201. |
| AC6 | [AC6.txt](AC6.txt) | `PUT /games/{id}` gir 204, og `GET` viser nytt navn, beskrivelse, dato og anledning. |
| AC7 | [AC7.txt](AC7.txt) | Et seedet spill uten dato: PUT uten dato gir 400 og spillet er uendret. PUT med 2026-02-27 gir 204 og datoen er satt. |
| AC8 | [AC8.txt](AC8.txt), [AC10-liste.png](AC10-liste.png), [AC13.png](AC13.png) | API-et returnerer Mario Kart, Boccia, Dart. Admin- og offentlig liste viser samme kronologiske rekkefølge. |
| AC9 | [AC1.txt](AC1.txt), [AC8.txt](AC8.txt) | Både detalj og sammendrag har `playedOn` og `occasion`. |
| AC10 | [AC10-skjema.png](AC10-skjema.png), [AC10-liste.png](AC10-liste.png) | Datofeltet er forhåndsutfylt med dagens dato (28.09.2026). «Dart» ble opprettet fra skjemaet med 13. mars og «Fredagspils uke 11», og står i lista som `13. mars 2026 · Fredagspils uke 11`. |
| AC11 | [AC11.txt](AC11.txt) | Både opprett- og redigeringsskjemaet foreslår nøyaktig «Fredagspils uke 11» og «Fredagspils uke 12», og ikke anledningen fra en annen turnering. |
| AC12 | [AC12-forhandsutfylt.png](AC12-forhandsutfylt.png), [AC12-etter-lagring.png](AC12-etter-lagring.png), [AC12-uten-dato.png](AC12-uten-dato.png) | Skjemaet er forhåndsutfylt. Etter lagring viser siden 20. mars 2026. Et spill uten dato har tomt datofelt, og nettleseren nekter å sende skjemaet. |
| AC13 | [AC13.png](AC13.png), [AC13-uten-dato.png](AC13-uten-dato.png) | Den offentlige spillisten viser dato, anledning, navn og status med lenke til spillet, og «Dato ikke satt» sist for eldre spill. |
| AC14 | [AC14-offentlig.png](AC14-offentlig.png), [AC12-etter-lagring.png](AC12-etter-lagring.png) | Den offentlige og den administrative spillsiden viser dato og anledning. |
| AC15 | [AC15.txt](AC15.txt) | Scoreboardet er identisk før og etter at dato og anledning endres på et fullført spill. |
| AC16 | [AC16.txt](AC16.txt), [AC13-uten-dato.png](AC13-uten-dato.png) | Alle seedede spill fra før migreringen lastes med `playedOn: null` og `occasion: null`, og står sist. |

## Merknader

- Det native forslagsvinduet til `<datalist>` kommer ikke med på skjermbilder, så AC11 er bevist med innholdet i DOM-en.
- I AC10 ble skjemaet sendt med `requestSubmit` på knappen. De første Playwright-klikkene kom før siden var hydrert i dev-modus og ble ikke registrert. Redigeringsskjemaet i AC12 ble lagret med et vanlig klikk.
