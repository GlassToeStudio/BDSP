using System;
using System.Linq;
using System.Runtime.CompilerServices;
using BDSP.Core.Primitives;

namespace BDSP.Core.Berries
{
    public readonly struct Berry
    {
        /// <summary>The unique identifier of the berry.</summary>
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

        /// <summary>
        /// Smoothness contribution of the berry. Higher? is better.
        /// </summary>
        public readonly byte Smoothness;

        /// <summary>
        /// Rarity of the berry. Lower is better.
        /// </summary>
        public readonly byte Rarity;

        public Berry(
            BerryId id,
            byte spicy,
            byte dry,
            byte sweet,
            byte bitter,
            byte sour,
            byte smoothness,
            byte rarity)
        {
            Id = id;
            Spicy = spicy;
            Dry = dry;
            Sweet = sweet;
            Bitter = bitter;
            Sour = sour;
            Smoothness = smoothness;
            Rarity = rarity;
        }

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