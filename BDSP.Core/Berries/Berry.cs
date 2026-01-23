using System.Runtime.CompilerServices;

namespace BDSP.Core.Berries
{
    /// <summary>
    /// Full berry metadata used for filtering, sorting, and pruning.
    /// </summary>
    public readonly struct Berry
    {
        /// <summary>Identifier for this berry.</summary>
        public readonly BerryId Id;
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
        /// <summary>Rarity value.</summary>
        public readonly byte Rarity;
        /// <summary>Main flavor (highest value, priority tie-break).</summary>
        public readonly Flavor MainFlavor;
        /// <summary>Secondary flavor (second highest value, priority tie-break).</summary>
        public readonly Flavor SecondaryFlavor;
        /// <summary>Main flavor value.</summary>
        public readonly byte MainFlavorValue;
        /// <summary>Secondary flavor value.</summary>
        public readonly byte SecondaryFlavorValue;
        /// <summary>Number of non-zero flavors.</summary>
        public readonly byte NumFlavors;

        public Berry(
            BerryId id,
            byte spicy,
            byte dry,
            byte sweet,
            byte bitter,
            byte sour,
            byte smoothness,
            byte rarity,
            Flavor mainFlavor,
            Flavor secondaryFlavor,
            byte mainFlavorValue,
            byte secondaryFlavorValue,
            byte numFlavors)
        {
            Id = id;
            Spicy = spicy;
            Dry = dry;
            Sweet = sweet;
            Bitter = bitter;
            Sour = sour;
            Smoothness = smoothness;
            Rarity = rarity;
            MainFlavor = mainFlavor;
            SecondaryFlavor = secondaryFlavor;
            MainFlavorValue = mainFlavorValue;
            SecondaryFlavorValue = secondaryFlavorValue;
            NumFlavors = numFlavors;
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

    }
}
