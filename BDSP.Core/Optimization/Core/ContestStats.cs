namespace BDSP.Core.Optimization.Core
{
    /// <summary>
    /// Represents contest condition stats plus sheen.
    /// </summary>
    /// <remarks>
    /// Flavor mapping:
    /// Spicy -> Coolness, Dry -> Beauty, Sweet -> Cuteness, Bitter -> Cleverness, Sour -> Toughness.
    /// </remarks>
    public readonly struct ContestStats
    {
        /// <summary>Coolness condition value (Spicy, 0-255).</summary>
        public readonly byte Coolness;
        /// <summary>Beauty condition value (Dry, 0-255).</summary>
        public readonly byte Beauty;
        /// <summary>Cuteness condition value (Sweet, 0-255).</summary>
        public readonly byte Cuteness;
        /// <summary>Cleverness condition value (Bitter, 0-255).</summary>
        public readonly byte Cleverness;
        /// <summary>Toughness condition value (Sour, 0-255).</summary>
        public readonly byte Toughness;
        /// <summary>Sheen value (0-255).</summary>
        public readonly byte Sheen;

        public ContestStats(
            byte coolness,
            byte beauty,
            byte cuteness,
            byte cleverness,
            byte toughness,
            byte sheen)
        {
            Coolness = coolness;
            Beauty = beauty;
            Cuteness = cuteness;
            Cleverness = cleverness;
            Toughness = toughness;
            Sheen = sheen;
        }
    }
}
