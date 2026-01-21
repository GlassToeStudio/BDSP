using System;
using System.Collections.Generic;
using BDSP.Core.Berries;
using BDSP.Core.Poffins;
using BDSP.Core.Feeding;
using BDSP.Core.Selection;

namespace BDSP.Core.Runner;

/// <summary>
/// High-level runner that:
/// 1) searches for top-K individual Poffins from a berry pool, then
/// 2) finds an optimal multi-Poffin feeding plan under sheen constraints.
/// </summary>
public static class PoffinFeedingSearchRunner
{
    /// <summary>
    /// Runs the end-to-end search-and-feed optimization.
    /// </summary>
    /// <param name="berryPool">Pool of berries available for cooking.</param>
    /// <param name="berriesPerPoffin">Number of berries used per Poffin (1–4).</param>
    /// <param name="topK">Number of top Poffins to carry into feeding optimization.</param>
    /// <param name="cookTimeSeconds">Cooking time in seconds.</param>
    /// <param name="errors">Number of cooking errors.</param>
    /// <param name="amityBonus">Amity Square smoothness bonus (BDSP).</param>
    /// <param name="comparer">Comparer for ranking individual Poffins during search.</param>
    /// <param name="predicate">Optional filter applied before ranking during search.</param>
    /// <param name="feedingOptions">
    /// Optional feeding options. If null, defaults to BDSP sheen cap (255) and a balanced stat score.
    /// </param>
    /// <param name="maxDegreeOfParallelism">
    /// Optional cap on parallel search workers. If null, the search runner uses a sensible default.
    /// </param>
    /// <returns>The optimized feeding plan.</returns>
    public static FeedingPlan Run(
        ReadOnlySpan<BerryId> berryPool,
        int berriesPerPoffin,
        int topK,
        byte cookTimeSeconds,
        byte errors,
        byte amityBonus,
        IPoffinComparer comparer,
        PoffinPredicate? predicate = null,
        FeedingOptions? feedingOptions = null,
        int? maxDegreeOfParallelism = null)
    {
        // ------------------------------------------------------------
        // 1) Search: find strong candidate Poffins
        // ------------------------------------------------------------
        var search = PoffinSearchRunner.Run(
            berryPool: berryPool,
            berriesPerPoffin: berriesPerPoffin,
            topK: topK,
            cookTimeSeconds: cookTimeSeconds,
            errors: errors,
            amityBonus: amityBonus,
            comparer: comparer,
            predicate: predicate,
            maxDegreeOfParallelism: maxDegreeOfParallelism);

        // Defensive: if no candidates, return empty plan.
        // (FeedingPlan ctor is internal; we can construct it here within the same assembly.)
        var candidates = search.TopPoffins;
        if (candidates is null || candidates.Length == 0)
        {
            return new FeedingPlan(
                [],
                new FeedingState(0, default));
        }

        // ------------------------------------------------------------
        // 2) Feeding optimization: maximize stats, then maximize sheen
        // ------------------------------------------------------------
        var opts = feedingOptions ?? new FeedingOptions { MaxSheen = 255 };

        var plan = FeedingOptimizer.Optimize(candidates, opts);

        // ------------------------------------------------------------
        // 3) Clamp sheen (BDSP hard cap is 255)
        // ------------------------------------------------------------
        if (plan.FinalState.Sheen > 255)
        {
            // FeedingPlan is a class (not a record), so "with" is invalid.
            // Reconstruct the plan with clamped sheen.
            var poffinsCopy = plan.Poffins as List<Poffin>
                              ?? [.. plan.Poffins];

            plan = new FeedingPlan(
                poffinsCopy,
                new FeedingState(255, plan.FinalState.Stats));
        }

        return plan;
    }
}
