using System.Runtime.CompilerServices;

namespace BDSP.Core.Berries
{
    public readonly struct Berry
    {
        public readonly BerryId Id;
        public readonly byte Spicy;
        public readonly byte Dry;
        public readonly byte Sweet;
        public readonly byte Bitter;
        public readonly byte Sour;
        public readonly byte Smoothness;
        public readonly byte Rarity;
        public readonly Flavor MainFlavor;
        public readonly Flavor SecondaryFlavor;
        public readonly byte MainFlavorValue;
        public readonly byte SecondaryFlavorValue;
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
