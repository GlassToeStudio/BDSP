using System.Runtime.CompilerServices;

namespace BDSP.Core.Berries
{
    public readonly struct BerryBase
    {
        public readonly BerryId Id;
        public readonly byte Spicy;
        public readonly byte Dry;
        public readonly byte Sweet;
        public readonly byte Bitter;
        public readonly byte Sour;
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
