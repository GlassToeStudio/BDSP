using System;
using System.Linq;
using BDSP.Core.Berries;
using BDSP.Core.Optimization.Core;
using BDSP.Core.Optimization.Search;
using BDSP.Core.Poffins;
using BDSP.Core.Poffins.Cooking;
using BDSP.Core.Poffins.Filters;
using Xunit;

namespace BDSP.Core.Tests.Optimization
{
    public class ContestStatsParityTests
    {
        [Fact]
        public void ContestSearch_MatchesPythonReferenceRecipe()
        {
            BerryId aguav = FindBerryId("Aguav Berry");
            BerryId coba = FindBerryId("Coba Berry");
            BerryId passho = FindBerryId("Passho Berry");
            BerryId ganlon = FindBerryId("Ganlon Berry");

            BerryId figy = FindBerryId("Figy Berry");
            BerryId chople = FindBerryId("Chople Berry");
            BerryId rindo = FindBerryId("Rindo Berry");
            BerryId petaya = FindBerryId("Petaya Berry");

            BerryId payapa = FindBerryId("Payapa Berry");
            BerryId salac = FindBerryId("Salac Berry");
            BerryId lansat = FindBerryId("Lansat Berry");
            BerryId starf = FindBerryId("Starf Berry");

            var recipe1 = new PoffinRecipe(new[] { aguav, coba, passho, ganlon }, 40, 0, 0, 9);
            var recipe2 = new PoffinRecipe(new[] { figy, chople, rindo, petaya }, 40, 0, 0, 9);
            var recipe3 = new PoffinRecipe(new[] { payapa, salac, lansat, starf }, 40, 0, 0, 9);

            Poffin p1 = Cook(recipe1);
            Poffin p2 = Cook(recipe2);
            Poffin p3 = Cook(recipe3);

            Assert.Equal(0, p1.Spicy);
            Assert.Equal(64, p1.Dry);
            Assert.Equal(0, p1.Sweet);
            Assert.Equal(100, p1.Bitter);
            Assert.Equal(0, p1.Sour);
            Assert.Equal(18, p1.Smoothness);
            Assert.Equal(PoffinNameKind.SuperMild, PoffinFilter.GetNameKind(in p1));

            Assert.Equal(100, p2.Spicy);
            Assert.Equal(0, p2.Dry);
            Assert.Equal(0, p2.Sweet);
            Assert.Equal(64, p2.Bitter);
            Assert.Equal(0, p2.Sour);
            Assert.Equal(18, p2.Smoothness);
            Assert.Equal(PoffinNameKind.SuperMild, PoffinFilter.GetNameKind(in p2));

            Assert.Equal(57, p3.Spicy);
            Assert.Equal(0, p3.Dry);
            Assert.Equal(100, p3.Sweet);
            Assert.Equal(0, p3.Bitter);
            Assert.Equal(64, p3.Sour);
            Assert.Equal(29, p3.Smoothness);
            Assert.Equal(PoffinNameKind.SuperMild, PoffinFilter.GetNameKind(in p3));

            var candidates = new[]
            {
                new PoffinWithRecipe(p1, recipe1),
                new PoffinWithRecipe(p2, recipe2),
                new PoffinWithRecipe(p3, recipe3)
            };

            var searchOptions = new ContestStatsSearchOptions(
                choose: 3,
                useParallel: false,
                maxPoffins: 12,
                pruneCandidates: false);
            var scoreOptions = new FeedingSearchOptions(rarityCostMode: RarityCostMode.SumBerryRarity, scoreMode: ContestScoreMode.SumOnly);

            ContestStatsResult[] results = ContestStatsSearch.Run(candidates, in searchOptions, in scoreOptions, topK: 10);

            ContestStatsResult? match = results.FirstOrDefault(r =>
                r.Rank == 1 &&
                r.PoffinsEaten == 12 &&
                r.TotalSheen == 255 &&
                r.TotalRarityCost == 80 &&
                r.UniqueBerries == 12 &&
                r.Stats.Coolness == 255 &&
                r.Stats.Beauty == 255 &&
                r.Stats.Cuteness == 255 &&
                r.Stats.Cleverness == 255 &&
                r.Stats.Toughness == 255);

            Assert.True(match.HasValue, "Expected a Rank 1 result with 12 poffins, sheen 255, rarity 80, and all stats maxed.");
        }

        private static Poffin Cook(PoffinRecipe recipe)
        {
            Span<BerryBase> bases = stackalloc BerryBase[recipe.Berries.Length];
            for (int i = 0; i < recipe.Berries.Length; i++)
            {
                bases[i] = BerryTable.GetBase(recipe.Berries[i]);
            }

            return PoffinCooker.Cook(bases, recipe.CookTimeSeconds, recipe.Spills, recipe.Burns, recipe.AmityBonus);
        }

        private static BerryId FindBerryId(string name)
        {
            for (ushort i = 0; i < BerryTable.Count; i++)
            {
                var id = new BerryId(i);
                if (string.Equals(BerryNames.GetName(id), name, StringComparison.OrdinalIgnoreCase))
                {
                    return id;
                }
            }

            throw new InvalidOperationException($"Berry not found: {name}");
        }
    }
}
