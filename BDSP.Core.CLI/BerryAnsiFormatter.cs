using System.Globalization;
using BDSP.Core.Berries;

namespace BDSP.Tools
{
    /// <summary>
    /// ANSI-colored formatter for berry output (terminal display only).
    /// </summary>
    public static class BerryAnsiFormatter
    {
        private const string Esc = "\u001b[";
        private const string Reset = "\u001b[0m";
        private const string Bold = "\u001b[1m";
        private const string Italic = "\u001b[3m";
        private const string NoItalic = "\u001b[23m";

        private static string Rgb(byte r, byte g, byte b) => $"{Esc}38;2;{r};{g};{b}m";

        private static readonly string SpicyColor = Rgb(255, 127, 0);
        private static readonly string DryColor = Rgb(0, 160, 255);
        private static readonly string SweetColor = Rgb(255, 105, 180);
        private static readonly string BitterColor = Rgb(0, 200, 0);
        private static readonly string SourColor = Rgb(255, 230, 0);
        private static readonly string SmoothColor = Rgb(220, 220, 220);

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

            var flavorName = $"{Italic}{berry.MainFlavor,-6}{NoItalic}";

            return string.Format(
                CultureInfo.InvariantCulture,
                "{0} {1}{2,-7}{3} {4}{5}{3} ({6,2}) - Flavors {7} {8}Smoothness:{3} {9,2} {10}Rarity:{3} {11,2}",
                emoji,
                Bold,
                name,
                Reset,
                mainColor,
                flavorName,
                berry.Smoothness,
                flavors,
                SmoothColor,
                berry.Smoothness,
                mainColor,
                berry.Rarity);
        }

        private static string ColorizeValue(Flavor flavor, byte value, Flavor mainFlavor)
        {
            var color = GetColor(flavor);
            var bold = flavor == mainFlavor ? Bold : string.Empty;
            return $"{color}{bold}{value,3}{Reset}";
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
                _ => Reset
            };
        }
    }
}
