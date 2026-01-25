using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using BDSP.Core.Berries;
using BDSP.Core.Poffins;
using BDSP.Core.Poffins.Cooking;
using BDSP.Core.Poffins.Filters;

namespace BDSP.Core.Poffins.Search
{
    /// <summary>
    /// Unified entry point for cooking poffins from either all berries or a filtered subset.
    /// </summary>
    public static class PoffinSearch
    {
        /// <summary>
        /// Runs a poffin search with an optional berry filter and optional poffin filter.
        /// The call shape is the same regardless of whether the user filtered berries.
        /// </summary>
        public static PoffinResult[] Run(in BerryFilterOptions berryOptions, in PoffinSearchOptions options, int topK = 100, in PoffinFilterOptions poffinOptions = default)
        {
            if (options.Choose < 2 || options.Choose > 4)
            {
                throw new ArgumentOutOfRangeException(nameof(options.Choose), "Choose must be 2, 3, or 4.");
            }

            var collector = new TopK<PoffinResult>(topK);

            if (IsAllBerries(in berryOptions) && options.UseComboTableWhenAllBerries)
            {
                RunFromComboTable(in options, in poffinOptions, topK, collector);
            }
            else
            {
                RunFromSubset(in berryOptions, in options, in poffinOptions, topK, collector);
            }

            return collector.ToSortedArray(CompareResults);
        }

        private static void RunFromComboTable(in PoffinSearchOptions options, in PoffinFilterOptions poffinOptions, int topK, TopK<PoffinResult> collector)
        {
            PoffinComboBase[] combos = PoffinComboTable.All.ToArray();
            int start;
            int end;
            GetComboRange(options.Choose, out start, out end);

            bool shouldParallel = options.UseParallel && ShouldParallelForAll(options.Choose);
            if (!shouldParallel)
            {
                for (int i = start; i < end; i++)
                {
                    Poffin poffin = PoffinCooker.Cook(
                        combos[i],
                        options.CookTimeSeconds,
                        options.Spills,
                        options.Burns,
                        options.AmityBonus);

                    if (!Matches(in poffin, in poffinOptions))
                    {
                        continue;
                    }

                    int score = Score(in poffin, in options.ScoreOptions);
                    collector.TryAdd(new PoffinResult(poffin, options.Choose, score), score);
                }
                return;
            }

            ParallelOptions parallelOptions = CreateParallelOptions(in options);
            object gate = new object();
            int cookTimeSeconds = options.CookTimeSeconds;
            int spills = options.Spills;
            int burns = options.Burns;
            int amityBonus = options.AmityBonus;
            PoffinFilterOptions poffinOptionsValue = poffinOptions;
            PoffinScoreOptions scoreOptions = options.ScoreOptions;
            int berryCount = options.Choose;

            Parallel.For(
                start,
                end,
                parallelOptions,
                () => new TopK<PoffinResult>(topK),
                (i, _, local) =>
                {
                    Poffin poffin = PoffinCooker.Cook(
                        combos[i],
                        cookTimeSeconds,
                        spills,
                        burns,
                        amityBonus);

                    if (Matches(in poffin, in poffinOptionsValue))
                    {
                        int score = Score(in poffin, in scoreOptions);
                        local.TryAdd(new PoffinResult(poffin, berryCount, score), score);
                    }

                    return local;
                },
                local =>
                {
                    lock (gate)
                    {
                        collector.MergeFrom(local);
                    }
                });
        }

        private static void RunFromSubset(in BerryFilterOptions berryOptions, in PoffinSearchOptions options, in PoffinFilterOptions poffinOptions, int topK, TopK<PoffinResult> collector)
        {
            Span<Berry> buffer = stackalloc Berry[BerryTable.Count];
            int count = BerryQuery.Execute(BerryTable.All, buffer, berryOptions, default);
            if (count <= 1)
            {
                return;
            }

            BerryId[] ids = new BerryId[count];
            for (int i = 0; i < count; i++)
            {
                ids[i] = buffer[i].Id;
            }

            bool shouldParallel = options.UseParallel && ShouldParallelForSubset(options.Choose, count);
            if (!shouldParallel)
            {
                CookSubsetSequential(ids, in options, in poffinOptions, collector);
                return;
            }

            CookSubsetParallel(ids, in options, in poffinOptions, topK, collector);
        }

