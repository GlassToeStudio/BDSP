namespace BDSP.Core.Feeding;

/// <summary>
/// Configuration for feeding optimization.
/// FeedingOptimizer becomes the only public feeding engine
/// </summary>
public sealed class FeedingOptions
{
    /// <summary>Maximum allowed sheen (BDSP cap is 255).</summary>
    public int MaxSheen { get; init; } = 255;

    /// <summary>
    /// Contest stat objective function.
    /// Determines how accumulated stats are scored.
    /// </summary>
    public Func<Contest.ContestStats, int> Score { get; init; }
        = static s => s.Cool + s.Beauty + s.Cute + s.Smart + s.Tough;
}
