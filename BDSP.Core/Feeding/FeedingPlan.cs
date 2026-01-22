using System;
using System.Collections.Generic;

namespace BDSP.Core.Feeding;

/// <summary>
/// Represents a concrete feeding plan consisting of multiple Poffins.
/// </summary>
public sealed class FeedingPlan
{
    /// <summary>The Poffins fed, in order.</summary>
    public IReadOnlyList<Poffins.Poffin> Poffins { get; }

    /// <summary>The Poffin recipes fed, in order (if available).</summary>
    public IReadOnlyList<Poffins.PoffinRecipe> Recipes { get; }

    /// <summary>Final accumulated state.</summary>
    public FeedingState FinalState { get; }

    internal FeedingPlan(
        List<Poffins.Poffin> poffins,
        FeedingState finalState)
    {
        Poffins = poffins;
        Recipes = Array.Empty<Poffins.PoffinRecipe>();
        FinalState = finalState;
    }

    internal FeedingPlan(
        List<Poffins.PoffinRecipe> recipes,
        FeedingState finalState)
    {
        Recipes = recipes;
        var poffins = new List<Poffins.Poffin>(recipes.Count);
        for (int i = 0; i < recipes.Count; i++)
            poffins.Add(recipes[i].Poffin);
        Poffins = poffins;
        FinalState = finalState;
    }

    public override string ToString()
    {
        return $"Poffins {Poffins.Count}, {FinalState}";
    }
}
