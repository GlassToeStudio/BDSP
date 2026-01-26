using System;
using System.Threading;
using System.Threading.Tasks;
using System.Numerics;
using BDSP.Core.Berries;
using BDSP.Core.Optimization.Core;
using BDSP.Core.Optimization.Filters;
using BDSP.Core.Poffins;
using BDSP.Core.Poffins.Search;

namespace BDSP.Core.Optimization.Search
{
    /// <summary>
    /// Searches ordered poffin permutations and returns top-ranked contest stat results.
    /// </summary>
    public static class ContestStatsSearch
    {
        /// <summary>
        /// Runs a contest-stat search over ordered poffin permutations (no repetition).
        /// </summary>
        /// <param name="candidates">Candidate poffins with recipes.</param>
        /// <param name="searchOptions">Permutation and execution options.</param>
        /// <param name="scoreOptions">Scoring weights and rarity mode.</param>
        /// <param name="topK">Maximum results to keep.</param>
        public static ContestStatsResult[] Run(
            ReadOnlySpan<PoffinWithRecipe> candidates,
            in ContestStatsSearchOptions searchOptions,
            in FeedingSearchOptions scoreOptions,
            int topK = 50)
        {
            if (candidates.Length == 0)
            {
                return Array.Empty<ContestStatsResult>();
            }

            if (searchOptions.Choose < 1 || searchOptions.Choose > 4)
            {
                throw new ArgumentOutOfRangeException(nameof(searchOptions.Choose), "Choose must be 1-4.");
            }

            PoffinWithRecipe[] pruned;
            if (searchOptions.PruneCandidates)
            {
                pruned = FeedingCandidatePruner.Prune(candidates, scoreOptions.RarityCostMode);
                if (pruned.Length == 0)
                {
                    return Array.Empty<ContestStatsResult>();
                }
            }
            else
            {
                pruned = candidates.ToArray();
            }

            int[]? rentedRarityCosts = null;
            int[] rarityCosts;
            if (pruned.Length <= 256)
            {
                rarityCosts = new int[pruned.Length];
            }
            else
            {
                rentedRarityCosts = System.Buffers.ArrayPool<int>.Shared.Rent(pruned.Length);
                rarityCosts = rentedRarityCosts;
            }

            for (int i = 0; i < pruned.Length; i++)
            {
                rarityCosts[i] = ComputeRarityCost(in pruned[i].Recipe, scoreOptions.RarityCostMode);
            }

            var collector = new TopK<ContestStatsResult>(topK);
            ContestStatsSearchOptions searchOptionsValue = searchOptions;
            FeedingSearchOptions scoreOptionsValue = scoreOptions;
            bool shouldParallel = searchOptionsValue.UseParallel && pruned.Length >= 64;
            Action<ContestSearchProgress>? progress = searchOptionsValue.Progress;
            int progressInterval = searchOptionsValue.ProgressInterval;
            if (progressInterval < 0) progressInterval = 0;

            if (!shouldParallel)
            {
                try
                {
                    EvaluateSequential(pruned, rarityCosts, searchOptionsValue, scoreOptionsValue, collector, progress, progressInterval);
                    return collector.ToSortedArray(CompareResults);
                }
                finally
                {
                    if (rentedRarityCosts is not null)
                    {
                        System.Buffers.ArrayPool<int>.Shared.Return(rentedRarityCosts);
                    }
                }
            }

            var options = new ParallelOptions();
            if (searchOptionsValue.MaxDegreeOfParallelism.HasValue)
            {
                options.MaxDegreeOfParallelism = searchOptionsValue.MaxDegreeOfParallelism.Value;
            }

            object gate = new object();
            int choose = searchOptionsValue.Choose;
            int totalOuter = pruned.Length;
            int completedOuter = 0;

            try
            {
                Parallel.For(
                    0,
                    pruned.Length,
                    options,
                    () => new TopK<ContestStatsResult>(topK),
                    (i, _, local) =>
                    {
                        EvaluateFromIndex(pruned, rarityCosts, i, choose, searchOptionsValue, scoreOptionsValue, local);
                        if (progress is not null && progressInterval > 0)
                        {
                            int done = Interlocked.Increment(ref completedOuter);
                            if (done % progressInterval == 0 || done == totalOuter)
                            {
                                progress(new ContestSearchProgress(done, totalOuter, choose, isParallel: true, candidateCount: totalOuter));
                            }
                        }
                        return local;
                    },
                    local =>
                    {
                        lock (gate)
                        {
                            collector.MergeFrom(local);
                        }
                    });

                return collector.ToSortedArray(CompareResults);
            }
            finally
            {
                if (rentedRarityCosts is not null)
                {
                    System.Buffers.ArrayPool<int>.Shared.Return(rentedRarityCosts);
                }
            }
        }

