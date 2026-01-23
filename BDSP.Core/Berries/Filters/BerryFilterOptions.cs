namespace BDSP.Core.Berries
{
    /// <summary>
    /// Filter options for berries. Use <see cref="Unset"/> for unused bounds.
    /// </summary>
    public readonly struct BerryFilterOptions
    {
        /// <summary>
        /// Sentinel value for unset bounds.
        /// </summary>
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
        /// <summary>Minimum rarity value.</summary>
        public readonly int MinRarity;
        /// <summary>Maximum rarity value.</summary>
        public readonly int MaxRarity;

        /// <summary>Minimum main flavor value.</summary>
        public readonly int MinMainFlavorValue;
        /// <summary>Maximum main flavor value.</summary>
        public readonly int MaxMainFlavorValue;
        /// <summary>Minimum secondary flavor value.</summary>
        public readonly int MinSecondaryFlavorValue;
        /// <summary>Maximum secondary flavor value.</summary>
        public readonly int MaxSecondaryFlavorValue;

        /// <summary>Minimum number of non-zero flavors.</summary>
        public readonly int MinNumFlavors;
        /// <summary>Maximum number of non-zero flavors.</summary>
        public readonly int MaxNumFlavors;

        /// <summary>When true, <see cref="MainFlavor"/> must match.</summary>
        public readonly bool RequireMainFlavor;
        /// <summary>The required main flavor (used when <see cref="RequireMainFlavor"/> is true).</summary>
        public readonly Flavor MainFlavor;
        /// <summary>When true, <see cref="SecondaryFlavor"/> must match.</summary>
        public readonly bool RequireSecondaryFlavor;
        /// <summary>The required secondary flavor (used when <see cref="RequireSecondaryFlavor"/> is true).</summary>
        public readonly Flavor SecondaryFlavor;

        /// <summary>
        /// Bitmask of flavors that must be present. Bits: 0=Spicy, 1=Dry, 2=Sweet, 3=Bitter, 4=Sour.
        /// </summary>
        public readonly byte RequiredFlavorMask;
        /// <summary>
        /// Bitmask of flavors that must be absent. Bits: 0=Spicy, 1=Dry, 2=Sweet, 3=Bitter, 4=Sour.
        /// </summary>
        public readonly byte ExcludedFlavorMask;

        /// <summary>
        /// Example:
        /// <code>
        /// var options = new BerryFilterOptions(
        ///     minRarity: 5,
        ///     maxRarity: 9,
        ///     minSmoothness: 20,
        ///     maxSmoothness: 40,
        ///     requireMainFlavor: true,
        ///     mainFlavor: Flavor.Spicy);
        /// </code>
        /// </summary>
        public BerryFilterOptions(
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
            int minRarity = Unset,
            int maxRarity = Unset,
            int minMainFlavorValue = Unset,
            int maxMainFlavorValue = Unset,
            int minSecondaryFlavorValue = Unset,
            int maxSecondaryFlavorValue = Unset,
            int minNumFlavors = Unset,
            int maxNumFlavors = Unset,
            bool requireMainFlavor = false,
            Flavor mainFlavor = Flavor.None,
            bool requireSecondaryFlavor = false,
            Flavor secondaryFlavor = Flavor.None,
            byte requiredFlavorMask = 0,
            byte excludedFlavorMask = 0)
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
            MinRarity = minRarity;
            MaxRarity = maxRarity;

            MinMainFlavorValue = minMainFlavorValue;
            MaxMainFlavorValue = maxMainFlavorValue;
            MinSecondaryFlavorValue = minSecondaryFlavorValue;
            MaxSecondaryFlavorValue = maxSecondaryFlavorValue;

            MinNumFlavors = minNumFlavors;
            MaxNumFlavors = maxNumFlavors;

            RequireMainFlavor = requireMainFlavor;
            MainFlavor = mainFlavor;
            RequireSecondaryFlavor = requireSecondaryFlavor;
            SecondaryFlavor = secondaryFlavor;

            RequiredFlavorMask = requiredFlavorMask;
            ExcludedFlavorMask = excludedFlavorMask;
        }
    }
}
