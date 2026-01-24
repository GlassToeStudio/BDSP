using BDSP.Core.Berries;
using BDSP.Core.Poffins.Cooking;
using BDSP.Core.Poffins;
using Xunit;

namespace BDSP.Core.Tests.Cooking
{
    public class PoffinComboTableTests
    {
        [Fact]
        public void Count_MatchesCombinatorics()
        {
            int n = BerryTable.Count;
            int expected = Choose(n, 2) + Choose(n, 3) + Choose(n, 4);
            Assert.Equal(expected, PoffinComboTable.Count);
            Assert.Equal(expected, PoffinComboTable.All.Length);
        }

        [Fact]
        public void FirstAndLast2Combo_MatchBaseSums()
        {
            ReadOnlySpan<BerryBase> bases = BerryTable.BaseAll;
            ReadOnlySpan<PoffinComboBase> combos = PoffinComboTable.All;

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

            int last2Index = Choose(BerryTable.Count, 2) - 1;
            PoffinComboBase last2 = combos[last2Index];
            ref readonly BerryBase aLast = ref bases[BerryTable.Count - 2];
            ref readonly BerryBase bLast = ref bases[BerryTable.Count - 1];

            Assert.Equal(aLast.WeakSpicy + bLast.WeakSpicy, last2.WeakSpicySum);
            Assert.Equal(aLast.WeakDry + bLast.WeakDry, last2.WeakDrySum);
            Assert.Equal(aLast.WeakSweet + bLast.WeakSweet, last2.WeakSweetSum);
            Assert.Equal(aLast.WeakBitter + bLast.WeakBitter, last2.WeakBitterSum);
            Assert.Equal(aLast.WeakSour + bLast.WeakSour, last2.WeakSourSum);
            Assert.Equal(aLast.Smoothness + bLast.Smoothness, last2.SmoothnessSum);
            Assert.Equal(2, last2.Count);
        }

        [Fact]
        public void CookFromCombo_MatchesSpanCook()
        {
            ReadOnlySpan<BerryBase> bases = BerryTable.BaseAll;
            PoffinComboBase combo = PoffinComboTable.All[0];

            Span<BerryBase> berries = stackalloc BerryBase[2];
            berries[0] = bases[0];
            berries[1] = bases[1];

            Poffin fromSpan = PoffinCooker.Cook(berries, cookTimeSeconds: 60, spills: 0, burns: 0);
            Poffin fromCombo = PoffinCooker.Cook(combo, cookTimeSeconds: 60, spills: 0, burns: 0);

            Assert.Equal(fromSpan.Spicy, fromCombo.Spicy);
            Assert.Equal(fromSpan.Dry, fromCombo.Dry);
            Assert.Equal(fromSpan.Sweet, fromCombo.Sweet);
            Assert.Equal(fromSpan.Bitter, fromCombo.Bitter);
            Assert.Equal(fromSpan.Sour, fromCombo.Sour);
            Assert.Equal(fromSpan.Smoothness, fromCombo.Smoothness);
            Assert.Equal(fromSpan.IsFoul, fromCombo.IsFoul);
        }

        private static int Choose(int n, int k)
        {
            if (k == 2) return n * (n - 1) / 2;
            if (k == 3) return n * (n - 1) * (n - 2) / 6;
            if (k == 4) return n * (n - 1) * (n - 2) * (n - 3) / 24;
            return 0;
        }
    }
}
