using System;
using System.Threading.Tasks;
using BDSP.Core.Berries;
using BDSP.Core.Poffins;
using BDSP.Core.Selection;

namespace BDSP.Core.Runner;

/// <summary>
/// Executes a deterministic search over berry combinations to find optimal poffins.
/// </summary>
public static class PoffinSearchRunner
{

    /// <summary>
    /// Runs a full Poffin search using the provided parameters.
    /// </summary>
    /// <param name="berryPool">
    /// Pool of berries to select from. Each entry represents a unique <see cref="BerryId"/>.
    /// </param>
    /// <param name="berriesPerPoffin">
    /// Number of berries used per Poffin (1–4).
    /// </param>
    /// <param name="topK">
    /// Maximum number of best Poffins to retain.
    /// </param>
    /// <param name="cookTimeSeconds">
    /// Cooking time in seconds.
    /// </param>
    /// <param name="errors">
    /// Number of cooking errors.
    /// </param>
    /// <param name="amityBonus">
    /// Amity Square smoothness bonus (BDSP only).
    /// </param>
    /// <param name="comparer">
    /// Comparer used to rank Poffins.
    /// </param>
    /// <param name="predicate">
    /// Optional predicate used to filter Poffins before ranking.
    /// </param>
    /// <param name="maxDegreeOfParallelism">
    /// Optional limit for parallel execution. If null, a sensible default is used.
    /// </param>
    /// <returns>
    /// A deterministic <see cref="PoffinSearchResult"/> containing the best Poffins found.
    /// </returns>
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

        if (berryPool.Length < berriesPerPoffin)
            return new PoffinSearchResult(Array.Empty<Poffin>());

        // Materialize to avoid capturing a ref-like span in worker lambdas.
        BerryId[] pool = berryPool.ToArray();

        int workers =
            maxDegreeOfParallelism ??
            Math.Max(1, Environment.ProcessorCount - 1);

        int firstIndexCount = pool.Length - berriesPerPoffin + 1;
        int partitions = Math.Min(workers, Math.Max(1, firstIndexCount));
        var ranges = PartitionFirstIndices(firstIndexCount, partitions);

        var globalSelector = new TopKPoffinSelector(topK, comparer);
        object mergeLock = new();

        Parallel.For(
            0,
            ranges.Length,
            new ParallelOptions { MaxDegreeOfParallelism = workers },
            workerIndex =>
            {
                var localSelector = new TopKPoffinSelector(topK, comparer);
                var range = ranges[workerIndex];

                ForEachByFirstIndex(
                    pool,
                    berriesPerPoffin,
                    range.Start,
                    range.End,
                    combo =>
                    {
                        var poffin = PoffinCooker.CookUnique(
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

    private readonly struct IndexRange
    {
        public IndexRange(int start, int end)
        {
            Start = start;
            End = end;
        }

        public int Start { get; }
        public int End { get; }
    }

    // ------------------------------------------------------------
    // Helper: partition first-index range across workers
    // ------------------------------------------------------------
    private static IndexRange[] PartitionFirstIndices(
        int count,
        int partitions)
    {
        if (partitions <= 0)
            throw new ArgumentOutOfRangeException(nameof(partitions));

        var ranges = new IndexRange[partitions];
        for (int p = 0; p < partitions; p++)
        {
            int start = (int)((long)p * count / partitions);
            int end = (int)((long)(p + 1) * count / partitions);
            ranges[p] = new IndexRange(start, end);
        }

        return ranges;
    }

    private static void ForEachByFirstIndex(
        ReadOnlySpan<BerryId> source,
        int choose,
        int start,
        int end,
        Action<ReadOnlySpan<BerryId>> action)
    {
        int n = source.Length;

        switch (choose)
        {
            case 1:
            {
                Span<BerryId> buffer = stackalloc BerryId[1];
                for (int i = start; i < end; i++)
                {
                    buffer[0] = source[i];
                    action(buffer);
                }
                break;
            }
            case 2:
            {
                Span<BerryId> buffer = stackalloc BerryId[2];
                for (int i = start; i < end; i++)
                {
                    buffer[0] = source[i];
                    for (int j = i + 1; j < n; j++)
                    {
                        buffer[1] = source[j];
                        action(buffer);
                    }
                }
                break;
            }
            case 3:
            {
                Span<BerryId> buffer = stackalloc BerryId[3];
                for (int i = start; i < end; i++)
                {
                    buffer[0] = source[i];
                    for (int j = i + 1; j < n - 1; j++)
                    {
                        buffer[1] = source[j];
                        for (int k = j + 1; k < n; k++)
                        {
                            buffer[2] = source[k];
                            action(buffer);
                        }
                    }
                }
                break;
            }
            case 4:
            {
                Span<BerryId> buffer = stackalloc BerryId[4];
                for (int i = start; i < end; i++)
                {
                    buffer[0] = source[i];
                    for (int j = i + 1; j < n - 2; j++)
                    {
                        buffer[1] = source[j];
                        for (int k = j + 1; k < n - 1; k++)
                        {
                            buffer[2] = source[k];
                            for (int l = k + 1; l < n; l++)
                            {
                                buffer[3] = source[l];
                                action(buffer);
                            }
                        }
                    }
                }
                break;
            }
        }
    }
}
