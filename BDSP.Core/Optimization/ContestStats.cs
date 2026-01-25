namespace BDSP.Core.Optimization
{
    /// <summary>
    /// Represents contest condition stats plus sheen.
    /// Note: stat names follow flavor axes (Spicy, Dry, Sweet, Bitter, Sour).
    /// </summary>
    public readonly struct ContestStats
    {
        /// <summary>Spicy condition value (0-255).</summary>
        public readonly byte Spicy;
        /// <summary>Dry condition value (0-255).</summary>
        public readonly byte Dry;
        /// <summary>Sweet condition value (0-255).</summary>
        public readonly byte Sweet;
        /// <summary>Bitter condition value (0-255).</summary>
        public readonly byte Bitter;
        /// <summary>Sour condition value (0-255).</summary>
        public readonly byte Sour;
        /// <summary>Sheen value (0-255).</summary>
        public readonly byte Sheen;

        public ContestStats(
            byte spicy,
            byte dry,
            byte sweet,
            byte bitter,
            byte sour,
            byte sheen)
        {
            Spicy = spicy;
            Dry = dry;
            Sweet = sweet;
            Bitter = bitter;
            Sour = sour;
            Sheen = sheen;
        }
    }
}
