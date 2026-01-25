using BDSP.Core.Poffins;

namespace BDSP.Core.Optimization
{
    /// <summary>
    /// Applies a poffin to contest stats, enforcing stat/sheens caps.
    /// </summary>
    public static class FeedingApplier
    {
        /// <summary>
        /// Applies a single poffin and returns the updated stats.
        /// </summary>
        public static ContestStats Apply(in ContestStats current, in Poffin poffin)
        {
            int coolness = current.Coolness + poffin.Spicy;
            int beauty = current.Beauty + poffin.Dry;
            int cuteness = current.Cuteness + poffin.Sweet;
            int cleverness = current.Cleverness + poffin.Bitter;
            int toughness = current.Toughness + poffin.Sour;
            int sheen = current.Sheen + poffin.Smoothness;

            if (coolness > 255) coolness = 255;
            if (beauty > 255) beauty = 255;
            if (cuteness > 255) cuteness = 255;
            if (cleverness > 255) cleverness = 255;
            if (toughness > 255) toughness = 255;
            if (sheen > 255) sheen = 255;

            return new ContestStats(
                (byte)coolness,
                (byte)beauty,
                (byte)cuteness,
                (byte)cleverness,
                (byte)toughness,
                (byte)sheen);
        }
    }
}
