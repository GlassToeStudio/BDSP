namespace BDSP.Core.Berries
{
    /// <summary>
    /// Indicates which berry filter bounds are active.
    /// </summary>
    [System.Flags]
    public enum BerryFilterMask : uint
    {
        None = 0,
        MinSpicy = 1 << 0,
        MaxSpicy = 1 << 1,
        MinDry = 1 << 2,
        MaxDry = 1 << 3,
        MinSweet = 1 << 4,
        MaxSweet = 1 << 5,
        MinBitter = 1 << 6,
        MaxBitter = 1 << 7,
        MinSour = 1 << 8,
        MaxSour = 1 << 9,
        MinSmoothness = 1 << 10,
        MaxSmoothness = 1 << 11,
        MinRarity = 1 << 12,
        MaxRarity = 1 << 13,
        MinMainFlavorValue = 1 << 14,
        MaxMainFlavorValue = 1 << 15,
        MinSecondaryFlavorValue = 1 << 16,
        MaxSecondaryFlavorValue = 1 << 17,
        MinNumFlavors = 1 << 18,
        MaxNumFlavors = 1 << 19,
        MinAnyFlavorValue = 1 << 20,
        MaxAnyFlavorValue = 1 << 21,
        MinWeakMainFlavorValue = 1 << 22,
        MaxWeakMainFlavorValue = 1 << 23,
        IdEquals = 1 << 24,
        IdNotEquals = 1 << 25,
        RequiredFlavorMask = 1 << 26,
        ExcludedFlavorMask = 1 << 27
    }

    /// <summary>
    /// Filter options for berries. Bounds are inclusive; use <see cref="Unset"/> for unused bounds.
    /// </summary>
    public readonly struct BerryFilterOptions
    {
        public static BerryFilterOptions None => new BerryFilterOptions(minSpicy: Unset);

        /// <summary>
        /// Sentinel value for unset bounds.
        /// </summary>
        public const int Unset = -1;

        /// <summary>Minimum spicy value (0-40).</summary>
        public readonly int MinSpicy;
        /// <summary>Maximum spicy value (0-40).</summary>
        public readonly int MaxSpicy;
        /// <summary>Minimum dry value (0-40).</summary>
        public readonly int MinDry;
        /// <summary>Maximum dry value (0-40).</summary>
        public readonly int MaxDry;
        /// <summary>Minimum sweet value (0-40).</summary>
        public readonly int MinSweet;
        /// <summary>Maximum sweet value (0-40).</summary>
        public readonly int MaxSweet;
        /// <summary>Minimum bitter value (0-40).</summary>
        public readonly int MinBitter;
        /// <summary>Maximum bitter value (0-40).</summary>
        public readonly int MaxBitter;
        /// <summary>Minimum sour value (0-40).</summary>
        public readonly int MinSour;
        /// <summary>Maximum sour value (0-40).</summary>
        public readonly int MaxSour;

        /// <summary>Minimum smoothness value (20-60).</summary>
        public readonly int MinSmoothness;
        /// <summary>Maximum smoothness value (20-60).</summary>
        public readonly int MaxSmoothness;
        /// <summary>Minimum rarity value (1-15).</summary>
        public readonly int MinRarity;
        /// <summary>Maximum rarity value (1-15).</summary>
        public readonly int MaxRarity;

        /// <summary>Minimum main flavor value (0-40).</summary>
        public readonly int MinMainFlavorValue;
        /// <summary>Maximum main flavor value (0-40).</summary>
        public readonly int MaxMainFlavorValue;
        /// <summary>Minimum secondary flavor value (0-40).</summary>
        public readonly int MinSecondaryFlavorValue;
        /// <summary>Maximum secondary flavor value (0-40).</summary>
        public readonly int MaxSecondaryFlavorValue;

        /// <summary>Minimum number of non-zero flavors (1-5).</summary>
        public readonly int MinNumFlavors;
        /// <summary>Maximum number of non-zero flavors (1-5).</summary>
        public readonly int MaxNumFlavors;
        /// <summary>Minimum value for any non-zero flavor (0-40).</summary>
        public readonly int MinAnyFlavorValue;
        /// <summary>Maximum value for any flavor (0-40).</summary>
        public readonly int MaxAnyFlavorValue;
        /// <summary>Minimum weakened main flavor value.</summary>
        public readonly int MinWeakMainFlavorValue;
        /// <summary>Maximum weakened main flavor value.</summary>
        public readonly int MaxWeakMainFlavorValue;
        /// <summary>Require a weakened main flavor name.</summary>
        public readonly bool RequireWeakenedMainFlavor;
        /// <summary>Required weakened main flavor name.</summary>
        public readonly Flavor WeakenedMainFlavor;
        /// <summary>Berry id value that must match.</summary>
        public readonly int IdEquals;
        /// <summary>Berry id value that must not match.</summary>
        public readonly int IdNotEquals;

        /// <summary>Mask of active bounds for fast checks.</summary>
        public readonly BerryFilterMask Mask;

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
            int minAnyFlavorValue = Unset,
            int maxAnyFlavorValue = Unset,
            int minWeakMainFlavorValue = Unset,
            int maxWeakMainFlavorValue = Unset,
            bool requireWeakenedMainFlavor = false,
            Flavor weakenedMainFlavor = Flavor.None,
            int idEquals = Unset,
            int idNotEquals = Unset,
            bool requireMainFlavor = false,
            Flavor mainFlavor = Flavor.None,
            bool requireSecondaryFlavor = false,
            Flavor secondaryFlavor = Flavor.None,
            byte requiredFlavorMask = 0,
            byte excludedFlavorMask = 0)
        {
            BerryFilterMask mask = BerryFilterMask.None;
            if (minSpicy != Unset) mask |= BerryFilterMask.MinSpicy;
            if (maxSpicy != Unset) mask |= BerryFilterMask.MaxSpicy;
            if (minDry != Unset) mask |= BerryFilterMask.MinDry;
            if (maxDry != Unset) mask |= BerryFilterMask.MaxDry;
            if (minSweet != Unset) mask |= BerryFilterMask.MinSweet;
            if (maxSweet != Unset) mask |= BerryFilterMask.MaxSweet;
            if (minBitter != Unset) mask |= BerryFilterMask.MinBitter;
            if (maxBitter != Unset) mask |= BerryFilterMask.MaxBitter;
            if (minSour != Unset) mask |= BerryFilterMask.MinSour;
            if (maxSour != Unset) mask |= BerryFilterMask.MaxSour;
            if (minSmoothness != Unset) mask |= BerryFilterMask.MinSmoothness;
            if (maxSmoothness != Unset) mask |= BerryFilterMask.MaxSmoothness;
            if (minRarity != Unset) mask |= BerryFilterMask.MinRarity;
            if (maxRarity != Unset) mask |= BerryFilterMask.MaxRarity;
            if (minMainFlavorValue != Unset) mask |= BerryFilterMask.MinMainFlavorValue;
            if (maxMainFlavorValue != Unset) mask |= BerryFilterMask.MaxMainFlavorValue;
            if (minSecondaryFlavorValue != Unset) mask |= BerryFilterMask.MinSecondaryFlavorValue;
            if (maxSecondaryFlavorValue != Unset) mask |= BerryFilterMask.MaxSecondaryFlavorValue;
            if (minNumFlavors != Unset) mask |= BerryFilterMask.MinNumFlavors;
            if (maxNumFlavors != Unset) mask |= BerryFilterMask.MaxNumFlavors;
            if (minAnyFlavorValue != Unset) mask |= BerryFilterMask.MinAnyFlavorValue;
            if (maxAnyFlavorValue != Unset) mask |= BerryFilterMask.MaxAnyFlavorValue;
            if (minWeakMainFlavorValue != Unset) mask |= BerryFilterMask.MinWeakMainFlavorValue;
            if (maxWeakMainFlavorValue != Unset) mask |= BerryFilterMask.MaxWeakMainFlavorValue;
            if (idEquals != Unset) mask |= BerryFilterMask.IdEquals;
            if (idNotEquals != Unset) mask |= BerryFilterMask.IdNotEquals;
            if (requiredFlavorMask != 0) mask |= BerryFilterMask.RequiredFlavorMask;
            if (excludedFlavorMask != 0) mask |= BerryFilterMask.ExcludedFlavorMask;

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
            MinAnyFlavorValue = minAnyFlavorValue;
            MaxAnyFlavorValue = maxAnyFlavorValue;
            MinWeakMainFlavorValue = minWeakMainFlavorValue;
            MaxWeakMainFlavorValue = maxWeakMainFlavorValue;
            RequireWeakenedMainFlavor = requireWeakenedMainFlavor;
            WeakenedMainFlavor = weakenedMainFlavor;
            IdEquals = idEquals;
            IdNotEquals = idNotEquals;

            Mask = mask;
            RequireMainFlavor = requireMainFlavor;
            MainFlavor = mainFlavor;
            RequireSecondaryFlavor = requireSecondaryFlavor;
            SecondaryFlavor = secondaryFlavor;

            RequiredFlavorMask = requiredFlavorMask;
            ExcludedFlavorMask = excludedFlavorMask;
        }
    }
}