        private static void CookSubsetSequential(ReadOnlySpan<BerryId> ids, in PoffinSearchOptions options, in PoffinFilterOptions poffinOptions, TopK<PoffinResult> collector)
        {
            ReadOnlySpan<BerryBase> bases = BerryTable.BaseAll;

            if (options.Choose == 2)
            {
                Span<BerryBase> buffer = stackalloc BerryBase[2];
                for (int i = 0; i < ids.Length - 1; i++)
                {
                    for (int j = i + 1; j < ids.Length; j++)
                    {
                        buffer[0] = bases[ids[i].Value];
                        buffer[1] = bases[ids[j].Value];
                        Poffin poffin = PoffinCooker.Cook(
                            buffer,
                            options.CookTimeSeconds,
                            options.Spills,
                            options.Burns,
                            options.AmityBonus);
                        AddIfMatch(in poffin, in options, in poffinOptions, collector);
                    }
                }
                return;
            }

            if (options.Choose == 3)
            {
                Span<BerryBase> buffer = stackalloc BerryBase[3];
                for (int i = 0; i < ids.Length - 2; i++)
                {
                    for (int j = i + 1; j < ids.Length - 1; j++)
                    {
                        for (int k = j + 1; k < ids.Length; k++)
                        {
                            buffer[0] = bases[ids[i].Value];
                            buffer[1] = bases[ids[j].Value];
                            buffer[2] = bases[ids[k].Value];
                            Poffin poffin = PoffinCooker.Cook(
                                buffer,
                                options.CookTimeSeconds,
                                options.Spills,
                                options.Burns,
                                options.AmityBonus);
                            AddIfMatch(in poffin, in options, in poffinOptions, collector);
                        }
                    }
                }
                return;
            }

            Span<BerryBase> buffer4 = stackalloc BerryBase[4];
            for (int i = 0; i < ids.Length - 3; i++)
            {
                for (int j = i + 1; j < ids.Length - 2; j++)
                {
                    for (int k = j + 1; k < ids.Length - 1; k++)
                    {
                        for (int l = k + 1; l < ids.Length; l++)
                        {
                            buffer4[0] = bases[ids[i].Value];
                            buffer4[1] = bases[ids[j].Value];
                            buffer4[2] = bases[ids[k].Value];
                            buffer4[3] = bases[ids[l].Value];
                            Poffin poffin = PoffinCooker.Cook(
                                buffer4,
                                options.CookTimeSeconds,
                                options.Spills,
                                options.Burns,
                                options.AmityBonus);
                            AddIfMatch(in poffin, in options, in poffinOptions, collector);
                        }
                    }
                }
            }
        }

