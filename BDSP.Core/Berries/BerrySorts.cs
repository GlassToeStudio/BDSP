namespace BDSP.Core.Berries;

public static class BerrySorts
{
    // Caller supplies a buffer (stackalloc recommended). We return the number of specs used.

    public static int RarityThenName(Span<BerrySortSpec> dst)
    {
        dst[0] = new BerrySortSpec(BerrySortField.Rarity, SortDirection.Asc);
        dst[1] = new BerrySortSpec(BerrySortField.Name, SortDirection.Asc);
        return 2;
    }

    public static int SmoothnessThenRarity(Span<BerrySortSpec> dst)
    {
        dst[0] = new BerrySortSpec(BerrySortField.Smoothness, SortDirection.Asc);
        dst[1] = new BerrySortSpec(BerrySortField.Rarity, SortDirection.Asc);
        return 2;
    }

    public static int MainFlavorValueThenSmoothness(Span<BerrySortSpec> dst)
    {
        dst[0] = new BerrySortSpec(BerrySortField.MainFlavorValue, SortDirection.Desc);
        dst[1] = new BerrySortSpec(BerrySortField.Smoothness, SortDirection.Asc);
        return 2;
    }

    public static int BestMainFlavorRatio(Span<BerrySortSpec> dst)
    {
        dst[0] = new BerrySortSpec(BerrySortField.MainFlavorToSmoothnessRatio, SortDirection.Desc);
        dst[1] = new BerrySortSpec(BerrySortField.Smoothness, SortDirection.Asc);
        return 2;
    }

    public static int IdAsc(Span<BerrySortSpec> dst)
    {
        dst[0] = new BerrySortSpec(BerrySortField.Id, SortDirection.Asc);
        return 1;
    }
}