        private static void EvaluateSequential(
            PoffinWithRecipe[] candidates,
            int[] rarityCosts,
            ContestStatsSearchOptions searchOptions,
            FeedingSearchOptions scoreOptions,
            TopK<ContestStatsResult> collector,
            Action<ContestSearchProgress>? progress,
            int progressInterval)
        {
            int choose = searchOptions.Choose;
            int totalOuter = candidates.Length;
            for (int i = 0; i < candidates.Length; i++)
            {
                EvaluateFromIndex(candidates, rarityCosts, i, choose, searchOptions, scoreOptions, collector);
                if (progress is not null && progressInterval > 0)
                {
                    int done = i + 1;
                    if (done % progressInterval == 0 || done == totalOuter)
                    {
                        progress(new ContestSearchProgress(done, totalOuter, choose, isParallel: false, candidateCount: totalOuter));
                    }
                }
            }
        }

        private static void EvaluateFromIndex(
            PoffinWithRecipe[] candidates,
            int[] rarityCosts,
            int i,
            int choose,
            ContestStatsSearchOptions searchOptions,
            FeedingSearchOptions scoreOptions,
            TopK<ContestStatsResult> collector)
        {
            if (choose == 1)
            {
                ApplyPermutation(candidates, rarityCosts, i, -1, -1, -1, 1, searchOptions, scoreOptions, collector);
                return;
            }

            if (searchOptions.MaxPoffins > 0 && searchOptions.MaxPoffins <= choose)
            {
                int fastCount = candidates.Length;
                for (int j = 0; j < fastCount; j++)
                {
                    if (j == i) continue;
                    if (choose == 2)
                    {
                        ApplyPermutation(candidates, rarityCosts, i, j, -1, -1, 2, searchOptions, scoreOptions, collector);
                        continue;
                    }

                    for (int k = 0; k < fastCount; k++)
                    {
                        if (k == i || k == j) continue;
                        if (choose == 3)
                        {
                            ApplyPermutation(candidates, rarityCosts, i, j, k, -1, 3, searchOptions, scoreOptions, collector);
                            continue;
                        }

                        for (int l = 0; l < fastCount; l++)
                        {
                            if (l == i || l == j || l == k) continue;
                            ApplyPermutation(candidates, rarityCosts, i, j, k, l, 4, searchOptions, scoreOptions, collector);
                        }
                    }
                }

                return;
            }

            int count = candidates.Length;
            for (int j = 0; j < count; j++)
            {
                if (j == i) continue;
                if (choose == 2)
                {
                    ApplyPermutation(candidates, rarityCosts, i, j, -1, -1, 2, searchOptions, scoreOptions, collector);
                    continue;
                }

                for (int k = 0; k < count; k++)
                {
                    if (k == i || k == j) continue;
                    if (choose == 3)
                    {
                        ApplyPermutation(candidates, rarityCosts, i, j, k, -1, 3, searchOptions, scoreOptions, collector);
                        continue;
                    }

                    for (int l = 0; l < count; l++)
                    {
                        if (l == i || l == j || l == k) continue;
                        ApplyPermutation(candidates, rarityCosts, i, j, k, l, 4, searchOptions, scoreOptions, collector);
                    }
                }
            }
        }

