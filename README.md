# BDSP Poffin Core

This repo contains a fresh C# implementation of BDSP Poffin mechanics with a focus on
low allocations, fast lookups, and deterministic results.

## What's Here
- `BDSP.Core`: Core data structures and algorithms (berries, cooking, queries).
- `BDSP.Core.Tests`: Unit tests for data integrity and filtering/sorting.
- `BDSP.Core.CLI`: Console helpers (ANSI-colored berry output).
- `BDSP.Core.Benchmarks`: BenchmarkDotNet benchmarks for cooking and search.
- `docs/`: API reference and diagrams.

## Docs
- Berry + cooking API overview: `docs/Berries.API.md`
- Class diagrams (berries + cooking): `docs/Berries.Diagrams.md`
- Cooking rules reference: `Poffins.md`
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

## Combo Cooking Example
```csharp
ReadOnlySpan<PoffinComboBase> combos = PoffinComboTable.All;
Poffin best = default;
for (int i = 0; i < combos.Length; i++)
{
    Poffin p = PoffinCooker.Cook(combos[i], cookTimeSeconds: 40, spills: 0, burns: 0);
    if (p.Level > best.Level)
    {
        best = p;
    }
}
```

## Build / Test
```powershell
dotnet test BDSP.slnx
```

## Benchmarks
```powershell
dotnet run --project BDSP.Core.Benchmarks -c Release
```
