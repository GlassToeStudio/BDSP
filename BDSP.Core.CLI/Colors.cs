using System;
using System.Globalization;
using System.Linq;

namespace BDSP.Core.CLI
{
    /// <summary>
    /// ANSI color and cursor helpers (ported from the Python colors.py).
    /// Provides methods and constants for terminal color formatting and cursor control.
    /// </summary>
    public static class Colors
    {
        /// <summary>
        /// ASCII escape character \u001b[
        /// </summary>
        public const string Esc = "\u001b[";

        #region Color and Formatting Methods

        /// <summary>
        /// Change the text to an RGB color.
        /// </summary>
        /// <param name="r">Red value (0-255)</param>
        /// <param name="g">Green value (0-255)</param>
        /// <param name="b">Blue value (0-255)</param>
        /// <returns>ESC 38;2;&lt;r&gt;;&lt;g&gt;;&lt;b&gt;m</returns>
        public static string Rgb(int r, int g, int b) => $"{Esc}38;2;{r};{g};{b}m";

        /// <summary>
        /// Change the background to an RGB color.
        /// </summary>
        /// <param name="r">Red value (0-255)</param>
        /// <param name="g">Green value (0-255)</param>
        /// <param name="b">Blue value (0-255)</param>
        /// <returns>ESC 48;2;&lt;r&gt;;&lt;g&gt;;&lt;b&gt;m</returns>
        public static string RgbBg(int r, int g, int b) => $"{Esc}48;2;{r};{g};{b}m";

        /// <summary>
        /// Change text to 1 of 256 colors.
        /// </summary>
        /// <param name="value">Color value (0-255)</param>
        /// <returns>ESC 38;5;&lt;c&gt;m</returns>
        public static string Color256(int value) => $"{Esc}38;5;{value}m";

        /// <summary>
        /// Change background to 1 of 256 colors.
        /// </summary>
        /// <param name="value">Color value (0-255)</param>
        /// <returns>ESC 48;5;&lt;c&gt;m</returns>
        public static string Color256Bg(int value) => $"{Esc}48;5;{value}m";

        /// <summary>
        /// Pass in any number of valid escape sequences and get a formatted string 
        /// with all the commands separated by semicolons.
        /// Can be used to change many things at once without calling each one individually.
        /// </summary>
        /// <param name="args">Escape sequence codes</param>
        /// <returns>ESC&lt;n1&gt;;&lt;n2&gt;;...&lt;n&gt;m</returns>
        public static string ChainSequence(params int[] args)
        {
            string chained = string.Join(";", args.Select(a => a.ToString(CultureInfo.InvariantCulture)));
            return $"{Esc}{chained}m";
        }

        #endregion

        #region Cursor Movement

        /// <summary>
        /// Move the cursor to row, column.
        /// </summary>
        /// <param name="rows">The row</param>
        /// <param name="columns">The column</param>
        /// <returns>ESC&lt;row&gt;;&lt;column&gt;H</returns>
        public static string Move(int rows, int columns) => $"{Esc}{rows};{columns}H";

        /// <summary>
        /// Move up by number of lines.
        /// </summary>
        /// <param name="lines">Number of lines to move up</param>
        /// <returns>ESC&lt;lines&gt;A</returns>
        public static string Up(int lines) => $"{Esc}{lines}A";

        /// <summary>
        /// Move down by number of lines.
        /// </summary>
        /// <param name="lines">Number of lines to move down</param>
        /// <returns>ESC&lt;lines&gt;B</returns>
        public static string Down(int lines) => $"{Esc}{lines}B";

        /// <summary>
        /// Move right by number of columns.
        /// </summary>
        /// <param name="columns">Number of columns to move right</param>
        /// <returns>ESC&lt;columns&gt;C</returns>
        public static string Right(int columns) => $"{Esc}{columns}C";

        /// <summary>
        /// Move left by number of columns.
        /// </summary>
        /// <param name="columns">Number of columns to move left</param>
        /// <returns>ESC&lt;columns&gt;D</returns>
        public static string Left(int columns) => $"{Esc}{columns}D";

