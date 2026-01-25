using BDSP.Core.Poffins;

namespace BDSP.Core.Optimization.Core
{
    /// <summary>
    /// Poffin result paired with its recipe metadata.
    /// </summary>
    public readonly struct PoffinWithRecipe
    {
        /// <summary>Cooked poffin.</summary>
        public readonly Poffin Poffin;
        /// <summary>Recipe metadata.</summary>
        public readonly PoffinRecipe Recipe;
        /// <summary>Number of duplicate recipes that produced identical poffin stats.</summary>
        public readonly int DuplicateCount;

        public PoffinWithRecipe(Poffin poffin, PoffinRecipe recipe, int duplicateCount = 1)
        {
            Poffin = poffin;
            Recipe = recipe;
            DuplicateCount = duplicateCount;
        }
    }
}
