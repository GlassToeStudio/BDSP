using BDSP.Core.Berries;
using BDSP.Core.Poffins.Filters;
using BDSP.Core.Poffins.Search;
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
            var filter = new PoffinFilterOptions(maxSmoothness: 0);
            var results = PoffinSearch.Run(default, options, topK: 50, filter);
            Assert.Empty(results);
        }

        [Fact]
        public void Run_SubsetPath_ReturnsResults()
        {
            var berryFilter = new BerryFilterOptions(minRarity: 15, maxRarity: 15);
            var options = new PoffinSearchOptions(choose: 2, cookTimeSeconds: 40, useParallel: false);
            var results = PoffinSearch.Run(berryFilter, options, topK: 5);
            Assert.True(results.Length <= 5);
        }
    }
}