        private static void CookSubsetParallel(BerryId[] ids, in PoffinSearchOptions options, in PoffinFilterOptions poffinOptions, int topK, TopK<PoffinResult> collector)
        {
            BerryBase[] bases = BerryTable.BaseAll.ToArray();
            ParallelOptions parallelOptions = CreateParallelOptions(in options);
            object gate = new object();
            int cookTimeSeconds = options.CookTimeSeconds;
            int spills = options.Spills;
            int burns = options.Burns;
            int amityBonus = options.AmityBonus;
            PoffinSearchOptions optionsValue = options;
            PoffinFilterOptions poffinOptionsValue = poffinOptions;

            if (options.Choose == 2)
            {
                Parallel.For(
                    0,
                    ids.Length - 1,
                    parallelOptions,
                    () => new LocalCookBuffer(2, topK),
                    (i, _, local) =>
                    {
                        for (int j = i + 1; j < ids.Length; j++)
                        {
                            local.Buffer[0] = bases[ids[i].Value];
                            local.Buffer[1] = bases[ids[j].Value];
                            Poffin poffin = PoffinCooker.Cook(
                                local.Buffer,
                                cookTimeSeconds,
                                spills,
                                burns,
                                amityBonus);
                            AddIfMatch(in poffin, in optionsValue, in poffinOptionsValue, local.TopK);
                        }
                        return local;
                    },
                    local =>
                    {
                        lock (gate)
                        {
                            collector.MergeFrom(local.TopK);
                        }
                    });
                return;
            }

            if (options.Choose == 3)
            {
                Parallel.For(
                    0,
                    ids.Length - 2,
                    parallelOptions,
                    () => new LocalCookBuffer(3, topK),
                    (i, _, local) =>
                    {
                        for (int j = i + 1; j < ids.Length - 1; j++)
                        {
                            for (int k = j + 1; k < ids.Length; k++)
                            {
                                local.Buffer[0] = bases[ids[i].Value];
                                local.Buffer[1] = bases[ids[j].Value];
                                local.Buffer[2] = bases[ids[k].Value];
                                Poffin poffin = PoffinCooker.Cook(
                                    local.Buffer,
                                    cookTimeSeconds,
                                    spills,
                                    burns,
                                    amityBonus);
                                AddIfMatch(in poffin, in optionsValue, in poffinOptionsValue, local.TopK);
                            }
                        }
                        return local;
                    },
                    local =>
                    {
                        lock (gate)
                        {
                            collector.MergeFrom(local.TopK);
                        }
                    });
                return;
            }

            Parallel.For(
                0,
                ids.Length - 3,
                parallelOptions,
                () => new LocalCookBuffer(4, topK),
                (i, _, local) =>
                {
                    for (int j = i + 1; j < ids.Length - 2; j++)
                    {
                        for (int k = j + 1; k < ids.Length - 1; k++)
                        {
                            for (int l = k + 1; l < ids.Length; l++)
                            {
                                local.Buffer[0] = bases[ids[i].Value];
                                local.Buffer[1] = bases[ids[j].Value];
                                local.Buffer[2] = bases[ids[k].Value];
                                local.Buffer[3] = bases[ids[l].Value];
                                Poffin poffin = PoffinCooker.Cook(
                                    local.Buffer,
                                    cookTimeSeconds,
                                    spills,
                                    burns,
                                    amityBonus);
                                AddIfMatch(in poffin, in optionsValue, in poffinOptionsValue, local.TopK);
                            }
                        }
                    }
                    return local;
                },
                local =>
                {
                    lock (gate)
                    {
                        collector.MergeFrom(local.TopK);
                    }
                });
        }

