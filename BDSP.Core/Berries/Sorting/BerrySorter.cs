using System;
using System.Collections.Generic;

namespace BDSP.Core.Berries
{
    /// <summary>
    /// Multi-key berry sorting utility.
    /// </summary>
    public static class BerrySorter
    {
        /// <summary>
        /// Sorts the first <paramref name="count"/> elements of <paramref name="buffer"/>
        /// using the provided sort keys.
        /// <code>
        /// var keys = new[] { new BerrySortKey(BerrySortField.Rarity), new BerrySortKey(BerrySortField.Name) };
        /// BerrySorter.Sort(buffer, count, keys);
        /// </code>
        /// </summary>
        public static void Sort(Span<Berry> buffer, int count, ReadOnlySpan<BerrySortKey> keys)
        {
            if (count <= 1 || keys.Length == 0)
            {
                return;
            }

            if (buffer.Length < count)
            {
                throw new ArgumentException("Buffer length is smaller than count.", nameof(buffer));
            }

            var array = buffer[..count].ToArray();
            Array.Sort(array, new BerryComparer(keys.ToArray()));
            array.CopyTo(buffer);
        }

        private sealed class BerryComparer : IComparer<Berry>
        {
            private readonly BerrySortKey[] _keys;

            public BerryComparer(BerrySortKey[] keys)
            {
                _keys = keys;
            }

            public int Compare(Berry x, Berry y)
            {
                for (var i = 0; i < _keys.Length; i++)
                {
                    var key = _keys[i];
                    var result = CompareField(x, y, key.Field);
                    if (result == 0)
                    {
                        continue;
                    }

                    return key.Descending ? -result : result;
                }

                return 0;
            }

            private static int CompareField(Berry x, Berry y, BerrySortField field)
            {
                return field switch
                {
                    BerrySortField.Id => x.Id.Value.CompareTo(y.Id.Value),
                    BerrySortField.Spicy => x.Spicy.CompareTo(y.Spicy),
                    BerrySortField.Dry => x.Dry.CompareTo(y.Dry),
                    BerrySortField.Sweet => x.Sweet.CompareTo(y.Sweet),
                    BerrySortField.Bitter => x.Bitter.CompareTo(y.Bitter),
                    BerrySortField.Sour => x.Sour.CompareTo(y.Sour),
                    BerrySortField.Smoothness => x.Smoothness.CompareTo(y.Smoothness),
                    BerrySortField.Rarity => x.Rarity.CompareTo(y.Rarity),
                    BerrySortField.MainFlavor => x.MainFlavor.CompareTo(y.MainFlavor),
                    BerrySortField.SecondaryFlavor => x.SecondaryFlavor.CompareTo(y.SecondaryFlavor),
                    BerrySortField.MainFlavorValue => x.MainFlavorValue.CompareTo(y.MainFlavorValue),
                    BerrySortField.SecondaryFlavorValue => x.SecondaryFlavorValue.CompareTo(y.SecondaryFlavorValue),
                    BerrySortField.NumFlavors => x.NumFlavors.CompareTo(y.NumFlavors),
                    BerrySortField.Name => StringComparer.Ordinal.Compare(
                        BerryNames.GetName(x.Id),
                        BerryNames.GetName(y.Id)),
                    _ => 0
                };
            }
        }
    }
}
