# BDSP Poffin Core

This repo contains a fresh C# implementation of BDSP Poffin mechanics with a focus on
low allocations, fast lookups, and deterministic results.

## What's Here
- `BDSP.Core`: Core data structures and algorithms (berries, cooking, queries).
- `BDSP.Core.Tests`: Unit tests for data integrity and filtering/sorting.
- `BDSP.Core.CLI`: Console helpers (ANSI-colored berry output).
- `docs/`: API reference and diagrams.

## Berry Docs
- API overview: `docs/Berries.API.md`
- Class diagram: `docs/Berries.Diagrams.md`
- Data tables / reference notes: `BDSP.Core/BerryDocs.xml`

## Fixtures
- `BDSP.Core.Tests/Fixtures/poffin_cook_cases.json` contains golden cooking cases (README examples + custom berry sets).
- Naming convention: `README_<RecipeName>_<BerryList>_<Time>` for README-derived cases, and descriptive names for custom cases.

## Quick Example
```csharp
var options = new BerryFilterOptions(minRarity: 5, maxRarity: 9);
Span<Berry> buffer = stackalloc Berry[BerryTable.Count];
var count = BerryQuery.Execute(BerryTable.All, buffer, options, default);
var results = buffer[..count];
```

## Build / Test
```powershell
dotnet test BDSP.slnx
```
