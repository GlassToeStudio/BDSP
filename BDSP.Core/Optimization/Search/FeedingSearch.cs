using System;
using System.Collections.Generic;
using BDSP.Core.Berries;
using BDSP.Core.Optimization.Core;
using BDSP.Core.Optimization.Filters;
using BDSP.Core.Poffins;

namespace BDSP.Core.Optimization.Search
{
    /// <summary>
    /// Baseline feeding search that consumes poffins in low-smoothness order.
    /// </summary>
    public static class FeedingSearch
    {
        /// <summary>
        /// Builds a feeding plan using a simple heuristic:
        /// lowest smoothness first, then lower rarity cost, then higher level.
        /// </summary>
        /// <param name="candidates">Poffins with recipe metadata.</param>
        /// <param name="options">Scoring and rarity options.</param>
        /// <param name="start">Optional starting contest stats (default = all zeros).</param>
        public static FeedingPlanResult BuildPlan(ReadOnlySpan<PoffinWithRecipe> candidates, in FeedingSearchOptions options, ContestStats start = default)
        {
            if (candidates.Length == 0)
            {
                return new FeedingPlanResult(
                    Array.Empty<FeedingStep>(),
                    start,
                    numPerfectValues: CountPerfect(in start),
                    rank: RankFromStats(in start),
                    uniqueBerries: 0,
                    totalRarityCost: 0,
                    totalPoffins: 0,
                    totalSheen: start.Sheen,
                    score: 0);
            }

            var ordered = FeedingCandidatePruner.Prune(candidates, options.RarityCostMode);
            if (ordered.Length == 0)
            {
                return new FeedingPlanResult(
                    Array.Empty<FeedingStep>(),
                    start,
                    numPerfectValues: CountPerfect(in start),
                    rank: RankFromStats(in start),
                    uniqueBerries: 0,
                    totalRarityCost: 0,
                    totalPoffins: 0,
                    totalSheen: start.Sheen,
                    score: 0);
            }

            Array.Sort(ordered, CompareCandidates);

            var steps = new List<FeedingStep>(ordered.Length);
            ContestStats current = start;
            int totalRarityCost = 0;
            int totalSheen = start.Sheen;

            for (int i = 0; i < ordered.Length; i++)
            {
                if (current.Sheen >= 255)
                {
                    break;
                }

                PoffinWithRecipe candidate = ordered[i];
                ContestStats before = current;
                ContestStats after = FeedingApplier.Apply(in before, in candidate.Poffin);

                int rarityCost = ComputeRarityCost(in candidate.Recipe, options.RarityCostMode);
                totalRarityCost += rarityCost;
                totalSheen = after.Sheen;

                steps.Add(new FeedingStep(steps.Count, candidate, before, after));
                current = after;
                if (CountPerfect(in current) == 5)
                {
                    break;
                }
            }

            int score = ScorePlan(in current, totalRarityCost, steps.Count, totalSheen, in options);
            int numPerfect = CountPerfect(in current);
            int rank = RankFromStats(in current);
            int uniqueBerries = CountUniqueBerries(steps);

            return new FeedingPlanResult(
                steps.ToArray(),
                current,
                numPerfect,
                rank,
                uniqueBerries,
                totalRarityCost,
                steps.Count,
                totalSheen,
                score);
        }

        private static int CompareCandidates(PoffinWithRecipe left, PoffinWithRecipe right)
        {
            int smooth = left.Poffin.Smoothness.CompareTo(right.Poffin.Smoothness);
            if (smooth != 0) return smooth;

            int rarity = ComputeRarityCost(in left.Recipe, RarityCostMode.MaxBerryRarity)
                .CompareTo(ComputeRarityCost(in right.Recipe, RarityCostMode.MaxBerryRarity));
            if (rarity != 0) return rarity;

            return right.Poffin.Level.CompareTo(left.Poffin.Level);
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

        private static int ScorePlan(in ContestStats finalStats, int totalRarityCost, int poffinCount, int totalSheen, in FeedingSearchOptions options)
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

        private static int RankFromStats(in ContestStats stats)
        {
            int perfect = CountPerfect(in stats);
            if (perfect == 5 && stats.Sheen >= 255) return 1;
            if (perfect == 5) return 2;
            return 3;
        }

        private static int CountUniqueBerries(List<FeedingStep> steps)
        {
            ulong mask0 = 0;
            ulong mask1 = 0;

            for (int i = 0; i < steps.Count; i++)
            {
                BerryId[] berries = steps[i].Poffin.Recipe.Berries;
                for (int j = 0; j < berries.Length; j++)
                {
                    int id = berries[j].Value;
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

            return System.Numerics.BitOperations.PopCount(mask0) + System.Numerics.BitOperations.PopCount(mask1);
        }
    }
}