        private static void ApplyPermutation(
            PoffinWithRecipe[] candidates,
            int[] rarityCosts,
            int i,
            int j,
            int k,
            int l,
            int count,
            ContestStatsSearchOptions searchOptions,
            FeedingSearchOptions scoreOptions,
            TopK<ContestStatsResult> collector)
        {
            ContestStats stats = searchOptions.Start;
            int totalRarity = 0;
            int totalSheen = stats.Sheen;
            int poffinsEaten = 0;
            int perfectCount = CountPerfect(in stats);
            int poffinsToMaxStats = perfectCount == 5 ? 0 : -1;
            int maxPoffins = searchOptions.MaxPoffins > 0 ? searchOptions.MaxPoffins : int.MaxValue;

            Span<int> indices = stackalloc int[4];
            int indexCount = 0;
            if (i >= 0) indices[indexCount++] = i;
            if (j >= 0) indices[indexCount++] = j;
            if (k >= 0) indices[indexCount++] = k;
            if (l >= 0) indices[indexCount++] = l;

            totalRarity = SumRarityCosts(indices, indexCount, rarityCosts);

            while (poffinsEaten < maxPoffins && totalSheen < 255)
            {
                int sheenBeforeCycle = totalSheen;
                for (int idx = 0; idx < indexCount && poffinsEaten < maxPoffins; idx++)
                {
                    int pos = indices[idx];
                    ContestStats before = stats;
                    stats = FeedingApplier.Apply(in stats, in candidates[pos].Poffin);
                    totalSheen = stats.Sheen;
                    poffinsEaten++;
                    perfectCount = UpdatePerfectCount(perfectCount, in before, in stats);
                    if (poffinsToMaxStats < 0 && perfectCount == 5)
                    {
                        poffinsToMaxStats = poffinsEaten;
                    }
                    if (totalSheen >= 255)
                    {
                        break;
                    }
                }

                if (totalSheen == sheenBeforeCycle)
                {
                    break;
                }
            }

            if (poffinsToMaxStats < 0 && perfectCount == 5)
            {
                poffinsToMaxStats = poffinsEaten;
            }
            else if (poffinsToMaxStats < 0)
            {
                poffinsToMaxStats = 0;
            }

            AddResult();

            void AddResult()
            {
                int score = ScorePlan(in stats, totalRarity, poffinsEaten, totalSheen, scoreOptions);
                var indices = new PoffinIndexSet(i, j, k, l, count);
                int numPerfect = perfectCount;
                int rank = RankFromCounts(perfectCount, totalSheen);
                int uniqueBerries = CountUniqueBerries(candidates, i, j, k, l);
                collector.TryAdd(new ContestStatsResult(indices, stats, poffinsEaten, totalRarity, totalSheen, score, numPerfect, rank, uniqueBerries, poffinsToMaxStats), score);
            }
        }

        private static int ComputeRarityCost(in PoffinRecipe recipe, RarityCostMode mode)
        {
            int cost = 0;
            for (int i = 0; i < recipe.Berries.Length; i++)
            {
                ref readonly var berry = ref BerryTable.Get(recipe.Berries[i]);
                int rarity = berry.Rarity;
                if (mode == RarityCostMode.MaxBerryRarity)
                {
                    if (rarity > cost) cost = rarity;
                }
                else
                {
                    cost += rarity;
                }
            }
            return cost;
        }

