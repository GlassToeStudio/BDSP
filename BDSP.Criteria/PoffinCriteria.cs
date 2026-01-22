using BDSP.Core.Berries;
using BDSP.Core.Berries.Data;

namespace BDSP.Criteria;

public sealed record PoffinCriteria
{
    // ---- Berry selection ----
    public IReadOnlyList<BerryId>? AllowedBerries { get; init; }

    // ---- Search scope ----
    public int BerriesPerPoffin { get; init; } = 4;

    // ---- Filters ----
    public bool ExcludeFoul { get; init; } = true;

    public byte? MinLevel { get; init; }
    public byte? MaxSmoothness { get; init; }

    public byte? MinSpicy { get; init; }
    public byte? MinDry { get; init; }
    public byte? MinSweet { get; init; }
    public byte? MinBitter { get; init; }
    public byte? MinSour { get; init; }

    // ---- Ranking ----
    public SortField PrimarySort { get; init; } = SortField.Level;
    public SortDirection PrimaryDirection { get; init; } = SortDirection.Desc;

    public SortField? SecondarySort { get; init; }
    public SortDirection SecondaryDirection { get; init; } = SortDirection.Desc;

    // ---- Output ----
    public int TopK { get; init; } = 50;
}
