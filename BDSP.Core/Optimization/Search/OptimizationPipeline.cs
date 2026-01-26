using System;
using System.Collections.Generic;
using BDSP.Core.Berries;
using BDSP.Core.Optimization.Core;
using BDSP.Core.Optimization.Filters;
using BDSP.Core.Poffins;
using BDSP.Core.Poffins.Cooking;
using BDSP.Core.Poffins.Enumeration;
using BDSP.Core.Poffins.Filters;
using BDSP.Core.Poffins.Search;

namespace BDSP.Core.Optimization.Search
{
    /// <summary>
    /// Orchestrates end-to-end workflows from berries to feeding plans or contest stats.
    /// </summary>
    public static class OptimizationPipeline
    {
        /// <summary>
        /// Builds top-ranked poffin candidates (with recipes) from a berry filter.
        /// </summary>
        public static PoffinWithRecipe[] BuildCandidates(
            in BerryFilterOptions berryOptions,
            in PoffinCandidateOptions candidateOptions,
            int topK,
            bool dedup = true)
        {
            if (topK <= 0)
            {
                return Array.Empty<PoffinWithRecipe>();
            }

            Span<Berry> buffer = stackalloc Berry[BerryTable.Count];
            int count = BerryQuery.Execute(BerryTable.All, buffer, berryOptions, default);
            if (count < 2)
            {
                return Array.Empty<PoffinWithRecipe>();
            }

            BerryId[] ids = new BerryId[count];
            for (int i = 0; i < count; i++)
            {
                ids[i] = buffer[i].Id;
            }

            if (dedup)
            {
                var unique = new Dictionary<PoffinKey, Candidate>(topK);
                AddCandidates(ids, in candidateOptions, unique);

                var top = new TopK<Candidate>(topK);
                foreach (var kvp in unique)
                {
                    top.TryAdd(kvp.Value, kvp.Value.Score);
                }

                Candidate[] winners = top.ToSortedArray((a, b) => b.Score.CompareTo(a.Score));
                return ExtractCandidates(winners);
            }

            var collector = new TopK<Candidate>(topK);
            AddCandidates(ids, in candidateOptions, collector);
            Candidate[] results = collector.ToSortedArray((a, b) => b.Score.CompareTo(a.Score));
            return ExtractCandidates(results);
        }

        /// <summary>
        /// Runs a feeding plan search from berries with candidate generation.
        /// </summary>
        public static FeedingPlanResult RunFeedingPlan(
            in BerryFilterOptions berryOptions,
            in PoffinCandidateOptions candidateOptions,
            int candidateTopK,
            in FeedingSearchOptions searchOptions,
            ContestStats start = default,
            bool dedup = true)
        {
            PoffinWithRecipe[] candidates = BuildCandidates(in berryOptions, in candidateOptions, candidateTopK, dedup);
            if (candidates.Length == 0)
            {
                return new FeedingPlanResult(
                    Array.Empty<FeedingStep>(),
                    start,
                    numPerfectValues: 0,
                    rank: 3,
                    uniqueBerries: 0,
                    totalRarityCost: 0,
                    totalPoffins: 0,
                    totalSheen: start.Sheen,
                    score: 0);
            }

            return FeedingSearch.BuildPlan(candidates, in searchOptions, start);
        }

        /// <summary>
        /// Runs a contest-stat search from berries with candidate generation.
        /// </summary>
        public static ContestStatsResult[] RunContestSearch(
            in BerryFilterOptions berryOptions,
            in PoffinCandidateOptions candidateOptions,
            int candidateTopK,
            in ContestStatsSearchOptions contestOptions,
            in FeedingSearchOptions scoringOptions,
            int topK = 50,
            bool dedup = true)
        {
            PoffinWithRecipe[] candidates = BuildCandidates(in berryOptions, in candidateOptions, candidateTopK, dedup);
            if (candidates.Length == 0)
            {
                return Array.Empty<ContestStatsResult>();
            }

            return ContestStatsSearch.Run(candidates, in contestOptions, in scoringOptions, topK);
        }

        private static void AddCandidates(
            ReadOnlySpan<BerryId> ids,
            in PoffinCandidateOptions options,
            TopK<Candidate> top)
        {
            PoffinCandidateOptions optionsValue = options;
            for (int c = 0; c < optionsValue.ChooseList.Length; c++)
            {
                int choose = optionsValue.ChooseList[c];
                if (choose < 2 || choose > 4)
                {
                    continue;
                }
                AddCandidatesForChoose(ids, choose, in optionsValue, top);
            }
        }

        private static void AddCandidates(
            ReadOnlySpan<BerryId> ids,
            in PoffinCandidateOptions options,
            Dictionary<PoffinKey, Candidate> unique)
        {
            PoffinCandidateOptions optionsValue = options;
            for (int c = 0; c < optionsValue.ChooseList.Length; c++)
            {
                int choose = optionsValue.ChooseList[c];
                if (choose < 2 || choose > 4)
                {
                    continue;
                }
                AddCandidatesForChoose(ids, choose, in optionsValue, unique);
            }
        }

