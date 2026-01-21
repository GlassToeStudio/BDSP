using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using BDSP.Core.Berries;
using BDSP.Core.Primitives;

namespace BDSP.Core.Poffins;

/// <summary>
/// Cooks Poffins according to BDSP (Generation VIII) rules.
/// </summary>
public static class PoffinCooker
{
    private static readonly Flavor[] FlavorPriority =
    {
        Flavor.Spicy,
        Flavor.Dry,
        Flavor.Sweet,
        Flavor.Bitter,
        Flavor.Sour
    };

    /// <summary>
    /// Cooks a poffin from the provided berries.
    /// </summary>
    /// <param name="berryIds">The berries used (1–4).</param>
    /// <param name="cookTimeSeconds">Cooking time in seconds.</param>
    /// <param name="errors">Number of cooking errors.</param>
    /// <param name="amityBonus">Amity Square smoothness bonus.</param>
    /// <returns>The cooked <see cref="Poffin"/>.</returns>
    public static Poffin Cook(
        ReadOnlySpan<BerryId> berryIds,
        byte cookTimeSeconds,
        byte errors,
        byte amityBonus)
    {
#if DEBUG
        Debug.Assert(berryIds.Length is >= 1 and <= 4);
#endif
        int count = berryIds.Length;

        // ---------- duplicate check ----------
        for (int i = 0; i < count - 1; i++)
        {
            for (int j = i + 1; j < count; j++)
            {
                if (berryIds[i].Value == berryIds[j].Value)
                {
                    return CreateFoul();
                }
            }
        }

        int spicy = 0, dry = 0, sweet = 0, bitter = 0, sour = 0;
        int smoothnessSum = 0;

        for (int i = 0; i < count; i++)
        {
            ref readonly Berry b = ref BerryTable.Get(berryIds[i]);

            spicy += b.Spicy;
            dry += b.Dry;
            sweet += b.Sweet;
            bitter += b.Bitter;
            sour += b.Sour;

            smoothnessSum += b.Smoothness;
        }

        // ---------- weakening (correct order) ----------
        int wSpicy = spicy - dry;
        int wDry = dry - sweet;
        int wSweet = sweet - bitter;
        int wBitter = bitter - sour;
        int wSour = sour - spicy;

        spicy = wSpicy;
        dry = wDry;
        sweet = wSweet;
        bitter = wBitter;
        sour = wSour;

        // ---------- negative penalty ----------
        int negativeCount = 0;
        if (spicy < 0) negativeCount++;
        if (dry < 0) negativeCount++;
        if (sweet < 0) negativeCount++;
        if (bitter < 0) negativeCount++;
        if (sour < 0) negativeCount++;

        if (negativeCount > 0)
        {
            spicy -= negativeCount;
            dry -= negativeCount;
            sweet -= negativeCount;
            bitter -= negativeCount;
            sour -= negativeCount;
        }

        // ---------- time multiplier ----------
        spicy = spicy * 60 / cookTimeSeconds;
        dry = dry * 60 / cookTimeSeconds;
        sweet = sweet * 60 / cookTimeSeconds;
        bitter = bitter * 60 / cookTimeSeconds;
        sour = sour * 60 / cookTimeSeconds;

        // ---------- mistake penalty ----------
        spicy -= errors;
        dry -= errors;
        sweet -= errors;
        bitter -= errors;
        sour -= errors;

        spicy = Clamp(spicy);
        dry = Clamp(dry);
        sweet = Clamp(sweet);
        bitter = Clamp(bitter);
        sour = Clamp(sour);

        // ---------- foul if all zero ----------
        if (spicy == 0 && dry == 0 && sweet == 0 && bitter == 0 && sour == 0)
        {
            return CreateFoul();
        }

        int level = Math.Max(spicy, Math.Max(dry, Math.Max(sweet, Math.Max(bitter, sour))));
        int secondLevel = SecondHighest(spicy, dry, sweet, bitter, sour);

        // ---------- smoothness (Gen VIII) ----------
        if (amityBonus > 9) amityBonus = 9;

        int smoothness = (smoothnessSum / count) - amityBonus;
        if (smoothness < 0) smoothness = 0;

        // ---------- classification ----------
        int nonZero =
            (spicy > 0 ? 1 : 0) +
            (dry > 0 ? 1 : 0) +
            (sweet > 0 ? 1 : 0) +
            (bitter > 0 ? 1 : 0) +
            (sour > 0 ? 1 : 0);

        PoffinType type;
        if (level >= 95 && errors == 0)
            type = PoffinType.SuperMild;
        else if (level >= 50)
            type = PoffinType.Mild;
        else if (nonZero == 1)
            type = PoffinType.SingleFlavor;
        else if (nonZero == 2)
            type = PoffinType.DualFlavor;
        else if (nonZero == 3)
            type = PoffinType.Rich;
        else
            type = PoffinType.Overripe;

        // ---------- primary / secondary flavor ----------
        Flavor primary = Flavor.Spicy;
        Flavor secondary = Flavor.Spicy;
        int best = -1, second = -1;

        foreach (var f in FlavorPriority)
        {
            int v = GetFlavorValue(f, spicy, dry, sweet, bitter, sour);
            if (v > best)
            {
                second = best;
                secondary = primary;
                best = v;
                primary = f;
            }
            else if (v > second && v > 0)
            {
                second = v;
                secondary = f;
            }
        }

        return new Poffin(
            (byte)level,
            (byte)secondLevel,
            (byte)smoothness,
            (byte)spicy,
            (byte)dry,
            (byte)sweet,
            (byte)bitter,
            (byte)sour,
            type,
            primary,
            secondary
        );
    }

    // ---------- helpers ----------

    private static Poffin CreateFoul()
    {
        // Canonical foul payload (randomization handled outside core)
        return new Poffin(
            level: 2,
            secondLevel: 2,
            smoothness: 0,
            spicy: 2,
            dry: 2,
            sweet: 2,
            bitter: 0,
            sour: 0,
            type: PoffinType.Foul,
            primaryFlavor: Flavor.Spicy,
            secondaryFlavor: Flavor.Dry
        );
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int Clamp(int v) => v < 0 ? 0 : (v > 100 ? 100 : v);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int SecondHighest(int a, int b, int c, int d, int e)
    {
        int max = Math.Max(a, Math.Max(b, Math.Max(c, Math.Max(d, e))));
        int second = -1;

        if (a != max && a > second) second = a;
        if (b != max && b > second) second = b;
        if (c != max && c > second) second = c;
        if (d != max && d > second) second = d;
        if (e != max && e > second) second = e;

        return second < 0 ? 0 : second;
    }

    private static int GetFlavorValue(
        Flavor f, int spicy, int dry, int sweet, int bitter, int sour)
        => f switch
        {
            Flavor.Spicy => spicy,
            Flavor.Dry => dry,
            Flavor.Sweet => sweet,
            Flavor.Bitter => bitter,
            Flavor.Sour => sour,
            _ => 0
        };
}
