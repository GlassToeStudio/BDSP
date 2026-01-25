namespace BDSP.Core.Optimization.Core
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
        /// <summary>Total rarity cost for the plan.</summary>
        public int TotalRarityCost { get; }
        /// <summary>Total number of poffins consumed.</summary>
        public int TotalPoffins { get; }
        /// <summary>Total sheen added by the plan.</summary>
        public int TotalSheen { get; }
        /// <summary>Aggregate plan score (higher is better).</summary>
        public int Score { get; }

        public FeedingPlanResult(
            FeedingStep[] steps,
            ContestStats finalStats,
            int totalRarityCost,
            int totalPoffins,
            int totalSheen,
            int score)
        {
            Steps = steps;
            FinalStats = finalStats;
            TotalRarityCost = totalRarityCost;
            TotalPoffins = totalPoffins;
            TotalSheen = totalSheen;
            Score = score;
        }
    }
}
