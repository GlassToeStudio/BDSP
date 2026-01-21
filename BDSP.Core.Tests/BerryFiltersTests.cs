using BDSP.Core.Berries;
using BDSP.Core.Primitives;

namespace BDSP.Core.Tests;

public sealed class BerryFiltersTests
{
    [Fact]
    public void Default_UsesDefaults()
    {
        var f = BerryFilters.Default;
        AssertDefaults(f);
    }

    [Fact]
    public void CommonOnly_SetsMaxRarity()
    {
        var f = BerryFilters.CommonOnly();
        AssertDefaults(f, rarity: false);
        Assert.Equal(3, f.MaxRarity);
    }

    [Fact]
    public void RareOnly_SetsMinRarity()
    {
        var f = BerryFilters.RareOnly();
        AssertDefaults(f, rarity: false);
        Assert.Equal(4, f.MinRarity);
    }

    [Fact]
    public void LowSmoothness_SetsMaxSmoothness()
    {
        var f = BerryFilters.LowSmoothness(30);
        AssertDefaults(f, smoothness: false);
        Assert.Equal(30, f.MaxSmoothness);
    }

    [Fact]
    public void VeryLowSmoothness_SetsMaxSmoothness()
    {
        var f = BerryFilters.VeryLowSmoothness();
        AssertDefaults(f, smoothness: false);
        Assert.Equal(25, f.MaxSmoothness);
    }

    [Fact]
    public void RequireFlavor_SetsRequiredFlavor()
    {
        var f = BerryFilters.RequireFlavor(Flavor.Sweet);
        AssertDefaults(f, requiredFlavor: false);
        Assert.True(f.HasRequiredFlavor);
        Assert.Equal((byte)Flavor.Sweet, f.RequiredFlavor);
    }

    [Fact]
    public void StrongMainFlavor_SetsMinMainFlavorValue()
    {
        var f = BerryFilters.StrongMainFlavor(12);
        AssertDefaults(f, mainFlavor: false);
        Assert.Equal(12, f.MinMainFlavorValue);
    }

    [Fact]
    public void MainFlavorRange_SetsMinAndMax()
    {
        var f = BerryFilters.MainFlavorRange(10, 25);
        AssertDefaults(f, mainFlavor: false);
        Assert.Equal(10, f.MinMainFlavorValue);
        Assert.Equal(25, f.MaxMainFlavorValue);
    }

    [Fact]
    public void NumFlavorsRange_SetsMinAndMax()
    {
        var f = BerryFilters.NumFlavorsRange(2, 4);
        AssertDefaults(f, numFlavors: false);
        Assert.Equal(2, f.MinNumFlavors);
        Assert.Equal(4, f.MaxNumFlavors);
    }

    [Fact]
    public void AnyNonZeroFlavorMin_SetsMinAnyNonZeroFlavorValue()
    {
        var f = BerryFilters.AnyNonZeroFlavorMin(8);
        AssertDefaults(f, anyNonZero: false);
        Assert.Equal(8, f.MinAnyNonZeroFlavorValue);
    }

    [Fact]
    public void AnyNonZeroFlavorMax_SetsMaxAnyNonZeroFlavorValue()
    {
        var f = BerryFilters.AnyNonZeroFlavorMax(18);
        AssertDefaults(f, anyNonZero: false);
        Assert.Equal(18, f.MaxAnyNonZeroFlavorValue);
    }

    [Fact]
    public void WeakenedMainFlavorRange_SetsMinAndMax()
    {
        var f = BerryFilters.WeakenedMainFlavorRange(-5, 20);
        AssertDefaults(f, weakened: false);
        Assert.Equal(-5, f.MinWeakenedMainFlavorValue);
        Assert.Equal(20, f.MaxWeakenedMainFlavorValue);
    }

    [Fact]
    public void Tight_SetsThreeConstraints()
    {
        var f = BerryFilters.Tight(maxSmoothness: 25, maxRarity: 3, minMainFlavorValue: 10);
        AssertDefaults(f, smoothness: false, rarity: false, mainFlavor: false);
        Assert.Equal(25, f.MaxSmoothness);
        Assert.Equal(3, f.MaxRarity);
        Assert.Equal(10, f.MinMainFlavorValue);
    }

