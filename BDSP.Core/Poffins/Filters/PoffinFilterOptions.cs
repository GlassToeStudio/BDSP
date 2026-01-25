using BDSP.Core.Berries;

namespace BDSP.Core.Poffins.Filters
{
    /// <summary>
    /// Filter options for cooked poffins. Use <see cref="Unset"/> for unused bounds.
    /// </summary>
    public readonly struct PoffinFilterOptions
    {

        /// <summary>Represents no filtering (all fields set to <see cref="unset"/>).</summary>
        public static PoffinFilterOptions None => new PoffinFilterOptions(minSpicy: Unset);
        /// <summary>Sentinel value for unset bounds.</summary>
        public const int Unset = -1;

        /// <summary>Minimum spicy value.</summary>
        public readonly int MinSpicy;
        /// <summary>Maximum spicy value.</summary>
        public readonly int MaxSpicy;
        /// <summary>Minimum dry value.</summary>
        public readonly int MinDry;
        /// <summary>Maximum dry value.</summary>
        public readonly int MaxDry;
        /// <summary>Minimum sweet value.</summary>
        public readonly int MinSweet;
        /// <summary>Maximum sweet value.</summary>
        public readonly int MaxSweet;
        /// <summary>Minimum bitter value.</summary>
        public readonly int MinBitter;
        /// <summary>Maximum bitter value.</summary>
        public readonly int MaxBitter;
        /// <summary>Minimum sour value.</summary>
        public readonly int MinSour;
        /// <summary>Maximum sour value.</summary>
        public readonly int MaxSour;

        /// <summary>Minimum smoothness value.</summary>
        public readonly int MinSmoothness;
        /// <summary>Maximum smoothness value.</summary>
        public readonly int MaxSmoothness;
        /// <summary>Minimum level (max flavor).</summary>
        public readonly int MinLevel;
        /// <summary>Maximum level (max flavor).</summary>
        public readonly int MaxLevel;
        /// <summary>Minimum number of non-zero flavors.</summary>
        public readonly int MinNumFlavors;
        /// <summary>Maximum number of non-zero flavors.</summary>
        public readonly int MaxNumFlavors;

        /// <summary>When true, <see cref="MainFlavor"/> must match.</summary>
        public readonly bool RequireMainFlavor;
        /// <summary>Required main flavor.</summary>
        public readonly Flavor MainFlavor;
        /// <summary>When true, <see cref="SecondaryFlavor"/> must match.</summary>
        public readonly bool RequireSecondaryFlavor;
        /// <summary>Required secondary flavor.</summary>
        public readonly Flavor SecondaryFlavor;

        public PoffinFilterOptions(
            int minSpicy = Unset,
            int maxSpicy = Unset,
            int minDry = Unset,
            int maxDry = Unset,
            int minSweet = Unset,
            int maxSweet = Unset,
            int minBitter = Unset,
            int maxBitter = Unset,
            int minSour = Unset,
            int maxSour = Unset,
            int minSmoothness = Unset,
            int maxSmoothness = Unset,
            int minLevel = Unset,
            int maxLevel = Unset,
            int minNumFlavors = Unset,
            int maxNumFlavors = Unset,
            bool requireMainFlavor = false,
            Flavor mainFlavor = Flavor.None,
            bool requireSecondaryFlavor = false,
            Flavor secondaryFlavor = Flavor.None)
        {
            MinSpicy = minSpicy;
            MaxSpicy = maxSpicy;
            MinDry = minDry;
            MaxDry = maxDry;
            MinSweet = minSweet;
            MaxSweet = maxSweet;
            MinBitter = minBitter;
            MaxBitter = maxBitter;
            MinSour = minSour;
            MaxSour = maxSour;

            MinSmoothness = minSmoothness;
            MaxSmoothness = maxSmoothness;
            MinLevel = minLevel;
            MaxLevel = maxLevel;
            MinNumFlavors = minNumFlavors;
            MaxNumFlavors = maxNumFlavors;

            RequireMainFlavor = requireMainFlavor;
            MainFlavor = mainFlavor;
            RequireSecondaryFlavor = requireSecondaryFlavor;
            SecondaryFlavor = secondaryFlavor;
        }
    }
}
