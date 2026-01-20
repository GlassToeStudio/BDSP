using BDSP.Core.Berries;
using BDSP.UI.Models;

namespace BDSP.UI.Mapping;

public static class BerryRadarMapping
{
    private static readonly string[] _labels =
    {
        "Spicy", "Dry", "Sweet", "Bitter", "Sour"
    };

    public static RadarData FromBerry(in Berry berry)
        => new RadarData(
            new double[]
            {
                berry.Spicy,
                berry.Dry,
                berry.Sweet,
                berry.Bitter,
                berry.Sour
            },
            _labels,
            maxValue: 40  // <-- IMPORTANT
        );
}
