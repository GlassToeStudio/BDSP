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

    // Optional: require a specific flavor to be present (> 0). Disabled when null.
    public readonly byte RequiredFlavor;     // uses BDSP.Core.Primitives.Flavor underlying values
    public readonly bool HasRequiredFlavor;

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
        byte requiredFlavor = 0)
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
    }

    public static BerryFilterOptions Default => new();

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
