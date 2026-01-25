using BDSP.Core.Berries;
using BDSP.Core.Optimization.Core;
using BDSP.Core.Optimization.Search;
using BDSP.Core.Poffins;
using Xunit;

namespace BDSP.Core.Tests.Flows
{
    public class ContestStatsSearchFlowTests
    {
        [Fact]
        public void Run_ReturnsRankedResults()
        {
            var recipe = new PoffinRecipe(new[] { new BerryId(0), new BerryId(1) }, 40, 0, 0, 9);

            var p1 = new Poffin(spicy: 10, dry: 10, sweet: 0, bitter: 0, sour: 0, smoothness: 10, isFoul: false);
            var p2 = new Poffin(spicy: 20, dry: 0, sweet: 0, bitter: 0, sour: 0, smoothness: 20, isFoul: false);
            var p3 = new Poffin(spicy: 0, dry: 20, sweet: 0, bitter: 0, sour: 0, smoothness: 15, isFoul: false);
            var p4 = new Poffin(spicy: 5, dry: 5, sweet: 5, bitter: 5, sour: 5, smoothness: 25, isFoul: false);

            var candidates = new[]
            {
                new PoffinWithRecipe(p1, recipe),
                new PoffinWithRecipe(p2, recipe),
                new PoffinWithRecipe(p3, recipe),
                new PoffinWithRecipe(p4, recipe)
            };

            var searchOptions = new ContestStatsSearchOptions(choose: 2, useParallel: false);
            var scoreOptions = FeedingSearchOptions.Default;

            ContestStatsResult[] results = ContestStatsSearch.Run(candidates, in searchOptions, in scoreOptions, topK: 5);

            Assert.NotEmpty(results);
            for (int i = 1; i < results.Length; i++)
            {
                Assert.True(results[i - 1].Score >= results[i].Score);
            }
        }
    }
}
