using System;
using System.Globalization;
using BDSP.Core.Berries;
using BDSP.Core.Optimization.Core;
using BDSP.Core.Optimization.Search;
using BDSP.Core.Poffins;

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

    var candidateOptions = new PoffinCandidateOptions(
        chooseList: new[] { choose },
        cookTimeSeconds: cookTimeSeconds);
    var option = FeedingSearchOptions.Default;
    var plan = OptimizationPipeline.RunFeedingPlan(
        berryOptions: default,
        candidateOptions: in candidateOptions,
        candidateTopK: topK,
        searchOptions: in option,
        start: default,
        dedup: true);

    Console.WriteLine($"Feeding plan (steps: {plan.TotalPoffins}, sheen: {plan.TotalSheen}, rarity cost: {plan.TotalRarityCost}, score: {plan.Score})");
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
    int candidateCount = GetIntArg(args, "--candidates", 500);
    string candidateChooseArg = GetStringArg(args, "--candidate-choose", "2,3,4");
    bool useParallel = GetBoolArg(args, "--parallel", fallback: false);
    bool dedup = !GetBoolArg(args, "--no-dedup", fallback: false);

    var chooseList = ParseChooseList(candidateChooseArg);
    var candidateOptions = new PoffinCandidateOptions(
        chooseList: chooseList,
        cookTimeSeconds: cookTimeSeconds);
    var option = FeedingSearchOptions.Default;
    var contestOptions = new ContestStatsSearchOptions(choose: choose, useParallel: useParallel);

    var results = OptimizationPipeline.RunContestSearch(
        berryOptions: default,
        candidateOptions: in candidateOptions,
        candidateTopK: candidateCount,
        contestOptions: in contestOptions,
        scoringOptions: in option,
        topK: topK,
        dedup: dedup);

    Console.WriteLine($"Contest search results: {results.Length}");
    for (int i = 0; i < results.Length; i++)
    {
        var r = results[i];
        Console.WriteLine(
            string.Format(
                CultureInfo.InvariantCulture,
                "{0,2}: Score {1,6} Poffins {2,2} Sheen {3,3} Rarity {4,3} Stats [C:{5,3} B:{6,3} Cu:{7,3} Cl:{8,3} T:{9,3}]",
                i + 1,
                r.Score,
                r.PoffinsEaten,
                r.TotalSheen,
                r.TotalRarityCost,
                r.Stats.Coolness,
                r.Stats.Beauty,
                r.Stats.Cuteness,
                r.Stats.Cleverness,
                r.Stats.Toughness));
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