        private static void AddCandidatesForChoose(
            ReadOnlySpan<BerryId> ids,
            int choose,
            in PoffinCandidateOptions options,
            TopK<Candidate> top)
        {
            PoffinCandidateOptions optionsValue = options;
            PoffinComboEnumerator.ForEach(ids, choose, combo =>
            {
                Span<BerryBase> bases = stackalloc BerryBase[choose];
                var recipeIds = new BerryId[choose];
                for (int i = 0; i < choose; i++)
                {
                    recipeIds[i] = combo[i];
                    bases[i] = BerryTable.GetBase(combo[i]);
                }

                Poffin poffin = PoffinCooker.Cook(
                    bases,
                    optionsValue.CookTimeSeconds,
                    optionsValue.Spills,
                    optionsValue.Burns,
                    optionsValue.AmityBonus);

                if (!PoffinFilter.Matches(in poffin, in optionsValue.FilterOptions))
                {
                    return;
                }

                int score = Score(in poffin, in optionsValue.ScoreOptions);
                var recipe = new PoffinRecipe(recipeIds, optionsValue.CookTimeSeconds, optionsValue.Spills, optionsValue.Burns, optionsValue.AmityBonus);
                var candidate = new Candidate(new PoffinWithRecipe(poffin, recipe), score);
                top.TryAdd(candidate, score);
            });
        }

        private static void AddCandidatesForChoose(
            ReadOnlySpan<BerryId> ids,
            int choose,
            in PoffinCandidateOptions options,
            Dictionary<PoffinKey, Candidate> unique)
        {
            PoffinCandidateOptions optionsValue = options;
            PoffinComboEnumerator.ForEach(ids, choose, combo =>
            {
                Span<BerryBase> bases = stackalloc BerryBase[choose];
                var recipeIds = new BerryId[choose];
                for (int i = 0; i < choose; i++)
                {
                    recipeIds[i] = combo[i];
                    bases[i] = BerryTable.GetBase(combo[i]);
                }

                Poffin poffin = PoffinCooker.Cook(
                    bases,
                    optionsValue.CookTimeSeconds,
                    optionsValue.Spills,
                    optionsValue.Burns,
                    optionsValue.AmityBonus);

                if (!PoffinFilter.Matches(in poffin, in optionsValue.FilterOptions))
                {
                    return;
                }

                int score = Score(in poffin, in optionsValue.ScoreOptions);
                var recipe = new PoffinRecipe(recipeIds, optionsValue.CookTimeSeconds, optionsValue.Spills, optionsValue.Burns, optionsValue.AmityBonus);
                var candidate = new Candidate(new PoffinWithRecipe(poffin, recipe), score);
                var key = new PoffinKey(in poffin);

                if (unique.TryGetValue(key, out Candidate existing))
                {
                    int duplicateCount = existing.Value.DuplicateCount + 1;
                    PoffinWithRecipe best =
                        score > existing.Score
                            ? new PoffinWithRecipe(poffin, recipe, duplicateCount)
                            : new PoffinWithRecipe(existing.Value.Poffin, existing.Value.Recipe, duplicateCount);
                    int bestScore = score > existing.Score ? score : existing.Score;
                    unique[key] = new Candidate(best, bestScore);
                    return;
                }

                unique[key] = candidate;
            });
        }

        private static PoffinWithRecipe[] ExtractCandidates(Candidate[] candidates)
        {
            var results = new PoffinWithRecipe[candidates.Length];
            for (int i = 0; i < candidates.Length; i++)
            {
                results[i] = candidates[i].Value;
            }
            return results;
        }

        private static int Score(in Poffin poffin, in PoffinScoreOptions options)
        {
            int totalFlavor = poffin.Spicy + poffin.Dry + poffin.Sweet + poffin.Bitter + poffin.Sour;
            int score = poffin.Level * options.LevelWeight;
            score += totalFlavor * options.TotalFlavorWeight;
            score -= poffin.Smoothness * options.SmoothnessPenalty;
            if (options.PreferredMainFlavor != Flavor.None && poffin.MainFlavor == options.PreferredMainFlavor)
            {
                score += options.PreferredMainFlavorBonus;
            }
            return score;
        }

        private readonly struct Candidate
        {
            public readonly PoffinWithRecipe Value;
            public readonly int Score;

            public Candidate(PoffinWithRecipe value, int score)
            {
                Value = value;
                Score = score;
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
            private readonly byte _secondLevel;
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
                _secondLevel = poffin.SecondLevel;
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
                       _secondLevel == other._secondLevel &&
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
                hash = (hash * 397) ^ _secondLevel;
                hash = (hash * 397) ^ _numFlavors;
                hash = (hash * 397) ^ (int)_mainFlavor;
                hash = (hash * 397) ^ (int)_secondaryFlavor;
                return hash;
            }
        }
    }

    /// <summary>
    /// Options for building poffin candidates from berries.
    /// </summary>
    public readonly struct PoffinCandidateOptions
    {
        /// <summary>Berry counts to include (2-4).</summary>
        public readonly int[] ChooseList;
        /// <summary>Cook time in seconds.</summary>
        public readonly int CookTimeSeconds;
        /// <summary>Number of spills during cooking.</summary>
        public readonly int Spills;
        /// <summary>Number of burns during cooking.</summary>
        public readonly int Burns;
        /// <summary>Amity bonus reduction (BDSP cap is 9).</summary>
        public readonly int AmityBonus;
        /// <summary>Scoring preferences for candidate ranking.</summary>
        public readonly PoffinScoreOptions ScoreOptions;
        /// <summary>Optional filter for candidate poffins.</summary>
        public readonly PoffinFilterOptions FilterOptions;

        public PoffinCandidateOptions(
            int[]? chooseList = null,
            int cookTimeSeconds = 40,
            int spills = 0,
            int burns = 0,
            int amityBonus = 9,
            PoffinScoreOptions scoreOptions = default,
            PoffinFilterOptions filterOptions = default)
        {
            ChooseList = chooseList ?? new[] { 2, 3, 4 };
            CookTimeSeconds = cookTimeSeconds;
            Spills = spills;
            Burns = burns;
            AmityBonus = amityBonus;
            ScoreOptions = scoreOptions;
            FilterOptions = filterOptions;
        }
    }
}
