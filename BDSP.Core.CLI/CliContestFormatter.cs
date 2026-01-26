using System;
using System.Globalization;
using System.Text;
using BDSP.Core.Optimization.Search;
using BDSP.Core.Optimization.Core;
using BDSP.Core.Berries;

namespace BDSP.Core.CLI
{
    /// <summary>
    /// CLI formatter for contest results and award output.
    /// </summary>
    public static class CliContestFormatter
    {
        public static void PrintContestResultDetails(ContestStatsResult result, PoffinWithRecipe[] candidates, int index, bool useColor, bool showAward)
        {
            Console.WriteLine();
            Console.WriteLine($"Result {index}: Rank {result.Rank} Poffins eaten: {result.PoffinsEaten} Rarity: {result.TotalRarityCost} Unique Berries: {result.UniqueBerries}");
            Console.WriteLine(new string('-', 75));

            int[] indices = GetIndices(result.Indices);
            for (int i = 0; i < indices.Length; i++)
            {
                int candidateIndex = indices[i];
                if ((uint)candidateIndex >= (uint)candidates.Length)
                {
                    Console.WriteLine($"* Missing candidate index {candidateIndex}");
                    continue;
                }

                ref readonly var candidate = ref candidates[candidateIndex];
                string block = CliPoffinFormatter.FormatPoffinBlock(candidate, useColor);
                Console.WriteLine(block);
                Console.WriteLine(new string('-', 75));
            }

            if (showAward)
            {
                PrintContestAward(result, useColor);
            }
        }

        public static void PrintContestAward(ContestStatsResult result, bool useColor)
        {
            const int width = 36;
            const string indent = "        ";
            string outline = useColor ? Colors.Color256(168) : string.Empty;
            string reset = useColor ? Colors.RESET : string.Empty;

            string coolLabel = FormatFlavorLabel(Flavor.Spicy, useColor);
            string dryLabel = FormatFlavorLabel(Flavor.Dry, useColor);
            string sweetLabel = FormatFlavorLabel(Flavor.Sweet, useColor);
            string bitterLabel = FormatFlavorLabel(Flavor.Bitter, useColor);
            string sourLabel = FormatFlavorLabel(Flavor.Sour, useColor);

            string ribbon = useColor ? CliFlavorStyle.ColorForFlavor(Flavor.Dry) : string.Empty;
            string starOrange = useColor ? Colors.Color256(208) : string.Empty;
            string coinGold = useColor ? Colors.Color256(226) : string.Empty;
            string coin = useColor ? $"{starOrange}O{reset}" : "O";
            Console.WriteLine($"{indent}{outline}{new string('-', width)}{reset}{ribbon}      ---------------{reset}");
            Console.WriteLine($"{indent}{outline}| ******   Contest Stats    ****** |{reset}{ribbon}     \\####|▓▓▓██|####/{reset}");
            Console.WriteLine($"{indent}{outline}{new string('-', width)}{reset}{ribbon}      \\###|█▓▓▓█|###/{reset}");
            Console.WriteLine($"{indent}{outline}| {coolLabel} ->  {FormatStatLabel("Coolness", result.Stats.Coolness, useColor),-18}{outline}|{reset}{ribbon}       `##|██▓▓▓|##'{reset}");
            Console.WriteLine($"{indent}{outline}| {dryLabel} ->  {FormatStatLabel("Beauty", result.Stats.Beauty, useColor),-18}{outline}|{reset}            {coinGold}({coin}{coinGold}){reset}");
            Console.WriteLine($"{indent}{outline}| {sweetLabel} ->  {FormatStatLabel("Cuteness", result.Stats.Cuteness, useColor),-18}{outline}|{reset}{coinGold}         .-'''''-.{reset}");
            Console.WriteLine($"{indent}{outline}| {bitterLabel} ->  {FormatStatLabel("Cleverness", result.Stats.Cleverness, useColor),-18}{outline}|{reset}{coinGold}       .'  {starOrange}* * *{coinGold}  `.{reset}");
            Console.WriteLine($"{indent}{outline}| {sourLabel} ->  {FormatStatLabel("Toughness", result.Stats.Toughness, useColor),-18}{outline}|{reset}{coinGold}      :  {starOrange}*       *{coinGold}  :{reset}");
            Console.WriteLine($"{indent}{outline}{new string('-', width)}{reset}{coinGold}     : ~ PO F F IN ~ :{reset}");
            Console.WriteLine($"{indent}{outline}| {FormatEatenSheen(result, useColor),-31}{outline}|{reset}{coinGold}     : ~ A W A R D ~ :{reset}");
            Console.WriteLine($"{indent}{outline}{new string('-', width)}{reset}      :  {starOrange}*       *{reset}  :");
            Console.WriteLine($"{indent}{outline}| {FormatRankLine(result, useColor),-31}{outline}|{reset}       `.  {starOrange}* * *{reset}  .'");
            Console.WriteLine($"{indent}{outline}{new string('-', width)}{reset}         `-.....-'");
            Console.WriteLine(new string('-', 75));
        }

        private static int[] GetIndices(PoffinIndexSet indices)
        {
            int count = indices.Count;
            var list = new int[count];
            if (count > 0) list[0] = indices.I0;
            if (count > 1) list[1] = indices.I1;
            if (count > 2) list[2] = indices.I2;
            if (count > 3) list[3] = indices.I3;
            return list;
        }

        private static string FormatStatLabel(string name, int value, bool useColor)
        {
            string valueColor = useColor ? (value >= 255 ? CliFlavorStyle.ColorForFlavor(Flavor.Bitter) : Colors.Color256(196)) : string.Empty;
            string reset = useColor ? Colors.RESET : string.Empty;
            return string.Format(CultureInfo.InvariantCulture, "{0,-14} : {1}{2,3}{3}", name, valueColor, value, reset);
        }

        private static string FormatFlavorLabel(Flavor flavor, bool useColor)
        {
            string raw = $"({flavor})";
            int pad = Math.Max(0, 8 - raw.Length);
            if (!useColor)
            {
                return raw + new string(' ', pad);
            }

            string color = CliFlavorStyle.ColorForFlavor(flavor);
            return $"{color}{raw}{Colors.RESET}{new string(' ', pad)}";
        }

        private static string FormatEatenSheen(ContestStatsResult result, bool useColor)
        {
            string reset = useColor ? Colors.RESET : string.Empty;
            string eatenWarn = useColor ? (result.PoffinsEaten > 0 ? Colors.Color256(196) : string.Empty) : string.Empty;
            string sheenColor = useColor ? (result.TotalSheen >= 255 ? Colors.BOLD : string.Empty) : string.Empty;
            return string.Format(
                CultureInfo.InvariantCulture,
                "Eaten{0}{1,6}{2}  Sheen         : {3}{4,3}{5} ",
                eatenWarn,
                result.PoffinsEaten,
                reset,
                sheenColor,
                result.TotalSheen,
                reset);
        }

        private static string FormatRankLine(ContestStatsResult result, bool useColor)
        {
            string rankColor = useColor
                ? (result.Rank == 1 ? CliFlavorStyle.ColorForFlavor(Flavor.Bitter) : result.Rank == 2 ? CliFlavorStyle.ColorForFlavor(Flavor.Spicy) : Colors.Color256(196))
                : string.Empty;
            string reset = useColor ? Colors.RESET : string.Empty;
            return string.Format(
                CultureInfo.InvariantCulture,
                "Rank : {0}{1,1}{2}     R/U        {3,2} : {4,2}  ",
                rankColor,
                result.Rank,
                reset,
                result.TotalRarityCost,
                result.UniqueBerries);
        }
    }
}
