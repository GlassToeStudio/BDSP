# Berry API (Current)

This document describes the current public API for berry data, filtering, and sorting.

## Types

### Flavor
Enum with five flavors plus None. Used by berries and queries.

### BerryId
Lightweight identifier. The numeric value is an index into berry tables.

### BerryBase
Minimal data used by cooking (flavors + smoothness), plus precomputed weakened values.

### Berry
Full metadata used for filtering, sorting, pruning, and display.

### BerryTable
Canonical data tables.
- `All`: `ReadOnlySpan<Berry>`
- `BaseAll`: `ReadOnlySpan<BerryBase>`
- `Get(BerryId)`
- `GetBase(BerryId)`

### BerryNames
Name lookup by `BerryId`.

### BerryAnsiFormatter (BDSP.Tools)
ANSI-colored formatter for terminal output.

### ANSI Formatter Example
```csharp
using BDSP.Core.Berries;
using BDSP.Core.CLI;

ref readonly var berry = ref BerryTable.Get(new BerryId(18));
Console.WriteLine(BerryAnsiFormatter.Format(berry));
```

### BerryFilterOptions
Immutable struct of optional filter bounds and masks.

Notes:
- All bounds are inclusive.
- Zero is a valid bound; use `default` or `BerryFilterOptions.None` for "no filters".
- Accepted ranges (berries):
  - Flavors: 0-40
  - Smoothness: 20-60
  - Rarity: 1-15
  - Main/Secondary flavor values: 0-40
  - NumFlavors: 1-5

### BerryQuery
Filter + sort entry point:
- `Execute(ReadOnlySpan<Berry> source, Span<Berry> destination, in BerryFilterOptions options, ReadOnlySpan<BerrySortKey> sortKeys)`

### BerrySortField / BerrySortKey / BerrySorter
Sort fields and multi-key sorting utility.

## Usage Examples

### Filter only (no sorting)
```csharp
var options = new BerryFilterOptions(minRarity: 5, maxRarity: 9);
Span<Berry> buffer = stackalloc Berry[BerryTable.Count];
var count = BerryQuery.Execute(BerryTable.All, buffer, options, default);
var results = buffer[..count];
```

### Filter + multi-key sort
```csharp
var options = new BerryFilterOptions(minSmoothness: 20, maxSmoothness: 40);
var sortKeys = new[]
{
    new BerrySortKey(BerrySortField.Rarity),
    new BerrySortKey(BerrySortField.Name)
};
Span<Berry> buffer = stackalloc Berry[BerryTable.Count];
var count = BerryQuery.Execute(BerryTable.All, buffer, options, sortKeys);
var results = buffer[..count];
```

### Sort only
```csharp
var sortKeys = new[]
{
    new BerrySortKey(BerrySortField.MainFlavorValue, descending: true),
    new BerrySortKey(BerrySortField.Smoothness)
};
Span<Berry> buffer = stackalloc Berry[BerryTable.Count];
var count = BerryQuery.Execute(BerryTable.All, buffer, default, sortKeys);
var results = buffer[..count];
```

### Base table for cooking
```csharp
ref readonly var baseBerry = ref BerryTable.GetBase(new BerryId(18)); // Ganlon
var spicy = baseBerry.Spicy;
var smoothness = baseBerry.Smoothness;
var weakDry = baseBerry.WeakDry;
```

## Poffin / Cooking API (Current)

### Poffin
Cooked result (flavors + smoothness), with derived main/secondary flavor and levels.

### PoffinCooker
Cooking rules implementation.
- `Cook(ReadOnlySpan<BerryBase> berries, int cookTimeSeconds, int spills, int burns, int amityBonus = 9)`
- `Cook(PoffinComboBase combo, int cookTimeSeconds, int spills, int burns, int amityBonus = 9)`

### PoffinComboBase / PoffinComboTable
Precomputed sums for all unique 2-4 berry combinations (full-berry fast path).
- `PoffinComboTable.All`: `ReadOnlySpan<PoffinComboBase>`
- `PoffinComboTable.Count`: total combo count

### PoffinComboBuilder
Subset precompute helper for advanced workflows.
- `PoffinComboBuilder.CreateFromSubset(ReadOnlySpan<BerryId>)`
### PoffinComboEnumerator
Non-allocating enumeration of 2-4 berry combinations from an arbitrary subset.
- `PoffinComboEnumerator.ForEach(source, choose, action)`

### PoffinSearch (Unified Entry Point)
Same call shape whether the user filtered berries or not. The engine automatically
switches between precomputed combo tables and subset enumeration, and it decides
whether to run in parallel based on subset size and choose count.

