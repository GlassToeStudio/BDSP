using System.Runtime.CompilerServices;
using BDSP.Core.Berries.Data;
using BDSP.Core.Primitives;

namespace BDSP.Core.Berries.Analysis;

public static class BerryFacts
{
    // Tie-break order for "main flavor" is deterministic.
    // You can change this once and the entire system remains consistent.
    private static readonly Flavor[] FlavorPriority =
    {
        Flavor.Spicy, Flavor.Dry, Flavor.Sweet, Flavor.Bitter, Flavor.Sour
    };

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int GetNumFlavors(in Berry b)
        => (b.Spicy > 0 ? 1 : 0)
         + (b.Dry > 0 ? 1 : 0)
         + (b.Sweet > 0 ? 1 : 0)
         + (b.Bitter > 0 ? 1 : 0)
         + (b.Sour > 0 ? 1 : 0);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static byte GetMainFlavorValue(in Berry b)
    {
        byte best = 0;
        // priority tie-break by ordering below
        if (b.Spicy > best) best = b.Spicy;
        if (b.Dry > best) best = b.Dry;
        if (b.Sweet > best) best = b.Sweet;
        if (b.Bitter > best) best = b.Bitter;
        if (b.Sour > best) best = b.Sour;
        return best;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Flavor GetMainFlavor(in Berry b)
    {
        byte best = 0;
        Flavor bestFlavor = Flavor.Spicy;

        foreach (var f in FlavorPriority)
        {
            var v = b.GetFlavor(f);
            if (v > best)
            {
                best = v;
                bestFlavor = f;
            }
        }

        return bestFlavor;
    }

    /// <summary>
    /// Computes "weakened flavor values" using the BDSP weakening cycle:
    /// Spicy <- weakened by Dry
    /// Sour  <- weakened by Spicy
    /// Bitter<- weakened by Sour
    /// Sweet <- weakened by Bitter
    /// Dry   <- weakened by Sweet
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int GetWeakenedMainFlavorValue(in Berry b)
    {
        int spicy = b.Spicy - b.Dry;
        int sour = b.Sour - b.Spicy;
        int bitter = b.Bitter - b.Sour;
        int sweet = b.Sweet - b.Bitter;
        int dry = b.Dry - b.Sweet;

        int best = int.MinValue;
        // tie-break order mirrors FlavorPriority
        foreach (var f in FlavorPriority)
        {
            int v = f switch
            {
                Flavor.Spicy => spicy,
                Flavor.Dry => dry,
                Flavor.Sweet => sweet,
                Flavor.Bitter => bitter,
                Flavor.Sour => sour,
                _ => int.MinValue
            };

            if (v > best)
                best = v;
        }

        return best;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Flavor GetWeakenedMainFlavor(in Berry b)
    {
        int spicy = b.Spicy - b.Dry;
        int sour = b.Sour - b.Spicy;
        int bitter = b.Bitter - b.Sour;
        int sweet = b.Sweet - b.Bitter;
        int dry = b.Dry - b.Sweet;

        int best = int.MinValue;
        Flavor bestFlavor = Flavor.Spicy;

        foreach (var f in FlavorPriority)
        {
            int v = f switch
            {
                Flavor.Spicy => spicy,
                Flavor.Dry => dry,
                Flavor.Sweet => sweet,
                Flavor.Bitter => bitter,
                Flavor.Sour => sour,
                _ => int.MinValue
            };

            if (v > best)
            {
                best = v;
                bestFlavor = f;
            }
        }

        return bestFlavor;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool HasAnyNonZeroFlavorLessThan(in Berry b, int threshold)
        => (b.Spicy > 0 && b.Spicy < threshold)
        || (b.Dry > 0 && b.Dry < threshold)
        || (b.Sweet > 0 && b.Sweet < threshold)
        || (b.Bitter > 0 && b.Bitter < threshold)
        || (b.Sour > 0 && b.Sour < threshold);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool HasAnyFlavorGreaterThan(in Berry b, int threshold)
        => b.Spicy > threshold
        || b.Dry > threshold
        || b.Sweet > threshold
        || b.Bitter > threshold
        || b.Sour > threshold;

    
}

