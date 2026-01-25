using BDSP.Core.Berries;

namespace BDSP.Core.Optimization
{
    /// <summary>
    /// Describes the berry recipe and cooking parameters used to create a poffin.
    /// </summary>
    public readonly struct PoffinRecipe
    {
        /// <summary>Berries used in this recipe (2-4).</summary>
        public readonly BerryId[] Berries;
        /// <summary>Cook time in seconds.</summary>
        public readonly int CookTimeSeconds;
        /// <summary>Number of spills during cooking.</summary>
        public readonly int Spills;
        /// <summary>Number of burns during cooking.</summary>
        public readonly int Burns;
        /// <summary>Amity bonus reduction (BDSP cap is 9).</summary>
        public readonly int AmityBonus;

        public PoffinRecipe(
            BerryId[] berries,
            int cookTimeSeconds,
            int spills,
            int burns,
            int amityBonus)
        {
            Berries = berries;
            CookTimeSeconds = cookTimeSeconds;
            Spills = spills;
            Burns = burns;
            AmityBonus = amityBonus;
        }
    }
}
