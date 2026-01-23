using BDSP.Core.Berries.Data;
using BDSP.Core.Poffins;
using BDSP.Core.Runner;
using BDSP.Core.Selection;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace BDSP.Core.Tests;

public sealed class PoffinSearchRunnerRecipeTests
{
    [Fact]
    public void RunWithRecipes_Matches_Run_ForPoffins()
    {
        var pool = new BerryId[]
        {
            new(0),
            new(7),
            new(14),
            new(34),
            new(64)
        };

        var comparer = new LevelThenSmoothnessComparer();
        PoffinPredicate predicate = static (in Poffin p) => p.Type != PoffinType.Foul;

        var baseline = PoffinSearchRunner.Run(
            berryPool: pool,
            berriesPerPoffin: 2,
            topK: 25,
            cookTimeSeconds: 40,
            errors: 0,
            amityBonus: 0,
            comparer: comparer,
            predicate: predicate);

        var withRecipes = PoffinSearchRunner.RunWithRecipes(
            berryPool: pool,
            berriesPerPoffin: 2,
            topK: 25,
            cookTimeSeconds: 40,
            errors: 0,
            amityBonus: 0,
            comparer: comparer,
            predicate: predicate);

        Assert.Equal(baseline.TopPoffins.Length, withRecipes.TopRecipes.Length);
        var baselineSet = new HashSet<Poffin>(baseline.TopPoffins);
        var recipeSet = new HashSet<Poffin>(withRecipes.TopRecipes.Select(r => r.Poffin));
        Assert.True(baselineSet.SetEquals(recipeSet));
    }

    [Fact]
    public void PoffinRecipe_IsImmutableToCaller()
    {
        var poffin = new Poffin(
            level: 10,
            secondLevel: 5,
            smoothness: 15,
            spicy: 10,
            dry: 0,
            sweet: 0,
            bitter: 0,
            sour: 0,
            type: PoffinType.SingleFlavor,
            primaryFlavor: BDSP.Core.Primitives.Flavor.Spicy,
            secondaryFlavor: BDSP.Core.Primitives.Flavor.Spicy);

        var berries = new[] { new BerryId(1), new BerryId(2) };
        var recipe = new PoffinRecipe(poffin, berries);

        berries[0] = new BerryId(5);
        Assert.Equal((ushort)1, recipe.Berries.Span[0].Value);
    }

    [Fact]
    public void RunWithRecipes_RecipesReproducePoffins()
    {
        var pool = new BerryId[]
        {
            new(0),
            new(7),
            new(14),
            new(34),
            new(64)
        };

        var comparer = new LevelThenSmoothnessComparer();
        var withRecipes = PoffinSearchRunner.RunWithRecipes(
            berryPool: pool,
            berriesPerPoffin: 2,
            topK: 10,
            cookTimeSeconds: 40,
            errors: 0,
            amityBonus: 0,
            comparer: comparer);

        foreach (var r in withRecipes.TopRecipes)
        {
            Assert.Equal(2, r.Berries.Length);

            var cooked = PoffinCooker.Cook(
                r.Berries.Span,
                cookTimeSeconds: 40,
                errors: 0,
                amityBonus: 0);

            Assert.Equal(cooked, r.Poffin);
        }
    }
}
