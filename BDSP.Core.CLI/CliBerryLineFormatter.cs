using System.Globalization;
using BDSP.Core.Berries;

namespace BDSP.Core.CLI
{
    /// <summary>
    /// Shared berry line formatters for CLI output.
    /// </summary>
    public static class CliBerryLineFormatter
    {
        private static readonly string SmoothColor = Colors.Rgb(220, 220, 220);

        /// <summary>
        /// Formats a compact, ANSI-colored berry line.
        /// </summary>
        public static string FormatCompact(in Berry berry)
        {
            var emoji = CliFlavorStyle.GetEmoji(berry.MainFlavor);
            var name = CliFlavorStyle.FormatBerryName(berry.Id);

            var mainColor = CliFlavorStyle.ColorForFlavor(berry.MainFlavor);
            var flavors = string.Format(
                CultureInfo.InvariantCulture,
                "[{0,3}, {1,3}, {2,3}, {3,3}, {4,3}]",
                ColorizeValue(Flavor.Spicy, berry.Spicy, berry.MainFlavor),
                ColorizeValue(Flavor.Dry, berry.Dry, berry.MainFlavor),
                ColorizeValue(Flavor.Sweet, berry.Sweet, berry.MainFlavor),
                ColorizeValue(Flavor.Bitter, berry.Bitter, berry.MainFlavor),
                ColorizeValue(Flavor.Sour, berry.Sour, berry.MainFlavor));

            var flavorName = $"{Colors.ITALIC}{berry.MainFlavor,-6}{Colors.N_ITALIC}";

            return string.Format(
                CultureInfo.InvariantCulture,
                "{0} {1}{2,-7}{3} {4}{5}{3} ({6,2}) - Flavors {7} {8}Smoothness:{3} {9,2} {10}Rarity:{3} {11,2}",
                emoji,
                Colors.BOLD,
                name,
                Colors.RESET,
                mainColor,
                flavorName,
                berry.MainFlavorValue,
                flavors,
                SmoothColor,
                berry.Smoothness,
                mainColor,
                berry.Rarity);
        }

        /// <summary>
        /// Formats a recipe-style berry line (used in contest result details).
        /// </summary>
        public static string FormatRecipe(in Berry berry, bool useColor)
        {
            string name = CliFlavorStyle.FormatBerryName(berry.Id);
            string emoji = CliFlavorStyle.GetEmoji(berry.MainFlavor);
            string flavorText = berry.MainFlavor.ToString();
            string flavorDisplay = useColor
                ? $"{CliFlavorStyle.ColorForFlavor(berry.MainFlavor)}{flavorText}{Colors.RESET}"
                : flavorText;
            string flavor = berry.MainFlavor.ToString();
            int flavorPad = Math.Max(0, 6 - flavor.Length);
            return string.Format(
                CultureInfo.InvariantCulture,
                "     {0} {1,-6} ({2}){3} {4,2} - Flavors [{5,3}, {6,3}, {7,3}, {8,3}, {9,3}] Rarity: {10,2}",
                emoji,
                name,
                flavorDisplay,
                new string(' ', flavorPad),
                berry.Smoothness,
                useColor ? $"{CliFlavorStyle.ColorForFlavor(Flavor.Spicy)}{berry.Spicy,3}{Colors.RESET}" : berry.Spicy.ToString(CultureInfo.InvariantCulture).PadLeft(3),
                useColor ? $"{CliFlavorStyle.ColorForFlavor(Flavor.Dry)}{berry.Dry,3}{Colors.RESET}" : berry.Dry.ToString(CultureInfo.InvariantCulture).PadLeft(3),
                useColor ? $"{CliFlavorStyle.ColorForFlavor(Flavor.Sweet)}{berry.Sweet,3}{Colors.RESET}" : berry.Sweet.ToString(CultureInfo.InvariantCulture).PadLeft(3),
                useColor ? $"{CliFlavorStyle.ColorForFlavor(Flavor.Bitter)}{berry.Bitter,3}{Colors.RESET}" : berry.Bitter.ToString(CultureInfo.InvariantCulture).PadLeft(3),
                useColor ? $"{CliFlavorStyle.ColorForFlavor(Flavor.Sour)}{berry.Sour,3}{Colors.RESET}" : berry.Sour.ToString(CultureInfo.InvariantCulture).PadLeft(3),
                berry.Rarity);
        }

        private static string ColorizeValue(Flavor flavor, byte value, Flavor mainFlavor)
        {
            var color = CliFlavorStyle.ColorForFlavor(flavor);
            var bold = flavor == mainFlavor ? Colors.BOLD : string.Empty;
            return $"{color}{bold}{value,3}{Colors.RESET}";
        }
    }
}
