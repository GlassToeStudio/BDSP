namespace BDSP.Core.Berries
{
    /// <summary>
    /// Single sort key used for multi-key sorting.
    /// </summary>
    public readonly struct BerrySortKey
    {
        /// <summary>The field to sort by.</summary>
        public readonly BerrySortField Field;
        /// <summary>True to sort in descending order.</summary>
        public readonly bool Descending;

        public BerrySortKey(BerrySortField field, bool descending = false)
        {
            Field = field;
            Descending = descending;
        }
    }
}