    [Fact]
    public void Filter_CommonOnly_ExcludesRareBerries()
    {
        var f = BerryFilters.CommonOnly();

        Span<BerryId> buf = stackalloc BerryId[BerryTable.Count];
        int count = BerryQuery.Filter(in f, buf);

        for (int i = 0; i < count; i++)
        {
            ref readonly var b = ref BerryTable.Get(buf[i]);
            Assert.True(b.Rarity <= 3);
        }
    }

    [Fact]
    public void Filter_RequireFlavor_OnlyIncludesBerriesWithFlavor()
    {
        var f = BerryFilters.RequireFlavor(Flavor.Sweet);

        Span<BerryId> buf = stackalloc BerryId[BerryTable.Count];
        int count = BerryQuery.Filter(in f, buf);

        for (int i = 0; i < count; i++)
        {
            ref readonly var b = ref BerryTable.Get(buf[i]);
            Assert.True(b.Sweet > 0);
        }
    }

    [Fact]
    public void Filter_RareOnly_OnlyIncludesRareBerries()
    {
        var f = BerryFilters.RareOnly();

        Span<BerryId> buf = stackalloc BerryId[BerryTable.Count];
        int count = BerryQuery.Filter(in f, buf);

        for (int i = 0; i < count; i++)
        {
            ref readonly var b = ref BerryTable.Get(buf[i]);
            Assert.True(b.Rarity >= 4);
        }
    }

    [Fact]
    public void Filter_LowSmoothness_OnlyIncludesAtOrBelowMax()
    {
        var f = BerryFilters.LowSmoothness(30);

        Span<BerryId> buf = stackalloc BerryId[BerryTable.Count];
        int count = BerryQuery.Filter(in f, buf);

        for (int i = 0; i < count; i++)
        {
            ref readonly var b = ref BerryTable.Get(buf[i]);
            Assert.True(b.Smoothness <= 30);
        }
    }

    [Fact]
    public void Filter_VeryLowSmoothness_OnlyIncludesAtOrBelow25()
    {
        var f = BerryFilters.VeryLowSmoothness();

        Span<BerryId> buf = stackalloc BerryId[BerryTable.Count];
        int count = BerryQuery.Filter(in f, buf);

        for (int i = 0; i < count; i++)
        {
            ref readonly var b = ref BerryTable.Get(buf[i]);
            Assert.True(b.Smoothness <= 25);
        }
    }

    [Fact]
    public void Filter_NumFlavorsRange_StaysWithinBounds()
    {
        var f = BerryFilters.NumFlavorsRange(2, 4);

        Span<BerryId> buf = stackalloc BerryId[BerryTable.Count];
        int count = BerryQuery.Filter(in f, buf);

        for (int i = 0; i < count; i++)
        {
            ref readonly var b = ref BerryTable.Get(buf[i]);
            int num = BerryFacts.GetNumFlavors(in b);
            Assert.True(num >= 2 && num <= 4);
        }
    }

    [Fact]
    public void Filter_MainFlavorRange_StaysWithinBounds()
    {
        var f = BerryFilters.MainFlavorRange(10, 25);

        Span<BerryId> buf = stackalloc BerryId[BerryTable.Count];
        int count = BerryQuery.Filter(in f, buf);

        for (int i = 0; i < count; i++)
        {
            ref readonly var b = ref BerryTable.Get(buf[i]);
            int main = BerryFacts.GetMainFlavorValue(in b);
            Assert.True(main >= 10 && main <= 25);
        }
    }

    [Fact]
    public void Filter_StrongMainFlavor_OnlyIncludesAtOrAboveMin()
    {
        var f = BerryFilters.StrongMainFlavor(12);

        Span<BerryId> buf = stackalloc BerryId[BerryTable.Count];
        int count = BerryQuery.Filter(in f, buf);

        for (int i = 0; i < count; i++)
        {
            ref readonly var b = ref BerryTable.Get(buf[i]);
            int main = BerryFacts.GetMainFlavorValue(in b);
            Assert.True(main >= 12);
        }
    }