        /// <summary>
        /// Moves cursor to beginning of next line, specified lines down.
        /// </summary>
        /// <param name="amount">Number of lines to move down</param>
        /// <returns>ESC&lt;amount&gt;E</returns>
        public static string StartDown(int amount) => $"{Esc}{amount}E";

        /// <summary>
        /// Moves cursor to beginning of previous line, specified lines up.
        /// </summary>
        /// <param name="amount">Number of lines to move up</param>
        /// <returns>ESC&lt;amount&gt;F</returns>
        public static string StartUp(int amount) => $"{Esc}{amount}F";

        /// <summary>
        /// Moves cursor to specified column.
        /// </summary>
        /// <param name="column">Column number to move to</param>
        /// <returns>ESC&lt;column&gt;G</returns>
        public static string MoveToColumn(int column) => $"{Esc}{column}G";

        #endregion

        #region Basic ANSI Colors

        // Basic text colors
        /// <summary>Black text 30m</summary>
        public static readonly string Black = $"{Esc}30m";
        /// <summary>Red text 31m</summary>
        public static readonly string Red = $"{Esc}31m";
        /// <summary>Green text 32m</summary>
        public static readonly string Green = $"{Esc}32m";
        /// <summary>Yellow text 33m</summary>
        public static readonly string Yellow = $"{Esc}33m";
        /// <summary>Blue text 34m</summary>
        public static readonly string Blue = $"{Esc}34m";
        /// <summary>Magenta text 35m</summary>
        public static readonly string Magenta = $"{Esc}35m";
        /// <summary>Cyan text 36m</summary>
        public static readonly string Cyan = $"{Esc}36m";
        /// <summary>White text 37m</summary>
        public static readonly string White = $"{Esc}37m";
        /// <summary>Default text color 39m</summary>
        public static readonly string Default = $"{Esc}39m";

        // Bright text colors
        /// <summary>Bright black 90m</summary>
        public static readonly string BlackBright = $"{Esc}90m";
        /// <summary>Bright red 91m</summary>
        public static readonly string RedBright = $"{Esc}91m";
        /// <summary>Bright green 92m</summary>
        public static readonly string GreenBright = $"{Esc}92m";
        /// <summary>Bright yellow 93m</summary>
        public static readonly string YellowBright = $"{Esc}93m";
        /// <summary>Bright blue 94m</summary>
        public static readonly string BlueBright = $"{Esc}94m";
        /// <summary>Bright magenta 95m</summary>
        public static readonly string MagentaBright = $"{Esc}95m";
        /// <summary>Bright cyan 96m</summary>
        public static readonly string CyanBright = $"{Esc}96m";
        /// <summary>Bright white 97m</summary>
        public static readonly string WhiteBright = $"{Esc}97m";

        // Background colors
        /// <summary>Black background 40m</summary>
        public static readonly string BlackBg = $"{Esc}40m";
        /// <summary>Red background 41m</summary>
        public static readonly string RedBg = $"{Esc}41m";
        /// <summary>Green background 42m</summary>
        public static readonly string GreenBg = $"{Esc}42m";
        /// <summary>Yellow background 43m</summary>
        public static readonly string YellowBg = $"{Esc}43m";
        /// <summary>Blue background 44m</summary>
        public static readonly string BlueBg = $"{Esc}44m";
        /// <summary>Magenta background 45m</summary>
        public static readonly string MagentaBg = $"{Esc}45m";
        /// <summary>Cyan background 46m</summary>
        public static readonly string CyanBg = $"{Esc}46m";
        /// <summary>White background 47m</summary>
        public static readonly string WhiteBg = $"{Esc}47m";
        /// <summary>Default background color 49m</summary>
        public static readonly string DefaultBg = $"{Esc}49m";

