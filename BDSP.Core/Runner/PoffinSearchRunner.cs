using System;
using System.Threading.Tasks;
using BDSP.Core.Berries;
using BDSP.Core.Poffins;
using BDSP.Core.Selection;

namespace BDSP.Core.Runner;

public static class PoffinSearchRunner
{
    public static PoffinSearchResult Run(
        ReadOnlySpan<BerryId> berryPool,
        int berriesPerPoffin,
        int topK,
        byte cookTimeSeconds,
        byte errors,
        byte amityBonus,
        IPoffinComparer comparer,
        PoffinPredicate? predicate = null,
        int? maxDegreeOfParallelism = null)
    {
        if (berriesPerPoffin < 1 || berriesPerPoffin > 4)
            throw new ArgumentOutOfRangeException(nameof(berriesPerPoffin));

        if (topK <= 0)
            throw new ArgumentOutOfRangeException(nameof(topK));

        int workers =
            maxDegreeOfParallelism ??
            Math.Max(1, Environment.ProcessorCount - 1);

        // Partition berry pool across workers
        BerryId[][] partitions = Partition(berryPool, workers);

        var globalSelector = new TopKPoffinSelector(topK, comparer);
        object mergeLock = new();

        Parallel.For(
            0,
            partitions.Length,
            new ParallelOptions { MaxDegreeOfParallelism = workers },
            workerIndex =>
            {
                var localSelector = new TopKPoffinSelector(topK, comparer);
                BerryId[] localPool = partitions[workerIndex];

                BerryCombinations.ForEach(
                    localPool,
                    berriesPerPoffin,
                    combo =>
                    {
                        var poffin = PoffinCooker.Cook(
                            combo,
                            cookTimeSeconds,
                            errors,
                            amityBonus);

                        if (predicate != null && !predicate(in poffin))
                            return;

                        localSelector.Consider(in poffin);
                    });

                // Merge local Top-K into global Top-K
                lock (mergeLock)
                {
                    foreach (ref readonly var p in localSelector.Results)
                    {
                        globalSelector.Consider(in p);
                    }
                }
            });

        return new PoffinSearchResult(
            globalSelector.Results.ToArray());
    }

    // ------------------------------------------------------------
    // Helper: partition berry pool using stride distribution
    // ------------------------------------------------------------
    private static BerryId[][] Partition(
        ReadOnlySpan<BerryId> source,
        int partitions)
    {
        if (partitions <= 0)
            throw new ArgumentOutOfRangeException(nameof(partitions));

        // Count elements per partition
        int[] counts = new int[partitions];
        for (int i = 0; i < source.Length; i++)
            counts[i % partitions]++;

        // Allocate exact-sized arrays
        var result = new BerryId[partitions][];
        for (int p = 0; p < partitions; p++)
            result[p] = new BerryId[counts[p]];

        // Reset counters
        Array.Clear(counts, 0, counts.Length);

        // Fill partitions
        for (int i = 0; i < source.Length; i++)
        {
            int p = i % partitions;
            result[p][counts[p]++] = source[i];
        }

        return result;
    }
}
