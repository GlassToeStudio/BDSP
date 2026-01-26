using System.Globalization;
using BDSP.Core.Berries;
using BDSP.Core.CLI;

namespace BDSP.Tools
{
    /// <summary>
    /// ANSI-colored formatter for berry output (terminal display only).
    /// </summary>
    public static class BerryAnsiFormatter
    {
        private static readonly string SpicyColor = Colors.Color256(208);
        private static readonly string DryColor = Colors.Color256(39);
        private static readonly string SweetColor = Colors.Color256(212);
        private static readonly string BitterColor = Colors.Color256(40);
        private static readonly string SourColor = Colors.Color256(226);
        private static readonly string SmoothColor = Colors.Rgb(220, 220, 220);

        private static readonly string[] EmojiTable =
        [
            " 🌶️ ",
            " 🍇 ",
            " 🍑 ",
            " 🍐 ",
            " 🍋 "
        ];

        /// <summary>
        /// Formats a berry string with ANSI colors.
        /// </summary>
        public static string Format(in Berry berry)
        {
            var emoji = EmojiTable[(int)berry.MainFlavor];
            var name = BerryNames.GetName(berry.Id);
            if (name.EndsWith(" Berry", StringComparison.Ordinal))
            {
                name = name[..^6];
            }
            name = name.ToLowerInvariant();

            var mainColor = GetColor(berry.MainFlavor);
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

        private static string ColorizeValue(Flavor flavor, byte value, Flavor mainFlavor)
        {
            var color = GetColor(flavor);
            var bold = flavor == mainFlavor ? Colors.BOLD : string.Empty;
            return $"{color}{bold}{value,3}{Colors.RESET}";
        }

        private static string GetColor(Flavor flavor)
        {
            return flavor switch
            {
                Flavor.Spicy => SpicyColor,
                Flavor.Dry => DryColor,
                Flavor.Sweet => SweetColor,
                Flavor.Bitter => BitterColor,
                Flavor.Sour => SourColor,
                _ => Colors.RESET
            };
        }
    }
}
