using BDSP.Core.Berries;
using Xunit;

namespace BDSP.Core.Tests;

public sealed class BerryDataInvariantTests
{
    [Fact]
    public void AllBerries_HaveValidFlavorValues()
    {
        for (ushort i = 0; i < BerryTable.Count; i++)
        {
            ref readonly var berry = ref BerryTable.Get(new BerryId(i));

            Assert.True(berry.Spicy >= 0);
            Assert.True(berry.Dry >= 0);
            Assert.True(berry.Sweet >= 0);
            Assert.True(berry.Bitter >= 0);
            Assert.True(berry.Sour >= 0);
        }
    }

    [Fact]
    public void AllBerries_HaveValidSmoothness()
    {
        for (ushort i = 0; i < BerryTable.Count; i++)
        {
            ref readonly var berry = ref BerryTable.Get(new BerryId(i));

            Assert.InRange(berry.Smoothness, 20, 60);
        }
    }

    [Fact]
    public void BerryIdLookup_IsStable()
    {
        for (ushort i = 0; i < BerryTable.Count; i++)
        {
            ref readonly var berry = ref BerryTable.Get(new BerryId(i));
            Assert.Equal(i, berry.Id.Value);
        }
    }
}
