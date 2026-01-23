using BDSP.Core.Berries;
using Xunit;

namespace BDSP.Core.Tests.Berries
{
    public class BerryQueryTests
    {
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
    }
}
