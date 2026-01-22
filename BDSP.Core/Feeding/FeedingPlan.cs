namespace BDSP.Core.Feeding;

/// <summary>
/// Represents a concrete feeding plan consisting of multiple Poffins.
/// </summary>
public sealed class FeedingPlan
{
    /// <summary>The Poffins fed, in order.</summary>
    public IReadOnlyList<Poffins.Poffin> Poffins { get; }

    /// <summary>Final accumulated state.</summary>
    public FeedingState FinalState { get; }

    internal FeedingPlan(
        List<Poffins.Poffin> poffins,
        FeedingState finalState)
    {
        Poffins = poffins;
        FinalState = finalState;
    }

    public override string ToString()
    {
        return $"Poffins {Poffins.Count}, {FinalState}";
    }
}