        // Bright background colors
        /// <summary>Bright black background 100m</summary>
        public static readonly string BlackBrightBg = $"{Esc}100m";
        /// <summary>Bright red background 101m</summary>
        public static readonly string RedBrightBg = $"{Esc}101m";
        /// <summary>Bright green background 102m</summary>
        public static readonly string GreenBrightBg = $"{Esc}102m";
        /// <summary>Bright yellow background 103m</summary>
        public static readonly string YellowBrightBg = $"{Esc}103m";
        /// <summary>Bright blue background 104m</summary>
        public static readonly string BlueBrightBg = $"{Esc}104m";
        /// <summary>Bright magenta background 105m</summary>
        public static readonly string MagentaBrightBg = $"{Esc}105m";
        /// <summary>Bright cyan background 106m</summary>
        public static readonly string CyanBrightBg = $"{Esc}106m";
        /// <summary>Bright white background 107m</summary>
        public static readonly string WhiteBrightBg = $"{Esc}107m";

        // Text formatting
        /// <summary>Bold text on 1m</summary>
        public static readonly string Bold = $"{Esc}1m";
        /// <summary>Bold text off 22m (also turns off Dim)</summary>
        public static readonly string NoBold = $"{Esc}22m";
        /// <summary>Dim text on 2m</summary>
        public static readonly string Dim = $"{Esc}2m";
        /// <summary>Dim text off 22m (also turns off Bold)</summary>
        public static readonly string NoDim = $"{Esc}22m";
        /// <summary>Italic text on 3m</summary>
        public static readonly string Italic = $"{Esc}3m";
        /// <summary>Italic text off 23m</summary>
        public static readonly string NoItalic = $"{Esc}23m";
        /// <summary>Underline text on 4m</summary>
        public static readonly string Underline = $"{Esc}4m";
        /// <summary>Underline text off 24m</summary>
        public static readonly string NoUnderline = $"{Esc}24m";
        /// <summary>Slow blink on 5m</summary>
        public static readonly string SlowBlink = $"{Esc}5m";
        /// <summary>Slow blink off 25m</summary>
        public static readonly string NoSlowBlink = $"{Esc}25m";
        /// <summary>Rapid blink on 6m</summary>
        public static readonly string RapidBlink = $"{Esc}6m";
        /// <summary>Rapid blink off 26m</summary>
        public static readonly string NoRapidBlink = $"{Esc}26m";
        /// <summary>Strikethrough text on 9m</summary>
        public static readonly string StrikeThrough = $"{Esc}9m";
        /// <summary>Strikethrough text off 29m</summary>
        public static readonly string NoStrikeThrough = $"{Esc}29m";

        // Special formatting
        /// <summary>Reset all styles 0m</summary>
        public static readonly string Reset = $"{Esc}0m";
        /// <summary>Cursor invisible ?25l</summary>
        public static readonly string CursorInvisible = $"{Esc}?25l";
        /// <summary>Cursor visible ?25h</summary>
        public static readonly string CursorVisible = $"{Esc}?25h";
        /// <summary>Hide text 8m</summary>
        public static readonly string Hide = $"{Esc}8m";
        /// <summary>Unhide text 28m</summary>
        public static readonly string NoHide = $"{Esc}28m";
        /// <summary>Move to home position (1,1 or 0,0) H</summary>
        public static readonly string Home = $"{Esc}H";

        #endregion

        #region Named RGB Colors - Rainbow Spectrum
        /// <summary>Calls Rgb(255, 0, 0)</summary>
        public static readonly string RgbRed = Rgb(255, 0, 0);
        /// <summary>Calls Rgb(255, 127, 0)</summary>
        public static readonly string RgbOrange = Rgb(255, 127, 0);
        /// <summary>Calls Rgb(255, 255, 0)</summary>
        public static readonly string RgbYellow = Rgb(255, 255, 0);
        /// <summary>Calls Rgb(0, 127, 0)</summary>
        public static readonly string RgbGreen = Rgb(0, 127, 0);
        /// <summary>Calls Rgb(0, 0, 255)</summary>
        public static readonly string RgbBlue = Rgb(0, 0, 255);
        /// <summary>Calls Rgb(75, 0, 130)</summary>
        public static readonly string RgbIndigo = Rgb(75, 0, 130);
        /// <summary>Calls Rgb(238, 130, 238)</summary>
        public static readonly string RgbViolet = Rgb(238, 130, 238);

