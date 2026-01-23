namespace BDSP.Criteria;

public static class PoffinPresets
{
    public static PoffinCriteria Cool => new()
    {
        ExcludeFoul = true,

        MinSpicy = 40,              // sensible default, not a hard rule
        MaxSmoothness = 25,

        PrimarySort = SortField.Spicy,
        PrimaryDirection = SortDirection.Desc,

        SecondarySort = SortField.Smoothness,
        SecondaryDirection = SortDirection.Asc,
    };

    public static PoffinCriteria Beauty => new()
    {
        ExcludeFoul = true,

        MinSweet = 40,
        MaxSmoothness = 25,

        PrimarySort = SortField.Sweet,
        PrimaryDirection = SortDirection.Desc,

        SecondarySort = SortField.Smoothness,
        SecondaryDirection = SortDirection.Asc,
    };

    public static PoffinCriteria Cute => new()
    {
        ExcludeFoul = true,

        MinDry = 40,
        MaxSmoothness = 25,

        PrimarySort = SortField.Dry,
        PrimaryDirection = SortDirection.Desc,

        SecondarySort = SortField.Smoothness,
        SecondaryDirection = SortDirection.Asc,
    };

    public static PoffinCriteria Smart => new()
    {
        ExcludeFoul = true,

        MinBitter = 40,
        MaxSmoothness = 25,

        PrimarySort = SortField.Bitter,
        PrimaryDirection = SortDirection.Desc,

        SecondarySort = SortField.Smoothness,
        SecondaryDirection = SortDirection.Asc,
    };

    public static PoffinCriteria Tough => new()
    {
        ExcludeFoul = true,

        MinSour = 40,
        MaxSmoothness = 25,

        PrimarySort = SortField.Sour,
        PrimaryDirection = SortDirection.Desc,

        SecondarySort = SortField.Smoothness,
        SecondaryDirection = SortDirection.Asc,
    };
}
