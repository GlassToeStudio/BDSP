using System;
using System.Text;
using BDSP.Core.Berries.Data;

namespace BDSP.Core.Poffins;

public readonly struct PoffinRecipe
{
    public readonly Poffin Poffin;
    private readonly BerryId[] _berries;

    public ReadOnlyMemory<BerryId> Berries => _berries;

    public PoffinRecipe(Poffin poffin, BerryId[] berries)
    {
        Poffin = poffin;
        _berries = berries.ToArray();
    }

    public override string ToString()
    {
        var sb = new StringBuilder();
        sb.Append(Poffin);
        sb.Append(" [");
        var span = _berries.AsSpan();
        for (int i = 0; i < span.Length; i++)
        {
            if (i > 0)
                sb.Append(", ");
            sb.Append(BerryNames.GetName(span[i]));
        }
        sb.Append(']');
        return sb.ToString();
    }
}