        #endregion

        #region Named RGB Colors - Purple
        /// <summary>Calls Rgb(128, 0, 128)</summary>
        public static readonly string RgbPurple = Rgb(128, 0, 128);
        /// <summary>Calls Rgb(139, 0, 139)</summary>
        public static readonly string RgbDarkMagenta = Rgb(139, 0, 139);
        /// <summary>Calls Rgb(148, 0, 211)</summary>
        public static readonly string RgbDarkViolet = Rgb(148, 0, 211);
        /// <summary>Calls Rgb(72, 61, 139)</summary>
        public static readonly string RgbDarkSlateBlue = Rgb(72, 61, 139);
        /// <summary>Calls Rgb(138, 43, 226)</summary>
        public static readonly string RgbBlueViolet = Rgb(138, 43, 226);
        /// <summary>Calls Rgb(153, 50, 204)</summary>
        public static readonly string RgbDarkOrchid = Rgb(153, 50, 204);
        /// <summary>Calls Rgb(255, 0, 255)</summary>
        public static readonly string RgbFuchsia = Rgb(255, 0, 255);
        /// <summary>Calls Rgb(106, 90, 205)</summary>
        public static readonly string RgbSlateBlue = Rgb(106, 90, 205);
        /// <summary>Calls Rgb(123, 104, 238)</summary>
        public static readonly string RgbMediumSlateBlue = Rgb(123, 104, 238);
        /// <summary>Calls Rgb(186, 85, 211)</summary>
        public static readonly string RgbMediumOrchid = Rgb(186, 85, 211);
        /// <summary>Calls Rgb(147, 112, 219)</summary>
        public static readonly string RgbMediumPurple = Rgb(147, 112, 219);
        /// <summary>Calls Rgb(218, 112, 214)</summary>
        public static readonly string RgbOrchid = Rgb(218, 112, 214);
        /// <summary>Calls Rgb(221, 160, 221)</summary>
        public static readonly string RgbPlum = Rgb(221, 160, 221);
        /// <summary>Calls Rgb(216, 191, 216)</summary>
        public static readonly string RgbThistle = Rgb(216, 191, 216);
        /// <summary>Calls Rgb(230, 230, 250)</summary>
        public static readonly string RgbLavender = Rgb(230, 230, 250);

        #endregion

        #region Named RGB Colors - Pink
        /// <summary>Calls Rgb(199, 21, 133)</summary>
        public static readonly string RgbMediumVioletRed = Rgb(199, 21, 133);
        /// <summary>Calls Rgb(255, 20, 147)</summary>
        public static readonly string RgbDeepPink = Rgb(255, 20, 147);
        /// <summary>Calls Rgb(219, 112, 147)</summary>
        public static readonly string RgbPaleVioletRed = Rgb(219, 112, 147);
        /// <summary>Calls Rgb(255, 105, 180)</summary>
        public static readonly string RgbHotPink = Rgb(255, 105, 180);
        /// <summary>Calls Rgb(255, 182, 193)</summary>
        public static readonly string RgbLightPink = Rgb(255, 182, 193);
        /// <summary>Calls Rgb(255, 192, 203)</summary>
        public static readonly string RgbPink = Rgb(255, 192, 203);

        #endregion

        #region Named RGB Colors - Red
        public static readonly string RgbMaroon = Rgb(128, 0, 0);
        public static readonly string RgbDarkRed = Rgb(139, 0, 0);
        public static readonly string RgbFireBrick = Rgb(178, 34, 34);
        public static readonly string RgbCrimson = Rgb(220, 20, 60);
        public static readonly string RgbIndianRed = Rgb(205, 92, 92);
        public static readonly string RgbLightCoral = Rgb(240, 128, 128);
        public static readonly string RgbSalmon = Rgb(250, 128, 114);
        public static readonly string RgbDarkSalmon = Rgb(233, 150, 122);
        public static readonly string RgbLightSalmon = Rgb(255, 160, 122);
       
        #endregion

