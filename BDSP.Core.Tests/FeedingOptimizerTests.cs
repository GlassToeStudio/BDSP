using BDSP.Core.Berries.Data;
using BDSP.Core.Feeding;
using BDSP.Core.Poffins;
using Xunit;

namespace BDSP.Core.Tests;

public sealed class FeedingOptimizerTests
{
    [Fact]
    public void Optimize_SkipsFoulPoffins()
    {
        var foulIds = TestHelpers.Ids(0, 0);
        var foul = PoffinCooker.Cook(foulIds, cookTimeSeconds: 40, errors: 0, amityBonus: 0);

        var goodIds = TestHelpers.Ids(7);
        var good = PoffinCooker.Cook(goodIds, cookTimeSeconds: 40, errors: 0, amityBonus: 0);

        var candidates = new[] { foul, good };
        var options = new FeedingOptions { MaxSheen = 255, MaxNodes = 10_000 };

        var plan = FeedingOptimizer.Optimize(candidates, options);

        Assert.DoesNotContain(plan.Poffins, p => p.Type == PoffinType.Foul);
    }
}
