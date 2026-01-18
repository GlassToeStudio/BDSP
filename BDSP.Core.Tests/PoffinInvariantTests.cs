using BDSP.Core.Berries;
using BDSP.Core.Poffins;
using Xunit;

namespace BDSP.Core.Tests;

public sealed class PoffinInvariantTests
{
    [Fact]
    public void CookedPoffins_NeverHaveNegativeFlavors()
    {
        for (int i = 0; i < BerryTable.Count; i++)
        {
            var ids = TestHelpers.Ids((ushort)i);

            var p = PoffinCooker.Cook(
                ids,
                cookTimeSeconds: 40,
                errors: 0,
                amityBonus: 0);

            Assert.True(p.Spicy >= 0);
            Assert.True(p.Dry >= 0);
            Assert.True(p.Sweet >= 0);
            Assert.True(p.Bitter >= 0);
            Assert.True(p.Sour >= 0);
        }
    }

    [Fact]
    public void FoulPoffins_HaveExactlyThreeFlavorsOfTwo()
    {
        var ids = TestHelpers.Ids(0, 0); // duplicate → Foul

        var p = PoffinCooker.Cook(
            ids,
            cookTimeSeconds: 40,
            errors: 0,
            amityBonus: 0);

        Assert.Equal(PoffinType.Foul, p.Type);

        int count =
            (p.Spicy == 2 ? 1 : 0) +
            (p.Dry == 2 ? 1 : 0) +
            (p.Sweet == 2 ? 1 : 0) +
            (p.Bitter == 2 ? 1 : 0) +
            (p.Sour == 2 ? 1 : 0);

        Assert.Equal(3, count);
    }

    [Fact]
    public void NonFoulPoffins_HaveAtLeastOnePositiveFlavor()
    {
        for (int i = 0; i < BerryTable.Count; i++)
        {
            var ids = TestHelpers.Ids((ushort)i);

            var p = PoffinCooker.Cook(
                ids,
                cookTimeSeconds: 40,
                errors: 0,
                amityBonus: 0);

            if (p.Type == PoffinType.Foul)
                continue;

            Assert.True(
                p.Spicy > 0 ||
                p.Dry > 0 ||
                p.Sweet > 0 ||
                p.Bitter > 0 ||
                p.Sour > 0);
        }
    }
}
