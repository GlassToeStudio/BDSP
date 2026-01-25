using BDSP.Core.Berries;

namespace BDSP.Core.Poffins.Filters
{
    /// <summary>
    /// Indicates which poffin filter bounds are active.
    /// </summary>
    [System.Flags]
    public enum PoffinFilterMask : uint
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
        MinLevel = 1 << 12,
        MaxLevel = 1 << 13,
        MinNumFlavors = 1 << 14,
        MaxNumFlavors = 1 << 15
    }

    /// <summary>
    /// Filter options for cooked poffins. Bounds are inclusive; use <see cref="Unset"/> for unused bounds.
    /// </summary>
    public readonly struct PoffinFilterOptions
    {

        /// <summary>Represents no filtering (all fields set to <see cref="unset"/>).</summary>
        public static PoffinFilterOptions None => new PoffinFilterOptions(minSpicy: Unset);
        /// <summary>Sentinel value for unset bounds.</summary>
        public const int Unset = -1;

        /// <summary>Minimum spicy value (0-255).</summary>
        public readonly int MinSpicy;
        /// <summary>Maximum spicy value (0-255).</summary>
        public readonly int MaxSpicy;
        /// <summary>Minimum dry value (0-255).</summary>
        public readonly int MinDry;
        /// <summary>Maximum dry value (0-255).</summary>
        public readonly int MaxDry;
        /// <summary>Minimum sweet value (0-255).</summary>
        public readonly int MinSweet;
        /// <summary>Maximum sweet value (0-255).</summary>
        public readonly int MaxSweet;
        /// <summary>Minimum bitter value (0-255).</summary>
        public readonly int MinBitter;
        /// <summary>Maximum bitter value (0-255).</summary>
        public readonly int MaxBitter;
        /// <summary>Minimum sour value (0-255).</summary>
        public readonly int MinSour;
        /// <summary>Maximum sour value (0-255).</summary>
        public readonly int MaxSour;

        /// <summary>Minimum smoothness value (0-255).</summary>
        public readonly int MinSmoothness;
        /// <summary>Maximum smoothness value (0-255).</summary>
        public readonly int MaxSmoothness;
        /// <summary>Minimum level (0-255).</summary>
        public readonly int MinLevel;
        /// <summary>Maximum level (0-255).</summary>
        public readonly int MaxLevel;
        /// <summary>Minimum number of non-zero flavors (0-5).</summary>
        public readonly int MinNumFlavors;
        /// <summary>Maximum number of non-zero flavors (0-5).</summary>
        public readonly int MaxNumFlavors;

        /// <summary>Mask of active bounds for fast checks.</summary>
        public readonly PoffinFilterMask Mask;

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
            PoffinFilterMask mask = PoffinFilterMask.None;
            if (minSpicy != Unset) mask |= PoffinFilterMask.MinSpicy;
            if (maxSpicy != Unset) mask |= PoffinFilterMask.MaxSpicy;
            if (minDry != Unset) mask |= PoffinFilterMask.MinDry;
            if (maxDry != Unset) mask |= PoffinFilterMask.MaxDry;
            if (minSweet != Unset) mask |= PoffinFilterMask.MinSweet;
            if (maxSweet != Unset) mask |= PoffinFilterMask.MaxSweet;
            if (minBitter != Unset) mask |= PoffinFilterMask.MinBitter;
            if (maxBitter != Unset) mask |= PoffinFilterMask.MaxBitter;
            if (minSour != Unset) mask |= PoffinFilterMask.MinSour;
            if (maxSour != Unset) mask |= PoffinFilterMask.MaxSour;
            if (minSmoothness != Unset) mask |= PoffinFilterMask.MinSmoothness;
            if (maxSmoothness != Unset) mask |= PoffinFilterMask.MaxSmoothness;
            if (minLevel != Unset) mask |= PoffinFilterMask.MinLevel;
            if (maxLevel != Unset) mask |= PoffinFilterMask.MaxLevel;
            if (minNumFlavors != Unset) mask |= PoffinFilterMask.MinNumFlavors;
            if (maxNumFlavors != Unset) mask |= PoffinFilterMask.MaxNumFlavors;

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

            Mask = mask;
            RequireMainFlavor = requireMainFlavor;
            MainFlavor = mainFlavor;
            RequireSecondaryFlavor = requireSecondaryFlavor;
            SecondaryFlavor = secondaryFlavor;
        }
    }
}
