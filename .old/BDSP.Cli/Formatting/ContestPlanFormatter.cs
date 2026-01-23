using System.Collections.Generic;
using System.Text;
using BDSP.Core.Berries.Analysis;
using BDSP.Core.Berries.Data;
using BDSP.Core.Feeding;
using BDSP.Core.Poffins;
using BDSP.Core.Primitives;

namespace BDSP.Cli.Formatting;

public static class ContestPlanFormatter
{
    private static class Ansi
    {
        public const string Reset = "\u001b[0m";
        public const string Bold = "\u001b[1m";
        public const string Red = "\u001b[31m";
        public const string Green = "\u001b[32m";
        public const string Yellow = "\u001b[33m";
        public const string Blue = "\u001b[34m";
        public const string Magenta = "\u001b[35m";
        public const string Cyan = "\u001b[36m";
        public const string White = "\u001b[37m";
    }

    private static readonly string[] FlavorColors =
    {
        Ansi.Red,     // Spicy
        Ansi.Blue,    // Dry
        Ansi.Magenta, // Sweet
        Ansi.Green,   // Bitter
        Ansi.Yellow   // Sour
    };

    private static readonly string[] FlavorNames =
    {
        "Spicy",
        "Dry",
        "Sweet",
        "Bitter",
        "Sour"
    };

    public static string Format(FeedingPlan plan)
    {
        var sb = new StringBuilder();

        var stats = plan.FinalState.Stats;
        int numPerfect = (stats.Coolness >= 255 ? 1 : 0)
                         + (stats.Beauty >= 255 ? 1 : 0)
                         + (stats.Cuteness >= 255 ? 1 : 0)
                         + (stats.Cleverness >= 255 ? 1 : 0)
                         + (stats.Toughness >= 255 ? 1 : 0);

        int rank = ComputeRank(numPerfect, plan.FinalState.Sheen);
        int poffinsEaten = plan.Poffins.Count;
        int berriesPerPoffin = plan.Recipes.Count > 0 ? plan.Recipes[0].Berries.Length : 0;
        int yield = berriesPerPoffin == 0 ? 0 : poffinsEaten * berriesPerPoffin;
        int totalRarity = ComputeTotalRarity(plan);
        int uniqueBerries = CountUniqueBerries(plan);

        foreach (var line in FormatPoffinLines(plan))
            sb.Append(line).Append('\n');

        const int amt = 36;
        string outline = Ansi.White;
        string badRed = Ansi.Red;

        sb.Append($"        {new string('-', amt)}      ---------------\n");
        sb.Append($"        | ******   Contest Stats    ****** |     \\####|▓▓▓██|####/\n");
        sb.Append($"        {new string('-', amt)}      \\###|█▓▓▓█|###/\n");

        sb.Append(StatLine("(Spicy)", "Coolness", stats.Coolness, 0, outline, badRed));
        sb.Append(StatLine("(Dry)", "Beauty", stats.Beauty, 1, outline, badRed));
        sb.Append(StatLine("(Sweet)", "Cuteness", stats.Cuteness, 2, outline, badRed));
        sb.Append(StatLine("(Bitter)", "Cleverness", stats.Cleverness, 3, outline, badRed));
        sb.Append(StatLine("(Sour)", "Toughness", stats.Toughness, 4, outline, badRed));

        sb.Append($"        {new string('-', amt)}     : ~ PO F F IN ~ :\n");

        string eatenColor = poffinsEaten > yield && yield > 0 ? badRed : Ansi.Reset;
        string sheenColor = plan.FinalState.Sheen >= 255 ? Ansi.Bold : Ansi.Reset;
        sb.Append($"        | Eaten{eatenColor}{poffinsEaten,6}{Ansi.Reset}  Sheen         : {sheenColor}{plan.FinalState.Sheen,4}{Ansi.Reset} |     : ~ A W A R D ~ :\n");
        sb.Append($"        {new string('-', amt)}      :  *       *  :\n");

        string rankColor = rank switch
        {
            1 => Ansi.Green,
            2 => Ansi.Yellow,
            _ => badRed
        };
        sb.Append($"        | Rank :{rankColor} {rank,-2}{Ansi.Reset}    R/U       {totalRarity,3} : {uniqueBerries,-2} |       `.  * * *  .'\n");
        sb.Append($"        {new string('-', amt)}         `-.....-'\n");
        sb.Append($"{new string('-', 75)}\n");

        return sb.ToString();
    }

