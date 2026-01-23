# Berry API (Current)

This document describes the current public API for berry data, filtering, and sorting.

## Types

### Flavor
Enum with five flavors plus None. Used by berries and queries.

### BerryId
Lightweight identifier. The numeric value is an index into berry tables.

### BerryBase
Minimal data used by cooking (flavors + smoothness).

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

### BerryFilterOptions
Immutable struct of optional filter bounds and masks.

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

### Base table for cooking
```csharp
ref readonly var baseBerry = ref BerryTable.GetBase(new BerryId(18)); // Ganlon
var spicy = baseBerry.Spicy;
var smoothness = baseBerry.Smoothness;
```
