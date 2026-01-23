using BDSP.Core.Poffins;
using BDSP.Core.Selection;

namespace BDSP.Criteria;

public sealed class DynamicPoffinComparer : IPoffinComparer
{
    private readonly SortField _primary;
    private readonly SortDirection _primaryDir;
    private readonly SortField? _secondary;
    private readonly SortDirection _secondaryDir;

    public DynamicPoffinComparer(
        SortField primary,
        SortDirection primaryDir,
        SortField? secondary,
        SortDirection secondaryDir)
    {
        _primary = primary;
        _primaryDir = primaryDir;
        _secondary = secondary;
        _secondaryDir = secondaryDir;
    }

    public bool IsBetter(in Poffin a, in Poffin b)
    {
        int p = CompareField(a, b, _primary);
        if (p != 0)
            return _primaryDir == SortDirection.Desc ? p > 0 : p < 0;

        if (_secondary.HasValue)
        {
            int s = CompareField(a, b, _secondary.Value);
            return _secondaryDir == SortDirection.Desc ? s > 0 : s < 0;
        }

        return false;
    }

    private static int CompareField(in Poffin a, in Poffin b, SortField f) => f switch
    {
        SortField.Level => a.Level.CompareTo(b.Level),
        SortField.Smoothness => b.Smoothness.CompareTo(a.Smoothness),
        SortField.Spicy => a.Spicy.CompareTo(b.Spicy),
        SortField.Dry => a.Dry.CompareTo(b.Dry),
        SortField.Sweet => a.Sweet.CompareTo(b.Sweet),
        SortField.Bitter => a.Bitter.CompareTo(b.Bitter),
        SortField.Sour => a.Sour.CompareTo(b.Sour),
        _ => 0
    };
}
