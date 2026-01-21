using BDSP.Core.Primitives;

namespace BDSP.Core.Berries;

public static class BerryFilters
{
    /// <summary>
    /// No filtering.
    /// </summary>
    public static BerryFilterOptions Default => BerryFilterOptions.Default;

    /// <summary>
    /// Limits to common berries (rarity <= 3).
    /// </summary>
    public static BerryFilterOptions CommonOnly()
        => new BerryFilterOptions(maxRarity: 3);

    /// <summary>
    /// Limits to rarer berries (rarity >= 4).
    /// </summary>
    public static BerryFilterOptions RareOnly()
        => new BerryFilterOptions(minRarity: 4);

    /// <summary>
    /// Limits to berries at or below the given smoothness.
    /// </summary>
    public static BerryFilterOptions LowSmoothness(byte maxSmoothness)
        => new BerryFilterOptions(maxSmoothness: maxSmoothness);

    /// <summary>
    /// Limits to very low smoothness (<= 25).
    /// </summary>
    public static BerryFilterOptions VeryLowSmoothness()
        => new BerryFilterOptions(maxSmoothness: 25);

    /// <summary>
    /// Requires a specific flavor to be present (> 0).
    /// </summary>
    public static BerryFilterOptions RequireFlavor(Flavor flavor)
        => new BerryFilterOptions(hasRequiredFlavor: true, requiredFlavor: (byte)flavor);

    /// <summary>
    /// Requires a minimum main flavor value.
    /// </summary>
    public static BerryFilterOptions StrongMainFlavor(int minMainFlavorValue)
        => new BerryFilterOptions(minMainFlavorValue: minMainFlavorValue);

    /// <summary>
    /// Limits main flavor value to a range.
    /// </summary>
    public static BerryFilterOptions MainFlavorRange(int minMainFlavorValue, int maxMainFlavorValue)
        => new BerryFilterOptions(
            minMainFlavorValue: minMainFlavorValue,
            maxMainFlavorValue: maxMainFlavorValue);

    /// <summary>
    /// Limits the number of non-zero flavors to a range.
    /// </summary>
    public static BerryFilterOptions NumFlavorsRange(int minNumFlavors, int maxNumFlavors)
        => new BerryFilterOptions(
            minNumFlavors: minNumFlavors,
            maxNumFlavors: maxNumFlavors);

    /// <summary>
    /// Requires all non-zero flavors to be at least the given value.
    /// </summary>
    public static BerryFilterOptions AnyNonZeroFlavorMin(int minAnyNonZeroFlavorValue)
        => new BerryFilterOptions(minAnyNonZeroFlavorValue: minAnyNonZeroFlavorValue);

    /// <summary>
    /// Requires all non-zero flavors to be at most the given value.
    /// </summary>
    public static BerryFilterOptions AnyNonZeroFlavorMax(int maxAnyNonZeroFlavorValue)
        => new BerryFilterOptions(maxAnyNonZeroFlavorValue: maxAnyNonZeroFlavorValue);

    /// <summary>
    /// Limits weakened main flavor value to a range (after BDSP weakening cycle).
    /// </summary>
    public static BerryFilterOptions WeakenedMainFlavorRange(
        int minWeakenedMainFlavorValue,
        int maxWeakenedMainFlavorValue)
        => new BerryFilterOptions(
            minWeakenedMainFlavorValue: minWeakenedMainFlavorValue,
            maxWeakenedMainFlavorValue: maxWeakenedMainFlavorValue);

    /// <summary>
    /// Compact filter for common, low-smoothness berries with strong main flavor.
    /// </summary>
    public static BerryFilterOptions Tight(
        int maxSmoothness,
        int maxRarity,
        int minMainFlavorValue)
        => new BerryFilterOptions(
            maxSmoothness: maxSmoothness,
            maxRarity: maxRarity,
            minMainFlavorValue: minMainFlavorValue);
}
