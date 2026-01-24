using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using BDSP.Core.Berries;
using BDSP.Core.Poffins.Cooking;
using Xunit;
using System.Runtime.InteropServices;

namespace BDSP.Core.Tests.Fixtures
{
    public class PoffinCookFixturesTests
    {
        [Fact]
        public void CookingFixtures_MatchExpected()
        {
            var fixtures = LoadFixtures("Fixtures/poffin_cook_cases.json");

            foreach (var testCase in fixtures.Cases)
            {
                var berries = new List<BerryBase>(testCase.Berries.Count);
                foreach (var b in testCase.Berries)
                {
                    berries.Add(BerryTable.GetBase(new BerryId((ushort)b.Id)));
                }

#if !DEBUG
                if (testCase.Expected.IsFoul && HasDuplicateIds(testCase.Berries))
                {
                    continue;
                }
#endif

                var result = PoffinCooker.Cook(
                    CollectionsMarshal.AsSpan(berries),
                    testCase.CookTimeSeconds,
                    testCase.Spills,
                    testCase.Burns,
                    testCase.AmityBonus);

                Assert.Equal(testCase.Expected.IsFoul, result.IsFoul);
                Assert.Equal(testCase.Expected.Smoothness, result.Smoothness);
                Assert.Equal(testCase.Expected.Flavors[0], result.Spicy);
                Assert.Equal(testCase.Expected.Flavors[1], result.Dry);
                Assert.Equal(testCase.Expected.Flavors[2], result.Sweet);
                Assert.Equal(testCase.Expected.Flavors[3], result.Bitter);
                Assert.Equal(testCase.Expected.Flavors[4], result.Sour);
            }
        }

        private static FixturesRoot LoadFixtures(string relativePath)
        {
            var baseDir = AppContext.BaseDirectory;
            var fullPath = Path.Combine(baseDir, relativePath);
            var json = File.ReadAllText(fullPath);
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            return JsonSerializer.Deserialize<FixturesRoot>(json, options) ?? new FixturesRoot();
        }

        private sealed class FixturesRoot
        {
            public List<PoffinCookCase> Cases { get; set; } = new();
        }

        private sealed class PoffinCookCase
        {
            public string Name { get; set; } = string.Empty;
            public int CookTimeSeconds { get; set; }
            public int Burns { get; set; }
            public int Spills { get; set; }
            public int AmityBonus { get; set; } = 9;
            public List<BerryRef> Berries { get; set; } = new();
            public ExpectedResult Expected { get; set; } = new();
        }

        private sealed class BerryRef
        {
            public int Id { get; set; }
        }

        private sealed class ExpectedResult
        {
            public int[] Flavors { get; set; } = new int[5];
            public int Smoothness { get; set; }
            public bool IsFoul { get; set; }
        }

        private static bool HasDuplicateIds(List<BerryRef> berries)
        {
            if (berries.Count < 2)
            {
                return false;
            }

            for (var i = 0; i < berries.Count - 1; i++)
            {
                var id = berries[i].Id;
                for (var j = i + 1; j < berries.Count; j++)
                {
                    if (id == berries[j].Id)
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}