    [Fact]
    public void Filter_AnyNonZeroFlavorMin_OnlyIncludesAtOrAboveMin()
    {
        var f = BerryFilters.AnyNonZeroFlavorMin(8);

        Span<BerryId> buf = stackalloc BerryId[BerryTable.Count];
        int count = BerryQuery.Filter(in f, buf);

        for (int i = 0; i < count; i++)
        {
            ref readonly var b = ref BerryTable.Get(buf[i]);
            Assert.False(BerryFacts.HasAnyNonZeroFlavorLessThan(in b, 8));
        }
    }

    [Fact]
    public void Filter_AnyNonZeroFlavorMax_OnlyIncludesAtOrBelowMax()
    {
        var f = BerryFilters.AnyNonZeroFlavorMax(18);

        Span<BerryId> buf = stackalloc BerryId[BerryTable.Count];
        int count = BerryQuery.Filter(in f, buf);

        for (int i = 0; i < count; i++)
        {
            ref readonly var b = ref BerryTable.Get(buf[i]);
            Assert.False(BerryFacts.HasAnyFlavorGreaterThan(in b, 18));
        }
    }

    [Fact]
    public void Filter_WeakenedMainFlavorRange_StaysWithinBounds()
    {
        var f = BerryFilters.WeakenedMainFlavorRange(-5, 20);

        Span<BerryId> buf = stackalloc BerryId[BerryTable.Count];
        int count = BerryQuery.Filter(in f, buf);

        for (int i = 0; i < count; i++)
        {
            ref readonly var b = ref BerryTable.Get(buf[i]);
            int weakened = BerryFacts.GetWeakenedMainFlavorValue(in b);
            Assert.True(weakened >= -5 && weakened <= 20);
        }
    }

    [Fact]
    public void Filter_Tight_CombinesConstraints()
    {
        var f = BerryFilters.Tight(maxSmoothness: 25, maxRarity: 3, minMainFlavorValue: 10);

        Span<BerryId> buf = stackalloc BerryId[BerryTable.Count];
        int count = BerryQuery.Filter(in f, buf);

        for (int i = 0; i < count; i++)
        {
            ref readonly var b = ref BerryTable.Get(buf[i]);
            int main = BerryFacts.GetMainFlavorValue(in b);
            Assert.True(b.Smoothness <= 25);
            Assert.True(b.Rarity <= 3);
            Assert.True(main >= 10);
        }
    }

    private static void AssertDefaults(
        BerryFilterOptions f,
        bool allowMask = true,
        bool smoothness = true,
        bool rarity = true,
        bool mainFlavor = true,
        bool numFlavors = true,
        bool anyNonZero = true,
        bool weakened = true,
        bool requiredFlavor = true)
    {
        if (allowMask)
        {
            Assert.Equal(0UL, f.AllowedMaskLo);
            Assert.Equal(0UL, f.AllowedMaskHi);
        }

        if (smoothness)
        {
            Assert.Equal(-1, f.MinSmoothness);
            Assert.Equal(int.MaxValue, f.MaxSmoothness);
        }

        if (rarity)
        {
            Assert.Equal(-1, f.MinRarity);
            Assert.Equal(int.MaxValue, f.MaxRarity);
        }

        if (mainFlavor)
        {
            Assert.Equal(-1, f.MinMainFlavorValue);
            Assert.Equal(int.MaxValue, f.MaxMainFlavorValue);
        }

        if (numFlavors)
        {
            Assert.Equal(-1, f.MinNumFlavors);
            Assert.Equal(int.MaxValue, f.MaxNumFlavors);
        }

        if (anyNonZero)
        {
            Assert.Equal(-1, f.MinAnyNonZeroFlavorValue);
            Assert.Equal(int.MaxValue, f.MaxAnyNonZeroFlavorValue);
        }

        if (weakened)
        {
            Assert.Equal(int.MinValue, f.MinWeakenedMainFlavorValue);
            Assert.Equal(int.MaxValue, f.MaxWeakenedMainFlavorValue);
        }

        if (requiredFlavor)
        {
            Assert.False(f.HasRequiredFlavor);
            Assert.Equal((byte)0, f.RequiredFlavor);
        }
    }
}
