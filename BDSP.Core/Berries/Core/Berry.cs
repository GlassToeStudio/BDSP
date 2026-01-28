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
        /// <summary>Spicy flavor value (0-40).</summary>
        public readonly byte Spicy;
        /// <summary>Dry flavor value (0-40).</summary>
        public readonly byte Dry;
        /// <summary>Sweet flavor value (0-40).</summary>
        public readonly byte Sweet;
        /// <summary>Bitter flavor value (0-40).</summary>
        public readonly byte Bitter;
        /// <summary>Sour flavor value (0-40).</summary>
        public readonly byte Sour;
        /// <summary>Smoothness value (20-60).</summary>
        public readonly byte Smoothness;
        /// <summary>Rarity value (1-15).</summary>
        public readonly byte Rarity;
        /// <summary>Main flavor (highest value, priority tie-break).</summary>
        public readonly Flavor MainFlavor;
        /// <summary>Secondary flavor (second highest value, priority tie-break).</summary>
        public readonly Flavor SecondaryFlavor;
        /// <summary>Main flavor value (0-40).</summary>
        public readonly byte MainFlavorValue;
        /// <summary>Secondary flavor value (0-40).</summary>
        public readonly byte SecondaryFlavorValue;
        /// <summary>Number of non-zero flavors (1-5).</summary>
        public readonly byte NumFlavors;

        public Berry(BerryId id, byte spicy, byte dry, byte sweet, byte bitter, byte sour, byte smoothness, byte rarity,
            Flavor mainFlavor, Flavor secondaryFlavor, byte mainFlavorValue, byte secondaryFlavorValue, byte numFlavors)
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
        public byte GetFlavorValue(Flavor flavor)
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

        /// <summary>
        /// Returns a aligned, human-readable description of the berry.
        /// </summary>
        /// <remarks>
        /// Example: ganlon  Dry    (30) - Flavors [  0,  30,  10,  30,   0] Smoothness: 40, Rarity:  9
        /// </remarks>
        public override string ToString()
        {
            var name = BerryNames.GetName(Id);
            return $"{(name.EndsWith(" Berry", StringComparison.Ordinal) ? name = name[..^6] : name).ToLowerInvariant(),-7} {MainFlavor,-6} ({MainFlavorValue,2}) - Flavors [{Spicy,3}, {Dry,3}, {Sweet,3}, {Bitter,3}, {Sour,3}] Smoothness: {Smoothness,2}, Rarity: {Rarity,2}";
        }

    }
}
