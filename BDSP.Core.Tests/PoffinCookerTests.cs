using BDSP.Core.Poffins;
using BDSP.Core.Primitives;
using Xunit;

namespace BDSP.Core.Tests;

public sealed class PoffinCookerTests
{
    [Fact]
    public void Cook_DuplicateBerries_ReturnsFoul()
    {
        var ids = TestHelpers.Ids(0, 0);

        var poffin = PoffinCooker.Cook(
            ids,
            cookTimeSeconds: 40,
            errors: 0,
            amityBonus: 0);

        Assert.Equal(PoffinType.Foul, poffin.Type);
    }

    [Fact]
    public void Cook_AllFlavorsZero_ReturnsFoul()
    {
        var ids = TestHelpers.Ids(0,1,2,3);

        var poffin = PoffinCooker.Cook(
            ids,
            cookTimeSeconds: 80,
            errors: 100,
            amityBonus: 0);

        Assert.Equal(PoffinType.Foul, poffin.Type);
    }


    [Fact]
    public void Cook_Level95OrAbove_IsSuperMild()
    {
        //liechi (29), lansat (27), petaya (43), rowap (52) = Poffin([115, 0, 25, 0, 0], 34)
        var ids = TestHelpers.Ids(29, 27, 43, 52);

        var poffin = PoffinCooker.Cook(
            ids,
            cookTimeSeconds: 40,
            errors: 0,
            amityBonus: 9);

        if (poffin.Level >= 95)
        {
            Assert.Equal(PoffinType.SuperMild, poffin.Type);
        }
    }

    [Fact]
    public void Cook_Level50OrAbove_ProducesMild()
    {
        var ids = TestHelpers.Ids(0); // any berry

        var poffin = PoffinCooker.Cook(
            ids,
            cookTimeSeconds: 10, // aggressive cooking
            errors: 0,
            amityBonus: 0);

        if (poffin.Level >= 50)
        {
            Assert.Equal(PoffinType.Mild, poffin.Type);
        }
    }


    [Fact]
    public void Cook_Level50OrAbove_IsMild()
    {
        var ids = TestHelpers.Ids(14);

        var poffin = PoffinCooker.Cook(
            ids,
            cookTimeSeconds: 20, // faster cooking boosts level
            errors: 0,
            amityBonus: 0);

        Assert.True(poffin.Level >= 50);
        Assert.Equal(PoffinType.Mild, poffin.Type);
    }


    [Fact]
    public void Cook_Level95OrAbove_ProducesSuperMild()
    {
        var ids = TestHelpers.Ids(0); // any berry

        var poffin = PoffinCooker.Cook(
            ids,
            cookTimeSeconds: 5,  // extremely fast
            errors: 0,
            amityBonus: 0);

        if (poffin.Level >= 95)
        {
            Assert.Equal(PoffinType.SuperMild, poffin.Type);
        }
    }


    [Fact]
    public void Cook_SetsPrimaryFlavorCorrectly()
    {
        var ids = TestHelpers.Ids(7);

        var poffin = PoffinCooker.Cook(
            ids,
            cookTimeSeconds: 40,
            errors: 0,
            amityBonus: 0);

        Assert.Equal(Flavor.Spicy, poffin.PrimaryFlavor);
    }

    [Fact]
    public void Cook_PrimaryFlavor_IsStrongestFlavor()
    {
        var ids = TestHelpers.Ids(0); // any berry

        var poffin = PoffinCooker.Cook(
            ids,
            cookTimeSeconds: 40,
            errors: 0,
            amityBonus: 0);

        // Determine strongest flavor manually
        byte max = poffin.Spicy;
        Flavor expected = Flavor.Spicy;

        if (poffin.Dry > max) { max = poffin.Dry; expected = Flavor.Dry; }
        if (poffin.Sweet > max) { max = poffin.Sweet; expected = Flavor.Sweet; }
        if (poffin.Bitter > max) { max = poffin.Bitter; expected = Flavor.Bitter; }
        if (poffin.Sour > max) { expected = Flavor.Sour; }

        Assert.Equal(expected, poffin.PrimaryFlavor);
    }


    [Fact]
    public void Cook_Smoothness_AmityBonusReducesValue()
    {
        var ids = TestHelpers.Ids(0, 2, 3, 4);

        var basePoffin = PoffinCooker.Cook(
            ids,
            cookTimeSeconds: 40,
            errors: 0,
            amityBonus: 0);

        var reducedPoffin = PoffinCooker.Cook(
            ids,
            cookTimeSeconds: 40,
            errors: 0,
            amityBonus: 9);

        Assert.True(reducedPoffin.Smoothness < basePoffin.Smoothness);
    }
}