    private static string StatLine(
        string flavorLabel,
        string statLabel,
        int value,
        int flavorIndex,
        string outline,
        string badRed)
    {
        string color = FlavorColors[flavorIndex];
        string valueColor = value < 255 ? badRed : FlavorColors[3];
        return $"        | {color}{flavorLabel,-8}{Ansi.Reset} ->  {Ansi.Bold}{color}{statLabel,-14}{Ansi.Reset}:{valueColor}{value,4}{Ansi.Reset} |       `##|██▓▓▓|##'\n";
    }

    private static IEnumerable<string> FormatPoffinLines(FeedingPlan plan)
    {
        if (plan.Recipes.Count > 0)
        {
            foreach (var r in plan.Recipes)
                foreach (var line in FormatRecipe(r))
                    yield return line;
            yield break;
        }

        foreach (var p in plan.Poffins)
            yield return p.ToString();
    }

    private static int ComputeRank(int numPerfectValues, int sheen)
    {
        int total = numPerfectValues + (sheen >= 255 ? 10 : 0);
        if (total == 15) return 1;
        if (total == 5) return 2;
        return 3;
    }

    private static int ComputeTotalRarity(FeedingPlan plan)
    {
        if (plan.Recipes.Count == 0)
            return 0;

        int total = 0;
        foreach (var r in plan.Recipes)
        {
            foreach (var id in r.Berries.Span)
                total += BerryTable.Get(id).Rarity;
        }
        return total;
    }

    private static int CountUniqueBerries(FeedingPlan plan)
    {
        if (plan.Recipes.Count == 0)
            return 0;

        var set = new HashSet<ushort>();
        foreach (var r in plan.Recipes)
        {
            foreach (var id in r.Berries.Span)
                set.Add(id.Value);
        }
        return set.Count;
    }

    private static IEnumerable<string> FormatRecipe(PoffinRecipe recipe)
    {
        const int width = 75;
        var p = recipe.Poffin;
        int rarity = 0;
        var berries = recipe.Berries.ToArray();
        foreach (var id in berries)
            rarity += BerryTable.Get(id).Rarity;

        yield return new string('-', width);
        yield return $"{p.Level,3} - {GetPoffinName(p),-24}{p.Smoothness,3} - Flavors {FormatFlavorValues(p)} Rarity: {rarity}";
        yield return new string('-', width);
        yield return $"* {FormatFlavorName(p.PrimaryFlavor),-6} {FormatFlavorName(p.SecondaryFlavor),-6} ({p.Level}, {p.SecondLevel})";
        yield return "* Berries used:";

        foreach (var id in berries)
        {
            ref readonly var b = ref BerryTable.Get(id);
            var mainFlavor = BerryFacts.GetMainFlavor(in b);
            string emoji = GetEmoji(mainFlavor);
            string berryName = NormalizeBerryName(BerryNames.GetName(id));
            string flavor = FlavorNames[(int)mainFlavor];
            string flavors = FormatFlavorValues(b);
            yield return $"     {emoji}{berryName,-7} ({flavor,-6}) {b.Smoothness,3} - Flavors {flavors} Rarity:{b.Rarity,3}";
        }

        yield return new string('-', width);
    }

    private static string FormatFlavorName(Flavor flavor)
        => FlavorNames[(int)flavor];

    private static string NormalizeBerryName(string name)
    {
        var trimmed = name.Trim();
        if (trimmed.EndsWith("Berry", StringComparison.OrdinalIgnoreCase))
            trimmed = trimmed[..^"Berry".Length].Trim();
        return trimmed.ToLowerInvariant();
    }

    private static string FormatFlavorValues(Poffin p)
        => $"[{p.Spicy,3}, {p.Dry,3}, {p.Sweet,3}, {p.Bitter,3}, {p.Sour,3}]";

    private static string FormatFlavorValues(in Berry b)
        => $"[{b.Spicy,3}, {b.Dry,3}, {b.Sweet,3}, {b.Bitter,3}, {b.Sour,3}]";

    private static string GetPoffinName(Poffin p)
    {
        return p.Type switch
        {
            PoffinType.SuperMild => "SUPER MILD POFFIN",
            PoffinType.Mild => "MILD POFFIN",
            PoffinType.Overripe => "OVERRIPE POFFIN",
            PoffinType.Rich => "RICH POFFIN",
            PoffinType.DualFlavor => "DUAL POFFIN",
            PoffinType.SingleFlavor => "SINGLE POFFIN",
            PoffinType.Foul => "FOUL POFFIN",
            _ => "POFFIN"
        };
    }

    private static string GetEmoji(Flavor flavor)
    {
        return flavor switch
        {
            Flavor.Spicy => " 🌶️ ",
            Flavor.Dry => " 🍇 ",
            Flavor.Sweet => " 🍑 ",
            Flavor.Bitter => " 🍐 ",
            Flavor.Sour => " 🍋 ",
            _ => " ❔ "
        };
    }
}
