namespace BDSP.Core.Optimization
{
    /// <summary>
    /// Summary of a feeding plan and its resulting stats.
    /// </summary>
    public sealed class FeedingPlanResult
    {
        /// <summary>Ordered feeding steps.</summary>
        public FeedingStep[] Steps { get; }
        /// <summary>Final stats after applying all steps.</summary>
        public ContestStats FinalStats { get; }
        /// <summary>Total number of poffins consumed.</summary>
        public int TotalPoffins { get; }
        /// <summary>Total sheen added by the plan.</summary>
        public int TotalSheen { get; }

        public FeedingPlanResult(FeedingStep[] steps, ContestStats finalStats, int totalPoffins, int totalSheen)
        {
            Steps = steps;
            FinalStats = finalStats;
            TotalPoffins = totalPoffins;
            TotalSheen = totalSheen;
        }
    }
}
