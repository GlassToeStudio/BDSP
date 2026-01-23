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
        var ordered = candidates.ToArray();
        var scorePerSheen = new int[ordered.Length];
        for (int i = 0; i < ordered.Length; i++)
            scorePerSheen[i] = ScorePerSheen(ordered[i], options);

        Array.Sort(
            scorePerSheen,
            ordered,
            Comparer<int>.Create((a, b) => b.CompareTo(a)));

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

    private static int ScorePerSheen(Poffins.Poffin p, FeedingOptions options)
    {
        return options.Score(
            Contest.ContestStatsCalculator.FromPoffin(p)) /
            Math.Max(1, (int)p.Smoothness);
    }
}
