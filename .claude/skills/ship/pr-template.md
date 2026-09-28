Closes #<N>

**Spec:** [`docs/specs/<N>-<slug>.md`](../blob/<branch>/docs/specs/<N>-<slug>.md)

## Hva er endret

<Tre til seks punkter i brukerens språk: hva kan man gjøre nå som man ikke kunne før.>

## Akseptansekriterier

| AC | Status | Bevis |
|---|---|---|
| AC1 <navn> | ✅ | [AC1.png](../blob/<branch>/docs/specs/<N>-<slug>/proof/AC1.png) |

## Test selv

```bash
gh pr checkout <PR>
dotnet run --project src/TronderLeikan.AppHost
# raskere, uten Zitadel og innlogging:
#   macOS/Linux: Auth__Mode=mock dotnet run --project src/TronderLeikan.AppHost
#   PowerShell:  $env:Auth__Mode="mock"; dotnet run --project src/TronderLeikan.AppHost
```

<Stegene for å prøve storyen, fra forsiden.>

## Lærdom

<Én linje per `chore(lesson)`-commit: hva agenten gjorde feil, og hvor lærdommen ligger nå (regel, test, hook, ADR eller skill). Legg til engangsfeil med én linje om hvorfor de ikke ble en regel. Skriv «Ingen korreksjoner» hvis det ikke var noen.>

## Kjente avveininger

<Vurderinger fra reviewen du bevisst lot stå, med én linje om hvorfor. Skriv «Ingen» hvis det ikke er noen.>

🤖 Generated with [Claude Code](https://claude.com/claude-code)
