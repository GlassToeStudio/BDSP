using System;
using System.Collections.Generic;
using BDSP.Core.Berries;
using BDSP.Core.Optimization.Core;
using BDSP.Core.Optimization.Search;
using BDSP.Core.Poffins;

namespace BDSP.Core.Optimization.Filters
{
    /// <summary>
    /// Prunes dominated poffins before feeding-plan search.
    /// </summary>
    public static class FeedingCandidatePruner
    {
        /// <summary>
        /// Removes candidates that are strictly dominated by another candidate.
        /// A dominates B if all flavors are &gt;=, smoothness &lt;=, rarity cost &lt;=,
        /// and at least one comparison is strict. Duplicate stat sets are collapsed first.
        /// </summary>
        public static PoffinWithRecipe[] Prune(
            ReadOnlySpan<PoffinWithRecipe> candidates,
            RarityCostMode rarityCostMode = RarityCostMode.MaxBerryRarity)
        {
            if (candidates.Length == 0)
            {
                return Array.Empty<PoffinWithRecipe>();
            }

            PoffinWithRecipe[] deduped = Deduplicate(candidates, rarityCostMode);
            if (deduped.Length == 0)
            {
                return Array.Empty<PoffinWithRecipe>();
            }

            var rarityCosts = new int[deduped.Length];
            for (int i = 0; i < deduped.Length; i++)
            {
                rarityCosts[i] = ComputeRarityCost(in deduped[i].Recipe, rarityCostMode);
            }

            var dominated = new bool[deduped.Length];
            for (int i = 0; i < deduped.Length; i++)
            {
                if (dominated[i])
                {
                    continue;
                }

                for (int j = 0; j < deduped.Length; j++)
                {
                    if (i == j || dominated[j])
                    {
                        continue;
                    }

                    if (Dominates(in deduped[j], rarityCosts[j], in deduped[i], rarityCosts[i]))
                    {
                        dominated[i] = true;
                        break;
                    }
                }
            }

            int keepCount = 0;
            for (int i = 0; i < dominated.Length; i++)
            {
                if (!dominated[i]) keepCount++;
            }

            var results = new PoffinWithRecipe[keepCount];
            int index = 0;
            for (int i = 0; i < deduped.Length; i++)
            {
                if (!dominated[i])
                {
                    results[index] = deduped[i];
                    index++;
                }
            }

            return results;
        }

        private static bool Dominates(
            in PoffinWithRecipe better,
            int betterRarity,
            in PoffinWithRecipe worse,
            int worseRarity)
        {
            var b = better.Poffin;
            var w = worse.Poffin;

            if (b.Spicy < w.Spicy) return false;
            if (b.Dry < w.Dry) return false;
            if (b.Sweet < w.Sweet) return false;
            if (b.Bitter < w.Bitter) return false;
            if (b.Sour < w.Sour) return false;
            if (b.Smoothness > w.Smoothness) return false;
            if (betterRarity > worseRarity) return false;

            bool strict =
                b.Spicy > w.Spicy ||
                b.Dry > w.Dry ||
                b.Sweet > w.Sweet ||
                b.Bitter > w.Bitter ||
                b.Sour > w.Sour ||
                b.Smoothness < w.Smoothness ||
                betterRarity < worseRarity;

            return strict;
        }

        private static int ComputeRarityCost(in PoffinRecipe recipe, RarityCostMode mode)
        {
            int cost = 0;
            for (int i = 0; i < recipe.Berries.Length; i++)
            {
                ref readonly var berry = ref BerryTable.Get(recipe.Berries[i]);
                int rarity = berry.Rarity;
                if (mode == RarityCostMode.MaxBerryRarity)
                {
                    if (rarity > cost) cost = rarity;
                }
                else
                {
                    cost += rarity;
                }
            }
            return cost;
        }

        private static PoffinWithRecipe[] Deduplicate(
            ReadOnlySpan<PoffinWithRecipe> candidates,
            RarityCostMode rarityCostMode)
        {
            var unique = new Dictionary<PoffinKey, DedupEntry>(candidates.Length);

            for (int i = 0; i < candidates.Length; i++)
            {
                PoffinWithRecipe candidate = candidates[i];
                int rarity = ComputeRarityCost(in candidate.Recipe, rarityCostMode);
                var key = new PoffinKey(in candidate.Poffin);

                if (!unique.TryGetValue(key, out DedupEntry entry))
                {
                    unique.Add(key, new DedupEntry(candidate, rarity, 1));
                    continue;
                }

                entry.DuplicateCount++;
                if (rarity < entry.BestRarityCost)
                {
                    entry.Best = candidate;
                    entry.BestRarityCost = rarity;
                }
                unique[key] = entry;
            }

            if (unique.Count == candidates.Length)
            {
                return candidates.ToArray();
            }

            var results = new PoffinWithRecipe[unique.Count];
            int index = 0;
            foreach (DedupEntry entry in unique.Values)
            {
                results[index] = new PoffinWithRecipe(entry.Best.Poffin, entry.Best.Recipe, entry.DuplicateCount);
                index++;
            }

            return results;
        }

        private struct DedupEntry
        {
            public PoffinWithRecipe Best;
            public int BestRarityCost;
            public int DuplicateCount;

            public DedupEntry(PoffinWithRecipe best, int bestRarityCost, int duplicateCount)
            {
                Best = best;
                BestRarityCost = bestRarityCost;
                DuplicateCount = duplicateCount;
            }
        }

        private readonly struct PoffinKey : IEquatable<PoffinKey>
        {
            private readonly byte _spicy;
            private readonly byte _dry;
            private readonly byte _sweet;
            private readonly byte _bitter;
            private readonly byte _sour;
            private readonly byte _smoothness;
            private readonly byte _level;
            private readonly byte _numFlavors;
            private readonly Flavor _mainFlavor;
            private readonly Flavor _secondaryFlavor;

            public PoffinKey(in Poffin poffin)
            {
                _spicy = poffin.Spicy;
                _dry = poffin.Dry;
                _sweet = poffin.Sweet;
                _bitter = poffin.Bitter;
                _sour = poffin.Sour;
                _smoothness = poffin.Smoothness;
                _level = poffin.Level;
                _numFlavors = poffin.NumFlavors;
                _mainFlavor = poffin.MainFlavor;
                _secondaryFlavor = poffin.SecondaryFlavor;
            }

            public bool Equals(PoffinKey other)
            {
                return _spicy == other._spicy &&
                       _dry == other._dry &&
                       _sweet == other._sweet &&
                       _bitter == other._bitter &&
                       _sour == other._sour &&
                       _smoothness == other._smoothness &&
                       _level == other._level &&
                       _numFlavors == other._numFlavors &&
                       _mainFlavor == other._mainFlavor &&
                       _secondaryFlavor == other._secondaryFlavor;
            }

            public override bool Equals(object? obj)
            {
                return obj is PoffinKey other && Equals(other);
            }

            public override int GetHashCode()
            {
                int hash = _spicy;
                hash = (hash * 397) ^ _dry;
                hash = (hash * 397) ^ _sweet;
                hash = (hash * 397) ^ _bitter;
                hash = (hash * 397) ^ _sour;
                hash = (hash * 397) ^ _smoothness;
                hash = (hash * 397) ^ _level;
                hash = (hash * 397) ^ _numFlavors;
                hash = (hash * 397) ^ (int)_mainFlavor;
                hash = (hash * 397) ^ (int)_secondaryFlavor;
                return hash;
            }
        }
    }
}
