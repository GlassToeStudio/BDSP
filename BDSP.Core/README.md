BDSP.Core

Intent:
- Core algorithms and data for berries, poffins, and contest stats.
- High-performance filtering and search utilities used by other projects.
- No UI or IO; pure library code with minimal allocations.


Usage Examples:

Enumerate berries and print names:
```csharp
using BDSP.Core.Berries.Data;
using BDSP.Core.Berries.Extensions;

for (ushort i = 0; i < (ushort)BerryTable.Count; i++)
{
    var id = new BerryId(i);
    var berry = BerryTable.Get(id);
    Console.WriteLine($"{id.Value}: {berry.GetName()} (Smoothness {berry.Smoothness})");
}
```

Filter berries into a stack buffer:
```csharp
using BDSP.Core.Berries.Data;
using BDSP.Core.Berries.Filters;
using BDSP.Criteria;

Span<BerryId> poolBuf = stackalloc BerryId[BerryTable.Count];
var filter = BerryFilters.Tight(maxSmoothness: 25, maxRarity: 3, minMainFlavorValue: 10);
int count = BerryQuery.Filter(in filter, poolBuf);
var berryPool = poolBuf[..count];
```

Filter by allow-list of IDs:
```csharp
using BDSP.Core.Berries.Data;
using BDSP.Core.Berries.Filters;

var allowed = new BerryId[] { new(0), new(5), new(34) };
var filter = BerryFilterOptions.WithAllowedIds(allowed);

Span<BerryId> poolBuf = stackalloc BerryId[BerryTable.Count];
int count = BerryQuery.Filter(in filter, poolBuf);
```

Cook a poffin from Berry IDs (duplicate check enabled):
```csharp
using BDSP.Core.Berries.Data;
using BDSP.Core.Poffins;

Span<BerryId> ids = stackalloc BerryId[3]
{
    new BerryId(5),
    new BerryId(34),
    new BerryId(64),
};

var poffin = PoffinCooker.Cook(
    ids,
    cookTimeSeconds: 40,
    errors: 0,
    amityBonus: 9);
```
Note: the core library truncates time scaling for speed, and foul poffins are deterministic.

Cook a poffin from unique Berry instances (skip duplicate check):
```csharp
using BDSP.Core.Berries.Data;
using BDSP.Core.Poffins;

Span<Berry> berries = stackalloc Berry[2];
berries[0] = BerryTable.Get(new BerryId(5));
berries[1] = BerryTable.Get(new BerryId(34));

var poffin = PoffinCooker.CookFromBerriesUnique(
    berries,
    cookTimeSeconds: 40,
    errors: 0,
    amityBonus: 9);
```

Convert a poffin to contest stats:
```csharp
using BDSP.Core.Contest;
using BDSP.Core.Poffins;

ContestStats stats = ContestStatsCalculator.FromPoffin(poffin);
Console.WriteLine($"{stats.Coolness}/{stats.Beauty}/{stats.Cuteness}/{stats.Cleverness}/{stats.Toughness}");
```

Run a full poffin search (parallel):
```csharp
using BDSP.Core.Berries.Data;
using BDSP.Core.Runner;
using BDSP.Core.Selection;

Span<BerryId> poolBuf = stackalloc BerryId[BerryTable.Count];
for (ushort i = 0; i < (ushort)BerryTable.Count; i++)
    poolBuf[i] = new BerryId(i);

var comparer = new LevelThenSmoothnessComparer();
var pruning = new BDSP.Core.Runner.PoffinSearchPruning(
    minLevel: 50,
    minSpicy: 20,
    maxSmoothness: 20);
var result = PoffinSearchRunner.Run(
    berryPool: poolBuf,
    berriesPerPoffin: 4,
    topK: 50,
    cookTimeSeconds: 40,
    errors: 0,
    amityBonus: 9,
    comparer: comparer,
    pruning: pruning);
```

Use a predicate to pre-filter poffins during search:
```csharp
using BDSP.Core.Poffins;
using BDSP.Core.Poffins.Filters;
using BDSP.Core.Selection;

var filter = new PoffinFilterOptions(
    excludeFoul: true,
    minLevel: 50,
    maxSmoothness: 20);
PoffinPredicate predicate = PoffinQuery.CompilePredicate(in filter);
```

Keep top-K results manually:
```csharp
using BDSP.Core.Poffins;
using BDSP.Core.Selection;

var selector = new TopKPoffinSelector(10, new LevelThenSmoothnessComparer());
selector.Consider(in poffin);
var best = selector.Results;
```

Optimize a feeding plan under sheen constraints:
```csharp
using BDSP.Core.Feeding;
using BDSP.Core.Poffins;
using BDSP.Core.Runner;

var options = new FeedingOptions
{
    MaxSheen = 255,
    MaxNodes = 200_000
};

FeedingPlan plan = FeedingOptimizer.Optimize(best, options);
Console.WriteLine($"Sheen: {plan.FinalState.Sheen}");
```
Note: foul poffins are skipped and never included in feeding plans.

End-to-end search + feed:
```csharp
using BDSP.Core.Berries.Data;
using BDSP.Core.Runner;
using BDSP.Core.Selection;

Span<BerryId> poolBuf = stackalloc BerryId[BerryTable.Count];
for (ushort i = 0; i < (ushort)BerryTable.Count; i++)
    poolBuf[i] = new BerryId(i);

var comparer = new LevelThenSmoothnessComparer();
var pruning = new PoffinSearchPruning(minLevel: 50, maxSmoothness: 20);
var plan = PoffinFeedingSearchRunner.Run(
    berryPool: poolBuf,
    berriesPerPoffin: 4,
    topK: 50,
    cookTimeSeconds: 40,
    errors: 0,
    amityBonus: 9,
    comparer: comparer,
    pruning: pruning);
```

Search all 4-berry combinations (no duplicates), get top 50 poffins, then rank top 5 feeding plans:
```csharp
using BDSP.Core.Berries.Data;
using BDSP.Core.Feeding;
using BDSP.Core.Runner;
using BDSP.Core.Selection;

Span<BerryId> poolBuf = stackalloc BerryId[BerryTable.Count];
for (ushort i = 0; i < (ushort)BerryTable.Count; i++)
    poolBuf[i] = new BerryId(i);

var comparer = new LevelThenSmoothnessComparer();
var result = PoffinSearchRunner.Run(
    berryPool: poolBuf,
    berriesPerPoffin: 4,
    topK: 50,
    cookTimeSeconds: 40,
    errors: 0,
    amityBonus: 9,
    comparer: comparer);

// Use the Top-50 as candidates for feeding optimization.
var options = new FeedingOptions
{
    MaxSheen = 255,
    MaxNodes = 200_000
};

// Quick way to get top 5 plans: remove the chosen poffins from the pool each time.
var candidates = result.TopPoffins.ToArray();
for (int i = 0; i < 5 && candidates.Length > 0; i++)
{
    var plan = FeedingOptimizer.Optimize(candidates, options);
    Console.WriteLine($"Plan {i + 1}: Sheen {plan.FinalState.Sheen}, Poffins {plan.Poffins.Count}");

    if (plan.Poffins.Count == 0)
        break;

    var used = new HashSet<Poffin>(plan.Poffins);
    candidates = candidates.Where(p => !used.Contains(p)).ToArray();
}
```

