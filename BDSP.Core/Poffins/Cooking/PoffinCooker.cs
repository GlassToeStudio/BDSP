using System;
using System.Collections.Generic;
using System.Diagnostics;
using BDSP.Core.Berries;
using BDSP.Core.Poffins;

namespace BDSP.Core.Poffins.Cooking
{
    /// <summary>
    /// Implements BDSP cooking rules for poffin creation.
    /// Expects 1-4 unique berries; duplicates are invalid.
    /// </summary>
    public static class PoffinCooker
    {
        public const int MaxFlavor = 100;

        /// <summary>
        /// Cook a poffin from a set of berries.
        /// </summary>
        /// <param name="berries">Unique berries used in the recipe (1-4).</param>
        /// <param name="cookTimeSeconds">Cook time in seconds.</param>
        /// <param name="spills">Number of spills during cooking.</param>
        /// <param name="burns">Number of burns during cooking.</param>
        /// <param name="amityBonus">Bonus smoothness reduction (BDSP cap is 9).</param>
        public static Poffin Cook(ReadOnlySpan<BerryBase> berries,int cookTimeSeconds,int spills,int burns,int amityBonus = 9)
        {
            int count = berries.Length;

#if DEBUG
            if (count == 0)
            {
                Debug.Assert(false, "No berries passed to cooker.");
                throw new ArgumentException("At least one berry is required.", nameof(berries));
            }

            if(HasDuplicateBerries(berries))
            {
                // Berries are expected to be unique in production combinations.
                return CreateFoul();
                
            }
#endif

            int spicySum = 0;
            int drySum = 0;
            int sweetSum = 0;
            int bitterSum = 0;
            int sourSum = 0;
            int smoothnessSum = 0;

            // Sum pre-weakened flavor values across all berries.
            // BerryBase already stores each flavor with its weakness applied.
            for (int i = 0; i < count; i++)
            {
                ref readonly BerryBase b = ref berries[i];
                spicySum += b.WeakSpicy;
                drySum += b.WeakDry;
                sweetSum += b.WeakSweet;
                bitterSum += b.WeakBitter;
                sourSum += b.WeakSour;
                smoothnessSum += b.Smoothness;
            }

            return CookFromBase(
                spicySum,
                drySum,
                sweetSum,
                bitterSum,
                sourSum,
                smoothnessSum,
                count,
                cookTimeSeconds,
                spills,
                burns,
                amityBonus);
        }

        /// <summary>
        /// Cook a poffin from a precomputed combination base.
        /// </summary>
        /// <param name="combo">Precomputed sums for a unique berry combo (2–4).</param>
        /// <param name="cookTimeSeconds">Cook time in seconds.</param>
        /// <param name="spills">Number of spills during cooking.</param>
        /// <param name="burns">Number of burns during cooking.</param>
        /// <param name="amityBonus">Bonus smoothness reduction (BDSP cap is 9).</param>
        public static Poffin Cook(in PoffinComboBase combo,int cookTimeSeconds,int spills,int burns,int amityBonus = 9)
        {
            return CookFromBase(
                combo.WeakSpicySum,
                combo.WeakDrySum,
                combo.WeakSweetSum,
                combo.WeakBitterSum,
                combo.WeakSourSum,
                combo.SmoothnessSum,
                combo.Count,
                cookTimeSeconds,
                spills,
                burns,
                amityBonus);
        }

        /// <summary>
        /// Shared cooking logic for either per-berry sums or precomputed combo sums.
        /// </summary>
        private static Poffin CookFromBase(int spicySum,int drySum,int sweetSum,int bitterSum,int sourSum,int smoothnessSum,int count,int cookTimeSeconds,int spills,int burns,int amityBonus)
        {
            int spicy = spicySum;
            int dry = drySum;
            int sweet = sweetSum;
            int bitter = bitterSum;
            int sour = sourSum;

            // Negative flavor penalty: each negative flavor reduces all five flavors by 1.
            int negatives = 0;
            if (spicy < 0) negatives++;
            if (dry < 0) negatives++;
            if (sweet < 0) negatives++;
            if (bitter < 0) negatives++;
            if (sour < 0) negatives++;

            if (negatives > 0)
            {
                spicy -= negatives;
                dry -= negatives;
                sweet -= negatives;
                bitter -= negatives;
                sour -= negatives;
            }

            // Time modifier: scale by value * 60 / cookTimeSeconds.
            // NOTE: Uses truncation (integer division) for speed and to match README behavior.
            int modifier = 60;
            if (cookTimeSeconds != modifier)
            {
                spicy = spicy * modifier / cookTimeSeconds;
                dry = dry * modifier / cookTimeSeconds;
                sweet = sweet * modifier / cookTimeSeconds;
                bitter = bitter * modifier / cookTimeSeconds;
                sour = sour * modifier / cookTimeSeconds;
            }

            // ---------- mistake penalty ----------
            int errors = spills + burns;
            if (errors != 0)
            {
                spicy -= errors;
                dry -= errors;
                sweet -= errors;
                bitter -= errors;
                sour -= errors;
            }

            // Clamp negatives to 0 and apply the Gen VIII flavor cap of 100.
            spicy = (byte)ClampFlavor(spicy);
            dry = (byte)ClampFlavor(dry);
            sweet = (byte)ClampFlavor(sweet);
            bitter = (byte)ClampFlavor(bitter);
            sour = (byte)ClampFlavor(sour);

            if (spicy == 0 && dry == 0 && sweet == 0 && bitter == 0 && sour == 0)
            {
                return CreateFoul();
            }

            // Smoothness: floor(average berry smoothness) - number of berries - amity bonus.
            int avgSmoothness = smoothnessSum / count;
            int smoothness = avgSmoothness - count - amityBonus;
            if (smoothness < 0)
            {
                smoothness = 0;
            }

            return new Poffin(
                (byte)spicy,
                (byte)dry,
                (byte)sweet,
                (byte)bitter,
                (byte)sour,
                (byte)smoothness,
                isFoul: false);
        }

        /// <summary>
        /// Checks for duplicate berries (debug-only validation).
        /// </summary>
        /// <param name="berries">Input berries.</param>
        private static bool HasDuplicateBerries(ReadOnlySpan<BerryBase> berries)
        {
            int count = berries.Length;
            if (count < 2)
            {
                return false;
            }

            for (int i = 0; i < count - 1; i++)
            {
                ushort id = berries[i].Id.Value;
                for (int j = i + 1; j < count; j++)
                {
                    if (id == berries[j].Id.Value)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private static int ClampFlavor(int value)
        {
            if (value < 0) return 0;
            if (value > MaxFlavor) return MaxFlavor;
            return value;
        }

        private static Poffin CreateFoul()
        {
            return new Poffin(2, 2, 2, 0, 0, 0, isFoul: true);
        }
    }
}
