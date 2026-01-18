using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BDSP.Core.Primitives
{
    public static class FlavorRules
    {
        // Index = flavor, value = weakened by
        public static readonly Flavor[] WeakenedBy =
        [
            Flavor.Dry,     // Spicy
            Flavor.Spicy,   // Dry
            Flavor.Sour,    // Sweet
            Flavor.Bitter,  // Bitter
            Flavor.Sweet    // Sour
        ];
    }
}
