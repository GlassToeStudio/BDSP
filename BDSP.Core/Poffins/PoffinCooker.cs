using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using BDSP.Core.Berries.Data;
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
    /// <param name="berryIds">The berries used (1�4).</param>
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
        return CookCoreFromIds(
            berryIds,
            cookTimeSeconds,
            errors,
            amityBonus,
            checkDuplicates: true);
    }

    /// <summary>
    /// Cooks a poffin from unique berry IDs (duplicate check skipped).
    /// </summary>
    internal static Poffin CookUnique(
        ReadOnlySpan<BerryId> berryIds,
        byte cookTimeSeconds,
        byte errors,
        byte amityBonus)
    {
        return CookCoreFromIds(
            berryIds,
            cookTimeSeconds,
            errors,
            amityBonus,
            checkDuplicates: false);
    }

    /// <summary>
    /// Cooks a poffin from unique berries (duplicate check skipped).
    /// </summary>
    internal static Poffin CookFromBerriesUnique(
        ReadOnlySpan<Berry> berries,
        byte cookTimeSeconds,
        byte errors,
        byte amityBonus)
    {
        if (cookTimeSeconds == 0)
            throw new ArgumentOutOfRangeException(nameof(cookTimeSeconds));

#if DEBUG
        Debug.Assert(berries.Length is >= 1 and <= 4);
#endif
        int count = berries.Length;

        int spicy = 0, dry = 0, sweet = 0, bitter = 0, sour = 0;
        int smoothnessSum = 0;

        for (int i = 0; i < count; i++)
        {
            ref readonly Berry b = ref berries[i];

            spicy += b.Spicy;
            dry += b.Dry;
            sweet += b.Sweet;
            bitter += b.Bitter;
            sour += b.Sour;

            smoothnessSum += b.Smoothness;
        }

        return CookFromTotals(
            spicy,
            dry,
            sweet,
            bitter,
            sour,
            smoothnessSum,
            count,
            cookTimeSeconds,
            errors,
            amityBonus);
    }

    private static Poffin CookCoreFromIds(
        ReadOnlySpan<BerryId> berryIds,
        byte cookTimeSeconds,
        byte errors,
        byte amityBonus,
        bool checkDuplicates)
    {
        if (cookTimeSeconds == 0)
            throw new ArgumentOutOfRangeException(nameof(cookTimeSeconds));

#if DEBUG
        Debug.Assert(berryIds.Length is >= 1 and <= 4);
#endif
        int count = berryIds.Length;

        // ---------- duplicate check ----------
        if (checkDuplicates)
        {
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

        return CookFromTotals(
            spicy,
            dry,
            sweet,
            bitter,
            sour,
            smoothnessSum,
            count,
            cookTimeSeconds,
            errors,
            amityBonus);
    }

    private static Poffin CookFromTotals(
        int spicy,
        int dry,
        int sweet,
        int bitter,
        int sour,
        int smoothnessSum,
        int count,
        byte cookTimeSeconds,
        byte errors,
        byte amityBonus)
    {
        // ---------- weakening (correct order) ----------
        /// Spicy → Dry → Sweet → Bitter → Sour → Spicy
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

        // ---------- time + mistake modifier (rounding) ----------
        // ---------- time multiplier (truncate) ----------
        // NOTE: Docs describe rounding, but we intentionally truncate for speed.
        if (cookTimeSeconds != 60)
        {
            spicy = spicy * 60 / cookTimeSeconds;
            dry = dry * 60 / cookTimeSeconds;
            sweet = sweet * 60 / cookTimeSeconds;
            bitter = bitter * 60 / cookTimeSeconds;
            sour = sour * 60 / cookTimeSeconds;
        }

        // ---------- mistake penalty ----------
        if (errors != 0)
        {
            spicy -= errors;
            dry -= errors;
            sweet -= errors;
            bitter -= errors;
            sour -= errors;
        }

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

        int smoothness = (smoothnessSum / count) - count - amityBonus;
        if (smoothness < 0) smoothness = 0;
        // NOTE: This matches BDSP (max bonus 9). Gen IV can reach a lower minimum
        // smoothness (bonus up to 10), which is not modeled here.

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
        GetPrimarySecondary(
            spicy,
            dry,
            sweet,
            bitter,
            sour,
            out Flavor primary,
            out Flavor secondary);

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
        int spicy = 0, dry = 0, sweet = 0, bitter = 0, sour = 0;
        Span<byte> pool = stackalloc byte[5] { 0, 1, 2, 3, 4 };

        // Fisher-Yates shuffle for 3 picks.
        for (int i = pool.Length - 1; i > 0; i--)
        {
            int j = Random.Shared.Next(i + 1);
            (pool[i], pool[j]) = (pool[j], pool[i]);
        }

        for (int i = 0; i < 3; i++)
        {
            switch ((Flavor)pool[i])
            {
                case Flavor.Spicy: spicy = 2; break;
                case Flavor.Dry: dry = 2; break;
                case Flavor.Sweet: sweet = 2; break;
                case Flavor.Bitter: bitter = 2; break;
                case Flavor.Sour: sour = 2; break;
            }
        }

        GetPrimarySecondary(
            spicy,
            dry,
            sweet,
            bitter,
            sour,
            out Flavor primary,
            out Flavor secondary);

        return new Poffin(
            level: 2,
            secondLevel: 2,
            smoothness: 0,
            spicy: (byte)spicy,
            dry: (byte)dry,
            sweet: (byte)sweet,
            bitter: (byte)bitter,
            sour: (byte)sour,
            type: PoffinType.Foul,
            primaryFlavor: primary,
            secondaryFlavor: secondary
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

    private static void GetPrimarySecondary(
        int spicy,
        int dry,
        int sweet,
        int bitter,
        int sour,
        out Flavor primary,
        out Flavor secondary)
    {
        primary = Flavor.Spicy;
        secondary = Flavor.Spicy;
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
    }
}
