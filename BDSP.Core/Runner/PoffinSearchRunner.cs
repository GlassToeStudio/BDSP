using System;
using System.Threading.Tasks;
using BDSP.Core.Berries.Data;
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
    /// <param name="pruning">
    /// Optional pruning hints to skip combinations that cannot meet minimum thresholds.
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
        int? maxDegreeOfParallelism = null,
        PoffinSearchPruning? pruning = null)
    {
        if (berriesPerPoffin < 1 || berriesPerPoffin > 4)
            throw new ArgumentOutOfRangeException(nameof(berriesPerPoffin));

        if (topK <= 0)
            throw new ArgumentOutOfRangeException(nameof(topK));

        if (berryPool.Length < berriesPerPoffin)
            return new PoffinSearchResult(Array.Empty<Poffin>());

        // Materialize to avoid capturing a ref-like span in worker lambdas.
        BerryId[] pool = berryPool.ToArray();
        var poolBerries = new Berry[pool.Length];
        for (int i = 0; i < pool.Length; i++)
            poolBerries[i] = BerryTable.Get(pool[i]);

        var pruningOptions = pruning.GetValueOrDefault();
        bool usePruning = pruning.HasValue && pruningOptions.IsEnabled;
        int[]? maxSpicySuffix = null;
        int[]? maxDrySuffix = null;
        int[]? maxSweetSuffix = null;
        int[]? maxBitterSuffix = null;
        int[]? maxSourSuffix = null;
        int[]? minSmoothnessSuffix = null;

        if (usePruning)
        {
            // Suffix maxima provide optimistic upper bounds for remaining flavor sums.
            maxSpicySuffix = new int[poolBerries.Length + 1];
            maxDrySuffix = new int[poolBerries.Length + 1];
            maxSweetSuffix = new int[poolBerries.Length + 1];
            maxBitterSuffix = new int[poolBerries.Length + 1];
            maxSourSuffix = new int[poolBerries.Length + 1];
            // Suffix minima provide optimistic lower bounds for smoothness.
            minSmoothnessSuffix = new int[poolBerries.Length + 1];
            minSmoothnessSuffix[poolBerries.Length] = int.MaxValue;

            for (int i = poolBerries.Length - 1; i >= 0; i--)
            {
                ref readonly var b = ref poolBerries[i];
                maxSpicySuffix[i] = Math.Max(maxSpicySuffix[i + 1], b.Spicy);
                maxDrySuffix[i] = Math.Max(maxDrySuffix[i + 1], b.Dry);
                maxSweetSuffix[i] = Math.Max(maxSweetSuffix[i + 1], b.Sweet);
                maxBitterSuffix[i] = Math.Max(maxBitterSuffix[i + 1], b.Bitter);
                maxSourSuffix[i] = Math.Max(maxSourSuffix[i + 1], b.Sour);
                minSmoothnessSuffix[i] = Math.Min(minSmoothnessSuffix[i + 1], b.Smoothness);
            }
        }

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

                if (predicate is null)
                {
                    if (usePruning)
                    {
                        // Pruned path avoids cooking when bounds cannot satisfy filters.
                        ProcessRangeNoPredicatePruned(
                            poolBerries,
                            berriesPerPoffin,
                            range.Start,
                            range.End,
                            cookTimeSeconds,
                            errors,
                            amityBonus,
                            localSelector,
                            pruningOptions,
                            maxSpicySuffix!,
                            maxDrySuffix!,
                            maxSweetSuffix!,
                            maxBitterSuffix!,
                            maxSourSuffix!,
                            minSmoothnessSuffix!);
                    }
                    else
                    {
                        ProcessRangeNoPredicate(
                            poolBerries,
                            berriesPerPoffin,
                            range.Start,
                            range.End,
                            cookTimeSeconds,
                            errors,
                            amityBonus,
                            localSelector);
                    }
                }
                else
                {
                    if (usePruning)
                    {
                        // Predicate path keeps pruning separate to avoid extra delegate hops.
                        ProcessRangeWithPredicatePruned(
                            poolBerries,
                            berriesPerPoffin,
                            range.Start,
                            range.End,
                            cookTimeSeconds,
                            errors,
                            amityBonus,
                            localSelector,
                            predicate,
                            pruningOptions,
                            maxSpicySuffix!,
                            maxDrySuffix!,
                            maxSweetSuffix!,
                            maxBitterSuffix!,
                            maxSourSuffix!,
                            minSmoothnessSuffix!);
                    }
                    else
                    {
                        ProcessRangeWithPredicate(
                            poolBerries,
                            berriesPerPoffin,
                            range.Start,
                            range.End,
                            cookTimeSeconds,
                            errors,
                            amityBonus,
                            localSelector,
                            predicate);
                    }
                }

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

    public static PoffinRecipeSearchResult RunWithRecipes(
        ReadOnlySpan<BerryId> berryPool,
        int berriesPerPoffin,
        int topK,
        byte cookTimeSeconds,
        byte errors,
        byte amityBonus,
        IPoffinComparer comparer,
        PoffinPredicate? predicate = null,
        int? maxDegreeOfParallelism = null,
        PoffinSearchPruning? pruning = null)
    {
        if (berriesPerPoffin < 1 || berriesPerPoffin > 4)
            throw new ArgumentOutOfRangeException(nameof(berriesPerPoffin));

        if (topK <= 0)
            throw new ArgumentOutOfRangeException(nameof(topK));

        if (berryPool.Length < berriesPerPoffin)
            return new PoffinRecipeSearchResult(Array.Empty<PoffinRecipe>());

        BerryId[] pool = berryPool.ToArray();
        var poolBerries = new Berry[pool.Length];
        for (int i = 0; i < pool.Length; i++)
            poolBerries[i] = BerryTable.Get(pool[i]);

        var pruningOptions = pruning.GetValueOrDefault();
        bool usePruning = pruning.HasValue && pruningOptions.IsEnabled;
        int[]? maxSpicySuffix = null;
        int[]? maxDrySuffix = null;
        int[]? maxSweetSuffix = null;
        int[]? maxBitterSuffix = null;
        int[]? maxSourSuffix = null;
        int[]? minSmoothnessSuffix = null;

        if (usePruning)
        {
            maxSpicySuffix = new int[poolBerries.Length + 1];
            maxDrySuffix = new int[poolBerries.Length + 1];
            maxSweetSuffix = new int[poolBerries.Length + 1];
            maxBitterSuffix = new int[poolBerries.Length + 1];
            maxSourSuffix = new int[poolBerries.Length + 1];
            minSmoothnessSuffix = new int[poolBerries.Length + 1];
            minSmoothnessSuffix[poolBerries.Length] = int.MaxValue;

            for (int i = poolBerries.Length - 1; i >= 0; i--)
            {
                ref readonly var b = ref poolBerries[i];
                maxSpicySuffix[i] = Math.Max(maxSpicySuffix[i + 1], b.Spicy);
                maxDrySuffix[i] = Math.Max(maxDrySuffix[i + 1], b.Dry);
                maxSweetSuffix[i] = Math.Max(maxSweetSuffix[i + 1], b.Sweet);
                maxBitterSuffix[i] = Math.Max(maxBitterSuffix[i + 1], b.Bitter);
                maxSourSuffix[i] = Math.Max(maxSourSuffix[i + 1], b.Sour);
                minSmoothnessSuffix[i] = Math.Min(minSmoothnessSuffix[i + 1], b.Smoothness);
            }
        }

        int workers =
            maxDegreeOfParallelism ??
            Math.Max(1, Environment.ProcessorCount - 1);

        int firstIndexCount = pool.Length - berriesPerPoffin + 1;
        int partitions = Math.Min(workers, Math.Max(1, firstIndexCount));
        var ranges = PartitionFirstIndices(firstIndexCount, partitions);

        var globalSelector = new TopKPoffinRecipeSelector(topK, comparer);
        object mergeLock = new();

        Parallel.For(
            0,
            ranges.Length,
            new ParallelOptions { MaxDegreeOfParallelism = workers },
            workerIndex =>
            {
                var localSelector = new TopKPoffinRecipeSelector(topK, comparer);
                var range = ranges[workerIndex];

                if (predicate is null)
                {
                    if (usePruning)
                    {
                        ProcessRangeNoPredicatePrunedRecipes(
                            poolBerries,
                            berriesPerPoffin,
                            range.Start,
                            range.End,
                            cookTimeSeconds,
                            errors,
                            amityBonus,
                            localSelector,
                            pruningOptions,
                            maxSpicySuffix!,
                            maxDrySuffix!,
                            maxSweetSuffix!,
                            maxBitterSuffix!,
                            maxSourSuffix!,
                            minSmoothnessSuffix!);
                    }
                    else
                    {
                        ProcessRangeNoPredicateRecipes(
                            poolBerries,
                            berriesPerPoffin,
                            range.Start,
                            range.End,
                            cookTimeSeconds,
                            errors,
                            amityBonus,
                            localSelector);
                    }
                }
                else
                {
                    if (usePruning)
                    {
                        ProcessRangeWithPredicatePrunedRecipes(
                            poolBerries,
                            berriesPerPoffin,
                            range.Start,
                            range.End,
                            cookTimeSeconds,
                            errors,
                            amityBonus,
                            localSelector,
                            predicate,
                            pruningOptions,
                            maxSpicySuffix!,
                            maxDrySuffix!,
                            maxSweetSuffix!,
                            maxBitterSuffix!,
                            maxSourSuffix!,
                            minSmoothnessSuffix!);
                    }
                    else
                    {
                        ProcessRangeWithPredicateRecipes(
                            poolBerries,
                            berriesPerPoffin,
                            range.Start,
                            range.End,
                            cookTimeSeconds,
                            errors,
                            amityBonus,
                            localSelector,
                            predicate);
                    }
                }

                lock (mergeLock)
                {
                    foreach (var r in localSelector.Results)
                        globalSelector.Consider(in r);
                }
            });

        return new PoffinRecipeSearchResult(
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

    private static void ProcessRangeNoPredicate(
        ReadOnlySpan<Berry> source,
        int choose,
        int start,
        int end,
        byte cookTimeSeconds,
        byte errors,
        byte amityBonus,
        TopKPoffinSelector selector)
    {
        int n = source.Length;

        switch (choose)
        {
            case 1:
            {
                Span<Berry> buffer = stackalloc Berry[1];
                for (int i = start; i < end; i++)
                {
                    buffer[0] = source[i];
                    var poffin = PoffinCooker.CookFromBerriesUnique(
                        buffer,
                        cookTimeSeconds,
                        errors,
                        amityBonus);
                    selector.Consider(in poffin);
                }
                break;
            }
            case 2:
            {
                Span<Berry> buffer = stackalloc Berry[2];
                for (int i = start; i < end; i++)
                {
                    buffer[0] = source[i];
                    for (int j = i + 1; j < n; j++)
                    {
                        buffer[1] = source[j];
                        var poffin = PoffinCooker.CookFromBerriesUnique(
                            buffer,
                            cookTimeSeconds,
                            errors,
                            amityBonus);
                        selector.Consider(in poffin);
                    }
                }
                break;
            }
            case 3:
            {
                Span<Berry> buffer = stackalloc Berry[3];
                for (int i = start; i < end; i++)
                {
                    buffer[0] = source[i];
                    for (int j = i + 1; j < n - 1; j++)
                    {
                        buffer[1] = source[j];
                        for (int k = j + 1; k < n; k++)
                        {
                            buffer[2] = source[k];
                            var poffin = PoffinCooker.CookFromBerriesUnique(
                                buffer,
                                cookTimeSeconds,
                                errors,
                                amityBonus);
                            selector.Consider(in poffin);
                        }
                    }
                }
                break;
            }
            case 4:
            {
                Span<Berry> buffer = stackalloc Berry[4];
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
                                var poffin = PoffinCooker.CookFromBerriesUnique(
                                    buffer,
                                    cookTimeSeconds,
                                    errors,
                                    amityBonus);
                                selector.Consider(in poffin);
                            }
                        }
                    }
                }
                break;
            }
        }
    }

    private static void ProcessRangeWithPredicate(
        ReadOnlySpan<Berry> source,
        int choose,
        int start,
        int end,
        byte cookTimeSeconds,
        byte errors,
        byte amityBonus,
        TopKPoffinSelector selector,
        PoffinPredicate predicate)
    {
        int n = source.Length;

        switch (choose)
        {
            case 1:
            {
                Span<Berry> buffer = stackalloc Berry[1];
                for (int i = start; i < end; i++)
                {
                    buffer[0] = source[i];
                    var poffin = PoffinCooker.CookFromBerriesUnique(
                        buffer,
                        cookTimeSeconds,
                        errors,
                        amityBonus);
                    if (!predicate(in poffin))
                        continue;
                    selector.Consider(in poffin);
                }
                break;
            }
            case 2:
            {
                Span<Berry> buffer = stackalloc Berry[2];
                for (int i = start; i < end; i++)
                {
                    buffer[0] = source[i];
                    for (int j = i + 1; j < n; j++)
                    {
                        buffer[1] = source[j];
                        var poffin = PoffinCooker.CookFromBerriesUnique(
                            buffer,
                            cookTimeSeconds,
                            errors,
                            amityBonus);
                        if (!predicate(in poffin))
                            continue;
                        selector.Consider(in poffin);
                    }
                }
                break;
            }
            case 3:
            {
                Span<Berry> buffer = stackalloc Berry[3];
                for (int i = start; i < end; i++)
                {
                    buffer[0] = source[i];
                    for (int j = i + 1; j < n - 1; j++)
                    {
                        buffer[1] = source[j];
                        for (int k = j + 1; k < n; k++)
                        {
                            buffer[2] = source[k];
                            var poffin = PoffinCooker.CookFromBerriesUnique(
                                buffer,
                                cookTimeSeconds,
                                errors,
                                amityBonus);
                            if (!predicate(in poffin))
                                continue;
                            selector.Consider(in poffin);
                        }
                    }
                }
                break;
            }
            case 4:
            {
                Span<Berry> buffer = stackalloc Berry[4];
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
                                var poffin = PoffinCooker.CookFromBerriesUnique(
                                    buffer,
                                    cookTimeSeconds,
                                    errors,
                                    amityBonus);
                                if (!predicate(in poffin))
                                    continue;
                                selector.Consider(in poffin);
                            }
                        }
                    }
                }
                break;
            }
        }
    }

    // Pruned variant: optimistic bounds can skip cooking entirely.
    private static void ProcessRangeNoPredicatePruned(
        ReadOnlySpan<Berry> source,
        int choose,
        int start,
        int end,
        byte cookTimeSeconds,
        byte errors,
        byte amityBonus,
        TopKPoffinSelector selector,
        PoffinSearchPruning pruning,
        int[] maxSpicySuffix,
        int[] maxDrySuffix,
        int[] maxSweetSuffix,
        int[] maxBitterSuffix,
        int[] maxSourSuffix,
        int[] minSmoothnessSuffix)
    {
        int n = source.Length;

        switch (choose)
        {
            case 1:
            {
                Span<Berry> buffer = stackalloc Berry[1];
                for (int i = start; i < end; i++)
                {
                    ref readonly var b = ref source[i];
                    if (!CanSatisfy(pruning, b.Spicy, b.Dry, b.Sweet, b.Bitter, b.Sour, b.Smoothness, 0, i + 1,
                            choose, amityBonus, maxSpicySuffix, maxDrySuffix, maxSweetSuffix, maxBitterSuffix,
                            maxSourSuffix, minSmoothnessSuffix))
                        continue;

                    buffer[0] = b;
                    var poffin = PoffinCooker.CookFromBerriesUnique(
                        buffer,
                        cookTimeSeconds,
                        errors,
                        amityBonus);
                    selector.Consider(in poffin);
                }
                break;
            }
            case 2:
            {
                Span<Berry> buffer = stackalloc Berry[2];
                for (int i = start; i < end; i++)
                {
                    ref readonly var b0 = ref source[i];
                    int spicy0 = b0.Spicy;
                    int dry0 = b0.Dry;
                    int sweet0 = b0.Sweet;
                    int bitter0 = b0.Bitter;
                    int sour0 = b0.Sour;
                    int smooth0 = b0.Smoothness;

                    if (!CanSatisfy(pruning, spicy0, dry0, sweet0, bitter0, sour0, smooth0, 1, i + 1,
                            choose, amityBonus, maxSpicySuffix, maxDrySuffix, maxSweetSuffix, maxBitterSuffix,
                            maxSourSuffix, minSmoothnessSuffix))
                        continue;

                    buffer[0] = b0;
                    for (int j = i + 1; j < n; j++)
                    {
                        ref readonly var b1 = ref source[j];
                        int spicy = spicy0 + b1.Spicy;
                        int dry = dry0 + b1.Dry;
                        int sweet = sweet0 + b1.Sweet;
                        int bitter = bitter0 + b1.Bitter;
                        int sour = sour0 + b1.Sour;
                        int smooth = smooth0 + b1.Smoothness;

                        if (!CanSatisfy(pruning, spicy, dry, sweet, bitter, sour, smooth, 0, j + 1,
                                choose, amityBonus, maxSpicySuffix, maxDrySuffix, maxSweetSuffix, maxBitterSuffix,
                                maxSourSuffix, minSmoothnessSuffix))
                            continue;

                        buffer[1] = b1;
                        var poffin = PoffinCooker.CookFromBerriesUnique(
                            buffer,
                            cookTimeSeconds,
                            errors,
                            amityBonus);
                        selector.Consider(in poffin);
                    }
                }
                break;
            }
            case 3:
            {
                Span<Berry> buffer = stackalloc Berry[3];
                for (int i = start; i < end; i++)
                {
                    ref readonly var b0 = ref source[i];
                    int spicy0 = b0.Spicy;
                    int dry0 = b0.Dry;
                    int sweet0 = b0.Sweet;
                    int bitter0 = b0.Bitter;
                    int sour0 = b0.Sour;
                    int smooth0 = b0.Smoothness;

                    if (!CanSatisfy(pruning, spicy0, dry0, sweet0, bitter0, sour0, smooth0, 2, i + 1,
                            choose, amityBonus, maxSpicySuffix, maxDrySuffix, maxSweetSuffix, maxBitterSuffix,
                            maxSourSuffix, minSmoothnessSuffix))
                        continue;

                    buffer[0] = b0;
                    for (int j = i + 1; j < n - 1; j++)
                    {
                        ref readonly var b1 = ref source[j];
                        int spicy1 = spicy0 + b1.Spicy;
                        int dry1 = dry0 + b1.Dry;
                        int sweet1 = sweet0 + b1.Sweet;
                        int bitter1 = bitter0 + b1.Bitter;
                        int sour1 = sour0 + b1.Sour;
                        int smooth1 = smooth0 + b1.Smoothness;

                        if (!CanSatisfy(pruning, spicy1, dry1, sweet1, bitter1, sour1, smooth1, 1, j + 1,
                                choose, amityBonus, maxSpicySuffix, maxDrySuffix, maxSweetSuffix, maxBitterSuffix,
                                maxSourSuffix, minSmoothnessSuffix))
                            continue;

                        buffer[1] = b1;
                        for (int k = j + 1; k < n; k++)
                        {
                            ref readonly var b2 = ref source[k];
                            int spicy = spicy1 + b2.Spicy;
                            int dry = dry1 + b2.Dry;
                            int sweet = sweet1 + b2.Sweet;
                            int bitter = bitter1 + b2.Bitter;
                            int sour = sour1 + b2.Sour;
                            int smooth = smooth1 + b2.Smoothness;

                            if (!CanSatisfy(pruning, spicy, dry, sweet, bitter, sour, smooth, 0, k + 1,
                                    choose, amityBonus, maxSpicySuffix, maxDrySuffix, maxSweetSuffix,
                                    maxBitterSuffix, maxSourSuffix, minSmoothnessSuffix))
                                continue;

                            buffer[2] = b2;
                            var poffin = PoffinCooker.CookFromBerriesUnique(
                                buffer,
                                cookTimeSeconds,
                                errors,
                                amityBonus);
                            selector.Consider(in poffin);
                        }
                    }
                }
                break;
            }
            case 4:
            {
                Span<Berry> buffer = stackalloc Berry[4];
                for (int i = start; i < end; i++)
                {
                    ref readonly var b0 = ref source[i];
                    int spicy0 = b0.Spicy;
                    int dry0 = b0.Dry;
                    int sweet0 = b0.Sweet;
                    int bitter0 = b0.Bitter;
                    int sour0 = b0.Sour;
                    int smooth0 = b0.Smoothness;

                    if (!CanSatisfy(pruning, spicy0, dry0, sweet0, bitter0, sour0, smooth0, 3, i + 1,
                            choose, amityBonus, maxSpicySuffix, maxDrySuffix, maxSweetSuffix, maxBitterSuffix,
                            maxSourSuffix, minSmoothnessSuffix))
                        continue;

                    buffer[0] = b0;
                    for (int j = i + 1; j < n - 2; j++)
                    {
                        ref readonly var b1 = ref source[j];
                        int spicy1 = spicy0 + b1.Spicy;
                        int dry1 = dry0 + b1.Dry;
                        int sweet1 = sweet0 + b1.Sweet;
                        int bitter1 = bitter0 + b1.Bitter;
                        int sour1 = sour0 + b1.Sour;
                        int smooth1 = smooth0 + b1.Smoothness;

                        if (!CanSatisfy(pruning, spicy1, dry1, sweet1, bitter1, sour1, smooth1, 2, j + 1,
                                choose, amityBonus, maxSpicySuffix, maxDrySuffix, maxSweetSuffix, maxBitterSuffix,
                                maxSourSuffix, minSmoothnessSuffix))
                            continue;

                        buffer[1] = b1;
                        for (int k = j + 1; k < n - 1; k++)
                        {
                            ref readonly var b2 = ref source[k];
                            int spicy2 = spicy1 + b2.Spicy;
                            int dry2 = dry1 + b2.Dry;
                            int sweet2 = sweet1 + b2.Sweet;
                            int bitter2 = bitter1 + b2.Bitter;
                            int sour2 = sour1 + b2.Sour;
                            int smooth2 = smooth1 + b2.Smoothness;

                            if (!CanSatisfy(pruning, spicy2, dry2, sweet2, bitter2, sour2, smooth2, 1, k + 1,
                                    choose, amityBonus, maxSpicySuffix, maxDrySuffix, maxSweetSuffix,
                                    maxBitterSuffix, maxSourSuffix, minSmoothnessSuffix))
                                continue;

                            buffer[2] = b2;
                            for (int l = k + 1; l < n; l++)
                            {
                                ref readonly var b3 = ref source[l];
                                int spicy = spicy2 + b3.Spicy;
                                int dry = dry2 + b3.Dry;
                                int sweet = sweet2 + b3.Sweet;
                                int bitter = bitter2 + b3.Bitter;
                                int sour = sour2 + b3.Sour;
                                int smooth = smooth2 + b3.Smoothness;

                                if (!CanSatisfy(pruning, spicy, dry, sweet, bitter, sour, smooth, 0, l + 1,
                                        choose, amityBonus, maxSpicySuffix, maxDrySuffix, maxSweetSuffix,
                                        maxBitterSuffix, maxSourSuffix, minSmoothnessSuffix))
                                    continue;

                                buffer[3] = b3;
                                var poffin = PoffinCooker.CookFromBerriesUnique(
                                    buffer,
                                    cookTimeSeconds,
                                    errors,
                                    amityBonus);
                                selector.Consider(in poffin);
                            }
                        }
                    }
                }
                break;
            }
        }
    }

    // Pruned variant with predicate filtering after cooking.
    private static void ProcessRangeWithPredicatePruned(
        ReadOnlySpan<Berry> source,
        int choose,
        int start,
        int end,
        byte cookTimeSeconds,
        byte errors,
        byte amityBonus,
        TopKPoffinSelector selector,
        PoffinPredicate predicate,
        PoffinSearchPruning pruning,
        int[] maxSpicySuffix,
        int[] maxDrySuffix,
        int[] maxSweetSuffix,
        int[] maxBitterSuffix,
        int[] maxSourSuffix,
        int[] minSmoothnessSuffix)
    {
        int n = source.Length;

        switch (choose)
        {
            case 1:
            {
                Span<Berry> buffer = stackalloc Berry[1];
                for (int i = start; i < end; i++)
                {
                    ref readonly var b = ref source[i];
                    if (!CanSatisfy(pruning, b.Spicy, b.Dry, b.Sweet, b.Bitter, b.Sour, b.Smoothness, 0, i + 1,
                            choose, amityBonus, maxSpicySuffix, maxDrySuffix, maxSweetSuffix, maxBitterSuffix,
                            maxSourSuffix, minSmoothnessSuffix))
                        continue;

                    buffer[0] = b;
                    var poffin = PoffinCooker.CookFromBerriesUnique(
                        buffer,
                        cookTimeSeconds,
                        errors,
                        amityBonus);
                    if (!predicate(in poffin))
                        continue;
                    selector.Consider(in poffin);
                }
                break;
            }
            case 2:
            {
                Span<Berry> buffer = stackalloc Berry[2];
                for (int i = start; i < end; i++)
                {
                    ref readonly var b0 = ref source[i];
                    int spicy0 = b0.Spicy;
                    int dry0 = b0.Dry;
                    int sweet0 = b0.Sweet;
                    int bitter0 = b0.Bitter;
                    int sour0 = b0.Sour;
                    int smooth0 = b0.Smoothness;

                    if (!CanSatisfy(pruning, spicy0, dry0, sweet0, bitter0, sour0, smooth0, 1, i + 1,
                            choose, amityBonus, maxSpicySuffix, maxDrySuffix, maxSweetSuffix, maxBitterSuffix,
                            maxSourSuffix, minSmoothnessSuffix))
                        continue;

                    buffer[0] = b0;
                    for (int j = i + 1; j < n; j++)
                    {
                        ref readonly var b1 = ref source[j];
                        int spicy = spicy0 + b1.Spicy;
                        int dry = dry0 + b1.Dry;
                        int sweet = sweet0 + b1.Sweet;
                        int bitter = bitter0 + b1.Bitter;
                        int sour = sour0 + b1.Sour;
                        int smooth = smooth0 + b1.Smoothness;

                        if (!CanSatisfy(pruning, spicy, dry, sweet, bitter, sour, smooth, 0, j + 1,
                                choose, amityBonus, maxSpicySuffix, maxDrySuffix, maxSweetSuffix, maxBitterSuffix,
                                maxSourSuffix, minSmoothnessSuffix))
                            continue;

                        buffer[1] = b1;
                        var poffin = PoffinCooker.CookFromBerriesUnique(
                            buffer,
                            cookTimeSeconds,
                            errors,
                            amityBonus);
                        if (!predicate(in poffin))
                            continue;
                        selector.Consider(in poffin);
                    }
                }
                break;
            }
            case 3:
            {
                Span<Berry> buffer = stackalloc Berry[3];
                for (int i = start; i < end; i++)
                {
                    ref readonly var b0 = ref source[i];
                    int spicy0 = b0.Spicy;
                    int dry0 = b0.Dry;
                    int sweet0 = b0.Sweet;
                    int bitter0 = b0.Bitter;
                    int sour0 = b0.Sour;
                    int smooth0 = b0.Smoothness;

                    if (!CanSatisfy(pruning, spicy0, dry0, sweet0, bitter0, sour0, smooth0, 2, i + 1,
                            choose, amityBonus, maxSpicySuffix, maxDrySuffix, maxSweetSuffix, maxBitterSuffix,
                            maxSourSuffix, minSmoothnessSuffix))
                        continue;

                    buffer[0] = b0;
                    for (int j = i + 1; j < n - 1; j++)
                    {
                        ref readonly var b1 = ref source[j];
                        int spicy1 = spicy0 + b1.Spicy;
                        int dry1 = dry0 + b1.Dry;
                        int sweet1 = sweet0 + b1.Sweet;
                        int bitter1 = bitter0 + b1.Bitter;
                        int sour1 = sour0 + b1.Sour;
                        int smooth1 = smooth0 + b1.Smoothness;

                        if (!CanSatisfy(pruning, spicy1, dry1, sweet1, bitter1, sour1, smooth1, 1, j + 1,
                                choose, amityBonus, maxSpicySuffix, maxDrySuffix, maxSweetSuffix, maxBitterSuffix,
                                maxSourSuffix, minSmoothnessSuffix))
                            continue;

                        buffer[1] = b1;
                        for (int k = j + 1; k < n; k++)
                        {
                            ref readonly var b2 = ref source[k];
                            int spicy = spicy1 + b2.Spicy;
                            int dry = dry1 + b2.Dry;
                            int sweet = sweet1 + b2.Sweet;
                            int bitter = bitter1 + b2.Bitter;
                            int sour = sour1 + b2.Sour;
                            int smooth = smooth1 + b2.Smoothness;

                            if (!CanSatisfy(pruning, spicy, dry, sweet, bitter, sour, smooth, 0, k + 1,
                                    choose, amityBonus, maxSpicySuffix, maxDrySuffix, maxSweetSuffix,
                                    maxBitterSuffix, maxSourSuffix, minSmoothnessSuffix))
                                continue;

                            buffer[2] = b2;
                            var poffin = PoffinCooker.CookFromBerriesUnique(
                                buffer,
                                cookTimeSeconds,
                                errors,
                                amityBonus);
                            if (!predicate(in poffin))
                                continue;
                            selector.Consider(in poffin);
                        }
                    }
                }
                break;
            }
            case 4:
            {
                Span<Berry> buffer = stackalloc Berry[4];
                for (int i = start; i < end; i++)
                {
                    ref readonly var b0 = ref source[i];
                    int spicy0 = b0.Spicy;
                    int dry0 = b0.Dry;
                    int sweet0 = b0.Sweet;
                    int bitter0 = b0.Bitter;
                    int sour0 = b0.Sour;
                    int smooth0 = b0.Smoothness;

                    if (!CanSatisfy(pruning, spicy0, dry0, sweet0, bitter0, sour0, smooth0, 3, i + 1,
                            choose, amityBonus, maxSpicySuffix, maxDrySuffix, maxSweetSuffix, maxBitterSuffix,
                            maxSourSuffix, minSmoothnessSuffix))
                        continue;

                    buffer[0] = b0;
                    for (int j = i + 1; j < n - 2; j++)
                    {
                        ref readonly var b1 = ref source[j];
                        int spicy1 = spicy0 + b1.Spicy;
                        int dry1 = dry0 + b1.Dry;
                        int sweet1 = sweet0 + b1.Sweet;
                        int bitter1 = bitter0 + b1.Bitter;
                        int sour1 = sour0 + b1.Sour;
                        int smooth1 = smooth0 + b1.Smoothness;

                        if (!CanSatisfy(pruning, spicy1, dry1, sweet1, bitter1, sour1, smooth1, 2, j + 1,
                                choose, amityBonus, maxSpicySuffix, maxDrySuffix, maxSweetSuffix, maxBitterSuffix,
                                maxSourSuffix, minSmoothnessSuffix))
                            continue;

                        buffer[1] = b1;
                        for (int k = j + 1; k < n - 1; k++)
                        {
                            ref readonly var b2 = ref source[k];
                            int spicy2 = spicy1 + b2.Spicy;
                            int dry2 = dry1 + b2.Dry;
                            int sweet2 = sweet1 + b2.Sweet;
                            int bitter2 = bitter1 + b2.Bitter;
                            int sour2 = sour1 + b2.Sour;
                            int smooth2 = smooth1 + b2.Smoothness;

                            if (!CanSatisfy(pruning, spicy2, dry2, sweet2, bitter2, sour2, smooth2, 1, k + 1,
                                    choose, amityBonus, maxSpicySuffix, maxDrySuffix, maxSweetSuffix,
                                    maxBitterSuffix, maxSourSuffix, minSmoothnessSuffix))
                                continue;

                            buffer[2] = b2;
                            for (int l = k + 1; l < n; l++)
                            {
                                ref readonly var b3 = ref source[l];
                                int spicy = spicy2 + b3.Spicy;
                                int dry = dry2 + b3.Dry;
                                int sweet = sweet2 + b3.Sweet;
                                int bitter = bitter2 + b3.Bitter;
                                int sour = sour2 + b3.Sour;
                                int smooth = smooth2 + b3.Smoothness;

                                if (!CanSatisfy(pruning, spicy, dry, sweet, bitter, sour, smooth, 0, l + 1,
                                        choose, amityBonus, maxSpicySuffix, maxDrySuffix, maxSweetSuffix,
                                        maxBitterSuffix, maxSourSuffix, minSmoothnessSuffix))
                                    continue;

                                buffer[3] = b3;
                                var poffin = PoffinCooker.CookFromBerriesUnique(
                                    buffer,
                                    cookTimeSeconds,
                                    errors,
                                    amityBonus);
                                if (!predicate(in poffin))
                                    continue;
                                selector.Consider(in poffin);
                            }
                        }
                    }
                }
                break;
            }
        }
    }

    private static void ProcessRangeNoPredicateRecipes(
        ReadOnlySpan<Berry> source,
        int choose,
        int start,
        int end,
        byte cookTimeSeconds,
        byte errors,
        byte amityBonus,
        TopKPoffinRecipeSelector selector)
    {
        int n = source.Length;

        switch (choose)
        {
            case 1:
            {
                Span<Berry> buffer = stackalloc Berry[1];
                Span<BerryId> ids = stackalloc BerryId[1];
                for (int i = start; i < end; i++)
                {
                    buffer[0] = source[i];
                    ids[0] = buffer[0].Id;
                    var poffin = PoffinCooker.CookFromBerriesUnique(
                        buffer,
                        cookTimeSeconds,
                        errors,
                        amityBonus);
                    selector.Consider(in poffin, ids);
                }
                break;
            }
            case 2:
            {
                Span<Berry> buffer = stackalloc Berry[2];
                Span<BerryId> ids = stackalloc BerryId[2];
                for (int i = start; i < end; i++)
                {
                    buffer[0] = source[i];
                    ids[0] = buffer[0].Id;
                    for (int j = i + 1; j < n; j++)
                    {
                        buffer[1] = source[j];
                        ids[1] = buffer[1].Id;
                        var poffin = PoffinCooker.CookFromBerriesUnique(
                            buffer,
                            cookTimeSeconds,
                            errors,
                            amityBonus);
                        selector.Consider(in poffin, ids);
                    }
                }
                break;
            }
            case 3:
            {
                Span<Berry> buffer = stackalloc Berry[3];
                Span<BerryId> ids = stackalloc BerryId[3];
                for (int i = start; i < end; i++)
                {
                    buffer[0] = source[i];
                    ids[0] = buffer[0].Id;
                    for (int j = i + 1; j < n - 1; j++)
                    {
                        buffer[1] = source[j];
                        ids[1] = buffer[1].Id;
                        for (int k = j + 1; k < n; k++)
                        {
                            buffer[2] = source[k];
                            ids[2] = buffer[2].Id;
                            var poffin = PoffinCooker.CookFromBerriesUnique(
                                buffer,
                                cookTimeSeconds,
                                errors,
                                amityBonus);
                            selector.Consider(in poffin, ids);
                        }
                    }
                }
                break;
            }
            case 4:
            {
                Span<Berry> buffer = stackalloc Berry[4];
                Span<BerryId> ids = stackalloc BerryId[4];
                for (int i = start; i < end; i++)
                {
                    buffer[0] = source[i];
                    ids[0] = buffer[0].Id;
                    for (int j = i + 1; j < n - 2; j++)
                    {
                        buffer[1] = source[j];
                        ids[1] = buffer[1].Id;
                        for (int k = j + 1; k < n - 1; k++)
                        {
                            buffer[2] = source[k];
                            ids[2] = buffer[2].Id;
                            for (int l = k + 1; l < n; l++)
                            {
                                buffer[3] = source[l];
                                ids[3] = buffer[3].Id;
                                var poffin = PoffinCooker.CookFromBerriesUnique(
                                    buffer,
                                    cookTimeSeconds,
                                    errors,
                                    amityBonus);
                                selector.Consider(in poffin, ids);
                            }
                        }
                    }
                }
                break;
            }
        }
    }

    private static void ProcessRangeWithPredicateRecipes(
        ReadOnlySpan<Berry> source,
        int choose,
        int start,
        int end,
        byte cookTimeSeconds,
        byte errors,
        byte amityBonus,
        TopKPoffinRecipeSelector selector,
        PoffinPredicate predicate)
    {
        int n = source.Length;

        switch (choose)
        {
            case 1:
            {
                Span<Berry> buffer = stackalloc Berry[1];
                Span<BerryId> ids = stackalloc BerryId[1];
                for (int i = start; i < end; i++)
                {
                    buffer[0] = source[i];
                    ids[0] = buffer[0].Id;
                    var poffin = PoffinCooker.CookFromBerriesUnique(
                        buffer,
                        cookTimeSeconds,
                        errors,
                        amityBonus);
                    if (!predicate(in poffin))
                        continue;
                    selector.Consider(in poffin, ids);
                }
                break;
            }
            case 2:
            {
                Span<Berry> buffer = stackalloc Berry[2];
                Span<BerryId> ids = stackalloc BerryId[2];
                for (int i = start; i < end; i++)
                {
                    buffer[0] = source[i];
                    ids[0] = buffer[0].Id;
                    for (int j = i + 1; j < n; j++)
                    {
                        buffer[1] = source[j];
                        ids[1] = buffer[1].Id;
                        var poffin = PoffinCooker.CookFromBerriesUnique(
                            buffer,
                            cookTimeSeconds,
                            errors,
                            amityBonus);
                        if (!predicate(in poffin))
                            continue;
                        selector.Consider(in poffin, ids);
                    }
                }
                break;
            }
            case 3:
            {
                Span<Berry> buffer = stackalloc Berry[3];
                Span<BerryId> ids = stackalloc BerryId[3];
                for (int i = start; i < end; i++)
                {
                    buffer[0] = source[i];
                    ids[0] = buffer[0].Id;
                    for (int j = i + 1; j < n - 1; j++)
                    {
                        buffer[1] = source[j];
                        ids[1] = buffer[1].Id;
                        for (int k = j + 1; k < n; k++)
                        {
                            buffer[2] = source[k];
                            ids[2] = buffer[2].Id;
                            var poffin = PoffinCooker.CookFromBerriesUnique(
                                buffer,
                                cookTimeSeconds,
                                errors,
                                amityBonus);
                            if (!predicate(in poffin))
                                continue;
                            selector.Consider(in poffin, ids);
                        }
                    }
                }
                break;
            }
            case 4:
            {
                Span<Berry> buffer = stackalloc Berry[4];
                Span<BerryId> ids = stackalloc BerryId[4];
                for (int i = start; i < end; i++)
                {
                    buffer[0] = source[i];
                    ids[0] = buffer[0].Id;
                    for (int j = i + 1; j < n - 2; j++)
                    {
                        buffer[1] = source[j];
                        ids[1] = buffer[1].Id;
                        for (int k = j + 1; k < n - 1; k++)
                        {
                            buffer[2] = source[k];
                            ids[2] = buffer[2].Id;
                            for (int l = k + 1; l < n; l++)
                            {
                                buffer[3] = source[l];
                                ids[3] = buffer[3].Id;
                                var poffin = PoffinCooker.CookFromBerriesUnique(
                                    buffer,
                                    cookTimeSeconds,
                                    errors,
                                    amityBonus);
                                if (!predicate(in poffin))
                                    continue;
                                selector.Consider(in poffin, ids);
                            }
                        }
                    }
                }
                break;
            }
        }
    }

    private static void ProcessRangeNoPredicatePrunedRecipes(
        ReadOnlySpan<Berry> source,
        int choose,
        int start,
        int end,
        byte cookTimeSeconds,
        byte errors,
        byte amityBonus,
        TopKPoffinRecipeSelector selector,
        PoffinSearchPruning pruning,
        int[] maxSpicySuffix,
        int[] maxDrySuffix,
        int[] maxSweetSuffix,
        int[] maxBitterSuffix,
        int[] maxSourSuffix,
        int[] minSmoothnessSuffix)
    {
        int n = source.Length;

        switch (choose)
        {
            case 1:
            {
                Span<Berry> buffer = stackalloc Berry[1];
                Span<BerryId> ids = stackalloc BerryId[1];
                for (int i = start; i < end; i++)
                {
                    ref readonly var b = ref source[i];
                    if (!CanSatisfy(pruning, b.Spicy, b.Dry, b.Sweet, b.Bitter, b.Sour, b.Smoothness, 0, i + 1,
                            choose, amityBonus, maxSpicySuffix, maxDrySuffix, maxSweetSuffix, maxBitterSuffix,
                            maxSourSuffix, minSmoothnessSuffix))
                        continue;

                    buffer[0] = b;
                    ids[0] = b.Id;
                    var poffin = PoffinCooker.CookFromBerriesUnique(
                        buffer,
                        cookTimeSeconds,
                        errors,
                        amityBonus);
                    selector.Consider(in poffin, ids);
                }
                break;
            }
            case 2:
            {
                Span<Berry> buffer = stackalloc Berry[2];
                Span<BerryId> ids = stackalloc BerryId[2];
                for (int i = start; i < end; i++)
                {
                    ref readonly var b0 = ref source[i];
                    int spicy0 = b0.Spicy;
                    int dry0 = b0.Dry;
                    int sweet0 = b0.Sweet;
                    int bitter0 = b0.Bitter;
                    int sour0 = b0.Sour;
                    int smooth0 = b0.Smoothness;

                    if (!CanSatisfy(pruning, spicy0, dry0, sweet0, bitter0, sour0, smooth0, 1, i + 1,
                            choose, amityBonus, maxSpicySuffix, maxDrySuffix, maxSweetSuffix, maxBitterSuffix,
                            maxSourSuffix, minSmoothnessSuffix))
                        continue;

                    buffer[0] = b0;
                    ids[0] = b0.Id;
                    for (int j = i + 1; j < n; j++)
                    {
                        ref readonly var b1 = ref source[j];
                        int spicy = spicy0 + b1.Spicy;
                        int dry = dry0 + b1.Dry;
                        int sweet = sweet0 + b1.Sweet;
                        int bitter = bitter0 + b1.Bitter;
                        int sour = sour0 + b1.Sour;
                        int smooth = smooth0 + b1.Smoothness;

                        if (!CanSatisfy(pruning, spicy, dry, sweet, bitter, sour, smooth, 0, j + 1,
                                choose, amityBonus, maxSpicySuffix, maxDrySuffix, maxSweetSuffix, maxBitterSuffix,
                                maxSourSuffix, minSmoothnessSuffix))
                            continue;

                        buffer[1] = b1;
                        ids[1] = b1.Id;
                        var poffin = PoffinCooker.CookFromBerriesUnique(
                            buffer,
                            cookTimeSeconds,
                            errors,
                            amityBonus);
                        selector.Consider(in poffin, ids);
                    }
                }
                break;
            }
            case 3:
            {
                Span<Berry> buffer = stackalloc Berry[3];
                Span<BerryId> ids = stackalloc BerryId[3];
                for (int i = start; i < end; i++)
                {
                    ref readonly var b0 = ref source[i];
                    int spicy0 = b0.Spicy;
                    int dry0 = b0.Dry;
                    int sweet0 = b0.Sweet;
                    int bitter0 = b0.Bitter;
                    int sour0 = b0.Sour;
                    int smooth0 = b0.Smoothness;

                    if (!CanSatisfy(pruning, spicy0, dry0, sweet0, bitter0, sour0, smooth0, 2, i + 1,
                            choose, amityBonus, maxSpicySuffix, maxDrySuffix, maxSweetSuffix, maxBitterSuffix,
                            maxSourSuffix, minSmoothnessSuffix))
                        continue;

                    buffer[0] = b0;
                    ids[0] = b0.Id;
                    for (int j = i + 1; j < n - 1; j++)
                    {
                        ref readonly var b1 = ref source[j];
                        int spicy1 = spicy0 + b1.Spicy;
                        int dry1 = dry0 + b1.Dry;
                        int sweet1 = sweet0 + b1.Sweet;
                        int bitter1 = bitter0 + b1.Bitter;
                        int sour1 = sour0 + b1.Sour;
                        int smooth1 = smooth0 + b1.Smoothness;

                        if (!CanSatisfy(pruning, spicy1, dry1, sweet1, bitter1, sour1, smooth1, 1, j + 1,
                                choose, amityBonus, maxSpicySuffix, maxDrySuffix, maxSweetSuffix, maxBitterSuffix,
                                maxSourSuffix, minSmoothnessSuffix))
                            continue;

                        buffer[1] = b1;
                        ids[1] = b1.Id;
                        for (int k = j + 1; k < n; k++)
                        {
                            ref readonly var b2 = ref source[k];
                            int spicy = spicy1 + b2.Spicy;
                            int dry = dry1 + b2.Dry;
                            int sweet = sweet1 + b2.Sweet;
                            int bitter = bitter1 + b2.Bitter;
                            int sour = sour1 + b2.Sour;
                            int smooth = smooth1 + b2.Smoothness;

                            if (!CanSatisfy(pruning, spicy, dry, sweet, bitter, sour, smooth, 0, k + 1,
                                    choose, amityBonus, maxSpicySuffix, maxDrySuffix, maxSweetSuffix,
                                    maxBitterSuffix, maxSourSuffix, minSmoothnessSuffix))
                                continue;

                            buffer[2] = b2;
                            ids[2] = b2.Id;
                            var poffin = PoffinCooker.CookFromBerriesUnique(
                                buffer,
                                cookTimeSeconds,
                                errors,
                                amityBonus);
                            selector.Consider(in poffin, ids);
                        }
                    }
                }
                break;
            }
            case 4:
            {
                Span<Berry> buffer = stackalloc Berry[4];
                Span<BerryId> ids = stackalloc BerryId[4];
                for (int i = start; i < end; i++)
                {
                    ref readonly var b0 = ref source[i];
                    int spicy0 = b0.Spicy;
                    int dry0 = b0.Dry;
                    int sweet0 = b0.Sweet;
                    int bitter0 = b0.Bitter;
                    int sour0 = b0.Sour;
                    int smooth0 = b0.Smoothness;

                    if (!CanSatisfy(pruning, spicy0, dry0, sweet0, bitter0, sour0, smooth0, 3, i + 1,
                            choose, amityBonus, maxSpicySuffix, maxDrySuffix, maxSweetSuffix, maxBitterSuffix,
                            maxSourSuffix, minSmoothnessSuffix))
                        continue;

                    buffer[0] = b0;
                    ids[0] = b0.Id;
                    for (int j = i + 1; j < n - 2; j++)
                    {
                        ref readonly var b1 = ref source[j];
                        int spicy1 = spicy0 + b1.Spicy;
                        int dry1 = dry0 + b1.Dry;
                        int sweet1 = sweet0 + b1.Sweet;
                        int bitter1 = bitter0 + b1.Bitter;
                        int sour1 = sour0 + b1.Sour;
                        int smooth1 = smooth0 + b1.Smoothness;

                        if (!CanSatisfy(pruning, spicy1, dry1, sweet1, bitter1, sour1, smooth1, 2, j + 1,
                                choose, amityBonus, maxSpicySuffix, maxDrySuffix, maxSweetSuffix, maxBitterSuffix,
                                maxSourSuffix, minSmoothnessSuffix))
                            continue;

                        buffer[1] = b1;
                        ids[1] = b1.Id;
                        for (int k = j + 1; k < n - 1; k++)
                        {
                            ref readonly var b2 = ref source[k];
                            int spicy2 = spicy1 + b2.Spicy;
                            int dry2 = dry1 + b2.Dry;
                            int sweet2 = sweet1 + b2.Sweet;
                            int bitter2 = bitter1 + b2.Bitter;
                            int sour2 = sour1 + b2.Sour;
                            int smooth2 = smooth1 + b2.Smoothness;

                            if (!CanSatisfy(pruning, spicy2, dry2, sweet2, bitter2, sour2, smooth2, 1, k + 1,
                                    choose, amityBonus, maxSpicySuffix, maxDrySuffix, maxSweetSuffix,
                                    maxBitterSuffix, maxSourSuffix, minSmoothnessSuffix))
                                continue;

                            buffer[2] = b2;
                            ids[2] = b2.Id;
                            for (int l = k + 1; l < n; l++)
                            {
                                ref readonly var b3 = ref source[l];
                                int spicy = spicy2 + b3.Spicy;
                                int dry = dry2 + b3.Dry;
                                int sweet = sweet2 + b3.Sweet;
                                int bitter = bitter2 + b3.Bitter;
                                int sour = sour2 + b3.Sour;
                                int smooth = smooth2 + b3.Smoothness;

                                if (!CanSatisfy(pruning, spicy, dry, sweet, bitter, sour, smooth, 0, l + 1,
                                        choose, amityBonus, maxSpicySuffix, maxDrySuffix, maxSweetSuffix,
                                        maxBitterSuffix, maxSourSuffix, minSmoothnessSuffix))
                                    continue;

                                buffer[3] = b3;
                                ids[3] = b3.Id;
                                var poffin = PoffinCooker.CookFromBerriesUnique(
                                    buffer,
                                    cookTimeSeconds,
                                    errors,
                                    amityBonus);
                                selector.Consider(in poffin, ids);
                            }
                        }
                    }
                }
                break;
            }
        }
    }

    private static void ProcessRangeWithPredicatePrunedRecipes(
        ReadOnlySpan<Berry> source,
        int choose,
        int start,
        int end,
        byte cookTimeSeconds,
        byte errors,
        byte amityBonus,
        TopKPoffinRecipeSelector selector,
        PoffinPredicate predicate,
        PoffinSearchPruning pruning,
        int[] maxSpicySuffix,
        int[] maxDrySuffix,
        int[] maxSweetSuffix,
        int[] maxBitterSuffix,
        int[] maxSourSuffix,
        int[] minSmoothnessSuffix)
    {
        int n = source.Length;

        switch (choose)
        {
            case 1:
            {
                Span<Berry> buffer = stackalloc Berry[1];
                Span<BerryId> ids = stackalloc BerryId[1];
                for (int i = start; i < end; i++)
                {
                    ref readonly var b = ref source[i];
                    if (!CanSatisfy(pruning, b.Spicy, b.Dry, b.Sweet, b.Bitter, b.Sour, b.Smoothness, 0, i + 1,
                            choose, amityBonus, maxSpicySuffix, maxDrySuffix, maxSweetSuffix, maxBitterSuffix,
                            maxSourSuffix, minSmoothnessSuffix))
                        continue;

                    buffer[0] = b;
                    ids[0] = b.Id;
                    var poffin = PoffinCooker.CookFromBerriesUnique(
                        buffer,
                        cookTimeSeconds,
                        errors,
                        amityBonus);
                    if (!predicate(in poffin))
                        continue;
                    selector.Consider(in poffin, ids);
                }
                break;
            }
            case 2:
            {
                Span<Berry> buffer = stackalloc Berry[2];
                Span<BerryId> ids = stackalloc BerryId[2];
                for (int i = start; i < end; i++)
                {
                    ref readonly var b0 = ref source[i];
                    int spicy0 = b0.Spicy;
                    int dry0 = b0.Dry;
                    int sweet0 = b0.Sweet;
                    int bitter0 = b0.Bitter;
                    int sour0 = b0.Sour;
                    int smooth0 = b0.Smoothness;

                    if (!CanSatisfy(pruning, spicy0, dry0, sweet0, bitter0, sour0, smooth0, 1, i + 1,
                            choose, amityBonus, maxSpicySuffix, maxDrySuffix, maxSweetSuffix, maxBitterSuffix,
                            maxSourSuffix, minSmoothnessSuffix))
                        continue;

                    buffer[0] = b0;
                    ids[0] = b0.Id;
                    for (int j = i + 1; j < n; j++)
                    {
                        ref readonly var b1 = ref source[j];
                        int spicy = spicy0 + b1.Spicy;
                        int dry = dry0 + b1.Dry;
                        int sweet = sweet0 + b1.Sweet;
                        int bitter = bitter0 + b1.Bitter;
                        int sour = sour0 + b1.Sour;
                        int smooth = smooth0 + b1.Smoothness;

                        if (!CanSatisfy(pruning, spicy, dry, sweet, bitter, sour, smooth, 0, j + 1,
                                choose, amityBonus, maxSpicySuffix, maxDrySuffix, maxSweetSuffix, maxBitterSuffix,
                                maxSourSuffix, minSmoothnessSuffix))
                            continue;

                        buffer[1] = b1;
                        ids[1] = b1.Id;
                        var poffin = PoffinCooker.CookFromBerriesUnique(
                            buffer,
                            cookTimeSeconds,
                            errors,
                            amityBonus);
                        if (!predicate(in poffin))
                            continue;
                        selector.Consider(in poffin, ids);
                    }
                }
                break;
            }
            case 3:
            {
                Span<Berry> buffer = stackalloc Berry[3];
                Span<BerryId> ids = stackalloc BerryId[3];
                for (int i = start; i < end; i++)
                {
                    ref readonly var b0 = ref source[i];
                    int spicy0 = b0.Spicy;
                    int dry0 = b0.Dry;
                    int sweet0 = b0.Sweet;
                    int bitter0 = b0.Bitter;
                    int sour0 = b0.Sour;
                    int smooth0 = b0.Smoothness;

                    if (!CanSatisfy(pruning, spicy0, dry0, sweet0, bitter0, sour0, smooth0, 2, i + 1,
                            choose, amityBonus, maxSpicySuffix, maxDrySuffix, maxSweetSuffix, maxBitterSuffix,
                            maxSourSuffix, minSmoothnessSuffix))
                        continue;

                    buffer[0] = b0;
                    ids[0] = b0.Id;
                    for (int j = i + 1; j < n - 1; j++)
                    {
                        ref readonly var b1 = ref source[j];
                        int spicy1 = spicy0 + b1.Spicy;
                        int dry1 = dry0 + b1.Dry;
                        int sweet1 = sweet0 + b1.Sweet;
                        int bitter1 = bitter0 + b1.Bitter;
                        int sour1 = sour0 + b1.Sour;
                        int smooth1 = smooth0 + b1.Smoothness;

                        if (!CanSatisfy(pruning, spicy1, dry1, sweet1, bitter1, sour1, smooth1, 1, j + 1,
                                choose, amityBonus, maxSpicySuffix, maxDrySuffix, maxSweetSuffix, maxBitterSuffix,
                                maxSourSuffix, minSmoothnessSuffix))
                            continue;

                        buffer[1] = b1;
                        ids[1] = b1.Id;
                        for (int k = j + 1; k < n; k++)
                        {
                            ref readonly var b2 = ref source[k];
                            int spicy = spicy1 + b2.Spicy;
                            int dry = dry1 + b2.Dry;
                            int sweet = sweet1 + b2.Sweet;
                            int bitter = bitter1 + b2.Bitter;
                            int sour = sour1 + b2.Sour;
                            int smooth = smooth1 + b2.Smoothness;

                            if (!CanSatisfy(pruning, spicy, dry, sweet, bitter, sour, smooth, 0, k + 1,
                                    choose, amityBonus, maxSpicySuffix, maxDrySuffix, maxSweetSuffix,
                                    maxBitterSuffix, maxSourSuffix, minSmoothnessSuffix))
                                continue;

                            buffer[2] = b2;
                            ids[2] = b2.Id;
                            var poffin = PoffinCooker.CookFromBerriesUnique(
                                buffer,
                                cookTimeSeconds,
                                errors,
                                amityBonus);
                            if (!predicate(in poffin))
                                continue;
                            selector.Consider(in poffin, ids);
                        }
                    }
                }
                break;
            }
            case 4:
            {
                Span<Berry> buffer = stackalloc Berry[4];
                Span<BerryId> ids = stackalloc BerryId[4];
                for (int i = start; i < end; i++)
                {
                    ref readonly var b0 = ref source[i];
                    int spicy0 = b0.Spicy;
                    int dry0 = b0.Dry;
                    int sweet0 = b0.Sweet;
                    int bitter0 = b0.Bitter;
                    int sour0 = b0.Sour;
                    int smooth0 = b0.Smoothness;

                    if (!CanSatisfy(pruning, spicy0, dry0, sweet0, bitter0, sour0, smooth0, 3, i + 1,
                            choose, amityBonus, maxSpicySuffix, maxDrySuffix, maxSweetSuffix, maxBitterSuffix,
                            maxSourSuffix, minSmoothnessSuffix))
                        continue;

                    buffer[0] = b0;
                    ids[0] = b0.Id;
                    for (int j = i + 1; j < n - 2; j++)
                    {
                        ref readonly var b1 = ref source[j];
                        int spicy1 = spicy0 + b1.Spicy;
                        int dry1 = dry0 + b1.Dry;
                        int sweet1 = sweet0 + b1.Sweet;
                        int bitter1 = bitter0 + b1.Bitter;
                        int sour1 = sour0 + b1.Sour;
                        int smooth1 = smooth0 + b1.Smoothness;

                        if (!CanSatisfy(pruning, spicy1, dry1, sweet1, bitter1, sour1, smooth1, 2, j + 1,
                                choose, amityBonus, maxSpicySuffix, maxDrySuffix, maxSweetSuffix, maxBitterSuffix,
                                maxSourSuffix, minSmoothnessSuffix))
                            continue;

                        buffer[1] = b1;
                        ids[1] = b1.Id;
                        for (int k = j + 1; k < n - 1; k++)
                        {
                            ref readonly var b2 = ref source[k];
                            int spicy2 = spicy1 + b2.Spicy;
                            int dry2 = dry1 + b2.Dry;
                            int sweet2 = sweet1 + b2.Sweet;
                            int bitter2 = bitter1 + b2.Bitter;
                            int sour2 = sour1 + b2.Sour;
                            int smooth2 = smooth1 + b2.Smoothness;

                            if (!CanSatisfy(pruning, spicy2, dry2, sweet2, bitter2, sour2, smooth2, 1, k + 1,
                                    choose, amityBonus, maxSpicySuffix, maxDrySuffix, maxSweetSuffix,
                                    maxBitterSuffix, maxSourSuffix, minSmoothnessSuffix))
                                continue;

                            buffer[2] = b2;
                            ids[2] = b2.Id;
                            for (int l = k + 1; l < n; l++)
                            {
                                ref readonly var b3 = ref source[l];
                                int spicy = spicy2 + b3.Spicy;
                                int dry = dry2 + b3.Dry;
                                int sweet = sweet2 + b3.Sweet;
                                int bitter = bitter2 + b3.Bitter;
                                int sour = sour2 + b3.Sour;
                                int smooth = smooth2 + b3.Smoothness;

                                if (!CanSatisfy(pruning, spicy, dry, sweet, bitter, sour, smooth, 0, l + 1,
                                        choose, amityBonus, maxSpicySuffix, maxDrySuffix, maxSweetSuffix,
                                        maxBitterSuffix, maxSourSuffix, minSmoothnessSuffix))
                                    continue;

                                buffer[3] = b3;
                                ids[3] = b3.Id;
                                var poffin = PoffinCooker.CookFromBerriesUnique(
                                    buffer,
                                    cookTimeSeconds,
                                    errors,
                                    amityBonus);
                                if (!predicate(in poffin))
                                    continue;
                                selector.Consider(in poffin, ids);
                            }
                        }
                    }
                }
                break;
            }
        }
    }

    // Conservative check: returns false only if the combination cannot meet the thresholds.
    private static bool CanSatisfy(
        in PoffinSearchPruning pruning,
        int spicySum,
        int drySum,
        int sweetSum,
        int bitterSum,
        int sourSum,
        int smoothnessSum,
        int remainingSlots,
        int nextIndex,
        int totalBerries,
        byte amityBonus,
        int[] maxSpicySuffix,
        int[] maxDrySuffix,
        int[] maxSweetSuffix,
        int[] maxBitterSuffix,
        int[] maxSourSuffix,
        int[] minSmoothnessSuffix)
    {
        if (remainingSlots > 0)
        {
            int sourceLength = maxSpicySuffix.Length - 1;
            if (nextIndex >= sourceLength)
                return false;
        }

        // Upper bounds for flavor totals (optimistic best case).
        int spicyMax = spicySum + maxSpicySuffix[nextIndex] * remainingSlots;
        int dryMax = drySum + maxDrySuffix[nextIndex] * remainingSlots;
        int sweetMax = sweetSum + maxSweetSuffix[nextIndex] * remainingSlots;
        int bitterMax = bitterSum + maxBitterSuffix[nextIndex] * remainingSlots;
        int sourMax = sourSum + maxSourSuffix[nextIndex] * remainingSlots;

        if (pruning.HasMinSpicy && spicyMax < pruning.MinSpicy) return false;
        if (pruning.HasMinDry && dryMax < pruning.MinDry) return false;
        if (pruning.HasMinSweet && sweetMax < pruning.MinSweet) return false;
        if (pruning.HasMinBitter && bitterMax < pruning.MinBitter) return false;
        if (pruning.HasMinSour && sourMax < pruning.MinSour) return false;

        if (pruning.HasMinLevel)
        {
            int maxLevel = spicyMax;
            if (dryMax > maxLevel) maxLevel = dryMax;
            if (sweetMax > maxLevel) maxLevel = sweetMax;
            if (bitterMax > maxLevel) maxLevel = bitterMax;
            if (sourMax > maxLevel) maxLevel = sourMax;
            if (maxLevel < pruning.MinLevel) return false;
        }

        if (pruning.HasMaxSmoothness)
        {
            // Lower bound for smoothness (optimistic best case).
            int minSmoothnessSum = smoothnessSum;
            if (remainingSlots > 0)
            {
                int minPer = minSmoothnessSuffix[nextIndex];
                if (minPer != int.MaxValue)
                    minSmoothnessSum += minPer * remainingSlots;
            }

            int bestSmoothness = (minSmoothnessSum / totalBerries) - totalBerries - amityBonus;
            if (bestSmoothness > pruning.MaxSmoothness) return false;
        }

        return true;
    }
}
