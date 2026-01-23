using System.Text;
using BDSP.Core.Berries.Data;

namespace BDSP.Core.Poffins;

public readonly struct PoffinRecipe
{
    public readonly Poffin Poffin;
    public readonly BerryId[] Berries;

    public PoffinRecipe(Poffin poffin, BerryId[] berries)
    {
        Poffin = poffin;
        Berries = berries;
    }

    public override string ToString()
    {
        var sb = new StringBuilder();
        sb.Append(Poffin);
        sb.Append(" [");
        for (int i = 0; i < Berries.Length; i++)
        {
            if (i > 0)
                sb.Append(", ");
            sb.Append(BerryNames.GetName(Berries[i]));
        }
        sb.Append(']');
        return sb.ToString();
    }
}
