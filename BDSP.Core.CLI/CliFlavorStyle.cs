using BDSP.Core.Berries;

namespace BDSP.Core.CLI
{
    /// <summary>
    /// Shared emoji and flavor color helpers for CLI output.
    /// </summary>
    public static class CliFlavorStyle
    {
        /// <summary>Gets the ANSI color for a flavor.</summary>
        public static string ColorForFlavor(Flavor flavor)
        {
            return flavor switch
            {
                Flavor.Spicy => Colors.Color256(208),
                Flavor.Dry => Colors.Color256(39),
                Flavor.Sweet => Colors.Color256(212),
                Flavor.Bitter => Colors.Color256(40),
                Flavor.Sour => Colors.Color256(226),
                _ => Colors.DEFAULT
            };
        }

        /// <summary>Gets the emoji for a flavor.</summary>
        public static string GetEmoji(Flavor flavor)
        {
            return flavor switch
            {
                Flavor.Spicy => "🌶️",
                Flavor.Dry => "🍇",
                Flavor.Sweet => "🍑",
                Flavor.Bitter => "🍐",
                Flavor.Sour => "🍋",
                _ => "❔"
            };
        }

        /// <summary>Formats a berry name for CLI output.</summary>
        public static string FormatBerryName(BerryId id)
        {
            string name = BerryNames.GetName(id);
            if (name.EndsWith(" Berry", StringComparison.Ordinal))
            {
                name = name[..^6];
            }
            return name.ToLowerInvariant();
        }
    }
}
