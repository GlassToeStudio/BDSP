namespace BDSP.Core.Runner;

/// <summary>
/// Optional pruning hints for search. These act as safe, optimistic bounds
/// to skip combinations that cannot satisfy minimum thresholds.
/// </summary>
/// <remarks>
/// All bounds are conservative (optimistic). If a bound says "cannot meet",
/// then the combination is safely skipped. If a bound says "maybe", the
/// combination is cooked normally.
/// </remarks>
public readonly struct PoffinSearchPruning
{
    /// <summary>Minimum required level (strongest flavor).</summary>
    public readonly bool HasMinLevel;
    public readonly byte MinLevel;

    /// <summary>Minimum required Spicy flavor value.</summary>
    public readonly bool HasMinSpicy;
    public readonly byte MinSpicy;

    /// <summary>Minimum required Dry flavor value.</summary>
    public readonly bool HasMinDry;
    public readonly byte MinDry;

    /// <summary>Minimum required Sweet flavor value.</summary>
    public readonly bool HasMinSweet;
    public readonly byte MinSweet;

    /// <summary>Minimum required Bitter flavor value.</summary>
    public readonly bool HasMinBitter;
    public readonly byte MinBitter;

    /// <summary>Minimum required Sour flavor value.</summary>
    public readonly bool HasMinSour;
    public readonly byte MinSour;

    /// <summary>Maximum allowed smoothness (lower is better).</summary>
    public readonly bool HasMaxSmoothness;
    public readonly byte MaxSmoothness;

    /// <summary>True when any pruning constraint is active.</summary>
    public bool IsEnabled =>
        HasMinLevel ||
        HasMinSpicy ||
        HasMinDry ||
        HasMinSweet ||
        HasMinBitter ||
        HasMinSour ||
        HasMaxSmoothness;

    /// <summary>
    /// Creates pruning constraints that align with common criteria filters.
    /// </summary>
    public PoffinSearchPruning(
        byte? minLevel = null,
        byte? minSpicy = null,
        byte? minDry = null,
        byte? minSweet = null,
        byte? minBitter = null,
        byte? minSour = null,
        byte? maxSmoothness = null)
    {
        HasMinLevel = minLevel.HasValue;
        MinLevel = minLevel ?? 0;

        HasMinSpicy = minSpicy.HasValue;
        MinSpicy = minSpicy ?? 0;

        HasMinDry = minDry.HasValue;
        MinDry = minDry ?? 0;

        HasMinSweet = minSweet.HasValue;
        MinSweet = minSweet ?? 0;

        HasMinBitter = minBitter.HasValue;
        MinBitter = minBitter ?? 0;

        HasMinSour = minSour.HasValue;
        MinSour = minSour ?? 0;

        HasMaxSmoothness = maxSmoothness.HasValue;
        MaxSmoothness = maxSmoothness ?? 0;
    }
}