        // Orange colors
        public static readonly string RgbOrangeRed = Rgb(255, 69, 0);
        public static readonly string RgbTomato = Rgb(255, 99, 71);
        public static readonly string RgbDarkOrange = Rgb(255, 140, 0);
        public static readonly string RgbCoral = Rgb(255, 127, 80);
        public static readonly string RgbLightOrange = Rgb(255, 165, 0);

        // Yellow colors
        public static readonly string RgbDarkKhaki = Rgb(189, 183, 107);
        public static readonly string RgbGold = Rgb(255, 215, 0);
        public static readonly string RgbKhaki = Rgb(240, 230, 140);
        public static readonly string RgbPeachPuff = Rgb(255, 218, 185);
        public static readonly string RgbPaleGoldenrod = Rgb(238, 232, 170);
        public static readonly string RgbMoccasin = Rgb(255, 228, 181);
        public static readonly string RgbPapayaWhip = Rgb(255, 239, 213);
        public static readonly string RgbLightGoldenrodYellow = Rgb(250, 250, 210);
        public static readonly string RgbLemonChiffon = Rgb(255, 250, 205);
        public static readonly string RgbLightYellow = Rgb(255, 255, 224);

        // Brown colors
        public static readonly string RgbBrown = Rgb(165, 42, 42);
        public static readonly string RgbSaddleBrown = Rgb(139, 69, 19);
        public static readonly string RgbSienna = Rgb(160, 82, 45);
        public static readonly string RgbChocolate = Rgb(210, 105, 30);
        public static readonly string RgbDarkGoldenrod = Rgb(184, 134, 11);
        public static readonly string RgbPeru = Rgb(205, 133, 63);
        public static readonly string RgbRosyBrown = Rgb(188, 143, 143);
        public static readonly string RgbGoldenrod = Rgb(218, 165, 32);
        public static readonly string RgbSandyBrown = Rgb(244, 164, 96);
        public static readonly string RgbTan = Rgb(210, 180, 140);
        public static readonly string RgbBurlyWood = Rgb(222, 184, 135);
        public static readonly string RgbWheat = Rgb(245, 222, 179);
        public static readonly string RgbNavajoWhite = Rgb(255, 222, 173);
        public static readonly string RgbBisque = Rgb(255, 228, 196);
        public static readonly string RgbBlanchedAlmond = Rgb(255, 235, 205);
        public static readonly string RgbCornsilk = Rgb(255, 248, 220);

        // Green colors
        public static readonly string RgbDarkGreen = Rgb(0, 100, 0);
        public static readonly string RgbDarkOliveGreen = Rgb(85, 107, 47);
        public static readonly string RgbForestGreen = Rgb(34, 139, 34);
        public static readonly string RgbSeaGreen = Rgb(46, 139, 87);
        public static readonly string RgbOlive = Rgb(128, 128, 0);
        public static readonly string RgbOliveDrab = Rgb(107, 142, 35);
        public static readonly string RgbMediumSeaGreen = Rgb(60, 179, 113);
        public static readonly string RgbLimeGreen = Rgb(50, 205, 50);
        public static readonly string RgbLime = Rgb(0, 255, 0);
        public static readonly string RgbSpringGreen = Rgb(0, 255, 127);
        public static readonly string RgbMediumSpringGreen = Rgb(0, 250, 154);
        public static readonly string RgbDarkSeaGreen = Rgb(143, 188, 143);
        public static readonly string RgbMediumAquamarine = Rgb(102, 205, 170);
        public static readonly string RgbYellowGreen = Rgb(154, 205, 50);
        public static readonly string RgbLawnGreen = Rgb(124, 252, 0);
        public static readonly string RgbChartreuse = Rgb(127, 255, 0);
        public static readonly string RgbLightGreen = Rgb(144, 238, 144);
        public static readonly string RgbGreenYellow = Rgb(173, 255, 47);
        public static readonly string RgbPaleGreen = Rgb(152, 251, 152);

