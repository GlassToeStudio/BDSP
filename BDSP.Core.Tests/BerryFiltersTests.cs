using BDSP.Core.Berries;
using BDSP.Core.Primitives;
using BDSP.Criteria;

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
    public void RequireFlavors_SetsRequiredFlavorMask()
    {
        var f = BerryFilters.RequireFlavors(Flavor.Spicy, Flavor.Sweet);
        AssertDefaults(f, requiredFlavorMask: false);
        Assert.Equal((byte)((1 << 0) | (1 << 2)), f.RequiredFlavorMask);
    }

    [Fact]
    public void ExcludeFlavors_SetsExcludedFlavorMask()
    {
        var f = BerryFilters.ExcludeFlavors(Flavor.Dry, Flavor.Bitter);
        AssertDefaults(f, excludedFlavorMask: false);
        Assert.Equal((byte)((1 << 1) | (1 << 3)), f.ExcludedFlavorMask);
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
    public void FlavorRange_SetsPerFlavorBounds()
    {
        var f = BerryFilters.FlavorRange(Flavor.Sour, 5, 20);
        AssertDefaults(f, flavorRanges: false);
        Assert.Equal(5, f.MinSour);
        Assert.Equal(20, f.MaxSour);
    }

    [Fact]
    public void MinFlavor_SetsPerFlavorMin()
    {
        var f = BerryFilters.MinFlavor(Flavor.Dry, 10);
        AssertDefaults(f, flavorRanges: false);
        Assert.Equal(10, f.MinDry);
    }

    [Fact]
    public void MaxFlavor_SetsPerFlavorMax()
    {
        var f = BerryFilters.MaxFlavor(Flavor.Bitter, 12);
        AssertDefaults(f, flavorRanges: false);
        Assert.Equal(12, f.MaxBitter);
    }

    [Fact]
    public void AllowOnly_SetsAllowMask()
    {
        var f = BerryFilters.AllowOnly(TestHelpers.Ids(1, 3, 5));
        AssertDefaults(f, allowMask: false);
        Assert.True(f.AllowedMaskLo != 0 || f.AllowedMaskHi != 0);
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
    public void Filter_RequireFlavors_OnlyIncludesBerriesWithAllFlavors()
    {
        var f = BerryFilters.RequireFlavors(Flavor.Spicy, Flavor.Dry);

        Span<BerryId> buf = stackalloc BerryId[BerryTable.Count];
        int count = BerryQuery.Filter(in f, buf);

        for (int i = 0; i < count; i++)
        {
            ref readonly var b = ref BerryTable.Get(buf[i]);
            Assert.True(b.Spicy > 0 && b.Dry > 0);
        }
    }

    [Fact]
    public void Filter_ExcludeFlavors_OnlyIncludesBerriesWithoutFlavors()
    {
        var f = BerryFilters.ExcludeFlavors(Flavor.Bitter);

        Span<BerryId> buf = stackalloc BerryId[BerryTable.Count];
        int count = BerryQuery.Filter(in f, buf);

        for (int i = 0; i < count; i++)
        {
            ref readonly var b = ref BerryTable.Get(buf[i]);
            Assert.True(b.Bitter == 0);
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

    [Fact]
    public void Filter_FlavorRange_StaysWithinBounds()
    {
        var f = BerryFilters.FlavorRange(Flavor.Sour, 5, 20);

        Span<BerryId> buf = stackalloc BerryId[BerryTable.Count];
        int count = BerryQuery.Filter(in f, buf);

        for (int i = 0; i < count; i++)
        {
            ref readonly var b = ref BerryTable.Get(buf[i]);
            Assert.True(b.Sour >= 5 && b.Sour <= 20);
        }
    }

    [Fact]
    public void Filter_MinFlavor_OnlyIncludesAtOrAboveMin()
    {
        var f = BerryFilters.MinFlavor(Flavor.Spicy, 10);

        Span<BerryId> buf = stackalloc BerryId[BerryTable.Count];
        int count = BerryQuery.Filter(in f, buf);

        for (int i = 0; i < count; i++)
        {
            ref readonly var b = ref BerryTable.Get(buf[i]);
            Assert.True(b.Spicy >= 10);
        }
    }

    [Fact]
    public void Filter_MaxFlavor_OnlyIncludesAtOrBelowMax()
    {
        var f = BerryFilters.MaxFlavor(Flavor.Dry, 15);

        Span<BerryId> buf = stackalloc BerryId[BerryTable.Count];
        int count = BerryQuery.Filter(in f, buf);

        for (int i = 0; i < count; i++)
        {
            ref readonly var b = ref BerryTable.Get(buf[i]);
            Assert.True(b.Dry <= 15);
        }
    }

    [Fact]
    public void Filter_AllowOnly_OnlyIncludesAllowedIds()
    {
        var allowed = TestHelpers.Ids(1, 3, 5);
        var f = BerryFilters.AllowOnly(allowed);

        Span<BerryId> buf = stackalloc BerryId[BerryTable.Count];
        int count = BerryQuery.Filter(in f, buf);

        for (int i = 0; i < count; i++)
        {
            Assert.Contains(buf[i].Value, new ushort[] { 1, 3, 5 });
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
        bool requiredFlavor = true,
        bool requiredFlavorMask = true,
        bool excludedFlavorMask = true,
        bool flavorRanges = true)
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

        if (requiredFlavorMask)
            Assert.Equal((byte)0, f.RequiredFlavorMask);

        if (excludedFlavorMask)
            Assert.Equal((byte)0, f.ExcludedFlavorMask);

        if (flavorRanges)
        {
            Assert.Equal(-1, f.MinSpicy);
            Assert.Equal(int.MaxValue, f.MaxSpicy);
            Assert.Equal(-1, f.MinDry);
            Assert.Equal(int.MaxValue, f.MaxDry);
            Assert.Equal(-1, f.MinSweet);
            Assert.Equal(int.MaxValue, f.MaxSweet);
            Assert.Equal(-1, f.MinBitter);
            Assert.Equal(int.MaxValue, f.MaxBitter);
            Assert.Equal(-1, f.MinSour);
            Assert.Equal(int.MaxValue, f.MaxSour);
        }
    }
}
