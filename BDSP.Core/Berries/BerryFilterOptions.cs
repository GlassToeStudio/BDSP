using System.Runtime.CompilerServices;

namespace BDSP.Core.Berries;

public readonly struct BerryFilterOptions
{
    // If bit is 1 => allowed. If both masks are 0 => "all allowed".
    public readonly ulong AllowedMaskLo; // bits 0..63
    public readonly ulong AllowedMaskHi; // bits 64..127 (we only need bit 64)

    public readonly int MinSmoothness;      // -1 disables
    public readonly int MaxSmoothness;      // int.MaxValue disables

    public readonly int MinRarity;          // -1 disables
    public readonly int MaxRarity;          // int.MaxValue disables

    public readonly int MinMainFlavorValue; // -1 disables
    public readonly int MaxMainFlavorValue; // int.MaxValue disables

    public readonly int MinNumFlavors;      // -1 disables
    public readonly int MaxNumFlavors;      // int.MaxValue disables

    // Python parity: ignore zeros when applying these.
    public readonly int MinAnyNonZeroFlavorValue; // -1 disables
    public readonly int MaxAnyNonZeroFlavorValue; // int.MaxValue disables

    public readonly int MinWeakenedMainFlavorValue; // int.MinValue disables
    public readonly int MaxWeakenedMainFlavorValue; // int.MaxValue disables

    // Flavor-specific filters (-1/MaxValue disable).
    public readonly int MinSpicy;
    public readonly int MaxSpicy;
    public readonly int MinDry;
    public readonly int MaxDry;
    public readonly int MinSweet;
    public readonly int MaxSweet;
    public readonly int MinBitter;
    public readonly int MaxBitter;
    public readonly int MinSour;
    public readonly int MaxSour;

    // Optional: require a specific flavor to be present (> 0). Disabled when null.
    public readonly byte RequiredFlavor;     // uses BDSP.Core.Primitives.Flavor underlying values
    public readonly bool HasRequiredFlavor;

    // Optional: require/exclude multiple flavors (bitmask, bit 0..4).
    public readonly byte RequiredFlavorMask;
    public readonly byte ExcludedFlavorMask;

    public BerryFilterOptions(
        ulong allowedMaskLo = 0,
        ulong allowedMaskHi = 0,
        int minSmoothness = -1,
        int maxSmoothness = int.MaxValue,
        int minRarity = -1,
        int maxRarity = int.MaxValue,
        int minMainFlavorValue = -1,
        int maxMainFlavorValue = int.MaxValue,
        int minNumFlavors = -1,
        int maxNumFlavors = int.MaxValue,
        int minAnyNonZeroFlavorValue = -1,
        int maxAnyNonZeroFlavorValue = int.MaxValue,
        int minWeakenedMainFlavorValue = int.MinValue,
        int maxWeakenedMainFlavorValue = int.MaxValue,
        bool hasRequiredFlavor = false,
        byte requiredFlavor = 0,
        byte requiredFlavorMask = 0,
        byte excludedFlavorMask = 0,
        int minSpicy = -1,
        int maxSpicy = int.MaxValue,
        int minDry = -1,
        int maxDry = int.MaxValue,
        int minSweet = -1,
        int maxSweet = int.MaxValue,
        int minBitter = -1,
        int maxBitter = int.MaxValue,
        int minSour = -1,
        int maxSour = int.MaxValue)
    {
        AllowedMaskLo = allowedMaskLo;
        AllowedMaskHi = allowedMaskHi;

        MinSmoothness = minSmoothness;
        MaxSmoothness = maxSmoothness;

        MinRarity = minRarity;
        MaxRarity = maxRarity;

        MinMainFlavorValue = minMainFlavorValue;
        MaxMainFlavorValue = maxMainFlavorValue;

        MinNumFlavors = minNumFlavors;
        MaxNumFlavors = maxNumFlavors;

        MinAnyNonZeroFlavorValue = minAnyNonZeroFlavorValue;
        MaxAnyNonZeroFlavorValue = maxAnyNonZeroFlavorValue;

        MinWeakenedMainFlavorValue = minWeakenedMainFlavorValue;
        MaxWeakenedMainFlavorValue = maxWeakenedMainFlavorValue;

        HasRequiredFlavor = hasRequiredFlavor;
        RequiredFlavor = requiredFlavor;

        RequiredFlavorMask = requiredFlavorMask;
        ExcludedFlavorMask = excludedFlavorMask;

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
    }

    public static BerryFilterOptions Default => new BerryFilterOptions(
        allowedMaskLo: 0,
        allowedMaskHi: 0,
        minSmoothness: -1,
        maxSmoothness: int.MaxValue,
        minRarity: -1,
        maxRarity: int.MaxValue,
        minMainFlavorValue: -1,
        maxMainFlavorValue: int.MaxValue,
        minNumFlavors: -1,
        maxNumFlavors: int.MaxValue,
        minAnyNonZeroFlavorValue: -1,
        maxAnyNonZeroFlavorValue: int.MaxValue,
        minWeakenedMainFlavorValue: int.MinValue,
        maxWeakenedMainFlavorValue: int.MaxValue,
        hasRequiredFlavor: false,
        requiredFlavor: 0,
        requiredFlavorMask: 0,
        excludedFlavorMask: 0,
        minSpicy: -1,
        maxSpicy: int.MaxValue,
        minDry: -1,
        maxDry: int.MaxValue,
        minSweet: -1,
        maxSweet: int.MaxValue,
        minBitter: -1,
        maxBitter: int.MaxValue,
        minSour: -1,
        maxSour: int.MaxValue);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Allows(ushort idValue)
    {
        // If no allow-mask is specified, allow all.
        if ((AllowedMaskLo | AllowedMaskHi) == 0)
            return true;

        if (idValue < 64)
            return ((AllowedMaskLo >> (int)idValue) & 1UL) != 0;

        return ((AllowedMaskHi >> (int)(idValue - 64)) & 1UL) != 0;
    }

    // Convenience: build allow-mask from BerryId list without allocations.
    public static BerryFilterOptions WithAllowedIds(ReadOnlySpan<BerryId> ids)
    {
        ulong lo = 0, hi = 0;
        for (int i = 0; i < ids.Length; i++)
        {
            var v = ids[i].Value;
            if (v < 64) lo |= 1UL << (int)v;
            else hi |= 1UL << (int)(v - 64);
        }

        return new BerryFilterOptions(allowedMaskLo: lo, allowedMaskHi: hi);
    }
}
