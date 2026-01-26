using System.Globalization;
using System.Text;
using BDSP.Core.Berries;
using BDSP.Core.Poffins;
using BDSP.Core.Poffins.Filters;
using BDSP.Core.Optimization.Core;
namespace BDSP.Core.CLI
{
    /// <summary>
    /// CLI formatter for poffins and their recipes.
    /// </summary>
    public static class CliPoffinFormatter
    {
        /// <summary>
        /// Formats a poffin block with its berry recipe.
        /// </summary>
        public static string FormatPoffinBlock(in PoffinWithRecipe poffin, bool useColor)
        {
            var builder = new StringBuilder();
            builder.AppendLine(FormatPoffinHeader(poffin.Poffin, poffin.Recipe.Berries, useColor));
            builder.AppendLine(FormatPoffinFlavorLine(poffin.Poffin, useColor));
            builder.AppendLine("* Berries used:");
            for (int i = 0; i < poffin.Recipe.Berries.Length; i++)
            {
                ref readonly Berry berry = ref BerryTable.Get(poffin.Recipe.Berries[i]);
                builder.AppendLine(CliBerryLineFormatter.FormatRecipe(berry, useColor));
            }
            return builder.ToString().TrimEnd();
        }

        /// <summary>
        /// Formats a one-line poffin header.
        /// </summary>
        public static string FormatPoffinHeader(in Poffin poffin, BerryId[] recipe, bool useColor)
        {
            int recipeRarity = ComputeRecipeRarity(recipe);
            string name = FormatPoffinName(poffin, useColor);
            return string.Format(
                CultureInfo.InvariantCulture,
                "{0,3} - {1} {2,2} - Flavors [{3,3}, {4,3}, {5,3}, {6,3}, {7,3}] Rarity: {8,2}",
                poffin.Level,
                name,
                poffin.Smoothness,
                useColor ? $"{CliFlavorStyle.ColorForFlavor(Flavor.Spicy)}{poffin.Spicy,3}{Colors.RESET}" : poffin.Spicy.ToString(CultureInfo.InvariantCulture).PadLeft(3),
                useColor ? $"{CliFlavorStyle.ColorForFlavor(Flavor.Dry)}{poffin.Dry,3}{Colors.RESET}" : poffin.Dry.ToString(CultureInfo.InvariantCulture).PadLeft(3),
                useColor ? $"{CliFlavorStyle.ColorForFlavor(Flavor.Sweet)}{poffin.Sweet,3}{Colors.RESET}" : poffin.Sweet.ToString(CultureInfo.InvariantCulture).PadLeft(3),
                useColor ? $"{CliFlavorStyle.ColorForFlavor(Flavor.Bitter)}{poffin.Bitter,3}{Colors.RESET}" : poffin.Bitter.ToString(CultureInfo.InvariantCulture).PadLeft(3),
                useColor ? $"{CliFlavorStyle.ColorForFlavor(Flavor.Sour)}{poffin.Sour,3}{Colors.RESET}" : poffin.Sour.ToString(CultureInfo.InvariantCulture).PadLeft(3),
                recipeRarity);
        }

        /// <summary>
        /// Formats the main/secondary flavor line for a poffin.
        /// </summary>
        public static string FormatPoffinFlavorLine(in Poffin poffin, bool useColor)
        {
            if (poffin.SecondaryFlavor == Flavor.None)
            {
                return string.Format(
                    CultureInfo.InvariantCulture,
                    "* {0} ({1})",
                    poffin.MainFlavor,
                    poffin.Level);
            }

            return string.Format(
                CultureInfo.InvariantCulture,
                "* {0,-6} {1,-8} ({2,3}, {3,2})",
                useColor ? $"{CliFlavorStyle.ColorForFlavor(poffin.MainFlavor)}{poffin.MainFlavor}{Colors.RESET}" : poffin.MainFlavor.ToString(),
                useColor ? $"{CliFlavorStyle.ColorForFlavor(poffin.SecondaryFlavor)}{poffin.SecondaryFlavor}{Colors.RESET}" : poffin.SecondaryFlavor.ToString(),
                poffin.Level,
                poffin.SecondLevel);
        }

        private static int ComputeRecipeRarity(BerryId[] berries)
        {
            int total = 0;
            for (int i = 0; i < berries.Length; i++)
            {
                ref readonly Berry berry = ref BerryTable.Get(berries[i]);
                total += berry.Rarity;
            }
            return total;
        }

        private static string FormatPoffinName(in Poffin poffin, bool useColor)
        {
            PoffinNameKind kind = PoffinFilter.GetNameKind(in poffin);
            string plain = kind switch
            {
                PoffinNameKind.Foul => "FOUL POFFIN",
                PoffinNameKind.SuperMild => "SUPER MILD POFFIN",
                PoffinNameKind.Mild => "MILD POFFIN",
                PoffinNameKind.Rich => "RICH POFFIN",
                PoffinNameKind.Overripe => "OVERRIPE POFFIN",
                _ => "POFFIN"
            };

            if (!useColor)
            {
                return plain.PadRight(17);
            }

            string padded = plain.PadRight(17);
            return kind switch
            {
                PoffinNameKind.Foul => $"{Colors.BOLD}{Colors.Color256(237)}{padded}{Colors.RESET}",
                PoffinNameKind.SuperMild => $"{FormatSuperMildPoffin()}{new string(' ', Math.Max(0, 17 - plain.Length))}",
                PoffinNameKind.Mild => $"{Colors.BOLD}{Colors.Color256(11)}{padded}{Colors.RESET}",
                PoffinNameKind.Rich => $"{Colors.BOLD}{Colors.Color256(247)}{padded}{Colors.RESET}",
                PoffinNameKind.Overripe => $"{Colors.BOLD}{Colors.Color256(242)}{padded}{Colors.RESET}",
                _ => padded
            };
        }

        private static string FormatSuperMildPoffin()
        {
            return $"{Colors.BOLD}{Colors.RGB_RED}SU{Colors.RGB_ORANGE}PER {Colors.RGB_YELLOW}MI{Colors.RGB_GREEN}LD {Colors.RGB_BLUE}PO{Colors.RGB_DARK_VIOLET}FF{Colors.RGB_VIOLET}IN{Colors.RESET}";
        }
    }
}
