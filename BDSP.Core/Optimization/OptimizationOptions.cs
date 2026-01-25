namespace BDSP.Core.Optimization
{
    /// <summary>
    /// Controls how feeding plans are scored and ranked.
    /// </summary>
    public readonly struct OptimizationOptions
    {
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

        public OptimizationOptions(
            int statsWeight = 1000,
            int poffinCountPenalty = 10,
            int sheenPenalty = 1,
            int rarityPenalty = 5,
            RarityCostMode rarityCostMode = RarityCostMode.MaxBerryRarity)
        {
            StatsWeight = statsWeight;
            PoffinCountPenalty = poffinCountPenalty;
            SheenPenalty = sheenPenalty;
            RarityPenalty = rarityPenalty;
            RarityCostMode = rarityCostMode;
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
}
