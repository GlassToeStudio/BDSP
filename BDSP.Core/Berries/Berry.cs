using System;
using System.Linq;
using System.Runtime.CompilerServices;
using BDSP.Core.Primitives;

namespace BDSP.Core.Berries
{
    public readonly struct Berry
    {
        public readonly BerryId Id;

        // Packed flavor values: length = 5
        // Spicy, Dry, Sweet, Bitter, Sour
        public readonly byte Spicy;
        public readonly byte Dry;
        public readonly byte Sweet;
        public readonly byte Bitter;
        public readonly byte Sour;

        public readonly byte Smoothness;
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