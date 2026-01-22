using BDSP.Core.Berries.Data;
using BDSP.Core.Poffins;
using BDSP.Core.Runner;
using BDSP.Core.Selection;
using Xunit;

namespace BDSP.Core.Tests;

public sealed class PoffinSearchRunnerPruningTests
{
    [Fact]
    public void Run_WithPruning_MatchesUnprunedResults()
    {
        var pool = new BerryId[]
        {
            new(0),
            new(7),
            new(14),
            new(34),
            new(64)
        };

        PoffinPredicate predicate = static (in Poffin p) =>
            p.Type != PoffinType.Foul &&
            p.Level >= 10 &&
            p.Spicy >= 10;

        var comparer = new LevelThenSmoothnessComparer();

        var baseline = PoffinSearchRunner.Run(
            berryPool: pool,
            berriesPerPoffin: 2,
            topK: 50,
            cookTimeSeconds: 40,
            errors: 0,
            amityBonus: 0,
            comparer: comparer,
            predicate: predicate);

        var pruned = PoffinSearchRunner.Run(
            berryPool: pool,
            berriesPerPoffin: 2,
            topK: 50,
            cookTimeSeconds: 40,
            errors: 0,
            amityBonus: 0,
            comparer: comparer,
            predicate: predicate,
            pruning: new PoffinSearchPruning(minLevel: 10, minSpicy: 10));

        Assert.Equal(baseline.TopPoffins.Length, pruned.TopPoffins.Length);
        for (int i = 0; i < baseline.TopPoffins.Length; i++)
        {
            Assert.Equal(baseline.TopPoffins[i], pruned.TopPoffins[i]);
        }
    }
}
