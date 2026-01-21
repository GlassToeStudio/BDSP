using System;
using System.Linq;
using System.Runtime.CompilerServices;
using BDSP.Core.Primitives;

namespace BDSP.Core.Berries.Data
{
    /// <summary>
    /// Represents the canonical, immutable data for a single Pokémon berry in Generation VIII.
    /// </summary>
    /// <param name="id">The unique identifier of the berry.</param>
    /// <param name="spicy">Spicy flavor value.</param>
    /// <param name="dry">Dry flavor value.</param>
    /// <param name="sweet">Sweet flavor value.</param>
    /// <param name="bitter">Bitter flavor value.</param>
    /// <param name="sour">Sour flavor value.</param>
    /// <param name="smoothness">The smoothness value of the berry.</param>
    /// <param name="rarity">The rarity value of the berry.</param>
    public readonly struct Berry(
        BerryId id,
        byte spicy,
        byte dry,
        byte sweet,
        byte bitter,
        byte sour,
        byte smoothness,
        byte rarity)
    {
        /// <summary>The unique identifier of the berry.</summary>
        public readonly BerryId Id = id;

        /// <summary>Spicy flavor value.</summary>
        public readonly byte Spicy = spicy;

        /// <summary>Dry flavor value.</summary>
        public readonly byte Dry = dry;

        /// <summary>Sweet flavor value.</summary>
        public readonly byte Sweet = sweet;

        /// <summary>Bitter flavor value.</summary>
        public readonly byte Bitter = bitter;

        /// <summary>Sour flavor value.</summary>
        public readonly byte Sour = sour;

        /// <summary>Smoothness contribution of the berry. Lower values are generally better as they contribute less to a Pokémon's Sheen.</summary>
        public readonly byte Smoothness = smoothness;

        /// <summary>Rarity of the berry. Lower values are generally more common and thus preferred for recipes.</summary>
        public readonly byte Rarity = rarity;

        /// <summary>
        /// Gets the value for a specific flavor using the <see cref="Flavor"/> enum.
        /// </summary>
        /// <param name="flavor">The flavor to retrieve the value for.</param>
        /// <returns>The byte value of the requested flavor.</returns>
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
