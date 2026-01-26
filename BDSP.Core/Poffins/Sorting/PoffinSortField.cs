namespace BDSP.Core.Poffins
{
    /// <summary>
    /// Fields available for multi-key poffin sorting.
    /// </summary>
    public enum PoffinSortField : byte
    {
        /// <summary>Spicy flavor value.</summary>
        Spicy,
        /// <summary>Dry flavor value.</summary>
        Dry,
        /// <summary>Sweet flavor value.</summary>
        Sweet,
        /// <summary>Bitter flavor value.</summary>
        Bitter,
        /// <summary>Sour flavor value.</summary>
        Sour,
        /// <summary>Sum of all five flavor values.</summary>
        TotalFlavor,
        /// <summary>Smoothness value.</summary>
        Smoothness,
        /// <summary>Highest flavor value (level).</summary>
        Level,
        /// <summary>Second-highest flavor value (second level).</summary>
        SecondLevel,
        /// <summary>Main flavor (highest value, tie-break priority).</summary>
        MainFlavor,
        /// <summary>Secondary flavor (second-highest value).</summary>
        SecondaryFlavor,
        /// <summary>Number of non-zero flavors.</summary>
        NumFlavors,
        /// <summary>Poffin name category (foul/mild/rich/overripe/super mild).</summary>
        NameKind,
        /// <summary>Level-to-smoothness ratio (scaled integer).</summary>
        LevelToSmoothnessRatio,
        /// <summary>Total-flavor-to-smoothness ratio (scaled integer).</summary>
        TotalFlavorToSmoothnessRatio
    }
}
