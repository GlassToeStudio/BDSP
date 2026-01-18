using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BDSP.Core.Primitives;

namespace BDSP.Core.Poffins
{
    public readonly struct Poffin
    {
        public readonly byte Level;
        public readonly byte SecondLevel;
        public readonly byte Smoothness;

        public readonly byte Spicy;
        public readonly byte Dry;
        public readonly byte Sweet;
        public readonly byte Bitter;
        public readonly byte Sour;

        public readonly PoffinType Type;

        // Naming metadata (no strings)
        public readonly Flavor PrimaryFlavor;
        public readonly Flavor SecondaryFlavor;

        public Poffin(
            byte level,
            byte secondLevel,
            byte smoothness,
            byte spicy,
            byte dry,
            byte sweet,
            byte bitter,
            byte sour,
            PoffinType type,
            Flavor primaryFlavor,
            Flavor secondaryFlavor)
        {
            Level = level;
            SecondLevel = secondLevel;
            Smoothness = smoothness;

            Spicy = spicy;
            Dry = dry;
            Sweet = sweet;
            Bitter = bitter;
            Sour = sour;

            Type = type;
            PrimaryFlavor = primaryFlavor;
            SecondaryFlavor = secondaryFlavor;
        }
    }
}