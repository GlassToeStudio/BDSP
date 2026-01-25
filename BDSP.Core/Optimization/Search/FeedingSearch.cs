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
            }

            int score = ScorePlan(in current, totalRarityCost, steps.Count, totalSheen, in options);

            return new FeedingPlanResult(
                steps.ToArray(),
                current,
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
            score -= poffinCount * options.PoffinCountPenalty;
            score -= totalSheen * options.SheenPenalty;
            score -= totalRarityCost * options.RarityPenalty;
            return score;
        }
    }
}
