BDSP.Criteria

Intent:
- Criteria model and compilation helpers for selecting and ranking poffins.
- Provides reusable filtering and comparer logic for searches.
- Includes reusable berry filter presets (see `BerryFilters`).

Usage example:
```csharp
using BDSP.Criteria;
using BDSP.Core.Berries.Data;
using BDSP.Core.Runner;

var criteria = new PoffinCriteria
{
    AllowedBerries = new[]
    {
        new BerryId(7),  // Cheri
        new BerryId(41), // Pecha
        new BerryId(37)  // Oran
    },
    MaxBerryRarity = 5,
    MinLevel = 50,
    MaxSmoothness = 20,
    MinSpicy = 20
};

var predicate = PoffinCriteriaCompiler.CompilePredicate(criteria);
var comparer = PoffinCriteriaCompiler.CompileComparer(criteria);
var pruning = PoffinCriteriaCompiler.CompilePruning(criteria);
var berryPool = PoffinCriteriaCompiler.CompileBerryPool(criteria);

var result = PoffinSearchRunner.Run(
    berryPool,
    criteria.BerriesPerPoffin,
    criteria.TopK,
    cookTimeSeconds: 40,
    errors: 0,
    amityBonus: 9,
    comparer,
    predicate,
    pruning: pruning);
```
