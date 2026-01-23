using System.Runtime.CompilerServices;
using BDSP.Core.Berries;

namespace BDSP.Core.Poffins
{
    /// <summary>
    /// Represents a cooked poffin (final flavor values + smoothness).
    /// </summary>
    public readonly struct Poffin
    {
        /// <summary>Spicy flavor value.</summary>
        public readonly byte Spicy;
        /// <summary>Dry flavor value.</summary>
        public readonly byte Dry;
        /// <summary>Sweet flavor value.</summary>
        public readonly byte Sweet;
        /// <summary>Bitter flavor value.</summary>
        public readonly byte Bitter;
        /// <summary>Sour flavor value.</summary>
        public readonly byte Sour;
        /// <summary>Smoothness value.</summary>
        public readonly byte Smoothness;
        /// <summary>True if this poffin is foul (invalid recipe).</summary>
        public readonly bool IsFoul;
        /// <summary>Highest flavor value (poffin level).</summary>
        public readonly byte Level;
        /// <summary>Second-highest flavor value.</summary>
        public readonly byte SecondLevel;
        /// <summary>Main flavor (highest value, priority tie-break).</summary>
        public readonly Flavor MainFlavor;
        /// <summary>Secondary flavor (second highest value, priority tie-break).</summary>
        public readonly Flavor SecondaryFlavor;
        /// <summary>Number of non-zero flavors.</summary>
        public readonly byte NumFlavors;

        public Poffin(
            byte spicy,
            byte dry,
            byte sweet,
            byte bitter,
            byte sour,
            byte smoothness,
            bool isFoul)
        {
            Spicy = spicy;
            Dry = dry;
            Sweet = sweet;
            Bitter = bitter;
            Sour = sour;
            Smoothness = smoothness;
            IsFoul = isFoul;

            NumFlavors = CountNonZero(spicy, dry, sweet, bitter, sour);
            (Flavor mainFlavor, byte level) = GetMainFlavor(spicy, dry, sweet, bitter, sour);
            (Flavor secondaryFlavor, byte secondLevel) = GetSecondaryFlavor(
                spicy, dry, sweet, bitter, sour, mainFlavor, NumFlavors);
            MainFlavor = mainFlavor;
            Level = level;
            SecondaryFlavor = secondaryFlavor;
            SecondLevel = secondLevel;
        }

        /// <summary>
        /// Returns the value for the given flavor.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public byte GetFlavor(Flavor flavor)
        {
            return flavor switch
            {
                Flavor.Spicy => Spicy,
                Flavor.Dry => Dry,
                Flavor.Sweet => Sweet,
                Flavor.Bitter => Bitter,
                Flavor.Sour => Sour,
                _ => 0
            };
        }

        private static byte CountNonZero(byte spicy, byte dry, byte sweet, byte bitter, byte sour)
        {
            byte count = 0;
            if (spicy > 0) count++;
            if (dry > 0) count++;
            if (sweet > 0) count++;
            if (bitter > 0) count++;
            if (sour > 0) count++;
            return count;
        }

        private static (Flavor Flavor, byte Value) GetMainFlavor(
            byte spicy,
            byte dry,
            byte sweet,
            byte bitter,
            byte sour)
        {
            var bestFlavor = Flavor.Spicy;
            var bestValue = spicy;

            Consider(Flavor.Dry, dry, ref bestFlavor, ref bestValue);
            Consider(Flavor.Sweet, sweet, ref bestFlavor, ref bestValue);
            Consider(Flavor.Bitter, bitter, ref bestFlavor, ref bestValue);
            Consider(Flavor.Sour, sour, ref bestFlavor, ref bestValue);

            return (bestFlavor, bestValue);
        }

        private static (Flavor Flavor, byte Value) GetSecondaryFlavor(
            byte spicy,
            byte dry,
            byte sweet,
            byte bitter,
            byte sour,
            Flavor mainFlavor,
            byte numFlavors)
        {
            if (numFlavors < 2)
            {
                return (Flavor.None, 0);
            }

            var bestFlavor = Flavor.None;
            byte bestValue = 0;

            ConsiderSecondary(Flavor.Spicy, spicy, mainFlavor, ref bestFlavor, ref bestValue);
            ConsiderSecondary(Flavor.Dry, dry, mainFlavor, ref bestFlavor, ref bestValue);
            ConsiderSecondary(Flavor.Sweet, sweet, mainFlavor, ref bestFlavor, ref bestValue);
            ConsiderSecondary(Flavor.Bitter, bitter, mainFlavor, ref bestFlavor, ref bestValue);
            ConsiderSecondary(Flavor.Sour, sour, mainFlavor, ref bestFlavor, ref bestValue);

            return (bestFlavor, bestValue);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void Consider(
            Flavor flavor,
            byte value,
            ref Flavor bestFlavor,
            ref byte bestValue)
        {
            if (value > bestValue || (value == bestValue && HasHigherPriority(flavor, bestFlavor)))
            {
                bestFlavor = flavor;
                bestValue = value;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void ConsiderSecondary(
            Flavor flavor,
            byte value,
            Flavor mainFlavor,
            ref Flavor bestFlavor,
            ref byte bestValue)
        {
            if (flavor == mainFlavor)
            {
                return;
            }

            if (value > bestValue || (value == bestValue && HasHigherPriority(flavor, bestFlavor)))
            {
                bestFlavor = flavor;
                bestValue = value;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool HasHigherPriority(Flavor candidate, Flavor current)
        {
            return GetPriority(candidate) > GetPriority(current);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int GetPriority(Flavor flavor)
        {
            return flavor switch
            {
                Flavor.Spicy => 5,
                Flavor.Dry => 4,
                Flavor.Sweet => 3,
                Flavor.Bitter => 2,
                Flavor.Sour => 1,
                _ => 0
            };
        }
    }
}
