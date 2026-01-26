namespace BDSP.Core.Optimization.Search
{
    /// <summary>
    /// Controls how feeding plans are scored and ranked.
    /// </summary>
    public readonly struct FeedingSearchOptions
    {
        /// <summary>Default scoring options.</summary>
        public static FeedingSearchOptions Default => new FeedingSearchOptions(
            statsWeight: 1000,
            poffinCountPenalty: 10,
            sheenPenalty: 1,
            rarityPenalty: 5,
            rarityCostMode: RarityCostMode.MaxBerryRarity,
            scoreMode: ContestScoreMode.Balanced,
            minStatWeight: 2000);
        /// <summary>Weight for total contest stat completion (higher is better).</summary>
        public readonly int StatsWeight;
        /// <summary>Penalty per poffin consumed (lower is better).</summary>
        public readonly int PoffinCountPenalty;
        /// <summary>Penalty for total sheen used (lower is better).</summary>
        public readonly int SheenPenalty;
        /// <summary>Penalty for berry rarity cost (lower is better).</summary>
        public readonly int RarityPenalty;
        /// <summary>How to compute rarity cost for a poffin recipe.</summary>
        public readonly RarityCostMode RarityCostMode;
        /// <summary>How contest stats are scored (sum-only vs balance-aware).</summary>
        public readonly ContestScoreMode ScoreMode;
        /// <summary>Weight for the weakest contest stat when using balance-aware scoring.</summary>
        public readonly int MinStatWeight;

        public FeedingSearchOptions(
            int statsWeight = 1000,
            int poffinCountPenalty = 10,
            int sheenPenalty = 1,
            int rarityPenalty = 5,
            RarityCostMode rarityCostMode = RarityCostMode.MaxBerryRarity,
            ContestScoreMode scoreMode = ContestScoreMode.Balanced,
            int minStatWeight = 2000)
        {
            StatsWeight = statsWeight;
            PoffinCountPenalty = poffinCountPenalty;
            SheenPenalty = sheenPenalty;
            RarityPenalty = rarityPenalty;
            RarityCostMode = rarityCostMode;
            ScoreMode = scoreMode;
            MinStatWeight = minStatWeight;
        }
    }

    /// <summary>
    /// Determines how rarity cost is computed for a poffin recipe.
    /// </summary>
    public enum RarityCostMode : byte
    {
        /// <summary>Use the most rare berry in the recipe (current default).</summary>
        MaxBerryRarity = 0,
        /// <summary>Sum all berry rarities in the recipe.</summary>
        SumBerryRarity = 1
    }

    /// <summary>
    /// Controls how contest stat results are scored.
    /// </summary>
    public enum ContestScoreMode : byte
    {
        /// <summary>Rank by total stat sum only.</summary>
        SumOnly = 0,
        /// <summary>Favor balanced stats by rewarding the weakest stat.</summary>
        Balanced = 1
    }
}
