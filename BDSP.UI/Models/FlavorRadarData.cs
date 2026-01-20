using BDSP.Core.Berries;

namespace BDSP.UI.Models
{
    public sealed class FlavorRadarData
    {
        public double Spicy { get; init; }
        public double Dry { get; init; }
        public double Sweet { get; init; }
        public double Bitter { get; init; }
        public double Sour { get; init; }

        public static FlavorRadarData FromBerry(in Berry b) => new()
        {
            Spicy = b.Spicy,
            Dry = b.Dry,
            Sweet = b.Sweet,
            Bitter = b.Bitter,
            Sour = b.Sour
        };
    }
}