using BDSP.Core.Poffins;
using BDSP.Core.Selection;

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
}
