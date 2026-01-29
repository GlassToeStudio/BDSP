using System;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using BDSP.Core.Berries;
using BDSP.Core.CLI;
using BDSP.Core.Optimization.Core;
using BDSP.Core.Optimization.Search;
using BDSP.Core.Poffins;
using BDSP.Core.Poffins.Filters;
using BDSP.Core.Poffins.Search;


internal class Program
{
    private static async Task Main(string[] args)
    {
       
        //Console.WriteLine(ImageToConsole.GetImageString(1));
        Console.WriteLine(ImageToConsole.GetImageString(14));
        return;

        if (args.Length > 0 && args[0].Equals("feeding-plan", StringComparison.OrdinalIgnoreCase))
        {
            RunFeedingPlan(args);
            return;
        }
        if (args.Length > 0 && args[0].Equals("contest-search", StringComparison.OrdinalIgnoreCase))
        {
            RunContestSearch(args);
            return;
        }
        if (args.Length > 0 && args[0].Equals("award-sample", StringComparison.OrdinalIgnoreCase))
        {
            RunAwardSample(args);
            return;
        }

        for (ushort i = 0; i < 65; i++)
        {
            ref readonly var berry = ref BerryTable.Get(new BerryId(i));
            Console.WriteLine(berry);
            Console.WriteLine(BerryAnsiFormatter.Format(berry));
        }

        return;

        static void RunFeedingPlan(string[] args)
        {
            int choose = GetIntArg(args, "--choose", 4);
            int cookTimeSeconds = GetIntArg(args, "--time", 40);
            int topK = GetIntArg(args, "--topk", 10);
            string candidateChooseArg = GetStringArg(args, "--candidate-choose", "4");
            bool showProgress = GetBoolArg(args, "--progress", fallback: true);
            string berrySortSpec = GetStringArg(args, "--berry-sort", string.Empty);
            string poffinSortSpec = GetStringArg(args, "--poffin-sort", string.Empty);
            string berryIncludeSpec = GetStringArg(args, "--berry-include", string.Empty);
            string berryExcludeSpec = GetStringArg(args, "--berry-exclude", string.Empty);

            BerryFilterOptions berryOptions = BuildBerryFilters(args);
            PoffinFilterOptions poffinFilter = BuildPoffinFilters(args);
            PoffinScoreOptions poffinScore = BuildPoffinScoreOptions(args);
            FeedingSearchOptions feedingOptions = BuildFeedingOptions(args);
            int minPoffinRarity = GetIntArg(args, "--poffin-min-rarity", -1);
            int maxPoffinRarity = GetIntArg(args, "--poffin-max-rarity", -1);
            int maxSimilar = GetIntArg(args, "--poffin-max-similar", 0);

            var candidateOptions = new PoffinCandidateOptions(
                chooseList: ParseChooseList(candidateChooseArg),
                cookTimeSeconds: cookTimeSeconds,
                scoreOptions: poffinScore,
                filterOptions: poffinFilter,
                minRarityCost: minPoffinRarity,
                maxRarityCost: maxPoffinRarity,
                maxSimilar: maxSimilar);
            BerrySortKey[] berrySortKeys = ParseBerrySortKeys(berrySortSpec);
            PoffinSortKey[] poffinSortKeys = ParsePoffinSortKeys(poffinSortSpec);
            if (showProgress)
            {
                Console.WriteLine("Setting up berry filters...");
            }

            BerryId[]? sortedBerryIds = null;
            BerryId[]? berryInclude = ParseBerryNameList(berryIncludeSpec);
            BerryId[]? berryExclude = ParseBerryNameList(berryExcludeSpec);
            int berryCount;
            if (berryInclude is not null)
            {
                sortedBerryIds = FilterBerryIds(berryInclude, berryExclude, in berryOptions, berrySortKeys);
                berryCount = sortedBerryIds.Length;
            }
            else
            {
                if (berryExclude is not null)
                {
                    var all = GetFilteredBerryIds(in berryOptions, berrySortKeys);
                    sortedBerryIds = FilterBerryIds(all, berryExclude, in berryOptions, berrySortKeys);
                    berryCount = sortedBerryIds.Length;
                }
                else if (berrySortKeys.Length > 0)
                {
                    sortedBerryIds = GetFilteredBerryIds(in berryOptions, berrySortKeys);
                    berryCount = sortedBerryIds.Length;
                }
                else
                {
                    berryCount = CountFilteredBerries(in berryOptions);
                }
            }
            if (showProgress)
            {
                Console.WriteLine($"Filtered berries: {berryCount}");
                long combos = CountCombinations(berryCount, candidateOptions.ChooseList);
                Console.WriteLine($"Combinations (sum nCk): {combos}");
                Console.WriteLine("Cooking candidate poffins...");
            }

            PoffinWithRecipe[] candidates = sortedBerryIds is null
                ? OptimizationPipeline.BuildCandidates(in berryOptions, in candidateOptions, topK, dedup: true)
                : OptimizationPipeline.BuildCandidatesFromIds(sortedBerryIds, in candidateOptions, topK, dedup: true);
            if (poffinSortKeys.Length > 0)
            {
                PoffinSorter.Sort(candidates, candidates.Length, poffinSortKeys);
            }

            if (showProgress)
            {
                Console.WriteLine($"Candidates kept (topK): {candidates.Length}");
                long permutations = CountPermutations(candidates.Length, choose);
                Console.WriteLine($"Permutations (nPk): {permutations}");
                Console.WriteLine("Building feeding plan...");
            }

            FeedingPlanResult plan = FeedingSearch.BuildPlan(candidates, in feedingOptions, start: default);

            Console.WriteLine($"Feeding plan (steps: {plan.TotalPoffins}, sheen: {plan.TotalSheen}, rarity cost: {plan.TotalRarityCost}, unique berries: {plan.UniqueBerries}, perfect: {plan.NumPerfectValues}, rank: {plan.Rank}, score: {plan.Score})");
            Console.WriteLine($"Final stats: Cool {plan.FinalStats.Coolness}, Beauty {plan.FinalStats.Beauty}, Cute {plan.FinalStats.Cuteness}, Clever {plan.FinalStats.Cleverness}, Tough {plan.FinalStats.Toughness}, Sheen {plan.FinalStats.Sheen}");

            for (int i = 0; i < plan.Steps.Length; i++)
            {
                ref readonly var step = ref plan.Steps[i];
                Console.WriteLine(
                    string.Format(
                        CultureInfo.InvariantCulture,
                        "{0,2}: Lvl {1,3} Smooth {2,3} Dup {3,3} Recipe [{4}]",
                        step.Index + 1,
                        step.Poffin.Poffin.Level,
                        step.Poffin.Poffin.Smoothness,
                        step.Poffin.DuplicateCount,
                        string.Join(",", FormatRecipe(step.Poffin.Recipe.Berries))));
            }
        }

        static void RunContestSearch(string[] args)
        {
            int choose = GetIntArg(args, "--choose", 3);
            int cookTimeSeconds = GetIntArg(args, "--time", 40);
            int topK = GetIntArg(args, "--topk", 50);
            int candidateCount = GetIntArg(args, "--candidates", 5000);
            string candidateChooseArg = GetStringArg(args, "--candidate-choose", "4");
            bool useParallel = GetBoolArg(args, "--parallel", fallback: false);
            bool dedup = !GetBoolArg(args, "--no-dedup", fallback: false);
            bool keepDuplicates = GetBoolArg(args, "--keep-duplicates", fallback: false);
            bool pruneCandidates = !GetBoolArg(args, "--no-prune", fallback: false);
            bool showProgress = GetBoolArg(args, "--progress", fallback: false);
            bool showRecipes = GetBoolArg(args, "--show-recipes", fallback: false);
            int showRecipesCount = GetIntArg(args, "--show-recipes-count", 1);
            bool showAward = GetBoolArg(args, "--show-award", fallback: false);
            bool noColor = GetBoolArg(args, "--no-color", fallback: false);
            string berrySortSpec = GetStringArg(args, "--berry-sort", string.Empty);
            string poffinSortSpec = GetStringArg(args, "--poffin-sort", string.Empty);
            string berryIncludeSpec = GetStringArg(args, "--berry-include", string.Empty);
            string berryExcludeSpec = GetStringArg(args, "--berry-exclude", string.Empty);
            ContestScoreMode scoreMode = ParseScoreMode(GetStringArg(args, "--score", "balanced"));
            int maxRank = GetIntArg(args, "--max-rank", -1);
            int maxPoffins = GetIntArg(args, "--max-poffins", -1);

            var chooseList = ParseChooseList(candidateChooseArg);
            BerryFilterOptions berryOptions = BuildBerryFilters(args);
            PoffinFilterOptions poffinFilter = BuildPoffinFilters(args);
            PoffinScoreOptions poffinScore = BuildPoffinScoreOptions(args);
            int minPoffinRarity = GetIntArg(args, "--poffin-min-rarity", -1);
            int maxPoffinRarity = GetIntArg(args, "--poffin-max-rarity", -1);
            int maxSimilar = GetIntArg(args, "--poffin-max-similar", 0);
            var candidateOptions = new PoffinCandidateOptions(
                chooseList: chooseList,
                cookTimeSeconds: cookTimeSeconds,
                scoreOptions: poffinScore,
                filterOptions: poffinFilter,
                minRarityCost: minPoffinRarity,
                maxRarityCost: maxPoffinRarity,
                maxSimilar: maxSimilar);
            BerrySortKey[] berrySortKeys = ParseBerrySortKeys(berrySortSpec);
            PoffinSortKey[] poffinSortKeys = ParsePoffinSortKeys(poffinSortSpec);
            var option = BuildFeedingOptions(args, scoreMode);
            if (keepDuplicates)
            {
                dedup = false;
            }

            var contestOptions = new ContestStatsSearchOptions(
                choose: choose,
                useParallel: useParallel,
                maxPoffins: maxPoffins,
                pruneCandidates: pruneCandidates,
                progress: showProgress ? ReportContestProgress : null,
                progressInterval: 64);

            if (showProgress)
            {
                Console.WriteLine("Setting up berry filters...");
            }

            BerryId[]? sortedBerryIds = null;
            BerryId[]? berryInclude = ParseBerryNameList(berryIncludeSpec);
            BerryId[]? berryExclude = ParseBerryNameList(berryExcludeSpec);
            int berryCount;
            if (berryInclude is not null)
            {
                sortedBerryIds = FilterBerryIds(berryInclude, berryExclude, in berryOptions, berrySortKeys);
                berryCount = sortedBerryIds.Length;
            }
            else
            {
                if (berryExclude is not null)
                {
                    var all = GetFilteredBerryIds(in berryOptions, berrySortKeys);
                    sortedBerryIds = FilterBerryIds(all, berryExclude, in berryOptions, berrySortKeys);
                    berryCount = sortedBerryIds.Length;
                }
                else if (berrySortKeys.Length > 0)
                {
                    sortedBerryIds = GetFilteredBerryIds(in berryOptions, berrySortKeys);
                    berryCount = sortedBerryIds.Length;
                }
                else
                {
                    berryCount = CountFilteredBerries(in berryOptions);
                }
            }
            if (showProgress)
            {
                Console.WriteLine($"Filtered berries: {berryCount}");
                long combos = CountCombinations(berryCount, candidateOptions.ChooseList);
                Console.WriteLine($"Combinations (sum nCk): {combos}");
                Console.WriteLine("Cooking candidate poffins...");
            }

            PoffinWithRecipe[] candidates = sortedBerryIds is null
                ? OptimizationPipeline.BuildCandidates(in berryOptions, in candidateOptions, candidateCount, dedup: dedup)
                : OptimizationPipeline.BuildCandidatesFromIds(sortedBerryIds, in candidateOptions, candidateCount, dedup: dedup);
            if (poffinSortKeys.Length > 0)
            {
                PoffinSorter.Sort(candidates, candidates.Length, poffinSortKeys);
            }

            if (showProgress)
            {
                Console.WriteLine($"Candidates kept (topK): {candidates.Length}");
                long permutations = CountPermutations(candidates.Length, choose);
                Console.WriteLine($"Permutations (nPk): {permutations}");
                if (useParallel)
                {
                    int cores = Environment.ProcessorCount;
                    Console.WriteLine($"Starting parallel processing on {cores} cores...");
                }
            }

            ContestStatsResult[] results = ContestStatsSearch.Run(candidates, in contestOptions, in option, topK);

            int minRank = GetIntArg(args, "--min-rank", -1);
            int minPoffins = GetIntArg(args, "--min-poffins", -1);
            int minRarity = GetIntArg(args, "--min-rarity", -1);
            int maxRarity = GetIntArg(args, "--max-rarity", -1);
            int minPerfect = GetIntArg(args, "--min-perfect", -1);
            int maxPerfect = GetIntArg(args, "--max-perfect", -1);
            string statsSort = GetStringArg(args, "--stats-sort", string.Empty);

            ContestStatsResult[] filtered = FilterContestResults(results, minRank, maxRank, minPoffins, maxPoffins, minRarity, maxRarity, minPerfect, maxPerfect);
            if (!string.IsNullOrWhiteSpace(statsSort))
            {
                SortContestResults(filtered, statsSort);
            }
            Console.WriteLine($"Contest search results: {filtered.Length}");
            for (int i = 0; i < filtered.Length; i++)
            {
                var r = filtered[i];
                int totalPoffins = r.PoffinsEaten;
                int toMaxStats = r.PoffinsToMaxStats > 0 ? r.PoffinsToMaxStats : totalPoffins;
                int afterStats = totalPoffins - toMaxStats;
                Console.WriteLine(
                    string.Format(
                        CultureInfo.InvariantCulture,
                        "{0,2}: Score {1,6} Poffins {2,2} ({3,2}+{4,2}) Sheen {5,3} Rarity {6,3} Unique {7,2} Perfect {8,1} Rank {9,1} Stats [C:{10,3} B:{11,3} Cu:{12,3} Cl:{13,3} T:{14,3}]",
                        i + 1,
                        r.Score,
                        toMaxStats,
                        afterStats,
                        r.TotalSheen,
                        r.TotalRarityCost,
                        r.UniqueBerries,
                        r.NumPerfectValues,
                        r.Rank,
                        r.Stats.Coolness,
                        r.Stats.Beauty,
                        r.Stats.Cuteness,
                        r.Stats.Cleverness,
                        r.Stats.Toughness));
            }

            if (showRecipes && filtered.Length > 0)
            {
                int count = Math.Min(Math.Max(showRecipesCount, 1), filtered.Length);
                for (int i = 0; i < count; i++)
                {
                    CliContestFormatter.PrintContestResultDetails(filtered[i], candidates, i + 1, useColor: !noColor, showAward: showAward);
                }
            }
        }

        static void RunAwardSample(string[] args)
        {
            bool noColor = GetBoolArg(args, "--no-color", fallback: false);
            int coolness = GetIntArg(args, "--coolness", 255);
            int beauty = GetIntArg(args, "--beauty", 255);
            int cuteness = GetIntArg(args, "--cuteness", 255);
            int cleverness = GetIntArg(args, "--cleverness", 255);
            int toughness = GetIntArg(args, "--toughness", 255);
            int sheen = GetIntArg(args, "--sheen", 255);
            int rank = GetIntArg(args, "--rank", 1);
            int poffins = GetIntArg(args, "--poffins", 12);
            int rarity = GetIntArg(args, "--rarity", 80);
            int unique = GetIntArg(args, "--unique", 12);
            int perfect = GetIntArg(args, "--perfect", 5);
            int score = GetIntArg(args, "--score", 0);
            int toMax = GetIntArg(args, "--to-max", poffins);

            var stats = new ContestStats(
                coolness: (byte)Math.Clamp(coolness, 0, 255),
                beauty: (byte)Math.Clamp(beauty, 0, 255),
                cuteness: (byte)Math.Clamp(cuteness, 0, 255),
                cleverness: (byte)Math.Clamp(cleverness, 0, 255),
                toughness: (byte)Math.Clamp(toughness, 0, 255),
                sheen: (byte)Math.Clamp(sheen, 0, 255));

            var result = new ContestStatsResult(
                indices: default,
                stats: stats,
                poffinsEaten: poffins,
                totalRarityCost: rarity,
                totalSheen: sheen,
                score: score,
                numPerfectValues: perfect,
                rank: rank,
                uniqueBerries: unique,
                poffinsToMaxStats: toMax);

            CliContestFormatter.PrintContestAward(result, useColor: !noColor);
        }

        static void ReportContestProgress(ContestSearchProgress progress)
        {
            double pct = progress.TotalOuter == 0 ? 100 : (double)progress.CompletedOuter / progress.TotalOuter * 100.0;
            Console.Write($"\rProgress: {progress.CompletedOuter}/{progress.TotalOuter} outer ({pct:0.0}%)");
            if (progress.CompletedOuter >= progress.TotalOuter)
            {
                Console.WriteLine();
            }
        }

        static string[] FormatRecipe(BerryId[] ids)
        {
            var names = new string[ids.Length];
            for (int i = 0; i < ids.Length; i++)
            {
                names[i] = BerryNames.GetName(ids[i]);
            }
            return names;
        }

        // static void PrintContestResultDetails(ContestStatsResult result, PoffinWithRecipe[] candidates, int index, bool useColor, bool showAward)
        // {
        //     Console.WriteLine();
        //     Console.WriteLine($"Result {index}: Rank {result.Rank} Poffins eaten: {result.PoffinsEaten} Rarity: {result.TotalRarityCost} Unique Berries: {result.UniqueBerries}");
        //     Console.WriteLine(new string('-', 75));

        //     int[] indices = GetIndices(result.Indices);
        //     for (int i = 0; i < indices.Length; i++)
        //     {
        //         int candidateIndex = indices[i];
        //         if ((uint)candidateIndex >= (uint)candidates.Length)
        //         {
        //             Console.WriteLine($"* Missing candidate index {candidateIndex}");
        //             continue;
        //         }

        //         ref readonly var candidate = ref candidates[candidateIndex];
        //         int recipeRarity = ComputeRecipeRarity(candidate.Recipe.Berries);
        //         string name = FormatPoffinName(candidate.Poffin, useColor);
        //         Console.WriteLine(
        //             string.Format(
        //                 CultureInfo.InvariantCulture,
        //                 "{0,3} - {1} {2,2} - Flavors [{3,3}, {4,3}, {5,3}, {6,3}, {7,3}] Rarity: {8,2}",
        //                 candidate.Poffin.Level,
        //                 name,
        //                 candidate.Poffin.Smoothness,
        //                 useColor ? $"{ColorForFlavor(Flavor.Spicy)}{candidate.Poffin.Spicy,3}{Colors.Reset}" : candidate.Poffin.Spicy.ToString(CultureInfo.InvariantCulture).PadLeft(3),
        //                 useColor ? $"{ColorForFlavor(Flavor.Dry)}{candidate.Poffin.Dry,3}{Colors.Reset}" : candidate.Poffin.Dry.ToString(CultureInfo.InvariantCulture).PadLeft(3),
        //                 useColor ? $"{ColorForFlavor(Flavor.Sweet)}{candidate.Poffin.Sweet,3}{Colors.Reset}" : candidate.Poffin.Sweet.ToString(CultureInfo.InvariantCulture).PadLeft(3),
        //                 useColor ? $"{ColorForFlavor(Flavor.Bitter)}{candidate.Poffin.Bitter,3}{Colors.Reset}" : candidate.Poffin.Bitter.ToString(CultureInfo.InvariantCulture).PadLeft(3),
        //                 useColor ? $"{ColorForFlavor(Flavor.Sour)}{candidate.Poffin.Sour,3}{Colors.Reset}" : candidate.Poffin.Sour.ToString(CultureInfo.InvariantCulture).PadLeft(3),
        //                 recipeRarity));

        //         if (candidate.Poffin.SecondaryFlavor != Flavor.None)
        //         {
        //             Console.WriteLine(
        //                 string.Format(
        //                     CultureInfo.InvariantCulture,
        //                     "* {0,-6} {1,-8} ({2,3}, {3,2})",
        //                     useColor ? $"{ColorForFlavor(candidate.Poffin.MainFlavor)}{candidate.Poffin.MainFlavor}{Colors.Reset}" : candidate.Poffin.MainFlavor.ToString(),
        //                     useColor ? $"{ColorForFlavor(candidate.Poffin.SecondaryFlavor)}{candidate.Poffin.SecondaryFlavor}{Colors.Reset}" : candidate.Poffin.SecondaryFlavor.ToString(),
        //                     candidate.Poffin.Level,
        //                     candidate.Poffin.SecondLevel));
        //         }
        //         else
        //         {
        //             Console.WriteLine(
        //                 string.Format(
        //                     CultureInfo.InvariantCulture,
        //                     "* {0} ({1})",
        //                     candidate.Poffin.MainFlavor,
        //                     candidate.Poffin.Level));
        //         }

        //         Console.WriteLine("* Berries used:");
        //         for (int b = 0; b < candidate.Recipe.Berries.Length; b++)
        //         {
        //             ref readonly Berry berry = ref BerryTable.Get(candidate.Recipe.Berries[b]);
        //             Console.WriteLine(FormatBerryLine(berry, useColor));
        //         }

        //         Console.WriteLine(new string('-', 75));
        //     }

        //     if (showAward)
        //     {
        //         //PrintContestAward(result, useColor);
        //         CliContestFormatter.PrintContestResultDetails(result, candidates, index, useColor, showAward);
        //     }
        // }

        // static int[] GetIndices(PoffinIndexSet indices)
        // {
        //     int count = indices.Count;
        //     var list = new int[count];
        //     if (count > 0) list[0] = indices.I0;
        //     if (count > 1) list[1] = indices.I1;
        //     if (count > 2) list[2] = indices.I2;
        //     if (count > 3) list[3] = indices.I3;
        //     return list;
        // }

        // static string FormatBerryLine(in Berry berry, bool useColor)
        // {
        //     string name = BerryNames.GetName(berry.Id);
        //     if (name.EndsWith(" Berry", StringComparison.Ordinal))
        //     {
        //         name = name[..^6];
        //     }
        //     name = name.ToLowerInvariant();
        //     string emoji = GetFlavorEmoji(berry.MainFlavor);
        //     string flavorText = berry.MainFlavor.ToString();
        //     string flavorDisplay = useColor
        //         ? $"{ColorForFlavor(berry.MainFlavor)}{flavorText}{Colors.Reset}"
        //         : flavorText;
        //     string flavor = berry.MainFlavor.ToString();
        //     int flavorPad = Math.Max(0, 6 - flavor.Length);
        //     return string.Format(
        //         CultureInfo.InvariantCulture,
        //         "     {0} {1,-6} ({2}){3} {4,2} - Flavors [{5,3}, {6,3}, {7,3}, {8,3}, {9,3}] Rarity: {10,2}",
        //         emoji,
        //         name,
        //         flavorDisplay,
        //         new string(' ', flavorPad),
        //         berry.Smoothness,
        //         useColor ? $"{ColorForFlavor(Flavor.Spicy)}{berry.Spicy,3}{Colors.Reset}" : berry.Spicy.ToString(CultureInfo.InvariantCulture).PadLeft(3),
        //         useColor ? $"{ColorForFlavor(Flavor.Dry)}{berry.Dry,3}{Colors.Reset}" : berry.Dry.ToString(CultureInfo.InvariantCulture).PadLeft(3),
        //         useColor ? $"{ColorForFlavor(Flavor.Sweet)}{berry.Sweet,3}{Colors.Reset}" : berry.Sweet.ToString(CultureInfo.InvariantCulture).PadLeft(3),
        //         useColor ? $"{ColorForFlavor(Flavor.Bitter)}{berry.Bitter,3}{Colors.Reset}" : berry.Bitter.ToString(CultureInfo.InvariantCulture).PadLeft(3),
        //         useColor ? $"{ColorForFlavor(Flavor.Sour)}{berry.Sour,3}{Colors.Reset}" : berry.Sour.ToString(CultureInfo.InvariantCulture).PadLeft(3),
        //         berry.Rarity);
        // }

        // static string GetFlavorEmoji(Flavor flavor)
        // {
        //     return flavor switch
        //     {
        //         Flavor.Spicy => "🌶️",
        //         Flavor.Dry => "🍇",
        //         Flavor.Sweet => "🍑",
        //         Flavor.Bitter => "🍐",
        //         Flavor.Sour => "🍋",
        //         _ => "❔"
        //     };
        // }

        // static void PrintContestAward(ContestStatsResult result, bool useColor)
        // {
        //     const int width = 36;
        //     string indent = $"        {Colors.Bold}";
        //     string outline = useColor ? Colors.Color256(168) : string.Empty;
        //     string reset = useColor ? Colors.Reset : string.Empty;

        //     string coolLabel = FormatFlavorLabel(Flavor.Spicy, useColor);
        //     string dryLabel = FormatFlavorLabel(Flavor.Dry, useColor);
        //     string sweetLabel = FormatFlavorLabel(Flavor.Sweet, useColor);
        //     string bitterLabel = FormatFlavorLabel(Flavor.Bitter, useColor);
        //     string sourLabel = FormatFlavorLabel(Flavor.Sour, useColor);

        //     string ribbon = useColor ? ColorForFlavor(Flavor.Dry) : string.Empty;
        //     string starOrange = useColor ? Colors.Color256(208) : string.Empty;
        //     string coinGold = useColor ? Colors.Color256(226) : string.Empty;
        //     string coin = useColor ? $"{starOrange}O{reset}" : "O";
        //     Console.WriteLine($"{indent}{outline}{new string('-', width)}{reset}{ribbon}      ---------------{reset}");
        //     Console.WriteLine($"{indent}{outline}| ******   Contest Stats    ****** |{reset}{ribbon}     \\####|▓▓▓██|####/{reset}");
        //     Console.WriteLine($"{indent}{outline}{new string('-', width)}{reset}{ribbon}      \\###|█▓▓▓█|###/{reset}");
        //     Console.WriteLine($"{indent}{outline}| {coolLabel} ->  {FormatStatLabel("Coolness", result.Stats.Coolness, useColor),-18}{outline}|{reset}{ribbon}       `##|██▓▓▓|##'{reset}");
        //     Console.WriteLine($"{indent}{outline}| {dryLabel} ->  {FormatStatLabel("Beauty", result.Stats.Beauty, useColor),-18}{outline}|{reset}            {coinGold}({coin}{coinGold}){reset}");
        //     Console.WriteLine($"{indent}{outline}| {sweetLabel} ->  {FormatStatLabel("Cuteness", result.Stats.Cuteness, useColor),-18}{outline}|{reset}{coinGold}         .-'''''-.{reset}");
        //     Console.WriteLine($"{indent}{outline}| {bitterLabel} ->  {FormatStatLabel("Cleverness", result.Stats.Cleverness, useColor),-18}{outline}|{reset}{coinGold}       .'  {starOrange}* * *{coinGold}  `.{reset}");
        //     Console.WriteLine($"{indent}{outline}| {sourLabel} ->  {FormatStatLabel("Toughness", result.Stats.Toughness, useColor),-18}{outline}|{reset}{coinGold}      :  {starOrange}*       *{coinGold}  :{reset}");
        //     Console.WriteLine($"{indent}{outline}{new string('-', width)}{reset}{coinGold}     : ~ PO F F IN ~ :{reset}");
        //     Console.WriteLine($"{indent}{outline}| {FormatEatenSheen(result, useColor),-31}{outline}|{reset}{coinGold}     : ~ A W A R D ~ :{reset}");
        //     Console.WriteLine($"{indent}{outline}{new string('-', width)}{reset}{coinGold}      :  {starOrange}*       *{coinGold}  :{reset}");
        //     Console.WriteLine($"{indent}{outline}| {FormatRankLine(result, useColor),-31}{outline}|{reset}{coinGold}       `.  {starOrange}* * *{reset}  {coinGold}.'{reset}");
        //     Console.WriteLine($"{indent}{outline}{new string('-', width)}{reset}{coinGold}         `-.....-'{reset}");
        //     Console.WriteLine(new string('-', 75));
        // }


        // static string FormatStatLabel(string name, int value, bool useColor)
        // {
        //     string valueColor = useColor ? (value >= 255 ? ColorForFlavor(Flavor.Bitter) : Colors.Color256(196)) : string.Empty;
        //     string reset = useColor ? Colors.Reset : string.Empty;
        //     return string.Format(CultureInfo.InvariantCulture, "{0,-14} : {1}{2,3}{3}", name, valueColor, value, reset);
        // }

        // static string FormatFlavorLabel(Flavor flavor, bool useColor)
        // {
        //     string raw = $"({flavor})";
        //     int pad = Math.Max(0, 8 - raw.Length);
        //     if (!useColor)
        //     {
        //         return raw + new string(' ', pad);
        //     }

        //     string color = ColorForFlavor(flavor);
        //     return $"{color}{raw}{Colors.Reset}{new string(' ', pad)}";
        // }

        // static string FormatEatenSheen(ContestStatsResult result, bool useColor)
        // {
        //     string reset = useColor ? Colors.Reset : string.Empty;
        //     string eatenWarn = useColor ? (result.PoffinsEaten > 0 ? Colors.Color256(196) : string.Empty) : string.Empty;
        //     string sheenColor = useColor ? (result.TotalSheen >= 255 ? Colors.Bold : string.Empty) : string.Empty;
        //     return string.Format(
        //         CultureInfo.InvariantCulture,
        //         "{0}Eaten{1}{2,6}{3}  Sheen          : {4}{5,3}{6}",
        //         reset,
        //         eatenWarn,
        //         result.PoffinsEaten,
        //         reset,
        //         sheenColor,
        //         result.TotalSheen,
        //         reset);
        // }

        // static string FormatRankLine(ContestStatsResult result, bool useColor)
        // {
        //     string rankColor = useColor
        //         ? (result.Rank == 1 ? ColorForFlavor(Flavor.Bitter) : result.Rank == 2 ? ColorForFlavor(Flavor.Spicy) : Colors.Color256(196))
        //         : string.Empty;
        //     string reset = useColor ? Colors.Reset : string.Empty;
        //     return string.Format(
        //         CultureInfo.InvariantCulture,
        //         "{0}Rank : {1}{2,1}{3}     R/U         {4,2} : {5,2} ",
        // reset,
        //         rankColor,
        //         result.Rank,
        //         reset,
        //         result.TotalRarityCost,
        //         result.UniqueBerries);
        // }

        // static int ComputeRecipeRarity(BerryId[] berries)
        // {
        //     int total = 0;
        //     for (int i = 0; i < berries.Length; i++)
        //     {
        //         ref readonly Berry berry = ref BerryTable.Get(berries[i]);
        //         total += berry.Rarity;
        //     }
        //     return total;
        // }

        // static string FormatPoffinName(in Poffin poffin, bool useColor)
        // {
        //     PoffinNameKind kind = PoffinFilter.GetNameKind(in poffin);
        //     string plain = kind switch
        //     {
        //         PoffinNameKind.Foul => "FOUL POFFIN",
        //         PoffinNameKind.SuperMild => "SUPER MILD POFFIN",
        //         PoffinNameKind.Mild => "MILD POFFIN",
        //         PoffinNameKind.Rich => "RICH POFFIN",
        //         PoffinNameKind.Overripe => "OVERRIPE POFFIN",
        //         _ => "POFFIN"
        //     };

        //     if (!useColor)
        //     {
        //         return plain.PadRight(17);
        //     }

        //     string padded = plain.PadRight(17);
        //     return kind switch
        //     {
        //         PoffinNameKind.Foul => $"{Colors.Bold}{Colors.Color256(237)}{padded}{Colors.Reset}",
        //         PoffinNameKind.SuperMild => $"{FormatSuperMildPoffin()}{new string(' ', Math.Max(0, 17 - plain.Length))}",
        //         PoffinNameKind.Mild => $"{Colors.Bold}{Colors.Color256(11)}{padded}{Colors.Reset}",
        //         PoffinNameKind.Rich => $"{Colors.Bold}{Colors.Color256(247)}{padded}{Colors.Reset}",
        //         PoffinNameKind.Overripe => $"{Colors.Bold}{Colors.Color256(242)}{padded}{Colors.Reset}",
        //         _ => padded
        //     };
        // }

        // static string FormatSuperMildPoffin()
        // {
        //     return $"{Colors.Bold}{Colors.RgbRed}SU{Colors.RgbOrange}PER {Colors.RgbYellow}MI{Colors.RgbGreen}LD {Colors.RgbBlue}PO{Colors.RgbDarkViolet}FF{Colors.RgbViolet}IN{Colors.Reset}";
        // }

        // static string ColorForFlavor(Flavor flavor)
        // {
        //     return flavor switch
        //     {
        //         Flavor.Spicy => Colors.Color256(208),
        //         Flavor.Dry => Colors.Color256(39),
        //         Flavor.Sweet => Colors.Color256(212),
        //         Flavor.Bitter => Colors.Color256(40),
        //         Flavor.Sour => Colors.Color256(226),
        //         _ => Colors.DEFAULT
        //     };
        // }

        static int GetIntArg(string[] args, string name, int fallback)
        {
            for (int i = 0; i < args.Length - 1; i++)
            {
                if (args[i].Equals(name, StringComparison.OrdinalIgnoreCase))
                {
                    if (int.TryParse(args[i + 1], NumberStyles.Integer, CultureInfo.InvariantCulture, out int value))
                    {
                        return value;
                    }
                }
            }
            return fallback;
        }

        static int GetIntArgOrUnset(string[] args, string name, int unset)
        {
            for (int i = 0; i < args.Length - 1; i++)
            {
                if (args[i].Equals(name, StringComparison.OrdinalIgnoreCase))
                {
                    if (int.TryParse(args[i + 1], NumberStyles.Integer, CultureInfo.InvariantCulture, out int value))
                    {
                        return value;
                    }
                }
            }
            return unset;
        }

        static bool TryGetStringArg(string[] args, string name, out string value)
        {
            for (int i = 0; i < args.Length - 1; i++)
            {
                if (args[i].Equals(name, StringComparison.OrdinalIgnoreCase))
                {
                    value = args[i + 1];
                    return true;
                }
            }
            value = string.Empty;
            return false;
        }

        static string GetStringArg(string[] args, string name, string fallback)
        {
            for (int i = 0; i < args.Length - 1; i++)
            {
                if (args[i].Equals(name, StringComparison.OrdinalIgnoreCase))
                {
                    return args[i + 1];
                }
            }
            return fallback;
        }

        static bool GetBoolArg(string[] args, string name, bool fallback)
        {
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i].Equals(name, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
            return fallback;
        }

        static ContestScoreMode ParseScoreMode(string value)
        {
            if (value.Equals("sum", StringComparison.OrdinalIgnoreCase) ||
                value.Equals("sumonly", StringComparison.OrdinalIgnoreCase))
            {
                return ContestScoreMode.SumOnly;
            }

            return ContestScoreMode.Balanced;
        }

        static int[] ParseChooseList(string value)
        {
            string[] parts = value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            if (parts.Length == 0)
            {
                return new[] { 2, 3, 4 };
            }

            var list = new int[parts.Length];
            for (int i = 0; i < parts.Length; i++)
            {
                if (!int.TryParse(parts[i], NumberStyles.Integer, CultureInfo.InvariantCulture, out int parsed))
                {
                    parsed = 2;
                }
                list[i] = parsed;
            }
            return list;
        }

        static int CountFilteredBerries(in BerryFilterOptions options)
        {
            Span<Berry> buffer = stackalloc Berry[BerryTable.Count];
            int count = BerryQuery.Execute(BerryTable.All, buffer, options, default);
            return count;
        }

        static BerryId[] GetFilteredBerryIds(in BerryFilterOptions options, ReadOnlySpan<BerrySortKey> sortKeys)
        {
            Span<Berry> buffer = stackalloc Berry[BerryTable.Count];
            int count = BerryQuery.Execute(BerryTable.All, buffer, options, sortKeys);
            if (count <= 0)
            {
                return Array.Empty<BerryId>();
            }

            var ids = new BerryId[count];
            for (int i = 0; i < count; i++)
            {
                ids[i] = buffer[i].Id;
            }
            return ids;
        }

        static BerryId[] FilterBerryIds(BerryId[] include, BerryId[]? exclude, in BerryFilterOptions options, ReadOnlySpan<BerrySortKey> sortKeys)
        {
            var berries = new Berry[include.Length];
            for (int i = 0; i < include.Length; i++)
            {
                berries[i] = BerryTable.Get(include[i]);
            }

            Span<Berry> buffer = stackalloc Berry[BerryTable.Count];
            int count = BerryQuery.Execute(berries, buffer, options, sortKeys);
            if (count <= 0)
            {
                return Array.Empty<BerryId>();
            }

            BerryId[] ids = new BerryId[count];
            int write = 0;
            if (exclude is null || exclude.Length == 0)
            {
                for (int i = 0; i < count; i++)
                {
                    ids[write++] = buffer[i].Id;
                }
            }
            else
            {
                var excluded = new HashSet<int>();
                for (int i = 0; i < exclude.Length; i++)
                {
                    excluded.Add(exclude[i].Value);
                }
                for (int i = 0; i < count; i++)
                {
                    int id = buffer[i].Id.Value;
                    if (!excluded.Contains(id))
                    {
                        ids[write++] = buffer[i].Id;
                    }
                }
            }

            if (write == ids.Length)
            {
                return ids;
            }

            var trimmed = new BerryId[write];
            Array.Copy(ids, trimmed, write);
            return trimmed;
        }

        static BerryId[]? ParseBerryNameList(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            string[] parts = value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            if (parts.Length == 0)
            {
                return null;
            }

            var ids = new List<BerryId>(parts.Length);
            var seen = new HashSet<int>();
            for (int i = 0; i < parts.Length; i++)
            {
                if (TryResolveBerryName(parts[i], out BerryId id) && seen.Add(id.Value))
                {
                    ids.Add(id);
                }
            }

            return ids.Count == 0 ? null : ids.ToArray();
        }

        static bool TryResolveBerryName(string raw, out BerryId id)
        {
            string token = NormalizeBerryName(raw);
            for (ushort i = 0; i < BerryTable.Count; i++)
            {
                BerryId candidate = new BerryId(i);
                string name = NormalizeBerryName(BerryNames.GetName(candidate));
                if (name.Equals(token, StringComparison.OrdinalIgnoreCase))
                {
                    id = candidate;
                    return true;
                }
            }

            id = default;
            return false;
        }

        static string NormalizeBerryName(string value)
        {
            string name = value.Trim();
            if (name.EndsWith("berry", StringComparison.OrdinalIgnoreCase))
            {
                name = name[..^5].Trim();
            }
            return name.Replace("-", string.Empty).Replace("_", string.Empty).ToLowerInvariant();
        }

        static long CountCombinations(int n, int[] chooseList)
        {
            long total = 0;
            for (int i = 0; i < chooseList.Length; i++)
            {
                int k = chooseList[i];
                if (k < 0 || k > n) continue;
                total += CountCombination(n, k);
            }
            return total;
        }

        static long CountCombination(int n, int k)
        {
            if (k < 0 || k > n) return 0;
            if (k == 0 || k == n) return 1;
            int kk = Math.Min(k, n - k);
            long result = 1;
            for (int i = 1; i <= kk; i++)
            {
                result = result * (n - (kk - i)) / i;
            }
            return result;
        }

        static long CountPermutations(int n, int k)
        {
            if (k < 0 || k > n) return 0;
            long result = 1;
            for (int i = 0; i < k; i++)
            {
                result *= n - i;
            }
            return result;
        }

        static BerryFilterOptions BuildBerryFilters(string[] args)
        {
            int unset = BerryFilterOptions.Unset;
            int minSpicy = GetIntArgOrUnset(args, "--berry-min-spicy", unset);
            int maxSpicy = GetIntArgOrUnset(args, "--berry-max-spicy", unset);
            int minDry = GetIntArgOrUnset(args, "--berry-min-dry", unset);
            int maxDry = GetIntArgOrUnset(args, "--berry-max-dry", unset);
            int minSweet = GetIntArgOrUnset(args, "--berry-min-sweet", unset);
            int maxSweet = GetIntArgOrUnset(args, "--berry-max-sweet", unset);
            int minBitter = GetIntArgOrUnset(args, "--berry-min-bitter", unset);
            int maxBitter = GetIntArgOrUnset(args, "--berry-max-bitter", unset);
            int minSour = GetIntArgOrUnset(args, "--berry-min-sour", unset);
            int maxSour = GetIntArgOrUnset(args, "--berry-max-sour", unset);
            int minSmoothness = GetIntArgOrUnset(args, "--berry-min-smoothness", unset);
            int maxSmoothness = GetIntArgOrUnset(args, "--berry-max-smoothness", unset);
            int minRarity = GetIntArgOrUnset(args, "--berry-min-rarity", unset);
            int maxRarity = GetIntArgOrUnset(args, "--berry-max-rarity", unset);
            int minMain = GetIntArgOrUnset(args, "--berry-min-main", unset);
            int maxMain = GetIntArgOrUnset(args, "--berry-max-main", unset);
            int minSecondary = GetIntArgOrUnset(args, "--berry-min-secondary", unset);
            int maxSecondary = GetIntArgOrUnset(args, "--berry-max-secondary", unset);
            int minNumFlavors = GetIntArgOrUnset(args, "--berry-min-num-flavors", unset);
            int maxNumFlavors = GetIntArgOrUnset(args, "--berry-max-num-flavors", unset);
            int minAnyFlavor = GetIntArgOrUnset(args, "--berry-any-flavor-min", unset);
            int maxAnyFlavor = GetIntArgOrUnset(args, "--berry-any-flavor-max", unset);
            int minWeakMain = GetIntArgOrUnset(args, "--berry-weak-main-min", unset);
            int maxWeakMain = GetIntArgOrUnset(args, "--berry-weak-main-max", unset);
            int idEquals = GetIntArgOrUnset(args, "--berry-id", unset);
            int idNotEquals = GetIntArgOrUnset(args, "--berry-exclude-id", unset);

            bool hasMainFlavor = TryGetStringArg(args, "--berry-main-flavor", out string mainFlavorRaw);
            Flavor mainFlavor = hasMainFlavor ? ParseFlavor(mainFlavorRaw) : Flavor.None;
            bool requireMain = (hasMainFlavor || GetBoolArg(args, "--berry-require-main", fallback: false)) && mainFlavor != Flavor.None;

            bool hasSecondaryFlavor = TryGetStringArg(args, "--berry-secondary-flavor", out string secondaryFlavorRaw);
            Flavor secondaryFlavor = hasSecondaryFlavor ? ParseFlavor(secondaryFlavorRaw) : Flavor.None;
            bool requireSecondary = (hasSecondaryFlavor || GetBoolArg(args, "--berry-require-secondary", fallback: false)) && secondaryFlavor != Flavor.None;

            bool hasWeakMainFlavor = TryGetStringArg(args, "--berry-weak-main-flavor", out string weakMainRaw);
            Flavor weakMainFlavor = hasWeakMainFlavor ? ParseFlavor(weakMainRaw) : Flavor.None;
            bool requireWeakMain = (hasWeakMainFlavor || GetBoolArg(args, "--berry-require-weak-main", fallback: false)) && weakMainFlavor != Flavor.None;

            byte requiredMask = ParseFlavorMask(GetStringArg(args, "--berry-required-flavors", string.Empty));
            byte excludedMask = ParseFlavorMask(GetStringArg(args, "--berry-excluded-flavors", string.Empty));

            return new BerryFilterOptions(
                minSpicy: minSpicy,
                maxSpicy: maxSpicy,
                minDry: minDry,
                maxDry: maxDry,
                minSweet: minSweet,
                maxSweet: maxSweet,
                minBitter: minBitter,
                maxBitter: maxBitter,
                minSour: minSour,
                maxSour: maxSour,
                minSmoothness: minSmoothness,
                maxSmoothness: maxSmoothness,
                minRarity: minRarity,
                maxRarity: maxRarity,
                minMainFlavorValue: minMain,
                maxMainFlavorValue: maxMain,
                minSecondaryFlavorValue: minSecondary,
                maxSecondaryFlavorValue: maxSecondary,
                minNumFlavors: minNumFlavors,
                maxNumFlavors: maxNumFlavors,
                minAnyFlavorValue: minAnyFlavor,
                maxAnyFlavorValue: maxAnyFlavor,
                minWeakMainFlavorValue: minWeakMain,
                maxWeakMainFlavorValue: maxWeakMain,
                requireWeakenedMainFlavor: requireWeakMain,
                weakenedMainFlavor: weakMainFlavor,
                idEquals: idEquals,
                idNotEquals: idNotEquals,
                requireMainFlavor: requireMain,
                mainFlavor: mainFlavor,
                requireSecondaryFlavor: requireSecondary,
                secondaryFlavor: secondaryFlavor,
                requiredFlavorMask: requiredMask,
                excludedFlavorMask: excludedMask);
        }

        static PoffinFilterOptions BuildPoffinFilters(string[] args)
        {
            int unset = PoffinFilterOptions.Unset;
            int minSpicy = GetIntArgOrUnset(args, "--poffin-min-spicy", unset);
            int maxSpicy = GetIntArgOrUnset(args, "--poffin-max-spicy", unset);
            int minDry = GetIntArgOrUnset(args, "--poffin-min-dry", unset);
            int maxDry = GetIntArgOrUnset(args, "--poffin-max-dry", unset);
            int minSweet = GetIntArgOrUnset(args, "--poffin-min-sweet", unset);
            int maxSweet = GetIntArgOrUnset(args, "--poffin-max-sweet", unset);
            int minBitter = GetIntArgOrUnset(args, "--poffin-min-bitter", unset);
            int maxBitter = GetIntArgOrUnset(args, "--poffin-max-bitter", unset);
            int minSour = GetIntArgOrUnset(args, "--poffin-min-sour", unset);
            int maxSour = GetIntArgOrUnset(args, "--poffin-max-sour", unset);
            int minSmoothness = GetIntArgOrUnset(args, "--poffin-min-smoothness", unset);
            int maxSmoothness = GetIntArgOrUnset(args, "--poffin-max-smoothness", unset);
            int minLevel = GetIntArgOrUnset(args, "--poffin-min-level", unset);
            int maxLevel = GetIntArgOrUnset(args, "--poffin-max-level", unset);
            int minSecondLevel = GetIntArgOrUnset(args, "--poffin-min-second-level", unset);
            int maxSecondLevel = GetIntArgOrUnset(args, "--poffin-max-second-level", unset);
            int minNumFlavors = GetIntArgOrUnset(args, "--poffin-min-num-flavors", unset);
            int maxNumFlavors = GetIntArgOrUnset(args, "--poffin-max-num-flavors", unset);
            int minAnyFlavor = GetIntArgOrUnset(args, "--poffin-any-flavor-min", unset);
            int maxAnyFlavor = GetIntArgOrUnset(args, "--poffin-any-flavor-max", unset);
            int idEquals = GetIntArgOrUnset(args, "--poffin-id", unset);
            int idNotEquals = GetIntArgOrUnset(args, "--poffin-exclude-id", unset);

            bool hasMainFlavor = TryGetStringArg(args, "--poffin-main-flavor", out string mainFlavorRaw);
            Flavor mainFlavor = hasMainFlavor ? ParseFlavor(mainFlavorRaw) : Flavor.None;
            bool requireMain = (hasMainFlavor || GetBoolArg(args, "--poffin-require-main", fallback: false)) && mainFlavor != Flavor.None;
            bool hasExcludedMain = TryGetStringArg(args, "--poffin-exclude-main-flavor", out string excludedMainRaw);
            Flavor excludedMain = hasExcludedMain ? ParseFlavor(excludedMainRaw) : Flavor.None;
            bool excludeMain = hasExcludedMain && excludedMain != Flavor.None;

            bool hasSecondaryFlavor = TryGetStringArg(args, "--poffin-secondary-flavor", out string secondaryFlavorRaw);
            Flavor secondaryFlavor = hasSecondaryFlavor ? ParseFlavor(secondaryFlavorRaw) : Flavor.None;
            bool requireSecondary = (hasSecondaryFlavor || GetBoolArg(args, "--poffin-require-secondary", fallback: false)) && secondaryFlavor != Flavor.None;
            bool hasExcludedSecondary = TryGetStringArg(args, "--poffin-exclude-secondary-flavor", out string excludedSecondaryRaw);
            Flavor excludedSecondary = hasExcludedSecondary ? ParseFlavor(excludedSecondaryRaw) : Flavor.None;
            bool excludeSecondary = hasExcludedSecondary && excludedSecondary != Flavor.None;

            PoffinNameKind nameEquals = ParsePoffinNameKind(GetStringArg(args, "--poffin-name", string.Empty));
            PoffinNameKind nameNotEquals = ParsePoffinNameKind(GetStringArg(args, "--poffin-exclude-name", string.Empty));

            return new PoffinFilterOptions(
                minSpicy: minSpicy,
                maxSpicy: maxSpicy,
                minDry: minDry,
                maxDry: maxDry,
                minSweet: minSweet,
                maxSweet: maxSweet,
                minBitter: minBitter,
                maxBitter: maxBitter,
                minSour: minSour,
                maxSour: maxSour,
                minSmoothness: minSmoothness,
                maxSmoothness: maxSmoothness,
                minLevel: minLevel,
                maxLevel: maxLevel,
                minSecondLevel: minSecondLevel,
                maxSecondLevel: maxSecondLevel,
                minNumFlavors: minNumFlavors,
                maxNumFlavors: maxNumFlavors,
                minAnyFlavorValue: minAnyFlavor,
                maxAnyFlavorValue: maxAnyFlavor,
                idEquals: idEquals,
                idNotEquals: idNotEquals,
                requireMainFlavor: requireMain,
                mainFlavor: mainFlavor,
                excludeMainFlavor: excludeMain,
                excludedMainFlavor: excludedMain,
                requireSecondaryFlavor: requireSecondary,
                secondaryFlavor: secondaryFlavor,
                excludeSecondaryFlavor: excludeSecondary,
                excludedSecondaryFlavor: excludedSecondary,
                nameEquals: nameEquals,
                nameNotEquals: nameNotEquals);
        }

        static PoffinScoreOptions BuildPoffinScoreOptions(string[] args)
        {
            int levelWeight = GetIntArg(args, "--poffin-level-weight", 1000);
            int totalFlavorWeight = GetIntArg(args, "--poffin-total-flavor-weight", 1);
            int smoothnessPenalty = GetIntArg(args, "--poffin-smoothness-penalty", 1);

            bool hasPreferred = TryGetStringArg(args, "--poffin-preferred-main-flavor", out string preferredRaw);
            Flavor preferred = hasPreferred ? ParseFlavor(preferredRaw) : Flavor.None;
            int preferredBonus = GetIntArg(args, "--poffin-preferred-main-bonus", 0);

            return new PoffinScoreOptions(
                levelWeight: levelWeight,
                totalFlavorWeight: totalFlavorWeight,
                smoothnessPenalty: smoothnessPenalty,
                preferredMainFlavor: preferred,
                preferredMainFlavorBonus: preferredBonus);
        }

        static FeedingSearchOptions BuildFeedingOptions(string[] args, ContestScoreMode? scoreModeOverride = null)
        {
            int statsWeight = GetIntArg(args, "--stats-weight", 1000);
            int poffinPenalty = GetIntArg(args, "--poffin-penalty", 10);
            int sheenPenalty = GetIntArg(args, "--sheen-penalty", 1);
            int rarityPenalty = GetIntArg(args, "--rarity-penalty", 5);
            int minStatWeight = GetIntArg(args, "--min-stat-weight", 2000);

            string rarityModeRaw = GetStringArg(args, "--rarity-mode", "max");
            RarityCostMode rarityMode = rarityModeRaw.Equals("sum", StringComparison.OrdinalIgnoreCase)
                ? RarityCostMode.SumBerryRarity
                : RarityCostMode.MaxBerryRarity;

            ContestScoreMode scoreMode = scoreModeOverride ?? ParseScoreMode(GetStringArg(args, "--score", "balanced"));

            return new FeedingSearchOptions(
                statsWeight: statsWeight,
                poffinCountPenalty: poffinPenalty,
                sheenPenalty: sheenPenalty,
                rarityPenalty: rarityPenalty,
                rarityCostMode: rarityMode,
                scoreMode: scoreMode,
                minStatWeight: minStatWeight);
        }

        static Flavor ParseFlavor(string value)
        {
            if (value.Equals("spicy", StringComparison.OrdinalIgnoreCase)) return Flavor.Spicy;
            if (value.Equals("dry", StringComparison.OrdinalIgnoreCase)) return Flavor.Dry;
            if (value.Equals("sweet", StringComparison.OrdinalIgnoreCase)) return Flavor.Sweet;
            if (value.Equals("bitter", StringComparison.OrdinalIgnoreCase)) return Flavor.Bitter;
            if (value.Equals("sour", StringComparison.OrdinalIgnoreCase)) return Flavor.Sour;
            return Flavor.None;
        }

        static BerrySortKey[] ParseBerrySortKeys(string sortSpec)
        {
            if (string.IsNullOrWhiteSpace(sortSpec))
            {
                return Array.Empty<BerrySortKey>();
            }

            string[] parts = sortSpec.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            var keys = new BerrySortKey[parts.Length];
            int count = 0;
            for (int i = 0; i < parts.Length; i++)
            {
                string part = parts[i];
                bool desc = false;
                int colon = part.IndexOf(':');
                if (colon >= 0)
                {
                    string dir = part[(colon + 1)..];
                    part = part[..colon];
                    desc = dir.Equals("desc", StringComparison.OrdinalIgnoreCase);
                }

                string token = NormalizeSortToken(part);
                BerrySortField field = token switch
                {
                    "id" => BerrySortField.Id,
                    "spicy" => BerrySortField.Spicy,
                    "dry" => BerrySortField.Dry,
                    "sweet" => BerrySortField.Sweet,
                    "bitter" => BerrySortField.Bitter,
                    "sour" => BerrySortField.Sour,
                    "smoothness" => BerrySortField.Smoothness,
                    "rarity" => BerrySortField.Rarity,
                    "main" => BerrySortField.MainFlavor,
                    "mainflavor" => BerrySortField.MainFlavor,
                    "secondary" => BerrySortField.SecondaryFlavor,
                    "secondaryflavor" => BerrySortField.SecondaryFlavor,
                    "mainvalue" => BerrySortField.MainFlavorValue,
                    "mainflavorvalue" => BerrySortField.MainFlavorValue,
                    "secondaryvalue" => BerrySortField.SecondaryFlavorValue,
                    "secondaryflavorvalue" => BerrySortField.SecondaryFlavorValue,
                    "numflavors" => BerrySortField.NumFlavors,
                    "name" => BerrySortField.Name,
                    _ => BerrySortField.Id
                };

                keys[count++] = new BerrySortKey(field, desc);
            }

            if (count == keys.Length)
            {
                return keys;
            }

            var trimmed = new BerrySortKey[count];
            Array.Copy(keys, trimmed, count);
            return trimmed;
        }

        static PoffinSortKey[] ParsePoffinSortKeys(string sortSpec)
        {
            if (string.IsNullOrWhiteSpace(sortSpec))
            {
                return Array.Empty<PoffinSortKey>();
            }

            string[] parts = sortSpec.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            var keys = new PoffinSortKey[parts.Length];
            int count = 0;
            for (int i = 0; i < parts.Length; i++)
            {
                string part = parts[i];
                bool desc = false;
                int colon = part.IndexOf(':');
                if (colon >= 0)
                {
                    string dir = part[(colon + 1)..];
                    part = part[..colon];
                    desc = dir.Equals("desc", StringComparison.OrdinalIgnoreCase);
                }

                string token = NormalizeSortToken(part);
                PoffinSortField field = token switch
                {
                    "spicy" => PoffinSortField.Spicy,
                    "dry" => PoffinSortField.Dry,
                    "sweet" => PoffinSortField.Sweet,
                    "bitter" => PoffinSortField.Bitter,
                    "sour" => PoffinSortField.Sour,
                    "totalflavor" => PoffinSortField.TotalFlavor,
                    "smoothness" => PoffinSortField.Smoothness,
                    "level" => PoffinSortField.Level,
                    "secondlevel" => PoffinSortField.SecondLevel,
                    "mainflavor" => PoffinSortField.MainFlavor,
                    "secondaryflavor" => PoffinSortField.SecondaryFlavor,
                    "numflavors" => PoffinSortField.NumFlavors,
                    "name" => PoffinSortField.NameKind,
                    "namekind" => PoffinSortField.NameKind,
                    "levelratio" => PoffinSortField.LevelToSmoothnessRatio,
                    "leveltosmoothnessratio" => PoffinSortField.LevelToSmoothnessRatio,
                    "totalratio" => PoffinSortField.TotalFlavorToSmoothnessRatio,
                    "totalflavortosmoothnessratio" => PoffinSortField.TotalFlavorToSmoothnessRatio,
                    _ => PoffinSortField.Level
                };

                keys[count++] = new PoffinSortKey(field, desc);
            }

            if (count == keys.Length)
            {
                return keys;
            }

            var trimmed = new PoffinSortKey[count];
            Array.Copy(keys, trimmed, count);
            return trimmed;
        }

        static string NormalizeSortToken(string value)
        {
            return value.Trim().Replace("-", string.Empty).Replace("_", string.Empty).ToLowerInvariant();
        }

        static PoffinNameKind ParsePoffinNameKind(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return PoffinNameKind.None;
            string normalized = value.Trim().Replace("-", " ").Replace("_", " ");
            if (normalized.Equals("foul", StringComparison.OrdinalIgnoreCase) ||
                normalized.Equals("foul poffin", StringComparison.OrdinalIgnoreCase))
                return PoffinNameKind.Foul;
            if (normalized.Equals("mild", StringComparison.OrdinalIgnoreCase) ||
                normalized.Equals("mild poffin", StringComparison.OrdinalIgnoreCase))
                return PoffinNameKind.Mild;
            if (normalized.Equals("rich", StringComparison.OrdinalIgnoreCase) ||
                normalized.Equals("rich poffin", StringComparison.OrdinalIgnoreCase))
                return PoffinNameKind.Rich;
            if (normalized.Equals("overripe", StringComparison.OrdinalIgnoreCase) ||
                normalized.Equals("overripe poffin", StringComparison.OrdinalIgnoreCase))
                return PoffinNameKind.Overripe;
            if (normalized.Equals("super mild", StringComparison.OrdinalIgnoreCase) ||
                normalized.Equals("super mild poffin", StringComparison.OrdinalIgnoreCase) ||
                normalized.Equals("supermild", StringComparison.OrdinalIgnoreCase))
                return PoffinNameKind.SuperMild;
            return PoffinNameKind.None;
        }

        static byte ParseFlavorMask(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return 0;
            string[] parts = value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            byte mask = 0;
            for (int i = 0; i < parts.Length; i++)
            {
                Flavor flavor = ParseFlavor(parts[i]);
                switch (flavor)
                {
                    case Flavor.Spicy:
                        mask |= 1 << 0;
                        break;
                    case Flavor.Dry:
                        mask |= 1 << 1;
                        break;
                    case Flavor.Sweet:
                        mask |= 1 << 2;
                        break;
                    case Flavor.Bitter:
                        mask |= 1 << 3;
                        break;
                    case Flavor.Sour:
                        mask |= 1 << 4;
                        break;
                }
            }
            return mask;
        }

        static ContestStatsResult[] FilterContestResults(ContestStatsResult[] results, int minRank, int maxRank, int minPoffins, int maxPoffins, int minRarity, int maxRarity, int minPerfect, int maxPerfect)
        {
            if (results.Length == 0 ||
                minRank < 0 && maxRank < 0 &&
                 minPoffins < 0 && maxPoffins < 0 &&
                 minRarity < 0 && maxRarity < 0 &&
                 minPerfect < 0 && maxPerfect < 0)
            {
                return results;
            }

            int count = 0;
            var buffer = new ContestStatsResult[results.Length];
            for (int i = 0; i < results.Length; i++)
            {
                ContestStatsResult result = results[i];
                if (minRank >= 0 && result.Rank < minRank)
                {
                    continue;
                }
                if (maxRank >= 0 && result.Rank > maxRank)
                {
                    continue;
                }
                if (minPoffins >= 0 && result.PoffinsEaten < minPoffins)
                {
                    continue;
                }
                if (maxPoffins >= 0 && result.PoffinsEaten > maxPoffins)
                {
                    continue;
                }
                if (minRarity >= 0 && result.TotalRarityCost < minRarity)
                {
                    continue;
                }
                if (maxRarity >= 0 && result.TotalRarityCost > maxRarity)
                {
                    continue;
                }
                if (minPerfect >= 0 && result.NumPerfectValues < minPerfect)
                {
                    continue;
                }
                if (maxPerfect >= 0 && result.NumPerfectValues > maxPerfect)
                {
                    continue;
                }
                buffer[count++] = result;
            }

            if (count == results.Length)
            {
                return results;
            }

            var trimmed = new ContestStatsResult[count];
            Array.Copy(buffer, trimmed, count);
            return trimmed;
        }

        static void SortContestResults(ContestStatsResult[] results, string sortSpec)
        {
            if (results.Length < 2)
            {
                return;
            }

            string[] parts = sortSpec.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            if (parts.Length == 0)
            {
                return;
            }

            var keys = new (string Key, bool Desc)[parts.Length];
            for (int i = 0; i < parts.Length; i++)
            {
                string part = parts[i];
                bool desc = false;
                int colon = part.IndexOf(':');
                if (colon >= 0)
                {
                    string dir = part[(colon + 1)..];
                    part = part[..colon];
                    desc = dir.Equals("desc", StringComparison.OrdinalIgnoreCase);
                }
                keys[i] = (part.Trim(), desc);
            }

            Array.Sort(results, (left, right) => CompareContestResults(left, right, keys));
        }

        static int CompareContestResults(ContestStatsResult left, ContestStatsResult right, (string Key, bool Desc)[] keys)
        {
            for (int i = 0; i < keys.Length; i++)
            {
                int cmp = keys[i].Key.ToLowerInvariant() switch
                {
                    "rank" => left.Rank.CompareTo(right.Rank),
                    "poffins" => left.PoffinsEaten.CompareTo(right.PoffinsEaten),
                    "rarity" => left.TotalRarityCost.CompareTo(right.TotalRarityCost),
                    "unique" => left.UniqueBerries.CompareTo(right.UniqueBerries),
                    "perfect" => left.NumPerfectValues.CompareTo(right.NumPerfectValues),
                    "coolness" => left.Stats.Coolness.CompareTo(right.Stats.Coolness),
                    "beauty" => left.Stats.Beauty.CompareTo(right.Stats.Beauty),
                    "cuteness" => left.Stats.Cuteness.CompareTo(right.Stats.Cuteness),
                    "cleverness" => left.Stats.Cleverness.CompareTo(right.Stats.Cleverness),
                    "toughness" => left.Stats.Toughness.CompareTo(right.Stats.Toughness),
                    "sheen" => left.TotalSheen.CompareTo(right.TotalSheen),
                    "score" => left.Score.CompareTo(right.Score),
                    _ => 0
                };

                if (cmp != 0)
                {
                    return keys[i].Desc ? -cmp : cmp;
                }
            }

            return 0;
        }
    }
}