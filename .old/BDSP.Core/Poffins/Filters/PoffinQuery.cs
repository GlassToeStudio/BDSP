using BDSP.Core.Selection;

namespace BDSP.Core.Poffins.Filters;

public static class PoffinQuery
{
    public static int Filter(
        in PoffinFilterOptions filter,
        ReadOnlySpan<Poffin> source,
        Span<Poffin> destination)
    {
        int count = 0;
        for (int i = 0; i < source.Length; i++)
        {
            ref readonly var p = ref source[i];
            if (!PassesFilters(in filter, in p))
                continue;

            destination[count++] = p;
        }

        return count;
    }

    public static Poffin[] Filter(in PoffinFilterOptions filter, ReadOnlySpan<Poffin> source)
    {
        var tmp = new Poffin[source.Length];
        int count = Filter(in filter, source, tmp);
        if (count == tmp.Length)
            return tmp;

        return tmp.AsSpan(0, count).ToArray();
    }

    public static PoffinPredicate CompilePredicate(in PoffinFilterOptions filter)
    {
        var f = filter;
        return (in Poffin p) => PassesFilters(in f, in p);
    }

    private static bool PassesFilters(in PoffinFilterOptions f, in Poffin p)
    {
        if (f.ExcludeFoul && p.Type == PoffinType.Foul) return false;

        if (f.MinLevel >= 0 && p.Level < f.MinLevel) return false;
        if (p.Level > f.MaxLevel) return false;

        if (f.MinSmoothness >= 0 && p.Smoothness < f.MinSmoothness) return false;
        if (p.Smoothness > f.MaxSmoothness) return false;

        if (f.AllowedTypeMask != 0)
        {
            if (((f.AllowedTypeMask >> (int)p.Type) & 1) == 0)
                return false;
        }

        if (f.ExcludedTypeMask != 0)
        {
            if (((f.ExcludedTypeMask >> (int)p.Type) & 1) != 0)
                return false;
        }

        if (f.RequiredFlavorMask != 0)
        {
            if ((f.RequiredFlavorMask & (1 << 0)) != 0 && p.Spicy == 0) return false;
            if ((f.RequiredFlavorMask & (1 << 1)) != 0 && p.Dry == 0) return false;
            if ((f.RequiredFlavorMask & (1 << 2)) != 0 && p.Sweet == 0) return false;
            if ((f.RequiredFlavorMask & (1 << 3)) != 0 && p.Bitter == 0) return false;
            if ((f.RequiredFlavorMask & (1 << 4)) != 0 && p.Sour == 0) return false;
        }

        if (f.ExcludedFlavorMask != 0)
        {
            if ((f.ExcludedFlavorMask & (1 << 0)) != 0 && p.Spicy > 0) return false;
            if ((f.ExcludedFlavorMask & (1 << 1)) != 0 && p.Dry > 0) return false;
            if ((f.ExcludedFlavorMask & (1 << 2)) != 0 && p.Sweet > 0) return false;
            if ((f.ExcludedFlavorMask & (1 << 3)) != 0 && p.Bitter > 0) return false;
            if ((f.ExcludedFlavorMask & (1 << 4)) != 0 && p.Sour > 0) return false;
        }

        if (f.MinSpicy >= 0 && p.Spicy < f.MinSpicy) return false;
        if (p.Spicy > f.MaxSpicy) return false;
        if (f.MinDry >= 0 && p.Dry < f.MinDry) return false;
        if (p.Dry > f.MaxDry) return false;
        if (f.MinSweet >= 0 && p.Sweet < f.MinSweet) return false;
        if (p.Sweet > f.MaxSweet) return false;
        if (f.MinBitter >= 0 && p.Bitter < f.MinBitter) return false;
        if (p.Bitter > f.MaxBitter) return false;
        if (f.MinSour >= 0 && p.Sour < f.MinSour) return false;
        if (p.Sour > f.MaxSour) return false;

        if (f.MinNumFlavors >= 0 || f.MaxNumFlavors != int.MaxValue)
        {
            int count =
                (p.Spicy > 0 ? 1 : 0) +
                (p.Dry > 0 ? 1 : 0) +
                (p.Sweet > 0 ? 1 : 0) +
                (p.Bitter > 0 ? 1 : 0) +
                (p.Sour > 0 ? 1 : 0);

            if (f.MinNumFlavors >= 0 && count < f.MinNumFlavors) return false;
            if (count > f.MaxNumFlavors) return false;
        }

        return true;
    }
}
