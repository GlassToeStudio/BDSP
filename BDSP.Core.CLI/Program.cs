using System;
using System.Globalization;
using BDSP.Core.Berries;
using BDSP.Core.Optimization;
using BDSP.Core.Poffins;
using BDSP.Core.Poffins.Cooking;
using BDSP.Core.Poffins.Enumeration;
using BDSP.Core.Poffins.Search;

if (args.Length > 0 && args[0].Equals("feeding-plan", StringComparison.OrdinalIgnoreCase))
{
    RunFeedingPlan(args);
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

    var searchOptions = new PoffinSearchOptions(
        choose: choose,
        cookTimeSeconds: cookTimeSeconds,
        useParallel: false);

    var scoreOptions = searchOptions.ScoreOptions;
    var top = new TopK<PoffinCandidate>(topK);

    Span<BerryId> allIds = stackalloc BerryId[BerryTable.Count];
    for (ushort i = 0; i < BerryTable.Count; i++)
    {
        allIds[i] = new BerryId(i);
    }

    PoffinComboEnumerator.ForEach(allIds, choose, combo =>
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
            cookTimeSeconds,
            spills: 0,
            burns: 0,
            amityBonus: 9);

        int score = PoffinScoring.Score(in poffin, in scoreOptions);
        var recipe = new PoffinRecipe(recipeIds, cookTimeSeconds, spills: 0, burns: 0, amityBonus: 9);
        var candidate = new PoffinCandidate(new PoffinWithRecipe(poffin, recipe), score);
        top.TryAdd(candidate, score);
    });

    var candidates = top.ToSortedArray((a, b) => b.Score.CompareTo(a.Score));
    var option = new FeedingSearchOptions();
    var plan = FeedingSearch.BuildPlan(SelectPoffins(candidates), in option);

    Console.WriteLine($"Feeding plan (steps: {plan.TotalPoffins}, sheen: {plan.TotalSheen}, rarity cost: {plan.TotalRarityCost}, score: {plan.Score})");
    Console.WriteLine($"Final stats: Cool {plan.FinalStats.Coolness}, Beauty {plan.FinalStats.Beauty}, Cute {plan.FinalStats.Cuteness}, Clever {plan.FinalStats.Cleverness}, Tough {plan.FinalStats.Toughness}, Sheen {plan.FinalStats.Sheen}");

    for (int i = 0; i < plan.Steps.Length; i++)
    {
        ref readonly var step = ref plan.Steps[i];
        Console.WriteLine(
            string.Format(
                CultureInfo.InvariantCulture,
                "{0,2}: Lvl {1,3} Smooth {2,3} Recipe [{3}]",
                step.Index + 1,
                step.Poffin.Poffin.Level,
                step.Poffin.Poffin.Smoothness,
                string.Join(",", FormatRecipe(step.Poffin.Recipe.Berries))));
    }
}

static ReadOnlySpan<PoffinWithRecipe> SelectPoffins(PoffinCandidate[] candidates)
{
    var list = new PoffinWithRecipe[candidates.Length];
    for (int i = 0; i < candidates.Length; i++)
    {
        list[i] = candidates[i].Value;
    }
    return list;
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

readonly struct PoffinCandidate
{
    public readonly PoffinWithRecipe Value;
    public readonly int Score;

    public PoffinCandidate(PoffinWithRecipe value, int score)
    {
        Value = value;
        Score = score;
    }
}

static class PoffinScoring
{
    public static int Score(in Poffin poffin, in PoffinScoreOptions options)
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
}
