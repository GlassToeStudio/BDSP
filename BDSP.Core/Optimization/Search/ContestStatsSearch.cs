using System;
using System.Threading.Tasks;
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

            var pruned = FeedingCandidatePruner.Prune(candidates, scoreOptions.RarityCostMode);
            if (pruned.Length == 0)
            {
                return Array.Empty<ContestStatsResult>();
            }

            int[] rarityCosts = new int[pruned.Length];
            for (int i = 0; i < pruned.Length; i++)
            {
                rarityCosts[i] = ComputeRarityCost(in pruned[i].Recipe, scoreOptions.RarityCostMode);
            }

            var collector = new TopK<ContestStatsResult>(topK);
            ContestStatsSearchOptions searchOptionsValue = searchOptions;
            FeedingSearchOptions scoreOptionsValue = scoreOptions;
            bool shouldParallel = searchOptionsValue.UseParallel && pruned.Length >= 64;

            if (!shouldParallel)
            {
                EvaluateSequential(pruned, rarityCosts, searchOptionsValue, scoreOptionsValue, collector);
                return collector.ToSortedArray(CompareResults);
            }

            var options = new ParallelOptions();
            if (searchOptionsValue.MaxDegreeOfParallelism.HasValue)
            {
                options.MaxDegreeOfParallelism = searchOptionsValue.MaxDegreeOfParallelism.Value;
            }

            object gate = new object();
            int choose = searchOptionsValue.Choose;

            Parallel.For(
                0,
                pruned.Length,
                options,
                () => new TopK<ContestStatsResult>(topK),
                (i, _, local) =>
                {
                    EvaluateFromIndex(pruned, rarityCosts, i, choose, searchOptionsValue, scoreOptionsValue, local);
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

        private static void EvaluateSequential(
            PoffinWithRecipe[] candidates,
            int[] rarityCosts,
            ContestStatsSearchOptions searchOptions,
            FeedingSearchOptions scoreOptions,
            TopK<ContestStatsResult> collector)
        {
            int choose = searchOptions.Choose;
            for (int i = 0; i < candidates.Length; i++)
            {
                EvaluateFromIndex(candidates, rarityCosts, i, choose, searchOptions, scoreOptions, collector);
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

            for (int j = 0; j < candidates.Length; j++)
            {
                if (j == i) continue;
                if (choose == 2)
                {
                    ApplyPermutation(candidates, rarityCosts, i, j, -1, -1, 2, searchOptions, scoreOptions, collector);
                    continue;
                }

                for (int k = 0; k < candidates.Length; k++)
                {
                    if (k == i || k == j) continue;
                    if (choose == 3)
                    {
                        ApplyPermutation(candidates, rarityCosts, i, j, k, -1, 3, searchOptions, scoreOptions, collector);
                        continue;
                    }

                    for (int l = 0; l < candidates.Length; l++)
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

            if (i >= 0)
            {
                stats = FeedingApplier.Apply(in stats, in candidates[i].Poffin);
                totalRarity += rarityCosts[i];
                totalSheen = stats.Sheen;
                poffinsEaten++;
                if (totalSheen >= 255)
                {
                    AddResult();
                    return;
                }
            }

            if (j >= 0)
            {
                stats = FeedingApplier.Apply(in stats, in candidates[j].Poffin);
                totalRarity += rarityCosts[j];
                totalSheen = stats.Sheen;
                poffinsEaten++;
                if (totalSheen >= 255)
                {
                    AddResult();
                    return;
                }
            }

            if (k >= 0)
            {
                stats = FeedingApplier.Apply(in stats, in candidates[k].Poffin);
                totalRarity += rarityCosts[k];
                totalSheen = stats.Sheen;
                poffinsEaten++;
                if (totalSheen >= 255)
                {
                    AddResult();
                    return;
                }
            }

            if (l >= 0)
            {
                stats = FeedingApplier.Apply(in stats, in candidates[l].Poffin);
                totalRarity += rarityCosts[l];
                totalSheen = stats.Sheen;
                poffinsEaten++;
                if (totalSheen >= 255)
                {
                    AddResult();
                    return;
                }
            }

            AddResult();

            void AddResult()
            {
                int score = ScorePlan(in stats, totalRarity, poffinsEaten, totalSheen, scoreOptions);
                var indices = new PoffinIndexSet(i, j, k, l, count);
                collector.TryAdd(new ContestStatsResult(indices, stats, poffinsEaten, totalRarity, totalSheen, score), score);
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
            score -= poffinCount * options.PoffinCountPenalty;
            score -= totalSheen * options.SheenPenalty;
            score -= totalRarityCost * options.RarityPenalty;
            return score;
        }

        private static int CompareResults(ContestStatsResult left, ContestStatsResult right)
        {
            return right.Score.CompareTo(left.Score);
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

        public ContestStatsResult(
            PoffinIndexSet indices,
            ContestStats stats,
            int poffinsEaten,
            int totalRarityCost,
            int totalSheen,
            int score)
        {
            Indices = indices;
            Stats = stats;
            PoffinsEaten = poffinsEaten;
            TotalRarityCost = totalRarityCost;
            TotalSheen = totalSheen;
            Score = score;
        }
    }
}
