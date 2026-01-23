using BDSP.Core.Berries.Data;

namespace BDSP.Core.Tests;

internal static class TestHelpers
{
    public static BerryId[] Ids(params ushort[] values)
    {
        var result = new BerryId[values.Length];
        for (int i = 0; i < values.Length; i++)
            result[i] = new BerryId(values[i]);
        return result;
    }
}