        private static int ScorePlan(
            in ContestStats finalStats,
            int totalRarityCost,
            int poffinCount,
            int totalSheen,
            FeedingSearchOptions options)
        {
            int statSum = finalStats.Coolness +
                         finalStats.Beauty +
                         finalStats.Cuteness +
                         finalStats.Cleverness +
                         finalStats.Toughness;

            int score = statSum * options.StatsWeight;
            if (options.ScoreMode == ContestScoreMode.Balanced)
            {
                int minStat = finalStats.Coolness;
                if (finalStats.Beauty < minStat) minStat = finalStats.Beauty;
                if (finalStats.Cuteness < minStat) minStat = finalStats.Cuteness;
                if (finalStats.Cleverness < minStat) minStat = finalStats.Cleverness;
                if (finalStats.Toughness < minStat) minStat = finalStats.Toughness;
                score += minStat * options.MinStatWeight;
            }
            score -= poffinCount * options.PoffinCountPenalty;
            score -= totalSheen * options.SheenPenalty;
            score -= totalRarityCost * options.RarityPenalty;
            return score;
        }

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        private static int CompareResults(ContestStatsResult left, ContestStatsResult right)
        {
            return right.Score.CompareTo(left.Score);
        }

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        private static int SumRarityCosts(ReadOnlySpan<int> indices, int count, int[] rarityCosts)
        {
            int total = 0;
            for (int i = 0; i < count; i++)
            {
                total += rarityCosts[indices[i]];
            }
            return total;
        }

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        private static int CountPerfect(in ContestStats stats)
        {
            int count = 0;
            if (stats.Coolness >= 255) count++;
            if (stats.Beauty >= 255) count++;
            if (stats.Cuteness >= 255) count++;
            if (stats.Cleverness >= 255) count++;
            if (stats.Toughness >= 255) count++;
            return count;
        }

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        private static int RankFromStats(in ContestStats stats)
        {
            int perfect = CountPerfect(in stats);
            if (perfect == 5 && stats.Sheen >= 255) return 1;
            if (perfect == 5) return 2;
            return 3;
        }

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        private static int RankFromCounts(int perfectCount, int sheen)
        {
            if (perfectCount == 5 && sheen >= 255) return 1;
            if (perfectCount == 5) return 2;
            return 3;
        }

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        private static int UpdatePerfectCount(int current, in ContestStats before, in ContestStats after)
        {
            int count = current;
            if (before.Coolness < 255 && after.Coolness >= 255) count++;
            if (before.Beauty < 255 && after.Beauty >= 255) count++;
            if (before.Cuteness < 255 && after.Cuteness >= 255) count++;
            if (before.Cleverness < 255 && after.Cleverness >= 255) count++;
            if (before.Toughness < 255 && after.Toughness >= 255) count++;
            return count;
        }

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        private static int CountUniqueBerries(PoffinWithRecipe[] candidates, int i, int j, int k, int l)
        {
            ulong mask0 = 0;
            ulong mask1 = 0;

            if (i >= 0) AddRecipeBits(candidates[i].Recipe.Berries, ref mask0, ref mask1);
            if (j >= 0) AddRecipeBits(candidates[j].Recipe.Berries, ref mask0, ref mask1);
            if (k >= 0) AddRecipeBits(candidates[k].Recipe.Berries, ref mask0, ref mask1);
            if (l >= 0) AddRecipeBits(candidates[l].Recipe.Berries, ref mask0, ref mask1);

            return BitOperations.PopCount(mask0) + BitOperations.PopCount(mask1);
        }

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        private static void AddRecipeBits(BerryId[] berries, ref ulong mask0, ref ulong mask1)
        {
            for (int i = 0; i < berries.Length; i++)
            {
                int id = berries[i].Value;
                if (id < 64)
                {
                    mask0 |= 1UL << id;
                }
                else
                {
                    mask1 |= 1UL << (id - 64);
                }
            }
        }
    }

    /// <summary>
    /// Compact index set for a poffin permutation (1-4 items).
    /// </summary>
    public readonly struct PoffinIndexSet
    {
        public readonly int I0;
        public readonly int I1;
        public readonly int I2;
        public readonly int I3;
        public readonly int Count;

        public PoffinIndexSet(int i0, int i1, int i2, int i3, int count)
        {
            I0 = i0;
            I1 = i1;
            I2 = i2;
            I3 = i3;
            Count = count;
        }
    }

    /// <summary>
    /// Lightweight result for contest-stat search.
    /// </summary>
    public readonly struct ContestStatsResult
    {
        public readonly PoffinIndexSet Indices;
        public readonly ContestStats Stats;
        public readonly int PoffinsEaten;
        public readonly int TotalRarityCost;
        public readonly int TotalSheen;
        public readonly int Score;
        public readonly int NumPerfectValues;
        public readonly int Rank;
        public readonly int UniqueBerries;
        public readonly int PoffinsToMaxStats;

        public ContestStatsResult(
            PoffinIndexSet indices,
            ContestStats stats,
            int poffinsEaten,
            int totalRarityCost,
            int totalSheen,
            int score,
            int numPerfectValues,
            int rank,
            int uniqueBerries,
            int poffinsToMaxStats)
        {
            Indices = indices;
            Stats = stats;
            PoffinsEaten = poffinsEaten;
            TotalRarityCost = totalRarityCost;
            TotalSheen = totalSheen;
            Score = score;
            NumPerfectValues = numPerfectValues;
            Rank = rank;
            UniqueBerries = uniqueBerries;
            PoffinsToMaxStats = poffinsToMaxStats;
        }
    }
}
