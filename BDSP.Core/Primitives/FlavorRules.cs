using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BDSP.Core.Primitives
{
    /// <summary>
    /// Defines flavor interaction rules for BDSP (Generation VIII).
    /// </summary>
    public static class FlavorRules
    {
        // Index = flavor, value = weakened by
        /// <summary>
        /// Applies flavor weakening according to the BDSP flavor cycle.
        /// </summary>
        /// <remarks>
        /// The flavor weakening cycle is as follows:
        /// Flavor X is weakened by Flavor Y:
        /// <br/>X -> Y <br/>
        /// Spicy → Dry → Sweet → Bitter → Sour → Spicy
        /// </remarks>
        public static readonly Flavor[] WeakenedBy =
        [
            Flavor.Dry,     // Weakens -> Spicy
            Flavor.Sweet,   // Weakens -> Dry
            Flavor.Bitter,  // Weakens -> Sweet
            Flavor.Sour,    // Weakens -> Bitter
            Flavor.Spicy    // Weakens -> Sour
        ];
    }
}
