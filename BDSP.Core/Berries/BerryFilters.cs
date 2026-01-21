using BDSP.Core.Primitives;

namespace BDSP.Core.Berries;

public static class BerryFilters
{
    public static BerryFilterOptions Default => BerryFilterOptions.Default;

    public static BerryFilterOptions CommonOnly()
        => new BerryFilterOptions(maxRarity: 3);

    public static BerryFilterOptions LowSmoothness(byte maxSmoothness)
        => new BerryFilterOptions(maxSmoothness: maxSmoothness);

    public static BerryFilterOptions RequireFlavor(Flavor flavor)
        => new BerryFilterOptions(hasRequiredFlavor: true, requiredFlavor: (byte)flavor);

    public static BerryFilterOptions StrongMainFlavor(int minMainFlavorValue)
        => new BerryFilterOptions(minMainFlavorValue: minMainFlavorValue);

    public static BerryFilterOptions Tight(
        int maxSmoothness,
        int maxRarity,
        int minMainFlavorValue)
        => new BerryFilterOptions(
            maxSmoothness: maxSmoothness,
            maxRarity: maxRarity,
            minMainFlavorValue: minMainFlavorValue);
}
