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
        /// <summary>Number of stats at or above 255 (0-5).</summary>
        public int NumPerfectValues { get; }
        /// <summary>Rank of the plan based on perfect stats and sheen.</summary>
        public int Rank { get; }
        /// <summary>Total number of unique berries used.</summary>
        public int UniqueBerries { get; }
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
            int numPerfectValues,
            int rank,
            int uniqueBerries,
            int totalRarityCost,
            int totalPoffins,
            int totalSheen,
            int score)
        {
            Steps = steps;
            FinalStats = finalStats;
            NumPerfectValues = numPerfectValues;
            Rank = rank;
            UniqueBerries = uniqueBerries;
            TotalRarityCost = totalRarityCost;
            TotalPoffins = totalPoffins;
            TotalSheen = totalSheen;
            Score = score;
        }
    }
}
