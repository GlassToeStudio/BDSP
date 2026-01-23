using System.Runtime.CompilerServices;

namespace BDSP.Core.Berries
{
    /// <summary>
    /// Minimal berry data required for cooking (flavors + smoothness).
    /// </summary>
    public readonly struct BerryBase
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

        public BerryBase(
            BerryId id,
            byte spicy,
            byte dry,
            byte sweet,
            byte bitter,
            byte sour,
            byte smoothness)
        {
            Id = id;
            Spicy = spicy;
            Dry = dry;
            Sweet = sweet;
            Bitter = bitter;
            Sour = sour;
            Smoothness = smoothness;
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
