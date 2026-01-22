using BDSP.Core.Primitives;

namespace BDSP.Core.Poffins.Filters;

public readonly struct PoffinFilterOptions
{
    public readonly bool ExcludeFoul;

    public readonly int MinLevel;       // -1 disables
    public readonly int MaxLevel;       // int.MaxValue disables

    public readonly int MinSmoothness;  // -1 disables
    public readonly int MaxSmoothness;  // int.MaxValue disables

    public readonly int MinSpicy;       // -1 disables
    public readonly int MaxSpicy;       // int.MaxValue disables
    public readonly int MinDry;
    public readonly int MaxDry;
    public readonly int MinSweet;
    public readonly int MaxSweet;
    public readonly int MinBitter;
    public readonly int MaxBitter;
    public readonly int MinSour;
    public readonly int MaxSour;

    public readonly int MinNumFlavors;  // -1 disables
    public readonly int MaxNumFlavors;  // int.MaxValue disables

    // Optional: require/exclude multiple flavors (bitmask, bit 0..4).
    public readonly byte RequiredFlavorMask;
    public readonly byte ExcludedFlavorMask;

    // Optional: allow/exclude specific poffin types (bitmask, bit 0..7).
    public readonly byte AllowedTypeMask;
    public readonly byte ExcludedTypeMask;

    public PoffinFilterOptions(
        bool excludeFoul = false,
        int minLevel = -1,
        int maxLevel = int.MaxValue,
        int minSmoothness = -1,
        int maxSmoothness = int.MaxValue,
        int minSpicy = -1,
        int maxSpicy = int.MaxValue,
        int minDry = -1,
        int maxDry = int.MaxValue,
        int minSweet = -1,
        int maxSweet = int.MaxValue,
        int minBitter = -1,
        int maxBitter = int.MaxValue,
        int minSour = -1,
        int maxSour = int.MaxValue,
        int minNumFlavors = -1,
        int maxNumFlavors = int.MaxValue,
        byte requiredFlavorMask = 0,
        byte excludedFlavorMask = 0,
        byte allowedTypeMask = 0,
        byte excludedTypeMask = 0)
    {
        ExcludeFoul = excludeFoul;

        MinLevel = minLevel;
        MaxLevel = maxLevel;

        MinSmoothness = minSmoothness;
        MaxSmoothness = maxSmoothness;

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

        MinNumFlavors = minNumFlavors;
        MaxNumFlavors = maxNumFlavors;

        RequiredFlavorMask = requiredFlavorMask;
        ExcludedFlavorMask = excludedFlavorMask;

        AllowedTypeMask = allowedTypeMask;
        ExcludedTypeMask = excludedTypeMask;
    }

    public static PoffinFilterOptions Default => new PoffinFilterOptions();

    public static byte BuildFlavorMask(params Flavor[] flavors)
    {
        byte mask = 0;
        for (int i = 0; i < flavors.Length; i++)
            mask |= (byte)(1 << (int)flavors[i]);
        return mask;
    }

    public static byte BuildTypeMask(params PoffinType[] types)
    {
        byte mask = 0;
        for (int i = 0; i < types.Length; i++)
            mask |= (byte)(1 << (int)types[i]);
        return mask;
    }
}
