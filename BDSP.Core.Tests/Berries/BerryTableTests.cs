using BDSP.Core.Berries;
using Xunit;

namespace BDSP.Core.Tests.Berries
{
    public class BerryTableTests
    {
        [Fact]
        public void AllBerriesHaveCorrectDerivedFields()
        {
            foreach (var berry in BerryTable.All)
            {
                var values = new[]
                {
                    (Flavor.Spicy, berry.Spicy),
                    (Flavor.Dry, berry.Dry),
                    (Flavor.Sweet, berry.Sweet),
                    (Flavor.Bitter, berry.Bitter),
                    (Flavor.Sour, berry.Sour)
                };

                var numFlavors = (byte)0;
                for (var i = 0; i < values.Length; i++)
                {
                    if (values[i].Item2 > 0)
                    {
                        numFlavors++;
                    }
                }

                var (mainFlavor, mainValue) = GetMainFlavor(values);
                var (secondaryFlavor, secondaryValue) = GetSecondaryFlavor(values, mainFlavor, numFlavors);

                Assert.Equal(numFlavors, berry.NumFlavors);
                Assert.Equal(mainFlavor, berry.MainFlavor);
                Assert.Equal(mainValue, berry.MainFlavorValue);
                Assert.Equal(secondaryFlavor, berry.SecondaryFlavor);
                Assert.Equal(secondaryValue, berry.SecondaryFlavorValue);
            }
        }

        private static (Flavor Flavor, byte Value) GetMainFlavor((Flavor Flavor, byte Value)[] values)
        {
            var bestFlavor = Flavor.Spicy;
            var bestValue = values[0].Value;

            for (var i = 1; i < values.Length; i++)
            {
                var (flavor, value) = values[i];
                if (value > bestValue || (value == bestValue && HasHigherPriority(flavor, bestFlavor)))
                {
                    bestFlavor = flavor;
                    bestValue = value;
                }
            }

            return (bestFlavor, bestValue);
        }

        private static (Flavor Flavor, byte Value) GetSecondaryFlavor(
            (Flavor Flavor, byte Value)[] values,
            Flavor mainFlavor,
            byte numFlavors)
        {
            if (numFlavors < 2)
            {
                return (Flavor.None, 0);
            }

            var bestFlavor = Flavor.None;
            byte bestValue = 0;

            for (var i = 0; i < values.Length; i++)
            {
                var (flavor, value) = values[i];
                if (flavor == mainFlavor)
                {
                    continue;
                }

                if (value > bestValue || (value == bestValue && HasHigherPriority(flavor, bestFlavor)))
                {
                    bestFlavor = flavor;
                    bestValue = value;
                }
            }

            return (bestFlavor, bestValue);
        }

        private static bool HasHigherPriority(Flavor candidate, Flavor current)
        {
            return GetPriority(candidate) > GetPriority(current);
        }

        private static int GetPriority(Flavor flavor)
        {
            return flavor switch
            {
                Flavor.Spicy => 5,
                Flavor.Dry => 4,
                Flavor.Sweet => 3,
                Flavor.Bitter => 2,
                Flavor.Sour => 1,
                _ => 0
            };
        }
    }
}
