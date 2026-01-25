# BDSP Poffin Core

This repo contains a fresh C# implementation of BDSP Poffin mechanics with a focus on
low allocations, fast lookups, and deterministic results.

## What's Here
- `BDSP.Core`: Core data structures and algorithms (berries, cooking, queries).
- `BDSP.Core.Tests`: Unit tests for data integrity and filtering/sorting.
- `BDSP.Core.CLI`: Console helpers (ANSI-colored berry output).
- `BDSP.Core.Benchmarks`: BenchmarkDotNet benchmarks for cooking and search.
- `docs/`: API reference and diagrams.

## Poffin Structure (BDSP.Core)
- `BDSP.Core/Poffins` mirrors the berry layout.
- `BDSP.Core/Poffins/Cooking` (combo tables + cooker).
- `BDSP.Core/Poffins/Filters` (poffin filters).
- `BDSP.Core/Poffins/Search` (unified search API + TopK).
- `PoffinComboBuilder` lives under `Cooking` for subset precompute.

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

## Unified Poffin Search Example
```csharp
var berryFilter = new BerryFilterOptions(minRarity: 3, maxRarity: 7);
var searchOptions = new PoffinSearchOptions(choose: 3, cookTimeSeconds: 40, useParallel: true);
var poffinFilter = new PoffinFilterOptions(minLevel: 30, maxSmoothness: 20);
var results = PoffinSearch.Run(berryFilter, searchOptions, topK: 100, poffinFilter);
```

## Cooking Rules (Summary)
1. Add together the respective flavors of all berries used.
2. For each flavor total, subtract the total of that flavor's weakening flavor (spicy <- dry, dry <- sweet, sweet <- bitter, bitter <- sour, sour <- spicy).
3. For each flavor that is negative, subtract 1 from all five flavors.
4. Multiply all flavors by `60 / cookTimeSeconds`, then subtract `burns + spills`.
   - Implementation note: the core library uses integer truncation.
5. Set any negative flavors to 0.
6. Cap flavor values to the generation limit (Gen IV = 99, Gen VIII = 100). The core library clamps to 100.

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
