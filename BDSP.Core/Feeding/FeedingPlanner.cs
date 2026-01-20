namespace BDSP.Core.Feeding;

/// <summary>
/// Computes optimal Poffin feeding plans under sheen constraints.
/// Use optimizer instead.
/// </summary>
internal static class FeedingPlanner
{
    /// <summary>
    /// Computes the best feeding plan from candidate Poffins.
    /// </summary>
    /// <param name="candidates">Candidate Poffins (e.g., Top-K).</param>
    /// <param name="options">Feeding options.</param>
    /// <returns>The optimal feeding plan.</returns>
    internal static FeedingPlan ComputePlan(
        ReadOnlySpan<Poffins.Poffin> candidates,
        FeedingOptions options)
    {
        var selected = new List<Poffins.Poffin>();
        var state = new FeedingState(0, default);

        // Sort by score-per-sheen descending
        var ordered = candidates
            .OrderByDescending(p =>
                options.Score(
                    Contest.ContestStatsCalculator.FromPoffin(p)) /
                Math.Max(1, p.Smoothness))
            .ToArray();

        foreach (var poffin in ordered)
        {
            if (state.Sheen + poffin.Smoothness > options.MaxSheen)
                continue;

            var stats = Contest.ContestStatsCalculator.FromPoffin(poffin);

            state = new FeedingState(
                state.Sheen + poffin.Smoothness,
                state.Stats + stats);

            selected.Add(poffin);
        }

        return new FeedingPlan(selected, state);
    }
}
