using System.Runtime.CompilerServices;

namespace BDSP.Core.Berries
{
    /// <summary>
    /// Minimal berry data required for cooking (flavors + smoothness),
    /// plus precomputed weakened flavor values.
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
        /// <summary>Spicy value after per-berry weakness subtraction.</summary>
        public readonly sbyte WeakSpicy;
        /// <summary>Dry value after per-berry weakness subtraction.</summary>
        public readonly sbyte WeakDry;
        /// <summary>Sweet value after per-berry weakness subtraction.</summary>
        public readonly sbyte WeakSweet;
        /// <summary>Bitter value after per-berry weakness subtraction.</summary>
        public readonly sbyte WeakBitter;
        /// <summary>Sour value after per-berry weakness subtraction.</summary>
        public readonly sbyte WeakSour;

        public BerryBase(
            BerryId id,
            byte spicy,
            byte dry,
            byte sweet,
            byte bitter,
            byte sour,
            byte smoothness,
            sbyte weakSpicy,
            sbyte weakDry,
            sbyte weakSweet,
            sbyte weakBitter,
            sbyte weakSour)
        {
            Id = id;
            Spicy = spicy;
            Dry = dry;
            Sweet = sweet;
            Bitter = bitter;
            Sour = sour;
            Smoothness = smoothness;
            WeakSpicy = weakSpicy;
            WeakDry = weakDry;
            WeakSweet = weakSweet;
            WeakBitter = weakBitter;
            WeakSour = weakSour;
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

        /// <summary>
        /// Returns the weakened value for the given flavor.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public sbyte GetWeakenedFlavor(Flavor flavor)
        {
            return flavor switch
            {
                Flavor.Spicy => WeakSpicy,
                Flavor.Dry => WeakDry,
                Flavor.Sweet => WeakSweet,
                Flavor.Bitter => WeakBitter,
                Flavor.Sour => WeakSour,
                _ => 0
            };
        }
    }
}
