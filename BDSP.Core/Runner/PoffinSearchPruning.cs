namespace BDSP.Core.Runner;

public readonly struct PoffinSearchPruning
{
    public readonly bool HasMinLevel;
    public readonly byte MinLevel;

    public readonly bool HasMinSpicy;
    public readonly byte MinSpicy;

    public readonly bool HasMinDry;
    public readonly byte MinDry;

    public readonly bool HasMinSweet;
    public readonly byte MinSweet;

    public readonly bool HasMinBitter;
    public readonly byte MinBitter;

    public readonly bool HasMinSour;
    public readonly byte MinSour;

    public bool IsEnabled =>
        HasMinLevel ||
        HasMinSpicy ||
        HasMinDry ||
        HasMinSweet ||
        HasMinBitter ||
        HasMinSour;

    public PoffinSearchPruning(
        byte? minLevel = null,
        byte? minSpicy = null,
        byte? minDry = null,
        byte? minSweet = null,
        byte? minBitter = null,
        byte? minSour = null)
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
    }
}
