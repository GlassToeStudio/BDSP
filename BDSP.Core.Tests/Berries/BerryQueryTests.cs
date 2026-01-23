using System;
using BDSP.Core.Berries;
using Xunit;

namespace BDSP.Core.Tests.Berries
{
    public class BerryQueryTests
    {
        [Fact]
        public void Filter_ByFlavorRanges()
        {
            Span<Berry> buffer = stackalloc Berry[BerryTable.Count];

            AssertRangeFilter(
                buffer,
                new BerryFilterOptions(minSpicy: 1, maxSpicy: 40),
                b => b.Spicy,
                1,
                40);
            AssertRangeFilter(
                buffer,
                new BerryFilterOptions(minDry: 1, maxDry: 40),
                b => b.Dry,
                1,
                40);
            AssertRangeFilter(
                buffer,
                new BerryFilterOptions(minSweet: 1, maxSweet: 40),
                b => b.Sweet,
                1,
                40);
            AssertRangeFilter(
                buffer,
                new BerryFilterOptions(minBitter: 1, maxBitter: 40),
                b => b.Bitter,
                1,
                40);
            AssertRangeFilter(
                buffer,
                new BerryFilterOptions(minSour: 1, maxSour: 40),
                b => b.Sour,
                1,
                40);
        }

        [Fact]
        public void Filter_ByDerivedRanges()
        {
            Span<Berry> buffer = stackalloc Berry[BerryTable.Count];

            AssertRangeFilter(
                buffer,
                new BerryFilterOptions(minSmoothness: 30, maxSmoothness: 40),
                b => b.Smoothness,
                30,
                40);
            AssertRangeFilter(
                buffer,
                new BerryFilterOptions(minRarity: 5, maxRarity: 9),
                b => b.Rarity,
                5,
                9);
            AssertRangeFilter(
                buffer,
                new BerryFilterOptions(minMainFlavorValue: 30, maxMainFlavorValue: 40),
                b => b.MainFlavorValue,
                30,
                40);
            AssertRangeFilter(
                buffer,
                new BerryFilterOptions(minSecondaryFlavorValue: 10, maxSecondaryFlavorValue: 30),
                b => b.SecondaryFlavorValue,
                10,
                30);
            AssertRangeFilter(
                buffer,
                new BerryFilterOptions(minNumFlavors: 2, maxNumFlavors: 3),
                b => b.NumFlavors,
                2,
                3);
        }

        [Fact]
        public void Filter_BySecondaryFlavor()
        {
            var options = new BerryFilterOptions(
                requireSecondaryFlavor: true,
                secondaryFlavor: Flavor.Sour);

            Span<Berry> buffer = stackalloc Berry[BerryTable.Count];
            var count = BerryQuery.Execute(BerryTable.All, buffer, options, default);
            var results = buffer[..count];

            Assert.NotEmpty(results.ToArray());
            foreach (var berry in results)
            {
                Assert.Equal(Flavor.Sour, berry.SecondaryFlavor);
            }
        }

        [Fact]
        public void Filter_ByMainFlavor_AndMinValue()
        {
            var options = new BerryFilterOptions(
                minSpicy: 30,
                requireMainFlavor: true,
                mainFlavor: Flavor.Spicy);

            Span<Berry> buffer = stackalloc Berry[BerryTable.Count];
            var count = BerryQuery.Execute(BerryTable.All, buffer, options, default);
            var results = buffer[..count];

            Assert.NotEmpty(results.ToArray());
            foreach (var berry in results)
            {
                Assert.Equal(Flavor.Spicy, berry.MainFlavor);
                Assert.True(berry.Spicy >= 30);
            }
        }

        [Fact]
        public void Filter_ByFlavorMask_IncludesAndExcludes()
        {
            const byte spicyMask = 1 << 0;
            const byte sourMask = 1 << 4;
            var options = new BerryFilterOptions(
                requiredFlavorMask: spicyMask,
                excludedFlavorMask: sourMask);

            Span<Berry> buffer = stackalloc Berry[BerryTable.Count];
            var count = BerryQuery.Execute(BerryTable.All, buffer, options, default);
            var results = buffer[..count];

            Assert.NotEmpty(results.ToArray());
            foreach (var berry in results)
            {
                Assert.True(berry.Spicy > 0);
                Assert.True(berry.Sour == 0);
            }
        }

