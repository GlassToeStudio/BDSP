using BDSP.Core.Berries;
using BDSP.Core.Optimization.Core;
using BDSP.Core.Optimization.Search;
using BDSP.Core.Poffins;
using Xunit;

namespace BDSP.Core.Tests.Flows
{
    public class FeedingSearchFlowTests
    {
        [Fact]
        public void BuildPlan_StopsAtSheenCap()
        {
            var recipe = new PoffinRecipe(new[] { new BerryId(0), new BerryId(1) }, 40, 0, 0, 9);
            var low = new Poffin(spicy: 10, dry: 0, sweet: 0, bitter: 0, sour: 0, smoothness: 100, isFoul: false);
            var high = new Poffin(spicy: 20, dry: 0, sweet: 0, bitter: 0, sour: 0, smoothness: 200, isFoul: false);

            var candidates = new[]
            {
                new PoffinWithRecipe(low, recipe),
                new PoffinWithRecipe(high, recipe)
            };

            var options = FeedingSearchOptions.Default;
            var plan = FeedingSearch.BuildPlan(candidates, in options);

            Assert.Equal(2, plan.TotalPoffins);
            Assert.Equal(255, plan.TotalSheen);
            Assert.Equal(255, plan.FinalStats.Sheen);
        }
    }
}
