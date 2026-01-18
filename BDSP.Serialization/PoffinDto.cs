using BDSP.Core.Poffins;

namespace BDSP.Serialization;

public sealed class PoffinDto
{
    public byte Level { get; init; }
    public byte Smoothness { get; init; }
    public PoffinType Type { get; init; }
    public byte Spicy { get; init; }
    public byte Dry { get; init; }
    public byte Sweet { get; init; }
    public byte Bitter { get; init; }
    public byte Sour { get; init; }

    public static PoffinDto From(in Poffin p) => new()
    {
        Level = p.Level,
        Smoothness = p.Smoothness,
        Type = p.Type,
        Spicy = p.Spicy,
        Dry = p.Dry,
        Sweet = p.Sweet,
        Bitter = p.Bitter,
        Sour = p.Sour
    };
}
