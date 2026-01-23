using BDSP.Core.Poffins;
using BDSP.Core.Selection;

using System.Collections.Generic;
using BDSP.Core.Berries;
using BDSP.Core.Berries.Data;
using BDSP.Core.Berries.Filters;

namespace BDSP.Criteria;

public static class PoffinCriteriaCompiler
{
    public static PoffinPredicate CompilePredicate(PoffinCriteria c)
    {
        return (in Poffin p) =>
        {
            if (c.ExcludeFoul && p.Type == PoffinType.Foul)
                return false;

            if (c.MinLevel.HasValue && p.Level < c.MinLevel.Value)
                return false;

            if (c.MaxSmoothness.HasValue && p.Smoothness > c.MaxSmoothness.Value)
                return false;

            if (c.MinSpicy.HasValue && p.Spicy < c.MinSpicy.Value)
                return false;

            if (c.MinDry.HasValue && p.Dry < c.MinDry.Value)
                return false;

            if (c.MinSweet.HasValue && p.Sweet < c.MinSweet.Value)
                return false;

            if (c.MinBitter.HasValue && p.Bitter < c.MinBitter.Value)
                return false;

            if (c.MinSour.HasValue && p.Sour < c.MinSour.Value)
                return false;

            return true;
        };
    }
    public static IPoffinComparer CompileComparer(PoffinCriteria c)
    {
        return new DynamicPoffinComparer(
            c.PrimarySort,
            c.PrimaryDirection,
            c.SecondarySort,
            c.SecondaryDirection);
    }

    public static BDSP.Core.Runner.PoffinSearchPruning CompilePruning(PoffinCriteria c)
    {
        return new BDSP.Core.Runner.PoffinSearchPruning(
            minLevel: c.MinLevel,
            minSpicy: c.MinSpicy,
            minDry: c.MinDry,
            minSweet: c.MinSweet,
            minBitter: c.MinBitter,
            minSour: c.MinSour,
            maxSmoothness: c.MaxSmoothness);
    }

    public static BerryId[] CompileBerryPool(PoffinCriteria c)
    {
        bool hasAllowed = c.AllowedBerries is { Count: > 0 };
        bool hasRarity = c.MinBerryRarity.HasValue || c.MaxBerryRarity.HasValue;

        if (!hasAllowed && !hasRarity)
        {
            var pool = new BerryId[BerryTable.Count];
            for (ushort i = 0; i < BerryTable.Count; i++)
                pool[i] = new BerryId(i);
            return pool;
        }

        ulong lo = 0, hi = 0;
        if (hasAllowed)
        {
            foreach (var id in c.AllowedBerries!)
            {
                var v = id.Value;
                if (v < 64) lo |= 1UL << (int)v;
                else hi |= 1UL << (int)(v - 64);
            }
        }

        int minRarity = c.MinBerryRarity ?? -1;
        int maxRarity = c.MaxBerryRarity ?? int.MaxValue;
        var filter = new BerryFilterOptions(
            allowedMaskLo: lo,
            allowedMaskHi: hi,
            minRarity: minRarity,
            maxRarity: maxRarity);

        return BerryQuery.Filter(in filter);
    }
}
