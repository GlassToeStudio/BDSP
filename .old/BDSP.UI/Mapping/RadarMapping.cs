using BDSP.Core.Poffins;
using BDSP.UI.Models;
using LiveChartsCore.SkiaSharpView.Avalonia;

namespace BDSP.UI.Mapping;

public static class RadarMapping
{
        /* Coolness -> Spicy
        * Beauty -> Dry
        * Cuteness -> Sweet
        * Cleverness -> Bitter
        * Tougnness -> Sour
        */

    public static ContestRadarData FromPoffin(in Poffin poffin)
    {
        return new ContestRadarData(
            new double[]
            {
                poffin.Spicy,
                poffin.Dry,
                poffin.Sweet,
                poffin.Bitter,
                poffin.Sour
            },
            new[] { "Coolness", "Beauty", "Cuteness", "Cleverness", "Toghness" },
            maxValue: 100);
    }

}
