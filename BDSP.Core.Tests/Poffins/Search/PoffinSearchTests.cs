using BDSP.Core.Berries;
using BDSP.Core.Poffins.Filters;
using BDSP.Core.Poffins.Search;
using System.Collections.Generic;
using Xunit;

namespace BDSP.Core.Tests.Poffins.Search
{
    public class PoffinSearchTests
    {
        [Fact]
        public void Run_ReturnsTopKResults()
        {
            var options = new PoffinSearchOptions(choose: 2, cookTimeSeconds: 40, useParallel: false);
            var results = PoffinSearch.Run(default, options, topK: 10);
            Assert.Equal(10, results.Length);
        }

        [Fact]
        public void Run_RespectsPoffinFilterOptions()
        {
            var options = new PoffinSearchOptions(choose: 2, cookTimeSeconds: 40, useParallel: false);
            var filter = new PoffinFilterOptions(minLevel: 256);
            var results = PoffinSearch.Run(default, options, topK: 50, filter);
            Assert.Empty(results);
        }

        [Fact]
        public void Run_AllowsExplicitZeroBounds()
        {
            var options = new PoffinSearchOptions(choose: 2, cookTimeSeconds: 40, useParallel: false);
            var filter = new PoffinFilterOptions(minSpicy: 0, maxSpicy: 0);
            var results = PoffinSearch.Run(default, options, topK: 50, filter);
            Assert.NotEmpty(results);
            foreach (var result in results)
            {
                Assert.Equal(0, result.Poffin.Spicy);
            }
        }

        [Fact]
        public void Run_SubsetPath_ReturnsResults()
        {
            var berryFilter = new BerryFilterOptions(minRarity: 15, maxRarity: 15);
            var options = new PoffinSearchOptions(choose: 2, cookTimeSeconds: 40, useParallel: false);
            var results = PoffinSearch.Run(berryFilter, options, topK: 5);
            Assert.True(results.Length <= 5);
        }

        [Fact]
        public void Run_RespectsMainFlavorFilter()
        {
            var options = new PoffinSearchOptions(choose: 2, cookTimeSeconds: 40, useParallel: false);
            var filter = new PoffinFilterOptions(requireMainFlavor: true, mainFlavor: Flavor.Spicy);
            var results = PoffinSearch.Run(default, options, topK: 20, filter);
            Assert.NotEmpty(results);
            foreach (var result in results)
            {
                Assert.Equal(Flavor.Spicy, result.Poffin.MainFlavor);
            }
        }

        [Fact]
        public void Run_InvalidRange_ReturnsEmpty()
        {
            var options = new PoffinSearchOptions(choose: 2, cookTimeSeconds: 40, useParallel: false);
            var filter = new PoffinFilterOptions(minSpicy: 10, maxSpicy: 5);
            var results = PoffinSearch.Run(default, options, topK: 20, filter);
            Assert.Empty(results);
        }

        [Fact]
        public void Run_ComboTableAndSubsetPathsMatch()
        {
            var optionsTable = new PoffinSearchOptions(
                choose: 2,
                cookTimeSeconds: 40,
                useParallel: false,
                useComboTableWhenAllBerries: true);
            var optionsSubset = new PoffinSearchOptions(
                choose: 2,
                cookTimeSeconds: 40,
                useParallel: false,
                useComboTableWhenAllBerries: false);

            var tableResults = PoffinSearch.Run(default, optionsTable, topK: 25);
            var subsetResults = PoffinSearch.Run(default, optionsSubset, topK: 25);

            Assert.Equal(tableResults.Length, subsetResults.Length);
            Assert.Equal(
                BuildResultCounts(tableResults),
                BuildResultCounts(subsetResults));
        }

        private static Dictionary<string, int> BuildResultCounts(PoffinResult[] results)
        {
            var counts = new Dictionary<string, int>();
            foreach (var result in results)
            {
                var key = string.Format(
                    System.Globalization.CultureInfo.InvariantCulture,
                    "{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12}",
                    result.Poffin.Spicy,
                    result.Poffin.Dry,
                    result.Poffin.Sweet,
                    result.Poffin.Bitter,
                    result.Poffin.Sour,
                    result.Poffin.Smoothness,
                    result.Poffin.Level,
                    result.Poffin.SecondLevel,
                    result.Poffin.MainFlavor,
                    result.Poffin.SecondaryFlavor,
                    result.Poffin.NumFlavors,
                    result.Score,
                    result.BerryCount);

                if (counts.TryGetValue(key, out int count))
                {
                    counts[key] = count + 1;
                }
                else
                {
                    counts[key] = 1;
                }
            }

            return counts;
        }
    }
}
