using BDSP.Core.Berries;
using BDSP.Core.Optimization.Core;
using BDSP.Core.Optimization.Filters;
using BDSP.Core.Optimization.Search;
using BDSP.Core.Poffins;
using Xunit;

namespace BDSP.Core.Tests.Optimization
{
    public class FeedingCandidatePrunerTests
    {
        [Fact]
        public void Prune_DeduplicatesIdenticalStats_AndKeepsLowestRarity()
        {
            ReadOnlySpan<Berry> all = BerryTable.All;
            BerryId lowId = all[0].Id;
            BerryId highId = all[0].Id;
            byte minRarity = all[0].Rarity;
            byte maxRarity = all[0].Rarity;

            for (int i = 1; i < all.Length; i++)
            {
                ref readonly var berry = ref all[i];
                if (berry.Rarity < minRarity)
                {
                    minRarity = berry.Rarity;
                    lowId = berry.Id;
                }
                if (berry.Rarity > maxRarity)
                {
                    maxRarity = berry.Rarity;
                    highId = berry.Id;
                }
            }

            var poffin = new Poffin(spicy: 12, dry: 8, sweet: 0, bitter: 0, sour: 0, smoothness: 20, isFoul: false);
            var lowRecipe = new PoffinRecipe(new[] { lowId, lowId }, cookTimeSeconds: 40, spills: 0, burns: 0, amityBonus: 9);
            var highRecipe = new PoffinRecipe(new[] { highId, highId }, cookTimeSeconds: 40, spills: 0, burns: 0, amityBonus: 9);

            var candidates = new[]
            {
                new PoffinWithRecipe(poffin, lowRecipe),
                new PoffinWithRecipe(poffin, highRecipe)
            };

            PoffinWithRecipe[] pruned = FeedingCandidatePruner.Prune(candidates, RarityCostMode.MaxBerryRarity);

            Assert.Single(pruned);
            Assert.Equal(2, pruned[0].DuplicateCount);

            int keptRarity = BerryTable.Get(pruned[0].Recipe.Berries[0]).Rarity;
            Assert.Equal(minRarity, keptRarity);
            Assert.True(minRarity <= maxRarity);
        }
    }
}
