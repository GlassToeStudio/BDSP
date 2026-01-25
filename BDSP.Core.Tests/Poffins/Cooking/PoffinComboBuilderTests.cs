using BDSP.Core.Berries;
using BDSP.Core.Poffins.Cooking;
using Xunit;

namespace BDSP.Core.Tests.Poffins.Cooking
{
    public class PoffinComboBuilderTests
    {
        [Fact]
        public void CreateFromSubset_ReturnsExpectedCount()
        {
            var ids = new[] { new BerryId(0), new BerryId(1), new BerryId(2), new BerryId(3) };
            var combos = PoffinComboBuilder.CreateFromSubset(ids);
            Assert.Equal(11, combos.Length); // C(4,2)+C(4,3)+C(4,4) = 6+4+1
        }

        [Fact]
        public void CreateFromSubset_FirstAndLastCombosMatchBaseSums()
        {
            var ids = new[] { new BerryId(0), new BerryId(1), new BerryId(2), new BerryId(3) };
            var combos = PoffinComboBuilder.CreateFromSubset(ids);
            ReadOnlySpan<BerryBase> bases = BerryTable.BaseAll;

            ref readonly BerryBase a0 = ref bases[0];
            ref readonly BerryBase b1 = ref bases[1];
            PoffinComboBase first = combos[0];

            Assert.Equal(a0.WeakSpicy + b1.WeakSpicy, first.WeakSpicySum);
            Assert.Equal(a0.WeakDry + b1.WeakDry, first.WeakDrySum);
            Assert.Equal(a0.WeakSweet + b1.WeakSweet, first.WeakSweetSum);
            Assert.Equal(a0.WeakBitter + b1.WeakBitter, first.WeakBitterSum);
            Assert.Equal(a0.WeakSour + b1.WeakSour, first.WeakSourSum);
            Assert.Equal(a0.Smoothness + b1.Smoothness, first.SmoothnessSum);
            Assert.Equal(2, first.Count);

            PoffinComboBase last = combos[^1];
            ref readonly BerryBase a = ref bases[0];
            ref readonly BerryBase b = ref bases[1];
            ref readonly BerryBase c = ref bases[2];
            ref readonly BerryBase d = ref bases[3];

            Assert.Equal(a.WeakSpicy + b.WeakSpicy + c.WeakSpicy + d.WeakSpicy, last.WeakSpicySum);
            Assert.Equal(a.WeakDry + b.WeakDry + c.WeakDry + d.WeakDry, last.WeakDrySum);
            Assert.Equal(a.WeakSweet + b.WeakSweet + c.WeakSweet + d.WeakSweet, last.WeakSweetSum);
            Assert.Equal(a.WeakBitter + b.WeakBitter + c.WeakBitter + d.WeakBitter, last.WeakBitterSum);
            Assert.Equal(a.WeakSour + b.WeakSour + c.WeakSour + d.WeakSour, last.WeakSourSum);
            Assert.Equal(a.Smoothness + b.Smoothness + c.Smoothness + d.Smoothness, last.SmoothnessSum);
            Assert.Equal(4, last.Count);
        }

        [Fact]
        public void CreateFromSubset_ReturnsEmptyForSingleOrEmpty()
        {
            Assert.Empty(PoffinComboBuilder.CreateFromSubset(ReadOnlySpan<BerryId>.Empty));
            Assert.Empty(PoffinComboBuilder.CreateFromSubset(new[] { new BerryId(0) }));
        }
    }
}
