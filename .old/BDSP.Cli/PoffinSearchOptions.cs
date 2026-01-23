namespace BDSP.Cli;

internal sealed record PoffinSearchOptions
{
    public int BerriesPerPoffin { get; init; } = 4;
    public int TopK { get; init; } = 50;
    public byte CookTimeSeconds { get; init; } = 40;
    public byte Errors { get; init; } = 0;
    public byte AmityBonus { get; init; } = 9;
    public bool ExcludeFoul { get; init; } = true;
}
