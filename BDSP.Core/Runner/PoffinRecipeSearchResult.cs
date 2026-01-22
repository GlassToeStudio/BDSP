using BDSP.Core.Poffins;

namespace BDSP.Core.Runner;

public readonly struct PoffinRecipeSearchResult
{
    public readonly PoffinRecipe[] TopRecipes;

    public PoffinRecipeSearchResult(PoffinRecipe[] topRecipes)
    {
        TopRecipes = topRecipes;
    }
}
