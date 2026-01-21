using System.Runtime.CompilerServices;

namespace BDSP.Core.Berries;

public static class BerryQuery
{
    /// <summary>
    /// Zero-allocation path: writes results into <paramref name="destination"/> and returns count.
    /// destination must be at least BerryTable.Count in length.
    /// </summary>
    /// <example>
    /// <code>
    /// Span&lt;BerryId&gt; poolBuf = stackalloc BerryId[BerryTable.Count];
    /// var filter = BerryFilters.Tight(maxSmoothness: 25, maxRarity: 3, minMainFlavorValue: 10);
    /// int count = BerryQuery.Filter(in filter, poolBuf);
    /// var berryPool = poolBuf[..count];
    /// </code>
    /// </example>
    public static int Filter(
        in BerryFilterOptions filter,
        Span<BerryId> destination)
    {
        int count = 0;
        for (ushort i = 0; i < (ushort)BerryTable.Count; i++)
        {
            if (!filter.Allows(i))
                continue;

            var id = new BerryId(i);
            ref readonly var b = ref BerryTable.Get(id);

            if (!PassesFilters(in filter, in b))
                continue;

            destination[count++] = id;
        }

        return count;
    }

    /// <summary>
    /// Convenience path: allocates a new array sized to count (<= 65).
    /// </summary>
    public static BerryId[] Filter(in BerryFilterOptions filter)
    {
        Span<BerryId> tmp = stackalloc BerryId[BerryTable.Count];
        int count = Filter(in filter, tmp);
        return tmp.Slice(0, count).ToArray();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool PassesFilters(in BerryFilterOptions f, in Berry b)
    {
        if (f.MinSmoothness >= 0 && b.Smoothness < f.MinSmoothness) return false;
        if (b.Smoothness > f.MaxSmoothness) return false;

        if (f.MinRarity >= 0 && b.Rarity < f.MinRarity) return false;
        if (b.Rarity > f.MaxRarity) return false;

        if (f.HasRequiredFlavor)
        {
            // Avoid enum dependency here; treat RequiredFlavor as underlying value.
            var flavor = (BDSP.Core.Primitives.Flavor)f.RequiredFlavor;
            if (b.GetFlavor(flavor) == 0)
                return false;
        }

        // Derived filters: only compute when enabled.
        if (f.MinMainFlavorValue >= 0 || f.MaxMainFlavorValue != int.MaxValue)
        {
            var main = BerryFacts.GetMainFlavorValue(in b);
            if (f.MinMainFlavorValue >= 0 && main < f.MinMainFlavorValue) return false;
            if (main > f.MaxMainFlavorValue) return false;
        }

        if (f.MinNumFlavors >= 0 || f.MaxNumFlavors != int.MaxValue)
        {
            var nf = BerryFacts.GetNumFlavors(in b);
            if (f.MinNumFlavors >= 0 && nf < f.MinNumFlavors) return false;
            if (nf > f.MaxNumFlavors) return false;
        }

        if (f.MinAnyNonZeroFlavorValue >= 0)
        {
            if (BerryFacts.HasAnyNonZeroFlavorLessThan(in b, f.MinAnyNonZeroFlavorValue))
                return false;
        }

        if (f.MaxAnyNonZeroFlavorValue != int.MaxValue)
        {
            if (BerryFacts.HasAnyFlavorGreaterThan(in b, f.MaxAnyNonZeroFlavorValue))
                return false;
        }

        if (f.MinWeakenedMainFlavorValue != int.MinValue || f.MaxWeakenedMainFlavorValue != int.MaxValue)
        {
            var weakenedVal = BerryFacts.GetWeakenedMainFlavorValue(in b);
            if (weakenedVal < f.MinWeakenedMainFlavorValue) return false;
            if (weakenedVal > f.MaxWeakenedMainFlavorValue) return false;
        }

        return true;
    }

    
}
