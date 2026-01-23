using System;

namespace BDSP.Core.Berries
{
    /// <summary>
    /// Filtering and sorting entry point for berries.
    /// </summary>
    public static class BerryQuery
    {
        /// <summary>
        /// Filters <paramref name="source"/> into <paramref name="destination"/> using <paramref name="options"/>,
        /// then sorts by <paramref name="sortKeys"/> if provided.
        /// Returns the number of matching berries written to <paramref name="destination"/>.
        /// <code>
        /// var options = new BerryFilterOptions(minRarity: 5, maxRarity: 9);
        /// Span&lt;Berry&gt; buffer = stackalloc Berry[BerryTable.Count];
        /// var count = BerryQuery.Execute(BerryTable.All, buffer, options, default);
        /// var results = buffer[..count];
        /// </code>
        /// </summary>
        public static int Execute(
            ReadOnlySpan<Berry> source,
            Span<Berry> destination,
            in BerryFilterOptions options,
            ReadOnlySpan<BerrySortKey> sortKeys)
        {
            var count = 0;
            for (var i = 0; i < source.Length; i++)
            {
                ref readonly var berry = ref source[i];
                if (!Matches(in berry, in options))
                {
                    continue;
                }

                destination[count] = berry;
                count++;
            }

            if (count > 1 && sortKeys.Length > 0)
            {
                BerrySorter.Sort(destination, count, sortKeys);
            }

            return count;
        }

        private static bool Matches(in Berry berry, in BerryFilterOptions o)
        {
            if (!InRange(berry.Spicy, o.MinSpicy, o.MaxSpicy)) return false;
            if (!InRange(berry.Dry, o.MinDry, o.MaxDry)) return false;
            if (!InRange(berry.Sweet, o.MinSweet, o.MaxSweet)) return false;
            if (!InRange(berry.Bitter, o.MinBitter, o.MaxBitter)) return false;
            if (!InRange(berry.Sour, o.MinSour, o.MaxSour)) return false;

            if (!InRange(berry.Smoothness, o.MinSmoothness, o.MaxSmoothness)) return false;
            if (!InRange(berry.Rarity, o.MinRarity, o.MaxRarity)) return false;
            if (!InRange(berry.MainFlavorValue, o.MinMainFlavorValue, o.MaxMainFlavorValue)) return false;
            if (!InRange(berry.SecondaryFlavorValue, o.MinSecondaryFlavorValue, o.MaxSecondaryFlavorValue)) return false;
            if (!InRange(berry.NumFlavors, o.MinNumFlavors, o.MaxNumFlavors)) return false;

            if (o.RequireMainFlavor && berry.MainFlavor != o.MainFlavor) return false;
            if (o.RequireSecondaryFlavor && berry.SecondaryFlavor != o.SecondaryFlavor) return false;

            if (o.RequiredFlavorMask != 0 || o.ExcludedFlavorMask != 0)
            {
                var mask = GetFlavorMask(in berry);
                if ((mask & o.RequiredFlavorMask) != o.RequiredFlavorMask) return false;
                if ((mask & o.ExcludedFlavorMask) != 0) return false;
            }

            return true;
        }

        private static bool InRange(byte value, int min, int max)
        {
            if (min != BerryFilterOptions.Unset && value < min) return false;
            if (max != BerryFilterOptions.Unset && value > max) return false;
            return true;
        }

        private static byte GetFlavorMask(in Berry berry)
        {
            byte mask = 0;
            if (berry.Spicy > 0) mask |= 1 << 0;
            if (berry.Dry > 0) mask |= 1 << 1;
            if (berry.Sweet > 0) mask |= 1 << 2;
            if (berry.Bitter > 0) mask |= 1 << 3;
            if (berry.Sour > 0) mask |= 1 << 4;
            return mask;
        }
    }
}
