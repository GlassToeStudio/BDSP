using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BDSP.Core.Primitives;

namespace BDSP.Core.Poffins
{
    /// <summary>
    /// Represents the immutable result of cooking a Poffin. This is a lightweight data container.
    /// </summary>
    public readonly struct Poffin
    {
        /// <summary>The level of the Poffin, determined by its strongest flavor value.</summary>
        public readonly byte Level;

        /// <summary>The value of the Poffin's second-strongest flavor.</summary>
        public readonly byte SecondLevel;

        /// <summary>The smoothness value of the Poffin. Lower is generally better.</summary>
        public readonly byte Smoothness;

        /// <summary>The final Spicy flavor value after cooking calculations.</summary>
        public readonly byte Spicy;

        /// <summary>The final Dry flavor value after cooking calculations.</summary>
        public readonly byte Dry;

        /// <summary>The final Sweet flavor value after cooking calculations.</summary>
        public readonly byte Sweet;

        /// <summary>The final Bitter flavor value after cooking calculations.</summary>
        public readonly byte Bitter;

        /// <summary>The final Sour flavor value after cooking calculations.</summary>
        public readonly byte Sour;

        /// <summary>The classification of the Poffin (e.g., Mild, Foul, Rich).</summary>
        public readonly PoffinType Type;

        /// <summary>The dominant flavor of the Poffin, used for naming and sorting.</summary>
        public readonly Flavor PrimaryFlavor;

        /// <summary>The second dominant flavor of the Poffin, used for naming and sorting.</summary>
        public readonly Flavor SecondaryFlavor;

        /// <summary>
        /// Initializes a new instance of the <see cref="Poffin"/> struct.
        /// </summary>
        /// <param name="level">The level of the Poffin.</param>
        /// <param name="secondLevel">The second level of the Poffin.</param>
        /// <param name="smoothness">The smoothness of the Poffin.</param>
        /// <param name="spicy">The final Spicy flavor value.</param>
        /// <param name="dry">The final Dry flavor value.</param>
        /// <param name="sweet">The final Sweet flavor value.</param>
        /// <param name="bitter">The final Bitter flavor value.</param>
        /// <param name="sour">The final Sour flavor value.</param>
        /// <param name="type">The type classification of the Poffin.</param>
        /// <param name="primaryFlavor">The primary flavor of the Poffin.</param>
        /// <param name="secondaryFlavor">The secondary flavor of the Poffin.</param>
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

        public override string ToString()
        {
            return $"{Type} L{Level} (Sm {Smoothness}) [{Spicy},{Dry},{Sweet},{Bitter},{Sour}]";
        }
    }
}
