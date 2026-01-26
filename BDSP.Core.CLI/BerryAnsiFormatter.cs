using BDSP.Core.Berries;

namespace BDSP.Core.CLI
{
    /// <summary>
    /// ANSI-colored formatter for berry output (terminal display only).
    /// </summary>
    public static class BerryAnsiFormatter
    {
        /// <summary>
        /// Formats a berry string with ANSI colors.
        /// </summary>
        public static string Format(in Berry berry)
        {
            return CliBerryLineFormatter.FormatCompact(berry);
        }
    }
}
