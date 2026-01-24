namespace BDSP.Core.Poffins.Cooking
{
    /// <summary>
    /// Precomputed base totals for a unique berry combination.
    /// Intended for high-volume cooking where per-berry summation is avoided.
    /// </summary>
    public readonly struct PoffinComboBase
    {
        /// <summary>Total weakened spicy value for the combo.</summary>
        public readonly short WeakSpicySum;
        /// <summary>Total weakened dry value for the combo.</summary>
        public readonly short WeakDrySum;
        /// <summary>Total weakened sweet value for the combo.</summary>
        public readonly short WeakSweetSum;
        /// <summary>Total weakened bitter value for the combo.</summary>
        public readonly short WeakBitterSum;
        /// <summary>Total weakened sour value for the combo.</summary>
        public readonly short WeakSourSum;
        /// <summary>Sum of smoothness values for the combo.</summary>
        public readonly ushort SmoothnessSum;
        /// <summary>Number of berries in the combo (2–4).</summary>
        public readonly byte Count;

        public PoffinComboBase(
            short weakSpicySum,
            short weakDrySum,
            short weakSweetSum,
            short weakBitterSum,
            short weakSourSum,
            ushort smoothnessSum,
            byte count)
        {
            WeakSpicySum = weakSpicySum;
            WeakDrySum = weakDrySum;
            WeakSweetSum = weakSweetSum;
            WeakBitterSum = weakBitterSum;
            WeakSourSum = weakSourSum;
            SmoothnessSum = smoothnessSum;
            Count = count;
        }
    }
}
