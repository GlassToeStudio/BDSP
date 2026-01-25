using System;
using BDSP.Core.Berries;
using BDSP.Core.Poffins.Filters;
using BDSP.Core.Poffins.Search;
using Xunit;

namespace BDSP.Core.Tests.Flows
{
    public class PoffinSearchFlowTests
    {
        [Fact]
        public void Run_AllBerries_ReturnsRankedResults()
        {
            var options = new PoffinSearchOptions(
                choose: 2,
                cookTimeSeconds: 40,
                useParallel: false,
                useComboTableWhenAllBerries: true);

            PoffinResult[] results = PoffinSearch.Run(default, options, topK: 20);

            Assert.NotEmpty(results);
            Assert.True(results.Length <= 20);

            for (int i = 1; i < results.Length; i++)
            {
                Assert.True(results[i - 1].Score >= results[i].Score);
            }
        }

        [Fact]
        public void Run_Subset_RespectsPoffinFilter()
        {
            var berryFilter = new BerryFilterOptions(minRarity: 1, maxRarity: 3);
            var options = new PoffinSearchOptions(
                choose: 2,
                cookTimeSeconds: 60,
                useParallel: false,
                useComboTableWhenAllBerries: true);
            var poffinFilter = new PoffinFilterOptions(minLevel: 1);

            PoffinResult[] results = PoffinSearch.Run(berryFilter, options, topK: 25, poffinFilter);

            Assert.NotEmpty(results);
            for (int i = 0; i < results.Length; i++)
            {
                Assert.True(results[i].Poffin.Level >= 1);
            }
        }
    }
}
