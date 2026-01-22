BDSP.Criteria

Intent:
- Criteria model and compilation helpers for selecting and ranking poffins.
- Provides reusable filtering and comparer logic for searches.
- Includes reusable berry filter presets (see `BerryFilters`).

Usage example:
```csharp
using BDSP.Criteria;
using BDSP.Core.Runner;

var criteria = new PoffinCriteria
{
    MinLevel = 50,
    MaxSmoothness = 20,
    MinSpicy = 20
};

var predicate = PoffinCriteriaCompiler.CompilePredicate(criteria);
var comparer = PoffinCriteriaCompiler.CompileComparer(criteria);
var pruning = PoffinCriteriaCompiler.CompilePruning(criteria);
```
