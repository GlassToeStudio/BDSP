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
            if (!InRange(berry.Spicy, o.MinSpicy, o.MaxSpicy, o.Mask, BerryFilterMask.MinSpicy, BerryFilterMask.MaxSpicy)) return false;
            if (!InRange(berry.Dry, o.MinDry, o.MaxDry, o.Mask, BerryFilterMask.MinDry, BerryFilterMask.MaxDry)) return false;
            if (!InRange(berry.Sweet, o.MinSweet, o.MaxSweet, o.Mask, BerryFilterMask.MinSweet, BerryFilterMask.MaxSweet)) return false;
            if (!InRange(berry.Bitter, o.MinBitter, o.MaxBitter, o.Mask, BerryFilterMask.MinBitter, BerryFilterMask.MaxBitter)) return false;
            if (!InRange(berry.Sour, o.MinSour, o.MaxSour, o.Mask, BerryFilterMask.MinSour, BerryFilterMask.MaxSour)) return false;

            if (!InRange(berry.Smoothness, o.MinSmoothness, o.MaxSmoothness, o.Mask, BerryFilterMask.MinSmoothness, BerryFilterMask.MaxSmoothness)) return false;
            if (!InRange(berry.Rarity, o.MinRarity, o.MaxRarity, o.Mask, BerryFilterMask.MinRarity, BerryFilterMask.MaxRarity)) return false;
            if (!InRange(berry.MainFlavorValue, o.MinMainFlavorValue, o.MaxMainFlavorValue, o.Mask, BerryFilterMask.MinMainFlavorValue, BerryFilterMask.MaxMainFlavorValue)) return false;
            if (!InRange(berry.SecondaryFlavorValue, o.MinSecondaryFlavorValue, o.MaxSecondaryFlavorValue, o.Mask, BerryFilterMask.MinSecondaryFlavorValue, BerryFilterMask.MaxSecondaryFlavorValue)) return false;
            if (!InRange(berry.NumFlavors, o.MinNumFlavors, o.MaxNumFlavors, o.Mask, BerryFilterMask.MinNumFlavors, BerryFilterMask.MaxNumFlavors)) return false;

            if (o.RequireMainFlavor && berry.MainFlavor != o.MainFlavor) return false;
            if (o.RequireSecondaryFlavor && berry.SecondaryFlavor != o.SecondaryFlavor) return false;
            if (o.RequireWeakenedMainFlavor && GetWeakenedMainFlavor(in berry) != o.WeakenedMainFlavor) return false;

            if ((o.Mask & (BerryFilterMask.RequiredFlavorMask | BerryFilterMask.ExcludedFlavorMask)) != 0)
            {
                var mask = GetFlavorMask(in berry);
                if ((mask & o.RequiredFlavorMask) != o.RequiredFlavorMask) return false;
                if ((mask & o.ExcludedFlavorMask) != 0) return false;
            }

            if ((o.Mask & (BerryFilterMask.MinAnyFlavorValue | BerryFilterMask.MaxAnyFlavorValue)) != 0 &&
                !AnyFlavorInRange(in berry, in o)) return false;
            if ((o.Mask & (BerryFilterMask.MinWeakMainFlavorValue | BerryFilterMask.MaxWeakMainFlavorValue)) != 0 &&
                !WeakenedMainFlavorInRange(in berry, in o)) return false;

            if ((o.Mask & BerryFilterMask.IdEquals) != 0 && ComputeId(in berry) != o.IdEquals) return false;
            if ((o.Mask & BerryFilterMask.IdNotEquals) != 0 && ComputeId(in berry) == o.IdNotEquals) return false;

            return true;
        }

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        private static bool InRange(
            byte value,
            int min,
            int max,
            BerryFilterMask mask,
            BerryFilterMask minFlag,
            BerryFilterMask maxFlag)
        {
            if ((mask & minFlag) != 0 && value < min) return false;
            if ((mask & maxFlag) != 0 && value > max) return false;
            return true;
        }

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
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

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        private static bool AnyFlavorInRange(in Berry berry, in BerryFilterOptions o)
        {
            if ((o.Mask & BerryFilterMask.MinAnyFlavorValue) != 0)
            {
                int min = o.MinAnyFlavorValue;
                if (berry.Spicy > 0 && berry.Spicy < min) return false;
                if (berry.Dry > 0 && berry.Dry < min) return false;
                if (berry.Sweet > 0 && berry.Sweet < min) return false;
                if (berry.Bitter > 0 && berry.Bitter < min) return false;
                if (berry.Sour > 0 && berry.Sour < min) return false;
            }

            if ((o.Mask & BerryFilterMask.MaxAnyFlavorValue) != 0)
            {
                int max = o.MaxAnyFlavorValue;
                if (berry.Spicy > max) return false;
                if (berry.Dry > max) return false;
                if (berry.Sweet > max) return false;
                if (berry.Bitter > max) return false;
                if (berry.Sour > max) return false;
            }

            return true;
        }

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        private static bool WeakenedMainFlavorInRange(in Berry berry, in BerryFilterOptions o)
        {
            if ((o.Mask & (BerryFilterMask.MinWeakMainFlavorValue | BerryFilterMask.MaxWeakMainFlavorValue)) == 0)
            {
                return true;
            }

            BerryBase baseBerry = BerryTable.GetBase(berry.Id);
            int weakenedMain = baseBerry.GetWeakenedFlavor(berry.MainFlavor);
            if ((o.Mask & BerryFilterMask.MinWeakMainFlavorValue) != 0 && weakenedMain < o.MinWeakMainFlavorValue) return false;
            if ((o.Mask & BerryFilterMask.MaxWeakMainFlavorValue) != 0 && weakenedMain > o.MaxWeakMainFlavorValue) return false;
            return true;
        }

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        private static Flavor GetWeakenedMainFlavor(in Berry berry)
        {
            BerryBase baseBerry = BerryTable.GetBase(berry.Id);
            sbyte spicy = baseBerry.WeakSpicy;
            sbyte dry = baseBerry.WeakDry;
            sbyte sweet = baseBerry.WeakSweet;
            sbyte bitter = baseBerry.WeakBitter;
            sbyte sour = baseBerry.WeakSour;

            Flavor bestFlavor = Flavor.Spicy;
            sbyte bestValue = spicy;
            ConsiderWeakened(Flavor.Dry, dry, ref bestFlavor, ref bestValue);
            ConsiderWeakened(Flavor.Sweet, sweet, ref bestFlavor, ref bestValue);
            ConsiderWeakened(Flavor.Bitter, bitter, ref bestFlavor, ref bestValue);
            ConsiderWeakened(Flavor.Sour, sour, ref bestFlavor, ref bestValue);
            return bestFlavor;
        }

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        private static void ConsiderWeakened(Flavor flavor, sbyte value, ref Flavor bestFlavor, ref sbyte bestValue)
        {
            if (value > bestValue)
            {
                bestFlavor = flavor;
                bestValue = value;
            }
        }

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        private static bool HasHigherPriority(Flavor candidate, Flavor current)
        {
            return GetPriority(candidate) > GetPriority(current);
        }

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        private static int GetPriority(Flavor flavor)
        {
            return flavor switch
            {
                Flavor.Spicy => 5,
                Flavor.Dry => 4,
                Flavor.Sweet => 3,
                Flavor.Bitter => 2,
                Flavor.Sour => 1,
                _ => 0
            };
        }

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        private static int ComputeId(in Berry berry)
        {
            int id = 0;
            AppendDigits(ref id, berry.Spicy);
            AppendDigits(ref id, berry.Dry);
            AppendDigits(ref id, berry.Sweet);
            AppendDigits(ref id, berry.Bitter);
            AppendDigits(ref id, berry.Sour);
            return id;
        }

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        private static void AppendDigits(ref int id, int value)
        {
            if (value >= 100)
            {
                id = id * 1000 + value;
                return;
            }

            if (value >= 10)
            {
                id = id * 100 + value;
                return;
            }

            id = id * 10 + value;
        }
    }
}
