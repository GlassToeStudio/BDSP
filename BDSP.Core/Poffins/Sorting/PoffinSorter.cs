using System;
using System.Collections.Generic;
using BDSP.Core.Optimization.Core;
using BDSP.Core.Poffins.Filters;

namespace BDSP.Core.Poffins
{
    /// <summary>
    /// Multi-key poffin sorting utility.
    /// </summary>
    public static class PoffinSorter
    {
        /// <summary>
        /// Sorts the first <paramref name="count"/> elements of <paramref name="buffer"/>
        /// using the provided sort keys.
        /// </summary>
        public static void Sort(Span<PoffinWithRecipe> buffer, int count, ReadOnlySpan<PoffinSortKey> keys)
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
            Array.Sort(array, new PoffinComparer(keys.ToArray()));
            array.CopyTo(buffer);
        }

        private sealed class PoffinComparer : IComparer<PoffinWithRecipe>
        {
            private readonly PoffinSortKey[] _keys;

            public PoffinComparer(PoffinSortKey[] keys)
            {
                _keys = keys;
            }

            public int Compare(PoffinWithRecipe x, PoffinWithRecipe y)
            {
                for (var i = 0; i < _keys.Length; i++)
                {
                    var key = _keys[i];
                    var result = CompareField(in x.Poffin, in y.Poffin, key.Field);
                    if (result == 0)
                    {
                        continue;
                    }

                    return key.Descending ? -result : result;
                }

                return 0;
            }

            private static int CompareField(in Poffin x, in Poffin y, PoffinSortField field)
            {
                return field switch
                {
                    PoffinSortField.Spicy => x.Spicy.CompareTo(y.Spicy),
                    PoffinSortField.Dry => x.Dry.CompareTo(y.Dry),
                    PoffinSortField.Sweet => x.Sweet.CompareTo(y.Sweet),
                    PoffinSortField.Bitter => x.Bitter.CompareTo(y.Bitter),
                    PoffinSortField.Sour => x.Sour.CompareTo(y.Sour),
                    PoffinSortField.TotalFlavor => GetTotalFlavor(in x).CompareTo(GetTotalFlavor(in y)),
                    PoffinSortField.Smoothness => x.Smoothness.CompareTo(y.Smoothness),
                    PoffinSortField.Level => x.Level.CompareTo(y.Level),
                    PoffinSortField.SecondLevel => x.SecondLevel.CompareTo(y.SecondLevel),
                    PoffinSortField.MainFlavor => x.MainFlavor.CompareTo(y.MainFlavor),
                    PoffinSortField.SecondaryFlavor => x.SecondaryFlavor.CompareTo(y.SecondaryFlavor),
                    PoffinSortField.NumFlavors => x.NumFlavors.CompareTo(y.NumFlavors),
                    PoffinSortField.NameKind => PoffinFilter.GetNameKind(in x).CompareTo(PoffinFilter.GetNameKind(in y)),
                    PoffinSortField.LevelToSmoothnessRatio => GetLevelRatio(in x).CompareTo(GetLevelRatio(in y)),
                    PoffinSortField.TotalFlavorToSmoothnessRatio => GetTotalFlavorRatio(in x).CompareTo(GetTotalFlavorRatio(in y)),
                    _ => 0
                };
            }

            [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
            private static int GetTotalFlavor(in Poffin poffin)
            {
                return poffin.Spicy + poffin.Dry + poffin.Sweet + poffin.Bitter + poffin.Sour;
            }

            [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
            private static int GetLevelRatio(in Poffin poffin)
            {
                int smooth = poffin.Smoothness == 0 ? 1 : poffin.Smoothness;
                return poffin.Level * 1000 / smooth;
            }

            [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
            private static int GetTotalFlavorRatio(in Poffin poffin)
            {
                int smooth = poffin.Smoothness == 0 ? 1 : poffin.Smoothness;
                int total = poffin.Spicy + poffin.Dry + poffin.Sweet + poffin.Bitter + poffin.Sour;
                return total * 1000 / smooth;
            }

            // Poffin name kind is shared with filters via PoffinFilter.GetNameKind.
        }
    }
}
