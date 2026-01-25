using System;
using BenchmarkDotNet.Attributes;
using BDSP.Core.Berries;
using BDSP.Core.Optimization.Core;
using BDSP.Core.Optimization.Filters;
using BDSP.Core.Poffins;
using BDSP.Core.Optimization.Search;

namespace BDSP.Core.Benchmarks
{
    [MemoryDiagnoser]
    public class DedupPruneBenchmarks
    {
        private PoffinWithRecipe[] _candidates = Array.Empty<PoffinWithRecipe>();

        [Params(1, 2, 4)]
        public int DuplicateFactor { get; set; } = 1;

        [Params(1_000, 10_000)]
        public int TotalCount { get; set; } = 1_000;

        [GlobalSetup]
        public void Setup()
        {
            int uniqueCount = Math.Max(1, TotalCount / DuplicateFactor);
            var uniques = new PoffinWithRecipe[uniqueCount];

            ref readonly var berryA = ref BerryTable.Get(new BerryId(0));
            ref readonly var berryB = ref BerryTable.Get(new BerryId(1));
            var recipeIds = new[] { berryA.Id, berryB.Id };

            var rng = new Random(12345);
            for (int i = 0; i < uniqueCount; i++)
            {
                byte spicy = (byte)rng.Next(0, 101);
                byte dry = (byte)rng.Next(0, 101);
                byte sweet = (byte)rng.Next(0, 101);
                byte bitter = (byte)rng.Next(0, 101);
                byte sour = (byte)rng.Next(0, 101);
                byte smoothness = (byte)rng.Next(0, 256);

                var poffin = new Poffin(spicy, dry, sweet, bitter, sour, smoothness, isFoul: false);
                var recipe = new PoffinRecipe(recipeIds, cookTimeSeconds: 40, spills: 0, burns: 0, amityBonus: 9);
                uniques[i] = new PoffinWithRecipe(poffin, recipe);
            }

            _candidates = new PoffinWithRecipe[TotalCount];
            for (int i = 0; i < TotalCount; i++)
            {
                _candidates[i] = uniques[i % uniqueCount];
            }
        }

        [Benchmark]
        public PoffinWithRecipe[] PruneCandidates()
        {
            return FeedingCandidatePruner.Prune(_candidates, RarityCostMode.MaxBerryRarity);
        }
    }
}
