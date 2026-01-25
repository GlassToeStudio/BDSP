using BDSP.Core.Poffins;

namespace BDSP.Core.Optimization
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

        public PoffinWithRecipe(Poffin poffin, PoffinRecipe recipe)
        {
            Poffin = poffin;
            Recipe = recipe;
        }
    }
}
