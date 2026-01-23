using BDSP.Core.Poffins;
using BDSP.Core.Primitives;
using BDSP.Core.Selection;
using Xunit;

namespace BDSP.Core.Tests;

public sealed class TopKPoffinSelectorTests
{
    private sealed class LevelComparer : IPoffinComparer
    {
        public bool IsBetter(in Poffin a, in Poffin b)
            => a.Level > b.Level;
    }

    [Fact]
    public void Selector_KeepsOnlyTopK()
    {
        var selector = new TopKPoffinSelector(3, new LevelComparer());

        for (byte i = 0; i < 10; i++)
        {
            var p = new Poffin(
                level: i,
                secondLevel: 0,
                smoothness: 0,
                spicy: i,
                dry: 0,
                sweet: 0,
                bitter: 0,
                sour: 0,
                type: PoffinType.SingleFlavor,
                primaryFlavor: Flavor.Spicy,
                secondaryFlavor: Flavor.Spicy);

            selector.Consider(in p);
        }

        var results = selector.Results;

        Assert.Equal(3, results.Length);

        byte min = byte.MaxValue;
        byte max = 0;

        foreach (var p in results)
        {
            if (p.Level < min) min = p.Level;
            if (p.Level > max) max = p.Level;
        }

        Assert.True(min >= 7);
        Assert.Equal(9, max);
    }
}
