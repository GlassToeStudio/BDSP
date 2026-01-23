using BDSP.Core.Berries;
using Xunit;

namespace BDSP.Core.Tests.Berries
{
    public class BerryTableTests
    {
        [Fact]
        public void CountMatchesTables()
        {
            Assert.Equal(BerryTable.Count, BerryTable.All.Length);
            Assert.Equal(BerryTable.Count, BerryTable.BaseAll.Length);
        }

        [Fact]
        public void BaseTableMatchesFullTable()
        {
            for (var i = 0; i < BerryTable.Count; i++)
            {
                var id = new BerryId((ushort)i);
                ref readonly var full = ref BerryTable.Get(id);
                ref readonly var core = ref BerryTable.GetBase(id);

                Assert.Equal(full.Id.Value, core.Id.Value);
                Assert.Equal(full.Spicy, core.Spicy);
                Assert.Equal(full.Dry, core.Dry);
                Assert.Equal(full.Sweet, core.Sweet);
                Assert.Equal(full.Bitter, core.Bitter);
                Assert.Equal(full.Sour, core.Sour);
                Assert.Equal(full.Smoothness, core.Smoothness);
                Assert.Equal((sbyte)(full.Spicy - full.Dry), core.WeakSpicy);
                Assert.Equal((sbyte)(full.Dry - full.Sweet), core.WeakDry);
                Assert.Equal((sbyte)(full.Sweet - full.Bitter), core.WeakSweet);
                Assert.Equal((sbyte)(full.Bitter - full.Sour), core.WeakBitter);
                Assert.Equal((sbyte)(full.Sour - full.Spicy), core.WeakSour);
            }
        }

        [Fact]
        public void NamesTableCoversAllIds()
        {
            var seen = new HashSet<string>();
            for (var i = 0; i < BerryTable.Count; i++)
            {
                var name = BerryNames.GetName(new BerryId((ushort)i));
                Assert.False(string.IsNullOrWhiteSpace(name));
                seen.Add(name);
            }

            Assert.Equal(BerryTable.Count, seen.Count);
        }

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

        [Fact]
        public void GetFlavor_ReturnsCorrectValues()
        {
            ref readonly var ganlon = ref BerryTable.Get(new BerryId(18));
            Assert.Equal(0, ganlon.GetFlavor(Flavor.Spicy));
            Assert.Equal(30, ganlon.GetFlavor(Flavor.Dry));
            Assert.Equal(10, ganlon.GetFlavor(Flavor.Sweet));
            Assert.Equal(30, ganlon.GetFlavor(Flavor.Bitter));
            Assert.Equal(0, ganlon.GetFlavor(Flavor.Sour));
            Assert.Equal(0, ganlon.GetFlavor(Flavor.None));

            ref readonly var ganlonBase = ref BerryTable.GetBase(new BerryId(18));
            Assert.Equal(0, ganlonBase.GetFlavor(Flavor.Spicy));
            Assert.Equal(30, ganlonBase.GetFlavor(Flavor.Dry));
            Assert.Equal(10, ganlonBase.GetFlavor(Flavor.Sweet));
            Assert.Equal(30, ganlonBase.GetFlavor(Flavor.Bitter));
            Assert.Equal(0, ganlonBase.GetFlavor(Flavor.Sour));
            Assert.Equal(0, ganlonBase.GetFlavor(Flavor.None));
            Assert.Equal(-30, ganlonBase.GetWeakenedFlavor(Flavor.Spicy));
            Assert.Equal(20, ganlonBase.GetWeakenedFlavor(Flavor.Dry));
            Assert.Equal(-20, ganlonBase.GetWeakenedFlavor(Flavor.Sweet));
            Assert.Equal(30, ganlonBase.GetWeakenedFlavor(Flavor.Bitter));
            Assert.Equal(0, ganlonBase.GetWeakenedFlavor(Flavor.Sour));
        }

        [Fact]
        public void SingleFlavorBerriesHaveNoSecondary()
        {
            foreach (var berry in BerryTable.All)
            {
                if (berry.NumFlavors == 1)
                {
                    Assert.Equal(Flavor.None, berry.SecondaryFlavor);
                    Assert.Equal(0, berry.SecondaryFlavorValue);
                }
            }
        }

        [Fact]
        public void NameOrdering_IsOrdinal()
        {
            var aguav = BerryNames.GetName(new BerryId(0));
            var apicot = BerryNames.GetName(new BerryId(1));
            Assert.True(string.CompareOrdinal(aguav, apicot) < 0);
        }

        [Fact]
        public void SpotCheck_Ganlon()
        {
            ref readonly var berry = ref BerryTable.Get(new BerryId(18));
            Assert.Equal("Ganlon Berry", BerryNames.GetName(berry.Id));
            Assert.Equal(Flavor.Dry, berry.MainFlavor);
            Assert.Equal(30, berry.MainFlavorValue);
            Assert.Equal(Flavor.Bitter, berry.SecondaryFlavor);
            Assert.Equal(30, berry.SecondaryFlavorValue);
            Assert.Equal(40, berry.Smoothness);
            Assert.Equal(9, berry.Rarity);
            Assert.Equal(3, berry.NumFlavors);
            Assert.Equal(0, berry.Spicy);
            Assert.Equal(30, berry.Dry);
            Assert.Equal(10, berry.Sweet);
            Assert.Equal(30, berry.Bitter);
            Assert.Equal(0, berry.Sour);
        }

        [Fact]
        public void SpotCheck_Enigma()
        {
            ref readonly var berry = ref BerryTable.Get(new BerryId(16));
            Assert.Equal("Enigma Berry", BerryNames.GetName(berry.Id));
            Assert.Equal(Flavor.Spicy, berry.MainFlavor);
            Assert.Equal(40, berry.MainFlavorValue);
            Assert.Equal(Flavor.Dry, berry.SecondaryFlavor);
            Assert.Equal(10, berry.SecondaryFlavorValue);
            Assert.Equal(60, berry.Smoothness);
            Assert.Equal(15, berry.Rarity);
            Assert.Equal(2, berry.NumFlavors);
            Assert.Equal(40, berry.Spicy);
            Assert.Equal(10, berry.Dry);
            Assert.Equal(0, berry.Sweet);
            Assert.Equal(0, berry.Bitter);
            Assert.Equal(0, berry.Sour);
        }

        [Fact]
        public void SpotCheck_Rowap()
        {
            ref readonly var berry = ref BerryTable.Get(new BerryId(52));
            Assert.Equal("Rowap Berry", BerryNames.GetName(berry.Id));
            Assert.Equal(Flavor.Sour, berry.MainFlavor);
            Assert.Equal(40, berry.MainFlavorValue);
            Assert.Equal(Flavor.Spicy, berry.SecondaryFlavor);
            Assert.Equal(10, berry.SecondaryFlavorValue);
            Assert.Equal(60, berry.Smoothness);
            Assert.Equal(15, berry.Rarity);
            Assert.Equal(2, berry.NumFlavors);
            Assert.Equal(10, berry.Spicy);
            Assert.Equal(0, berry.Dry);
            Assert.Equal(0, berry.Sweet);
            Assert.Equal(0, berry.Bitter);
            Assert.Equal(40, berry.Sour);
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
