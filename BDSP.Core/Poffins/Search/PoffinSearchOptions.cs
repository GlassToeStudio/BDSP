using BDSP.Core.Berries;

namespace BDSP.Core.Poffins.Search
{
    /// <summary>
    /// Options that control poffin cooking and search behavior.
    /// </summary>
    public readonly struct PoffinSearchOptions
    {
        /// <summary>Number of berries per combo (2–4).</summary>
        public readonly int Choose;
        /// <summary>Cook time in seconds.</summary>
        public readonly int CookTimeSeconds;
        /// <summary>Number of spills during cooking.</summary>
        public readonly int Spills;
        /// <summary>Number of burns during cooking.</summary>
        public readonly int Burns;
        /// <summary>Amity bonus reduction (BDSP cap is 9).</summary>
        public readonly int AmityBonus;
        /// <summary>When true, allows internal parallelization based on measured thresholds.</summary>
        public readonly bool UseParallel;
        /// <summary>Optional parallelism limit (null = default).</summary>
        public readonly int? MaxDegreeOfParallelism;
        /// <summary>When true, uses the precomputed combo table for full-berry runs.</summary>
        public readonly bool UseComboTableWhenAllBerries;
        /// <summary>Scoring preferences for ranking results.</summary>
        public readonly PoffinScoreOptions ScoreOptions;

        public PoffinSearchOptions(
            int choose = 2,
            int cookTimeSeconds = 40,
            int spills = 0,
            int burns = 0,
            int amityBonus = 9,
            bool useParallel = true,
            int? maxDegreeOfParallelism = null,
            bool useComboTableWhenAllBerries = true,
            PoffinScoreOptions scoreOptions = default)
        {
            Choose = choose;
            CookTimeSeconds = cookTimeSeconds;
            Spills = spills;
            Burns = burns;
            AmityBonus = amityBonus;
            UseParallel = useParallel;
            MaxDegreeOfParallelism = maxDegreeOfParallelism;
            UseComboTableWhenAllBerries = useComboTableWhenAllBerries;
            ScoreOptions = scoreOptions;
        }
    }

    /// <summary>
    /// Scoring controls for ranking poffins.
    /// Higher scores are better.
    /// </summary>
    public readonly struct PoffinScoreOptions
    {
        /// <summary>Weight for poffin level (primary strength).</summary>
        public readonly int LevelWeight;
        /// <summary>Weight for total flavor sum.</summary>
        public readonly int TotalFlavorWeight;
        /// <summary>Penalty weight for smoothness (lower is better).</summary>
        public readonly int SmoothnessPenalty;
        /// <summary>Optional main flavor bias (Flavor.None disables).</summary>
        public readonly Flavor PreferredMainFlavor;
        /// <summary>Bonus applied when main flavor matches.</summary>
        public readonly int PreferredMainFlavorBonus;

        public PoffinScoreOptions(
            int levelWeight = 1000,
            int totalFlavorWeight = 1,
            int smoothnessPenalty = 1,
            Flavor preferredMainFlavor = Flavor.None,
            int preferredMainFlavorBonus = 0)
        {
            LevelWeight = levelWeight;
            TotalFlavorWeight = totalFlavorWeight;
            SmoothnessPenalty = smoothnessPenalty;
            PreferredMainFlavor = preferredMainFlavor;
            PreferredMainFlavorBonus = preferredMainFlavorBonus;
        }
    }
}
