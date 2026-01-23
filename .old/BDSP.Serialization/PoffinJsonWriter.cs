using System.Text.Json;
using BDSP.Core.Poffins;

namespace BDSP.Serialization;

public static class PoffinJsonWriter
{
    public static string ToJson(ReadOnlySpan<Poffin> poffins)
    {
        var list = new List<PoffinDto>(poffins.Length);

        foreach (var p in poffins)
            list.Add(PoffinDto.From(in p));

        return JsonSerializer.Serialize(
            list,
            new JsonSerializerOptions { WriteIndented = true }
        );
    }
}