Namespace:
- `BDSP.Core.Poffins.Search` (PoffinSearch, PoffinSearchOptions, TopK)
- `BDSP.Core.Poffins.Filters` (PoffinFilterOptions)

Automatic strategy (current thresholds):
- All berries: use PoffinComboTable (precomputed).
- Subsets: no subset precompute in PoffinSearch (we do not track reuse yet).
- Parallel threshold for subsets (when UseParallel = true): nCk >= 500 (based on combo count).

```csharp
var berryFilter = new BerryFilterOptions(minRarity: 3, maxRarity: 7);
var options = new PoffinSearchOptions(choose: 3, cookTimeSeconds: 40, useParallel: true);
var poffinFilter = new PoffinFilterOptions(minLevel: 30, maxSmoothness: 20);
var results = PoffinSearch.Run(berryFilter, options, topK: 100, poffinFilter);
```

Full-berry search (fast path):
```csharp
var options = new PoffinSearchOptions(
    choose: 4,
    cookTimeSeconds: 40,
    useParallel: true);

var results = PoffinSearch.Run(default, options, topK: 200);
```

Subset-driven search (UI path):
```csharp
var berryFilter = new BerryFilterOptions(
    minRarity: 1,
    maxRarity: 5,
    requireMainFlavor: true,
    mainFlavor: Flavor.Spicy);

var options = new PoffinSearchOptions(
    choose: 2,
    cookTimeSeconds: 60,
    useParallel: true);

var results = PoffinSearch.Run(berryFilter, options, topK: 50);
```

Score tuning (favor main flavor, penalize smoothness):
```csharp
var score = new PoffinScoreOptions(
    levelWeight: 1000,
    totalFlavorWeight: 2,
    smoothnessPenalty: 5,
    preferredMainFlavor: Flavor.Dry,
    preferredMainFlavorBonus: 250);

var options = new PoffinSearchOptions(
    choose: 3,
    cookTimeSeconds: 40,
    useParallel: true,
    scoreOptions: score);

var results = PoffinSearch.Run(default, options, topK: 100);
```

Key types:
- `PoffinSearchOptions`
- `PoffinFilterOptions`
- `PoffinResult`
- `TopK<T>`

### PoffinFilterOptions
Immutable struct of optional filter bounds.

Notes:
- All bounds are inclusive.
- Zero is a valid bound; use `default` or `PoffinFilterOptions.None` for "no filters".
- Accepted ranges (poffins):
  - Flavor values: 0-255
  - Smoothness: 0-255
  - Level: 0-255
  - NumFlavors: 0-5

### Benchmarks
Benchmark project comparing combo-base cooking vs span-based cooking:

```powershell
dotnet run --project BDSP.Core.Benchmarks -c Release
```
Subset crossover benchmarks:
```powershell
dotnet run --project BDSP.Core.Benchmarks -c Release -- --filter *SubsetCookingBenchmarks*
```

### Precomputed combo cooking (full table)
```csharp
ReadOnlySpan<PoffinComboBase> combos = PoffinComboTable.All;
for (int i = 0; i < combos.Length; i++)
{
    Poffin poffin = PoffinCooker.Cook(combos[i], cookTimeSeconds: 40, spills: 0, burns: 0);
}
```

### Subset combo enumeration (UI)
```csharp
ReadOnlySpan<BerryId> selected = stackalloc BerryId[] { new BerryId(5), new BerryId(34), new BerryId(62) };
PoffinComboEnumerator.ForEach(selected, 2, combo =>
{
    Span<BerryBase> bases = stackalloc BerryBase[2];
    bases[0] = BerryTable.GetBase(combo[0]);
    bases[1] = BerryTable.GetBase(combo[1]);
    Poffin poffin = PoffinCooker.Cook(bases, cookTimeSeconds: 60, spills: 0, burns: 0);
});
```

## Capabilities Checklist
- Fixed berry data table (65 berries) with stable IDs.
- Base cooking data (`BerryBase`) for low-allocation hot paths.
- Full metadata (`Berry`) for filtering, sorting, and pruning.
- O(1) name lookup by `BerryId`.
- Range filtering on all flavor values, smoothness, rarity, derived values, and num flavors.
- Flavor mask include/exclude filtering.
- Main/secondary flavor filtering.
- Multi-key sorting on all berry attributes and name.
- Comprehensive unit tests for table integrity, derived values, name mapping, filtering, and sorting.
- Precomputed weakened flavor values on `BerryBase` for faster cooking.
- Precomputed 2-4 berry combo bases (`PoffinComboTable`) for high-volume cooking.
- Non-allocating subset combo enumeration (`PoffinComboEnumerator`) for UI workflows.