        private static void AddIfMatch(in Poffin poffin, in PoffinSearchOptions options, in PoffinFilterOptions poffinOptions, TopK<PoffinResult> collector)
        {
            if (!Matches(in poffin, in poffinOptions))
            {
                return;
            }

            int score = Score(in poffin, in options.ScoreOptions);
            collector.TryAdd(new PoffinResult(poffin, options.Choose, score), score);
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

        private static bool Matches(in Poffin poffin, in PoffinFilterOptions o)
        {
            if (!InRange(poffin.Spicy, o.MinSpicy, o.MaxSpicy, o.Mask, PoffinFilterMask.MinSpicy, PoffinFilterMask.MaxSpicy)) return false;
            if (!InRange(poffin.Dry, o.MinDry, o.MaxDry, o.Mask, PoffinFilterMask.MinDry, PoffinFilterMask.MaxDry)) return false;
            if (!InRange(poffin.Sweet, o.MinSweet, o.MaxSweet, o.Mask, PoffinFilterMask.MinSweet, PoffinFilterMask.MaxSweet)) return false;
            if (!InRange(poffin.Bitter, o.MinBitter, o.MaxBitter, o.Mask, PoffinFilterMask.MinBitter, PoffinFilterMask.MaxBitter)) return false;
            if (!InRange(poffin.Sour, o.MinSour, o.MaxSour, o.Mask, PoffinFilterMask.MinSour, PoffinFilterMask.MaxSour)) return false;
            if (!InRange(poffin.Smoothness, o.MinSmoothness, o.MaxSmoothness, o.Mask, PoffinFilterMask.MinSmoothness, PoffinFilterMask.MaxSmoothness)) return false;
            if (!InRange(poffin.Level, o.MinLevel, o.MaxLevel, o.Mask, PoffinFilterMask.MinLevel, PoffinFilterMask.MaxLevel)) return false;
            if (!InRange(poffin.NumFlavors, o.MinNumFlavors, o.MaxNumFlavors, o.Mask, PoffinFilterMask.MinNumFlavors, PoffinFilterMask.MaxNumFlavors)) return false;
            if (o.RequireMainFlavor && poffin.MainFlavor != o.MainFlavor) return false;
            if (o.RequireSecondaryFlavor && poffin.SecondaryFlavor != o.SecondaryFlavor) return false;
            return true;
        }

        private static bool InRange(byte value, int min, int max, PoffinFilterMask mask, PoffinFilterMask minFlag, PoffinFilterMask maxFlag)
        {
            if ((mask & minFlag) != 0 && value < min) return false;
            if ((mask & maxFlag) != 0 && value > max) return false;
            return true;
        }

        private static bool IsAllBerries(in BerryFilterOptions options)
        {
            return options.Mask == BerryFilterMask.None &&
                   !options.RequireMainFlavor &&
                   !options.RequireSecondaryFlavor &&
                   options.RequiredFlavorMask == 0 &&
                   options.ExcludedFlavorMask == 0;
        }

        private static void GetComboRange(int choose, out int start, out int end)
        {
            int n = BerryTable.Count;
            int count2 = Choose(n, 2);
            int count3 = Choose(n, 3);
            if (choose == 2)
            {
                start = 0;
                end = count2;
                return;
            }

            if (choose == 3)
            {
                start = count2;
                end = count2 + count3;
                return;
            }

            start = count2 + count3;
            end = start + Choose(n, 4);
        }

        private static int Choose(int n, int k)
        {
            if (k == 2) return n * (n - 1) / 2;
            if (k == 3) return n * (n - 1) * (n - 2) / 6;
            if (k == 4) return n * (n - 1) * (n - 2) * (n - 3) / 24;
            return 0;
        }

        private static ParallelOptions CreateParallelOptions(in PoffinSearchOptions options)
        {
            var parallelOptions = new ParallelOptions();
            if (options.MaxDegreeOfParallelism.HasValue)
            {
                parallelOptions.MaxDegreeOfParallelism = options.MaxDegreeOfParallelism.Value;
            }
            return parallelOptions;
        }

        private static bool ShouldParallelForAll(int choose)
        {
            return choose >= 2 && choose <= 4;
        }

        private static bool ShouldParallelForSubset(int choose, int subsetSize)
        {
            int combos = Choose(subsetSize, choose);
            return combos >= 500;
        }

        private static int CompareResults(PoffinResult left, PoffinResult right)
        {
            int score = right.Score.CompareTo(left.Score);
            if (score != 0) return score;

            int level = right.Poffin.Level.CompareTo(left.Poffin.Level);
            if (level != 0) return level;

            int smooth = left.Poffin.Smoothness.CompareTo(right.Poffin.Smoothness);
            if (smooth != 0) return smooth;

            int totalLeft = left.Poffin.Spicy + left.Poffin.Dry + left.Poffin.Sweet + left.Poffin.Bitter + left.Poffin.Sour;
            int totalRight = right.Poffin.Spicy + right.Poffin.Dry + right.Poffin.Sweet + right.Poffin.Bitter + right.Poffin.Sour;
            return totalRight.CompareTo(totalLeft);
        }

        private sealed class LocalCookBuffer
        {
            public readonly BerryBase[] Buffer;
            public readonly TopK<PoffinResult> TopK;

            public LocalCookBuffer(int size, int topKCapacity)
            {
                Buffer = new BerryBase[size];
                TopK = new TopK<PoffinResult>(topKCapacity);
            }
        }
    }

    /// <summary>
    /// Lightweight result wrapper for ranking and display.
    /// </summary>
    public readonly struct PoffinResult
    {
        public readonly Poffin Poffin;
        public readonly int BerryCount;
        public readonly int Score;

        public PoffinResult(Poffin poffin, int berryCount, int score)
        {
            Poffin = poffin;
            BerryCount = berryCount;
            Score = score;
        }
    }
}
