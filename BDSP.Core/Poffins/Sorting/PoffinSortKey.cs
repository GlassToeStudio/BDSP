namespace BDSP.Core.Poffins
{
    /// <summary>
    /// Sort key for <see cref="PoffinSorter"/>.
    /// </summary>
    public readonly struct PoffinSortKey
    {
        /// <summary>Field to sort by.</summary>
        public readonly PoffinSortField Field;
        /// <summary>True to sort descending.</summary>
        public readonly bool Descending;

        public PoffinSortKey(PoffinSortField field, bool descending = false)
        {
            Field = field;
            Descending = descending;
        }
    }
}