        [Fact]
        public void Sort_ByRarityThenName()
        {
            var keys = new[]
            {
                new BerrySortKey(BerrySortField.Rarity),
                new BerrySortKey(BerrySortField.Name)
            };

            Span<Berry> buffer = stackalloc Berry[BerryTable.Count];
            var count = BerryQuery.Execute(BerryTable.All, buffer, default, keys);
            var results = buffer[..count];

            for (var i = 1; i < results.Length; i++)
            {
                var prev = results[i - 1];
                var current = results[i];
                var rarityCompare = prev.Rarity.CompareTo(current.Rarity);
                if (rarityCompare < 0)
                {
                    continue;
                }

                if (rarityCompare == 0)
                {
                    var nameCompare = string.CompareOrdinal(
                        BerryNames.GetName(prev.Id),
                        BerryNames.GetName(current.Id));
                    Assert.True(nameCompare <= 0);
                    continue;
                }

                Assert.True(rarityCompare <= 0);
            }
        }

        [Fact]
        public void Sort_ByMainFlavorValueDescending()
        {
            var keys = new[] { new BerrySortKey(BerrySortField.MainFlavorValue, descending: true) };

            Span<Berry> buffer = stackalloc Berry[BerryTable.Count];
            var count = BerryQuery.Execute(BerryTable.All, buffer, default, keys);
            var results = buffer[..count];

            for (var i = 1; i < results.Length; i++)
            {
                Assert.True(results[i - 1].MainFlavorValue >= results[i].MainFlavorValue);
            }
        }

        [Fact]
        public void Sort_AllFields_AscendingAndDescending()
        {
            var fields = (BerrySortField[])Enum.GetValues(typeof(BerrySortField));
            foreach (var field in fields)
            {
                AssertSorted(field, descending: false);
                AssertSorted(field, descending: true);
            }
        }

        private static void AssertRangeFilter(
            Span<Berry> buffer,
            in BerryFilterOptions options,
            Func<Berry, byte> selector,
            int min,
            int max)
        {
            var count = BerryQuery.Execute(BerryTable.All, buffer, options, default);
            var results = buffer[..count];

            Assert.NotEmpty(results.ToArray());
            foreach (var berry in results)
            {
                var value = selector(berry);
                Assert.InRange(value, min, max);
            }
        }

        private static void AssertSorted(BerrySortField field, bool descending)
        {
            var keys = new[] { new BerrySortKey(field, descending) };
            Span<Berry> buffer = stackalloc Berry[BerryTable.Count];
            var count = BerryQuery.Execute(BerryTable.All, buffer, default, keys);
            var results = buffer[..count];

            for (var i = 1; i < results.Length; i++)
            {
                var prev = results[i - 1];
                var current = results[i];
                var cmp = CompareField(prev, current, field);
                if (descending)
                {
                    Assert.True(cmp >= 0);
                }
                else
                {
                    Assert.True(cmp <= 0);
                }
            }
        }

        private static int CompareField(Berry x, Berry y, BerrySortField field)
        {
            return field switch
            {
                BerrySortField.Id => x.Id.Value.CompareTo(y.Id.Value),
                BerrySortField.Spicy => x.Spicy.CompareTo(y.Spicy),
                BerrySortField.Dry => x.Dry.CompareTo(y.Dry),
                BerrySortField.Sweet => x.Sweet.CompareTo(y.Sweet),
                BerrySortField.Bitter => x.Bitter.CompareTo(y.Bitter),
                BerrySortField.Sour => x.Sour.CompareTo(y.Sour),
                BerrySortField.Smoothness => x.Smoothness.CompareTo(y.Smoothness),
                BerrySortField.Rarity => x.Rarity.CompareTo(y.Rarity),
                BerrySortField.MainFlavor => x.MainFlavor.CompareTo(y.MainFlavor),
                BerrySortField.SecondaryFlavor => x.SecondaryFlavor.CompareTo(y.SecondaryFlavor),
                BerrySortField.MainFlavorValue => x.MainFlavorValue.CompareTo(y.MainFlavorValue),
                BerrySortField.SecondaryFlavorValue => x.SecondaryFlavorValue.CompareTo(y.SecondaryFlavorValue),
                BerrySortField.NumFlavors => x.NumFlavors.CompareTo(y.NumFlavors),
                BerrySortField.Name => string.CompareOrdinal(
                    BerryNames.GetName(x.Id),
                    BerryNames.GetName(y.Id)),
                _ => 0
            };
        }
    }
}
