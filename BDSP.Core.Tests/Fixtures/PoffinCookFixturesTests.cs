using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using BDSP.Core.Berries;
using Xunit;

namespace BDSP.Core.Tests.Fixtures
{
    public class PoffinCookFixturesTests
    {
        [Fact(Skip = "Enable after PoffinCooker is implemented.")]
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

                // TODO: replace with actual cooker
                // var result = PoffinCooker.Cook(berries, testCase.CookTimeSeconds, testCase.Spills, testCase.Burns);
                // Assert.Equal(testCase.Expected.IsFoul, result.IsFoul);
                // Assert.Equal(testCase.Expected.Smoothness, result.Smoothness);
                // Assert.Equal(testCase.Expected.Flavors, result.Flavors);
            }
        }

        private static FixturesRoot LoadFixtures(string relativePath)
        {
            var fullPath = Path.Combine(Directory.GetCurrentDirectory(), relativePath);
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
    }
}
