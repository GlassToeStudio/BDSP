using System;
using System.Globalization;
using BDSP.Core.Berries;
using BDSP.Core.Optimization.Core;
using BDSP.Core.Optimization.Search;
using BDSP.Core.Poffins;
using BDSP.Core.Poffins.Filters;
using BDSP.Core.Poffins.Search;

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

for (ushort i = 0; i < 65; i++)
{
    ref readonly var berry = ref BerryTable.Get(new BerryId(i));
    Console.WriteLine(berry);
    Console.WriteLine(BDSP.Tools.BerryAnsiFormatter.Format(berry));
}

return;

static void RunFeedingPlan(string[] args)
{
    int choose = GetIntArg(args, "--choose", 2);
    int cookTimeSeconds = GetIntArg(args, "--time", 40);
    int topK = GetIntArg(args, "--topk", 10);
    string candidateChooseArg = GetStringArg(args, "--candidate-choose", choose.ToString(CultureInfo.InvariantCulture));
    bool showProgress = GetBoolArg(args, "--progress", fallback: false);

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
    if (showProgress)
    {
        Console.WriteLine("Setting up berry filters...");
    }

    int berryCount = CountFilteredBerries(in berryOptions);
    if (showProgress)
    {
        Console.WriteLine($"Filtered berries: {berryCount}");
        long combos = CountCombinations(berryCount, candidateOptions.ChooseList);
        Console.WriteLine($"Combinations (sum nCk): {combos}");
        Console.WriteLine("Cooking candidate poffins...");
    }

    PoffinWithRecipe[] candidates = OptimizationPipeline.BuildCandidates(
        in berryOptions,
        in candidateOptions,
        topK,
        dedup: true);

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
    string candidateChooseArg = GetStringArg(args, "--candidate-choose", "2,3,4");
    bool useParallel = GetBoolArg(args, "--parallel", fallback: false);
    bool dedup = !GetBoolArg(args, "--no-dedup", fallback: false);
    bool showProgress = GetBoolArg(args, "--progress", fallback: false);
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
    var option = BuildFeedingOptions(args, scoreMode);
    var contestOptions = new ContestStatsSearchOptions(
        choose: choose,
        useParallel: useParallel,
        maxPoffins: maxPoffins,
        progress: showProgress ? ReportContestProgress : null,
        progressInterval: 64);

    if (showProgress)
    {
        Console.WriteLine("Setting up berry filters...");
    }

    int berryCount = CountFilteredBerries(in berryOptions);
    if (showProgress)
    {
        Console.WriteLine($"Filtered berries: {berryCount}");
        long combos = CountCombinations(berryCount, candidateOptions.ChooseList);
        Console.WriteLine($"Combinations (sum nCk): {combos}");
        Console.WriteLine("Cooking candidate poffins...");
    }

    PoffinWithRecipe[] candidates = OptimizationPipeline.BuildCandidates(
        in berryOptions,
        in candidateOptions,
        candidateCount,
        dedup: dedup);

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
        Console.WriteLine(
            string.Format(
                CultureInfo.InvariantCulture,
                "{0,2}: Score {1,6} Poffins {2,2} +{3,2} Sheen {4,3} Rarity {5,3} Unique {6,2} Perfect {7,1} Rank {8,1} Stats [C:{9,3} B:{10,3} Cu:{11,3} Cl:{12,3} T:{13,3}]",
                i + 1,
                r.Score,
                r.PoffinsEaten,
                r.AdditionalPoffinsToMaxSheen,
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
        result *= (n - i);
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

static PoffinNameKind ParsePoffinNameKind(string value)
{
    if (string.IsNullOrWhiteSpace(value)) return PoffinNameKind.None;
    if (value.Equals("foul poffin", StringComparison.OrdinalIgnoreCase)) return PoffinNameKind.Foul;
    if (value.Equals("mild poffin", StringComparison.OrdinalIgnoreCase)) return PoffinNameKind.Mild;
    if (value.Equals("rich poffin", StringComparison.OrdinalIgnoreCase)) return PoffinNameKind.Rich;
    if (value.Equals("overripe poffin", StringComparison.OrdinalIgnoreCase)) return PoffinNameKind.Overripe;
    if (value.Equals("super mild poffin", StringComparison.OrdinalIgnoreCase)) return PoffinNameKind.SuperMild;
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

static ContestStatsResult[] FilterContestResults(
    ContestStatsResult[] results,
    int minRank,
    int maxRank,
    int minPoffins,
    int maxPoffins,
    int minRarity,
    int maxRarity,
    int minPerfect,
    int maxPerfect)
{
    if (results.Length == 0 ||
        (minRank < 0 && maxRank < 0 &&
         minPoffins < 0 && maxPoffins < 0 &&
         minRarity < 0 && maxRarity < 0 &&
         minPerfect < 0 && maxPerfect < 0))
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