        // Cyan colors
        public static readonly string RgbTeal = Rgb(0, 128, 128);
        public static readonly string RgbDarkCyan = Rgb(0, 139, 139);
        public static readonly string RgbLightSeaGreen = Rgb(32, 178, 170);
        public static readonly string RgbCadetBlue = Rgb(95, 158, 160);
        public static readonly string RgbDarkTurquoise = Rgb(0, 206, 209);
        public static readonly string RgbMediumTurquoise = Rgb(72, 209, 204);
        public static readonly string RgbTurquoise = Rgb(64, 224, 208);
        public static readonly string RgbAqua = Rgb(0, 255, 255);
        public static readonly string RgbCyan = Rgb(0, 255, 255);
        public static readonly string RgbAquamarine = Rgb(127, 255, 212);
        public static readonly string RgbPaleTurquoise = Rgb(175, 238, 238);
        public static readonly string RgbLightCyan = Rgb(224, 255, 255);

        // Blue colors
        public static readonly string RgbNavy = Rgb(0, 0, 128);
        public static readonly string RgbDarkBlue = Rgb(0, 0, 139);
        public static readonly string RgbMediumBlue = Rgb(0, 0, 205);
        public static readonly string RgbMidnightBlue = Rgb(25, 25, 112);
        public static readonly string RgbRoyalBlue = Rgb(65, 105, 225);
        public static readonly string RgbSteelBlue = Rgb(70, 130, 180);
        public static readonly string RgbDodgerBlue = Rgb(30, 144, 255);
        public static readonly string RgbDeepSkyBlue = Rgb(0, 191, 255);
        public static readonly string RgbCornflowerBlue = Rgb(100, 149, 237);
        public static readonly string RgbSkyBlue = Rgb(135, 206, 235);
        public static readonly string RgbLightSkyBlue = Rgb(135, 206, 250);
        public static readonly string RgbLightSteelBlue = Rgb(176, 196, 222);
        public static readonly string RgbLightBlue = Rgb(173, 216, 230);
        public static readonly string RgbPowderBlue = Rgb(176, 224, 230);

        // White colors
        public static readonly string RgbMistyRose = Rgb(255, 228, 225);
        public static readonly string RgbAntiqueWhite = Rgb(250, 235, 215);
        public static readonly string RgbLinen = Rgb(250, 240, 230);
        public static readonly string RgbBeige = Rgb(245, 245, 220);
        public static readonly string RgbWhiteSmoke = Rgb(245, 245, 245);
        public static readonly string RgbLavenderBlush = Rgb(255, 240, 245);
        public static readonly string RgbOldLace = Rgb(253, 245, 230);
        public static readonly string RgbAliceBlue = Rgb(240, 248, 255);
        public static readonly string RgbSeashell = Rgb(255, 245, 238);
        public static readonly string RgbGhostWhite = Rgb(248, 248, 255);
        public static readonly string RgbHoneydew = Rgb(240, 255, 240);
        public static readonly string RgbFloralWhite = Rgb(255, 250, 240);
        public static readonly string RgbAzure = Rgb(240, 255, 255);
        public static readonly string RgbMintCream = Rgb(245, 255, 250);
        public static readonly string RgbSnow = Rgb(255, 250, 250);
        public static readonly string RgbIvory = Rgb(255, 255, 240);
        public static readonly string RgbWhite = Rgb(255, 255, 255);

        // Gray and black
        public static readonly string RgbBlack = Rgb(0, 0, 0);
        public static readonly string RgbDarkSlateGray = Rgb(47, 79, 79);
        public static readonly string RgbDimGray = Rgb(105, 105, 105);
        public static readonly string RgbSlateGray = Rgb(112, 128, 144);
        public static readonly string RgbGray = Rgb(128, 128, 128);
        public static readonly string RgbLightSlateGray = Rgb(119, 136, 153);
        public static readonly string RgbDarkGray = Rgb(169, 169, 169);
        public static readonly string RgbSilver = Rgb(192, 192, 192);
        public static readonly string RgbLightGray = Rgb(211, 211, 211);
        public static readonly string RgbGainsboro = Rgb(220, 220, 220);
    }
}