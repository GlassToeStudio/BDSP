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
        public static string Rgb(byte r, byte g, byte b) => $"{Esc}38;2;{r};{g};{b}m";

        /// <summary>
        /// Change the background to an RGB color.
        /// </summary>
        /// <param name="r">Red value (0-255)</param>
        /// <param name="g">Green value (0-255)</param>
        /// <param name="b">Blue value (0-255)</param>
        /// <returns>ESC 48;2;&lt;r&gt;;&lt;g&gt;;&lt;b&gt;m</returns>
        public static string RgbBg(byte r, byte g, byte b) => $"{Esc}48;2;{r};{g};{b}m";

        /// <summary>
        /// Change text to 1 of 256 colors.
        /// </summary>
        /// <param name="value">Color value (0-255)</param>
        /// <returns>ESC 38;5;&lt;c&gt;m</returns>
        public static string Color256(byte value) => $"{Esc}38;5;{value}m";

        /// <summary>
        /// Change background to 1 of 256 colors.
        /// </summary>
        /// <param name="value">Color value (0-255)</param>
        /// <returns>ESC 48;5;&lt;c&gt;m</returns>
        public static string Color256Bg(byte value) => $"{Esc}48;5;{value}m";

        /// <summary>
        /// Pass in any number of valid escape sequences and get a formatted string 
        /// with all the commands separated by semicolons.
        /// Can be used to change many things at once without calling each one individually.
        /// </summary>
        /// <param name="args">Escape sequence codes</param>
        /// <returns>ESC&lt;n1&gt;;&lt;n2&gt;;...&lt;n&gt;m</returns>
        public static string ChainSequence(params byte[] args)
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
        public static string Move(byte rows, byte columns) => $"{Esc}{rows};{columns}H";

        /// <summary>
        /// Move up by number of lines.
        /// </summary>
        /// <param name="lines">Number of lines to move up</param>
        /// <returns>ESC&lt;lines&gt;A</returns>
        public static string Up(byte lines) => $"{Esc}{lines}A";

        /// <summary>
        /// Move down by number of lines.
        /// </summary>
        /// <param name="lines">Number of lines to move down</param>
        /// <returns>ESC&lt;lines&gt;B</returns>
        public static string Down(byte lines) => $"{Esc}{lines}B";

        /// <summary>
        /// Move right by number of columns.
        /// </summary>
        /// <param name="columns">Number of columns to move right</param>
        /// <returns>ESC&lt;columns&gt;C</returns>
        public static string Right(byte columns) => $"{Esc}{columns}C";

        /// <summary>
        /// Move left by number of columns.
        /// </summary>
        /// <param name="columns">Number of columns to move left</param>
        /// <returns>ESC&lt;columns&gt;D</returns>
        public static string Left(byte columns) => $"{Esc}{columns}D";

        /// <summary>
        /// Moves cursor to beginning of next line, specified lines down.
        /// </summary>
        /// <param name="amount">Number of lines to move down</param>
        /// <returns>ESC&lt;amount&gt;E</returns>
        public static string StartDown(byte amount) => $"{Esc}{amount}E";

        /// <summary>
        /// Moves cursor to beginning of previous line, specified lines up.
        /// </summary>
        /// <param name="amount">Number of lines to move up</param>
        /// <returns>ESC&lt;amount&gt;F</returns>
        public static string StartUp(byte amount) => $"{Esc}{amount}F";

        /// <summary>
        /// Moves cursor to specified column.
        /// </summary>
        /// <param name="column">Column number to move to</param>
        /// <returns>ESC&lt;column&gt;G</returns>
        public static string MoveToColumn(byte column) => $"{Esc}{column}G";
        #endregion

        #region Basic ANSI Colors and Styles
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
        /// <summary>
        /// <b>Red</b>
        /// </summary>
        /// <remarks>A primary, vibrant, and pure red, representing passion, energy, and intensity.</remarks>
        /// <returns>Rgb(255, 0, 0)</returns>
        public static readonly string RgbRed = Rgb(255, 0, 0);

        /// <summary>
        /// <b>Orange</b>
        /// </summary>
        /// <remarks>A bright, warm, and energetic color between red and yellow, often associated with enthusiasm and creativity.</remarks>
        /// <returns>Rgb(255, 127, 0)</returns>
        public static readonly string RgbOrange = Rgb(255, 127, 0);

        /// <summary>
        /// <b>Yellow</b>
        /// </summary>
        /// <remarks>A primary, bright, and sunny yellow, evoking feelings of happiness, warmth, and optimism.</remarks>
        /// <returns>Rgb(255, 255, 0)</returns>
        public static readonly string RgbYellow = Rgb(255, 255, 0);

        /// <summary>
        /// <b>Green</b>
        /// </summary>
        /// <remarks>A standard, balanced green, reminiscent of nature, growth, and harmony.</remarks>
        /// <returns>Rgb(0, 127, 0)</returns>
        public static readonly string RgbGreen = Rgb(0, 127, 0);

        /// <summary>
        /// <b>Blue</b>
        /// </summary>
        /// <remarks>A primary, pure, and vivid blue, often linked to calmness, stability, and the sky or sea.</remarks>
        /// <returns>Rgb(0, 0, 255)</returns>
        public static readonly string RgbBlue = Rgb(0, 0, 255);

        /// <summary>
        /// <b>Indigo</b>
        /// </summary>
        /// <remarks>A deep, rich blue-violet color, conveying depth, wisdom, and intuition.</remarks>
        /// <returns>Rgb(75, 0, 130)</returns>
        public static readonly string RgbIndigo = Rgb(75, 0, 130);

        /// <summary>
        /// <b>Violet</b>
        /// </summary>
        /// <remarks>A soft, light purple with pinkish undertones, suggesting gentleness and romance.</remarks>
        /// <returns>Rgb(238, 130, 238)</returns>
        public static readonly string RgbViolet = Rgb(238, 130, 238);
        #endregion

        #region Named RGB Colors - Purple
        /// <summary>
        /// <b>Purple</b>
        /// </summary>
        /// <remarks>A balanced, classic purple, traditionally associated with royalty, luxury, and ambition.</remarks>
        /// <returns>Rgb(128, 0, 128)</returns>
        public static readonly string RgbPurple = Rgb(128, 0, 128);

        /// <summary>
        /// <b>Dark Magenta</b>
        /// </summary>
        /// <remarks>A deep, rich magenta-purple, exuding a sense of sophistication and artistic flair.</remarks>
        /// <returns>Rgb(139, 0, 139)</returns>
        public static readonly string RgbDarkMagenta = Rgb(139, 0, 139);

        /// <summary>
        /// <b>Dark Violet</b>
        /// </summary>
        /// <remarks>A deep, intense shade of violet, conveying mystery and depth.</remarks>
        /// <returns>Rgb(148, 0, 211)</returns>
        public static readonly string RgbDarkViolet = Rgb(148, 0, 211);

        /// <summary>
        /// <b>Dark Slate Blue</b>
        /// </summary>
        /// <remarks>A dark, muted blue with strong gray undertones, suggesting formality and stability.</remarks>
        /// <returns>Rgb(72, 61, 139)</returns>
        public static readonly string RgbDarkSlateBlue = Rgb(72, 61, 139);

        /// <summary>
        /// <b>Blue Violet</b>
        /// </summary>
        /// <remarks>A vibrant color that is a mix of blue and violet, radiating energy and creativity.</remarks>
        /// <returns>Rgb(138, 43, 226)</returns>
        public static readonly string RgbBlueViolet = Rgb(138, 43, 226);

        /// <summary>
        /// <b>Dark Orchid</b>
        /// </summary>
        /// <remarks>A deep, vivid pinkish-purple, named after the orchid flower.</remarks>
        /// <returns>Rgb(153, 50, 204)</returns>
        public static readonly string RgbDarkOrchid = Rgb(153, 50, 204);

        /// <summary>
        /// <b>Fuchsia</b>
        /// </summary>
        /// <remarks>A vivid, purplish-pink color, often used to express confidence and boldness.</remarks>
        /// <returns>Rgb(255, 0, 255)</returns>
        public static readonly string RgbFuchsia = Rgb(255, 0, 255);

        /// <summary>
        /// <b>Slate Blue</b>
        /// </summary>
        /// <remarks>A medium, muted blue with gray undertones, creating a calm and sophisticated feel.</remarks>
        /// <returns>Rgb(106, 90, 205)</returns>
        public static readonly string RgbSlateBlue = Rgb(106, 90, 205);

        /// <summary>
        /// <b>Medium Slate Blue</b>
        /// </summary>
        /// <remarks>A vibrant, medium shade of slate blue that is both energetic and soothing.</remarks>
        /// <returns>Rgb(123, 104, 238)</returns>
        public static readonly string RgbMediumSlateBlue = Rgb(123, 104, 238);

        /// <summary>
        /// <b>Medium Orchid</b>
        /// </summary>
        /// <remarks>A medium, bright pinkish-purple, balancing the energy of magenta with the softness of orchid.</remarks>
        /// <returns>Rgb(186, 85, 211)</returns>
        public static readonly string RgbMediumOrchid = Rgb(186, 85, 211);

        /// <summary>
        /// <b>Medium Purple</b>
        /// </summary>
        /// <remarks>A soft, medium shade of purple that is gentle and approachable.</remarks>
        /// <returns>Rgb(147, 112, 219)</returns>
        public static readonly string RgbMediumPurple = Rgb(147, 112, 219);

        /// <summary>
        /// <b>Orchid</b>
        /// </summary>
        /// <remarks>A light, bright pinkish-purple, capturing the delicate beauty of its namesake flower.</remarks>
        /// <returns>Rgb(218, 112, 214)</returns>
        public static readonly string RgbOrchid = Rgb(218, 112, 214);

        /// <summary>
        /// <b>Plum</b>
        /// </summary>
        /// <remarks>A pale, dusty purple with pink undertones, like the skin of a ripe plum.</remarks>
        /// <returns>Rgb(221, 160, 221)</returns>
        public static readonly string RgbPlum = Rgb(221, 160, 221);

        /// <summary>
        /// <b>Thistle</b>
        /// </summary>
        /// <remarks>A very pale, grayish-violet, reminiscent of the soft, downy part of a thistle flower.</remarks>
        /// <returns>Rgb(216, 191, 216)</returns>
        public static readonly string RgbThistle = Rgb(216, 191, 216);

        /// <summary>
        /// <b>Lavender</b>
        /// </summary>
        /// <remarks>A light, pale purple with a bluish tint, known for its calming and fragrant associations.</remarks>
        /// <returns>Rgb(230, 230, 250)</returns>
        public static readonly string RgbLavender = Rgb(230, 230, 250);
        #endregion

        #region Named RGB Colors - Pink
        /// <summary>
        /// <b>Medium Violet Red</b>
        /// </summary>
        /// <remarks>A deep, rich pinkish-red that balances the intensity of red with the softness of violet.</remarks>
        /// <returns>Rgb(199, 21, 133)</returns>
        public static readonly string RgbMediumVioletRed = Rgb(199, 21, 133);

        /// <summary>
        /// <b>Deep Pink</b>
        /// </summary>
        /// <remarks>A highly saturated, vibrant pink that is bold and energetic.</remarks>
        /// <returns>Rgb(255, 20, 147)</returns>
        public static readonly string RgbDeepPink = Rgb(255, 20, 147);

        /// <summary>
        /// <b>Pale Violet Red</b>
        /// </summary>
        /// <remarks>A medium, muted pink with a hint of violet, softer than Medium Violet Red.</remarks>
        /// <returns>Rgb(219, 112, 147)</returns>
        public static readonly string RgbPaleVioletRed = Rgb(219, 112, 147);

        /// <summary>
        /// <b>Hot Pink</b>
        /// </summary>
        /// <remarks>A bright, vivid, and warm pink, often associated with fun and flamboyance.</remarks>
        /// <returns>Rgb(255, 105, 180)</returns>
        public static readonly string RgbHotPink = Rgb(255, 105, 180);

        /// <summary>
        /// <b>Light Pink</b>
        /// </summary>
        /// <remarks>A soft, pale, and classic pink, conveying gentleness and innocence.</remarks>
        /// <returns>Rgb(255, 182, 193)</returns>
        public static readonly string RgbLightPink = Rgb(255, 182, 193);

        /// <summary>
        /// <b>Pink</b>
        /// </summary>
        /// <remarks>A standard, soft pink, universally recognized and gentle in appearance.</remarks>
        /// <returns>Rgb(255, 192, 203)</returns>
        public static readonly string RgbPink = Rgb(255, 192, 203);
        #endregion

        #region Named RGB Colors - Red
        /// <summary>
        /// <b>Maroon</b>
        /// </summary>
        /// <remarks>A dark brownish-red, conveying a sense of richness and sophistication.</remarks>
        /// <returns>Rgb(128, 0, 0)</returns>
        public static readonly string RgbMaroon = Rgb(128, 0, 0);

        /// <summary>
        /// <b>Dark Red</b>
        /// </summary>
        /// <remarks>A deep, intense red, suggesting strength and power.</remarks>
        /// <returns>Rgb(139, 0, 0)</returns>
        public static readonly string RgbDarkRed = Rgb(139, 0, 0);

        /// <summary>
        /// <b>Fire Brick</b>
        /// </summary>
        /// <remarks>A medium, brownish-red, similar in color to a clay building brick.</remarks>
        /// <returns>Rgb(178, 34, 34)</returns>
        public static readonly string RgbFireBrick = Rgb(178, 34, 34);

        /// <summary>
        /// <b>Crimson</b>
        /// </summary>
        /// <remarks>A strong, deep red color, slightly inclined toward purple.</remarks>
        /// <returns>Rgb(220, 20, 60)</returns>
        public static readonly string RgbCrimson = Rgb(220, 20, 60);

        /// <summary>
        /// <b>Indian Red</b>
        /// </summary>
        /// <remarks>A medium, dusty reddish-brown, often associated with natural earth pigments.</remarks>
        /// <returns>Rgb(205, 92, 92)</returns>
        public static readonly string RgbIndianRed = Rgb(205, 92, 92);

        /// <summary>
        /// <b>Light Coral</b>
        /// </summary>
        /// <remarks>A light, pinkish-orange shade, reminiscent of the vibrant colors of coral reefs.</remarks>
        /// <returns>Rgb(240, 128, 128)</returns>
        public static readonly string RgbLightCoral = Rgb(240, 128, 128);

        /// <summary>
        /// <b>Salmon</b>
        /// </summary>
        /// <remarks>A pinkish-orange color, named after the flesh of salmon fish.</remarks>
        /// <returns>Rgb(250, 128, 114)</returns>
        public static readonly string RgbSalmon = Rgb(250, 128, 114);

        /// <summary>
        /// <b>Dark Salmon</b>
        /// </summary>
        /// <remarks>A deeper, richer shade of salmon, with more prominent orange and red tones.</remarks>
        /// <returns>Rgb(233, 150, 122)</returns>
        public static readonly string RgbDarkSalmon = Rgb(233, 150, 122);

        /// <summary>
        /// <b>Light Salmon</b>
        /// </summary>
        /// <remarks>A lighter, more orange-tinted salmon color, warm and inviting.</remarks>
        /// <returns>Rgb(255, 160, 122)</returns>
        public static readonly string RgbLightSalmon = Rgb(255, 160, 122);
        #endregion

        #region Named RGB Colors - Orange
        /// <summary>
        /// <b>Orange Red</b>
        /// </summary>
        /// <remarks>A vivid, fiery color that is a blend of orange and red.</remarks>
        /// <returns>Rgb(255, 69, 0)</returns>
        public static readonly string RgbOrangeRed = Rgb(255, 69, 0);

        /// <summary>
        /// <b>Tomato</b>
        /// </summary>
        /// <remarks>A bright, warm red with a noticeable orange tint, like a ripe tomato.</remarks>
        /// <returns>Rgb(255, 99, 71)</returns>
        public static readonly string RgbTomato = Rgb(255, 99, 71);

        /// <summary>
        /// <b>Dark Orange</b>
        /// </summary>
        /// <remarks>A deep, rich shade of orange, evoking warmth and autumn.</remarks>
        /// <returns>Rgb(255, 140, 0)</returns>
        public static readonly string RgbDarkOrange = Rgb(255, 140, 0);

        /// <summary>
        /// <b>Coral</b>
        /// </summary>
        /// <remarks>A vibrant pinkish-orange, named after the marine invertebrate.</remarks>
        /// <returns>Rgb(255, 127, 80)</returns>
        public static readonly string RgbCoral = Rgb(255, 127, 80);

        /// <summary>
        /// <b>Light Orange</b>
        /// </summary>
        /// <remarks>A standard, bright orange color, often simply called Orange.</remarks>
        /// <returns>Rgb(255, 165, 0)</returns>
        public static readonly string RgbLightOrange = Rgb(255, 165, 0);
        #endregion

        #region Named RGB Colors - Yellow
        /// <summary>
        /// <b>Dark Khaki</b>
        /// </summary>
        /// <remarks>A muted, brownish-yellow, often used in military and outdoor contexts.</remarks>
        /// <returns>Rgb(189, 183, 107)</returns>
        public static readonly string RgbDarkKhaki = Rgb(189, 183, 107);

        /// <summary>
        /// <b>Gold</b>
        /// </summary>
        /// <remarks>A bright, metallic yellow, representing wealth, success, and luxury.</remarks>
        /// <returns>Rgb(255, 215, 0)</returns>
        public static readonly string RgbGold = Rgb(255, 215, 0);

        /// <summary>
        /// <b>Khaki</b>
        /// </summary>
        /// <remarks>A light, brownish-yellow, similar to sand or dust.</remarks>
        /// <returns>Rgb(240, 230, 140)</returns>
        public static readonly string RgbKhaki = Rgb(240, 230, 140);

        /// <summary>
        /// <b>Peach Puff</b>
        /// </summary>
        /// <remarks>A very pale, peachy-yellow color, soft and warm.</remarks>
        /// <returns>Rgb(255, 218, 185)</returns>
        public static readonly string RgbPeachPuff = Rgb(255, 218, 185);

        /// <summary>
        /// <b>Pale Goldenrod</b>
        /// </summary>
        /// <remarks>A muted, pale yellow with a hint of green, like a faded goldenrod flower.</remarks>
        /// <returns>Rgb(238, 232, 170)</returns>
        public static readonly string RgbPaleGoldenrod = Rgb(238, 232, 170);

        /// <summary>
        /// <b>Moccasin</b>
        /// </summary>
        /// <remarks>A pale, creamy yellow-orange, similar to the color of moccasin leather.</remarks>
        /// <returns>Rgb(255, 228, 181)</returns>
        public static readonly string RgbMoccasin = Rgb(255, 228, 181);

        /// <summary>
        /// <b>Papaya Whip</b>
        /// </summary>
        /// <remarks>A very pale, creamy orange-yellow, like the flesh of a papaya.</remarks>
        /// <returns>Rgb(255, 239, 213)</returns>
        public static readonly string RgbPapayaWhip = Rgb(255, 239, 213);

        /// <summary>
        /// <b>Light Goldenrod Yellow</b>
        /// </summary>
        /// <remarks>A very pale, light yellow, almost off-white.</remarks>
        /// <returns>Rgb(250, 250, 210)</returns>
        public static readonly string RgbLightGoldenrodYellow = Rgb(250, 250, 210);

        /// <summary>
        /// <b>Lemon Chiffon</b>
        /// </summary>
        /// <remarks>A pale, light, and airy yellow, as delicate as chiffon fabric.</remarks>
        /// <returns>Rgb(255, 250, 205)</returns>
        public static readonly string RgbLemonChiffon = Rgb(255, 250, 205);

        /// <summary>
        /// <b>Light Yellow</b>
        /// </summary>
        /// <remarks>A very pale yellow, bordering on cream or ivory.</remarks>
        /// <returns>Rgb(255, 255, 224)</returns>
        public static readonly string RgbLightYellow = Rgb(255, 255, 224);
        #endregion

        #region Named RGB Colors - Brown
        /// <summary>
        /// <b>Brown</b>
        /// </summary>
        /// <remarks>A warm, reddish-brown color, earthy and natural.</remarks>
        /// <returns>Rgb(165, 42, 42)</returns>
        public static readonly string RgbBrown = Rgb(165, 42, 42);

        /// <summary>
        /// <b>Saddle Brown</b>
        /// </summary>
        /// <remarks>A rich, medium brown with warm undertones, like aged leather.</remarks>
        /// <returns>Rgb(139, 69, 19)</returns>
        public static readonly string RgbSaddleBrown = Rgb(139, 69, 19);

        /// <summary>
        /// <b>Sienna</b>
        /// </summary>
        /// <remarks>A reddish-brown earth pigment color, warm and rustic.</remarks>
        /// <returns>Rgb(160, 82, 45)</returns>
        public static readonly string RgbSienna = Rgb(160, 82, 45);

        /// <summary>
        /// <b>Chocolate</b>
        /// </summary>
        /// <remarks>A rich, dark brown, resembling the color of dark chocolate.</remarks>
        /// <returns>Rgb(210, 105, 30)</returns>
        public static readonly string RgbChocolate = Rgb(210, 105, 30);

        /// <summary>
        /// <b>Dark Goldenrod</b>
        /// </summary>
        /// <remarks>A muted, dark brownish-yellow.</remarks>
        /// <returns>Rgb(184, 134, 11)</returns>
        public static readonly string RgbDarkGoldenrod = Rgb(184, 134, 11);

        /// <summary>
        /// <b>Peru</b>
        /// </summary>
        /// <remarks>A medium, reddish-brown with orange undertones.</remarks>
        /// <returns>Rgb(205, 133, 63)</returns>
        public static readonly string RgbPeru = Rgb(205, 133, 63);

        /// <summary>
        /// <b>Rosy Brown</b>
        /// </summary>
        /// <remarks>A muted, pinkish-brown, soft and warm.</remarks>
        /// <returns>Rgb(188, 143, 143)</returns>
        public static readonly string RgbRosyBrown = Rgb(188, 143, 143);

        /// <summary>
        /// <b>Goldenrod</b>
        /// </summary>
        /// <remarks>A vibrant, golden-yellow, named after the goldenrod plant.</remarks>
        /// <returns>Rgb(218, 165, 32)</returns>
        public static readonly string RgbGoldenrod = Rgb(218, 165, 32);

        /// <summary>
        /// <b>Sandy Brown</b>
        /// </summary>
        /// <remarks>A warm, light brown with orange undertones, like wet sand.</remarks>
        /// <returns>Rgb(244, 164, 96)</returns>
        public static readonly string RgbSandyBrown = Rgb(244, 164, 96);

        /// <summary>
        /// <b>Tan</b>
        /// </summary>
        /// <remarks>A pale tone of brown, neutral and earthy.</remarks>
        /// <returns>Rgb(210, 180, 140)</returns>
        public static readonly string RgbTan = Rgb(210, 180, 140);

        /// <summary>
        /// <b>Burly Wood</b>
        /// </summary>
        /// <remarks>A medium, sandy-brown color, suggesting natural, unfinished wood.</remarks>
        /// <returns>Rgb(222, 184, 135)</returns>
        public static readonly string RgbBurlyWood = Rgb(222, 184, 135);

        /// <summary>
        /// <b>Wheat</b>
        /// </summary>
        /// <remarks>A pale yellow-brown, the color of ripe wheat fields.</remarks>
        /// <returns>Rgb(245, 222, 179)</returns>
        public static readonly string RgbWheat = Rgb(245, 222, 179);

        /// <summary>
        /// <b>Navajo White</b>
        /// </summary>
        /// <remarks>A pale, creamy off-white with a hint of peach or yellow.</remarks>
        /// <returns>Rgb(255, 222, 173)</returns>
        public static readonly string RgbNavajoWhite = Rgb(255, 222, 173);

        /// <summary>
        /// <b>Bisque</b>
        /// </summary>
        /// <remarks>A very pale, pinkish-brown, similar to the color of bisque porcelain.</remarks>
        /// <returns>Rgb(255, 228, 196)</returns>
        public static readonly string RgbBisque = Rgb(255, 228, 196);

        /// <summary>
        /// <b>Blanched Almond</b>
        /// </summary>
        /// <remarks>A very pale, creamy off-white, the color of a blanched almond nut.</remarks>
        /// <returns>Rgb(255, 235, 205)</returns>
        public static readonly string RgbBlanchedAlmond = Rgb(255, 235, 205);

        /// <summary>
        /// <b>Cornsilk</b>
        /// </summary>
        /// <remarks>A pale, creamy yellow, like the fine fibers of corn silk.</remarks>
        /// <returns>Rgb(255, 248, 220)</returns>
        public static readonly string RgbCornsilk = Rgb(255, 248, 220);
        #endregion

        #region Named RGB Colors - Green
        /// <summary>
        /// <b>Dark Green</b>
        /// </summary>
        /// <remarks>A deep, rich green, evoking dense forests and nature.</remarks>
        /// <returns>Rgb(0, 100, 0)</returns>
        public static readonly string RgbDarkGreen = Rgb(0, 100, 0);

        /// <summary>
        /// <b>Dark Olive Green</b>
        /// </summary>
        /// <remarks>A dark, muted green with yellow undertones, similar to an unripe olive.</remarks>
        /// <returns>Rgb(85, 107, 47)</returns>
        public static readonly string RgbDarkOliveGreen = Rgb(85, 107, 47);

        /// <summary>
        /// <b>Forest Green</b>
        /// </summary>
        /// <remarks>A rich, dark green, characteristic of the foliage in a forest.</remarks>
        /// <returns>Rgb(34, 139, 34)</returns>
        public static readonly string RgbForestGreen = Rgb(34, 139, 34);

        /// <summary>
        /// <b>Sea Green</b>
        /// </summary>
        /// <remarks>A medium green with a hint of blue, reminiscent of shallow sea water.</remarks>
        /// <returns>Rgb(46, 139, 87)</returns>
        public static readonly string RgbSeaGreen = Rgb(46, 139, 87);

        /// <summary>
        /// <b>Olive</b>
        /// </summary>
        /// <remarks>A muted, yellowish-green, the color of green olives.</remarks>
        /// <returns>Rgb(128, 128, 0)</returns>
        public static readonly string RgbOlive = Rgb(128, 128, 0);

        /// <summary>
        /// <b>Olive Drab</b>
        /// </summary>
        /// <remarks>A dull, brownish-green, commonly used in military camouflage.</remarks>
        /// <returns>Rgb(107, 142, 35)</returns>
        public static readonly string RgbOliveDrab = Rgb(107, 142, 35);

        /// <summary>
        /// <b>Medium Sea Green</b>
        /// </summary>
        /// <remarks>A vibrant, medium green with blue undertones.</remarks>
        /// <returns>Rgb(60, 179, 113)</returns>
        public static readonly string RgbMediumSeaGreen = Rgb(60, 179, 113);

        /// <summary>
        /// <b>Lime Green</b>
        /// </summary>
        /// <remarks>A bright, vivid green with a strong yellow tint, like the skin of a lime.</remarks>
        /// <returns>Rgb(50, 205, 50)</returns>
        public static readonly string RgbLimeGreen = Rgb(50, 205, 50);

        /// <summary>
        /// <b>Lime</b>
        /// </summary>
        /// <remarks>A pure, electric green, one of the primary colors in the additive color model.</remarks>
        /// <returns>Rgb(0, 255, 0)</returns>
        public static readonly string RgbLime = Rgb(0, 255, 0);

        /// <summary>
        /// <b>Spring Green</b>
        /// </summary>
        /// <remarks>A crisp, bright green with a hint of cyan, symbolizing new growth in spring.</remarks>
        /// <returns>Rgb(0, 255, 127)</returns>
        public static readonly string RgbSpringGreen = Rgb(0, 255, 127);

        /// <summary>
        /// <b>Medium Spring Green</b>
        /// </summary>
        /// <remarks>A vibrant, medium green with a strong cyan tint.</remarks>
        /// <returns>Rgb(0, 250, 154)</returns>
        public static readonly string RgbMediumSpringGreen = Rgb(0, 250, 154);

        /// <summary>
        /// <b>Dark Sea Green</b>
        /// </summary>
        /// <remarks>A muted, pale green with gray undertones.</remarks>
        /// <returns>Rgb(143, 188, 143)</returns>
        public static readonly string RgbDarkSeaGreen = Rgb(143, 188, 143);

        /// <summary>
        /// <b>Medium Aquamarine</b>
        /// </summary>
        /// <remarks>A soft, medium green-cyan color.</remarks>
        /// <returns>Rgb(102, 205, 170)</returns>
        public static readonly string RgbMediumAquamarine = Rgb(102, 205, 170);

        /// <summary>
        /// <b>Yellow Green</b>
        /// </summary>
        /// <remarks>A bright, lively color that is an equal mix of yellow and green.</remarks>
        /// <returns>Rgb(154, 205, 50)</returns>
        public static readonly string RgbYellowGreen = Rgb(154, 205, 50);

        /// <summary>
        /// <b>Lawn Green</b>
        /// </summary>
        /// <remarks>A bright, vibrant green, like a well-maintained summer lawn.</remarks>
        /// <returns>Rgb(124, 252, 0)</returns>
        public static readonly string RgbLawnGreen = Rgb(124, 252, 0);

        /// <summary>
        /// <b>Chartreuse</b>
        /// </summary>
        /// <remarks>A vivid, electric color halfway between yellow and green.</remarks>
        /// <returns>Rgb(127, 255, 0)</returns>
        public static readonly string RgbChartreuse = Rgb(127, 255, 0);

        /// <summary>
        /// <b>Light Green</b>
        /// </summary>
        /// <remarks>A soft, pale green, gentle and soothing.</remarks>
        /// <returns>Rgb(144, 238, 144)</returns>
        public static readonly string RgbLightGreen = Rgb(144, 238, 144);

        /// <summary>
        /// <b>Green Yellow</b>
        /// </summary>
        /// <remarks>A bright, sharp color dominated by yellow with a strong green tint.</remarks>
        /// <returns>Rgb(173, 255, 47)</returns>
        public static readonly string RgbGreenYellow = Rgb(173, 255, 47);

        /// <summary>
        /// <b>Pale Green</b>
        /// </summary>
        /// <remarks>A very light, soft shade of green.</remarks>
        /// <returns>Rgb(152, 251, 152)</returns>
        public static readonly string RgbPaleGreen = Rgb(152, 251, 152);
        #endregion

        #region Named RGB Colors - Cyan
        /// <summary>
        /// <b>Teal</b>
        /// </summary>
        /// <remarks>A deep, sophisticated green-blue, conveying elegance and calm.</remarks>
        /// <returns>Rgb(0, 128, 128)</returns>
        public static readonly string RgbTeal = Rgb(0, 128, 128);

        /// <summary>
        /// <b>Dark Cyan</b>
        /// </summary>
        /// <remarks>A rich, deep shade of cyan.</remarks>
        /// <returns>Rgb(0, 139, 139)</returns>
        public static readonly string RgbDarkCyan = Rgb(0, 139, 139);

        /// <summary>
        /// <b>Light Sea Green</b>
        /// </summary>
        /// <remarks>A vibrant, medium cyan-green, lighter and brighter than Sea Green.</remarks>
        /// <returns>Rgb(32, 178, 170)</returns>
        public static readonly string RgbLightSeaGreen = Rgb(32, 178, 170);

        /// <summary>
        /// <b>Cadet Blue</b>
        /// </summary>
        /// <remarks>A muted, medium blue-gray with a hint of green.</remarks>
        /// <returns>Rgb(95, 158, 160)</returns>
        public static readonly string RgbCadetBlue = Rgb(95, 158, 160);

        /// <summary>
        /// <b>Dark Turquoise</b>
        /// </summary>
        /// <remarks>A rich, medium cyan-blue color.</remarks>
        /// <returns>Rgb(0, 206, 209)</returns>
        public static readonly string RgbDarkTurquoise = Rgb(0, 206, 209);

        /// <summary>
        /// <b>Medium Turquoise</b>
        /// </summary>
        /// <remarks>A bright, cheerful cyan-blue.</remarks>
        /// <returns>Rgb(72, 209, 204)</returns>
        public static readonly string RgbMediumTurquoise = Rgb(72, 209, 204);

        /// <summary>
        /// <b>Turquoise</b>
        /// </summary>
        /// <remarks>A vibrant, greenish-blue, reminiscent of the gemstone.</remarks>
        /// <returns>Rgb(64, 224, 208)</returns>
        public static readonly string RgbTurquoise = Rgb(64, 224, 208);

        /// <summary>
        /// <b>Aqua</b>
        /// </summary>
        /// <remarks>A bright, pure cyan color, also known as Cyan.</remarks>
        /// <returns>Rgb(0, 255, 255)</returns>
        public static readonly string RgbAqua = Rgb(0, 255, 255);

        /// <summary>
        /// <b>Cyan</b>
        /// </summary>
        /// <remarks>A primary color in the subtractive color model, a perfect mix of green and blue.</remarks>
        /// <returns>Rgb(0, 255, 255)</returns>
        public static readonly string RgbCyan = Rgb(0, 255, 255);

        /// <summary>
        /// <b>Aquamarine</b>
        /// </summary>
        /// <remarks>A light, pale green-blue, like the gemstone of the same name.</remarks>
        /// <returns>Rgb(127, 255, 212)</returns>
        public static readonly string RgbAquamarine = Rgb(127, 255, 212);

        /// <summary>
        /// <b>Pale Turquoise</b>
        /// </summary>
        /// <remarks>A soft, light shade of turquoise.</remarks>
        /// <returns>Rgb(175, 238, 238)</returns>
        public static readonly string RgbPaleTurquoise = Rgb(175, 238, 238);

        /// <summary>
        /// <b>Light Cyan</b>
        /// </summary>
        /// <remarks>A very pale, airy cyan, almost white.</remarks>
        /// <returns>Rgb(224, 255, 255)</returns>
        public static readonly string RgbLightCyan = Rgb(224, 255, 255);
        #endregion

        #region Named RGB Colors - Blue
        /// <summary>
        /// <b>Navy</b>
        /// </summary>
        /// <remarks>A very dark blue, suggesting authority and formality.</remarks>
        /// <returns>Rgb(0, 0, 128)</returns>
        public static readonly string RgbNavy = Rgb(0, 0, 128);

        /// <summary>
        /// <b>Dark Blue</b>
        /// </summary>
        /// <remarks>A deep, rich blue, darker than primary blue.</remarks>
        /// <returns>Rgb(0, 0, 139)</returns>
        public static readonly string RgbDarkBlue = Rgb(0, 0, 139);

        /// <summary>
        /// <b>Medium Blue</b>
        /// </summary>
        /// <remarks>A strong, standard blue, darker than primary blue but brighter than navy.</remarks>
        /// <returns>Rgb(0, 0, 205)</returns>
        public static readonly string RgbMediumBlue = Rgb(0, 0, 205);

        /// <summary>
        /// <b>Midnight Blue</b>
        /// </summary>
        /// <remarks>A very dark shade of blue, like the sky on a moonless night.</remarks>
        /// <returns>Rgb(25, 25, 112)</returns>
        public static readonly string RgbMidnightBlue = Rgb(25, 25, 112);

        /// <summary>
        /// <b>Royal Blue</b>
        /// </summary>
        /// <remarks>A deep, vivid blue, traditionally associated with royalty.</remarks>
        /// <returns>Rgb(65, 105, 225)</returns>
        public static readonly string RgbRoyalBlue = Rgb(65, 105, 225);

        /// <summary>
        /// <b>Steel Blue</b>
        /// </summary>
        /// <remarks>A medium blue with gray undertones, like blued steel.</remarks>
        /// <returns>Rgb(70, 130, 180)</returns>
        public static readonly string RgbSteelBlue = Rgb(70, 130, 180);

        /// <summary>
        /// <b>Dodger Blue</b>
        /// </summary>
        /// <remarks>A bright, vivid shade of azure blue.</remarks>
        /// <returns>Rgb(30, 144, 255)</returns>
        public static readonly string RgbDodgerBlue = Rgb(30, 144, 255);

        /// <summary>
        /// <b>Deep Sky Blue</b>
        /// </summary>
        /// <remarks>A bright, vibrant blue, like a clear summer sky.</remarks>
        /// <returns>Rgb(0, 191, 255)</returns>
        public static readonly string RgbDeepSkyBlue = Rgb(0, 191, 255);

        /// <summary>
        /// <b>Cornflower Blue</b>
        /// </summary>
        /// <remarks>A medium-to-light blue, named after the cornflower.</remarks>
        /// <returns>Rgb(100, 149, 237)</returns>
        public static readonly string RgbCornflowerBlue = Rgb(100, 149, 237);

        /// <summary>
        /// <b>Sky Blue</b>
        /// </summary>
        /// <remarks>A light, pale blue, the color of the sky on a clear day.</remarks>
        /// <returns>Rgb(135, 206, 235)</returns>
        public static readonly string RgbSkyBlue = Rgb(135, 206, 235);

        /// <summary>
        /// <b>Light Sky Blue</b>
        /// </summary>
        /// <remarks>A very pale, airy blue, lighter than Sky Blue.</remarks>
        /// <returns>Rgb(135, 206, 250)</returns>
        public static readonly string RgbLightSkyBlue = Rgb(135, 206, 250);

        /// <summary>
        /// <b>Light Steel Blue</b>
        /// </summary>
        /// <remarks>A pale, grayish-blue, soft and calming.</remarks>
        /// <returns>Rgb(176, 196, 222)</returns>
        public static readonly string RgbLightSteelBlue = Rgb(176, 196, 222);

        /// <summary>
        /// <b>Light Blue</b>
        /// </summary>
        /// <remarks>A soft, pale blue, universally recognized and gentle.</remarks>
        /// <returns>Rgb(173, 216, 230)</returns>
        public static readonly string RgbLightBlue = Rgb(173, 216, 230);

        /// <summary>
        /// <b>Powder Blue</b>
        /// </summary>
        /// <remarks>A very pale, grayish-blue, like powdered minerals.</remarks>
        /// <returns>Rgb(176, 224, 230)</returns>
        public static readonly string RgbPowderBlue = Rgb(176, 224, 230);
        #endregion

        #region Named RGB Colors - White Tones
        /// <summary>
        /// <b>Misty Rose</b>
        /// </summary>
        /// <remarks>A delicate, pale pastel pink with soft, warm undertones, resembling a light-pink rose muted by haze or mist.</remarks>
        /// <returns>Rgb(255, 228, 225)</returns>
        public static readonly string RgbMistyRose = Rgb(255, 228, 225);

        /// <summary>
        /// <b>Antique White</b>
        /// </summary>
        /// <remarks>A pale, off-white with a yellowish tint, suggesting age and elegance.</remarks>
        /// <returns>Rgb(250, 235, 215)</returns>
        public static readonly string RgbAntiqueWhite = Rgb(250, 235, 215);

        /// <summary>
        /// <b>Linen</b>
        /// </summary>
        /// <remarks>A pale, off-white color with a slight beige tint, like natural linen fabric.</remarks>
        /// <returns>Rgb(250, 240, 230)</returns>
        public static readonly string RgbLinen = Rgb(250, 240, 230);

        /// <summary>
        /// <b>Beige</b>
        /// </summary>
        /// <remarks>A pale, sandy-brownish color, neutral and calming.</remarks>
        /// <returns>Rgb(245, 245, 220)</returns>
        public static readonly string RgbBeige = Rgb(245, 245, 220);

        /// <summary>
        /// <b>White Smoke</b>
        /// </summary>
        /// <remarks>A very pale gray, almost white, like a wisp of smoke.</remarks>
        /// <returns>Rgb(245, 245, 245)</returns>
        public static readonly string RgbWhiteSmoke = Rgb(245, 245, 245);

        /// <summary>
        /// <b>Lavender Blush</b>
        /// </summary>
        /// <remarks>A very pale, pinkish-purple, like a faint blush on a lavender petal.</remarks>
        /// <returns>Rgb(255, 240, 245)</returns>
        public static readonly string RgbLavenderBlush = Rgb(255, 240, 245);

        /// <summary>
        /// <b>Old Lace</b>
        /// </summary>
        /// <remarks>A pale, creamy off-white, reminiscent of aged lace fabric.</remarks>
        /// <returns>Rgb(253, 245, 230)</returns>
        public static readonly string RgbOldLace = Rgb(253, 245, 230);

        /// <summary>
        /// <b>Alice Blue</b>
        /// </summary>
        /// <remarks>A very pale, azure blue, almost white, named for Alice Roosevelt Longworth.</remarks>
        /// <returns>Rgb(240, 248, 255)</returns>
        public static readonly string RgbAliceBlue = Rgb(240, 248, 255);

        /// <summary>
        /// <b>Seashell</b>
        /// </summary>
        /// <remarks>A pale, pinkish-white, the color of many seashells.</remarks>
        /// <returns>Rgb(255, 245, 238)</returns>
        public static readonly string RgbSeashell = Rgb(255, 245, 238);

        /// <summary>
        /// <b>Ghost White</b>
        /// </summary>
        /// <remarks>A very pale, bluish-gray, almost white, with a cool undertone.</remarks>
        /// <returns>Rgb(248, 248, 255)</returns>
        public static readonly string RgbGhostWhite = Rgb(248, 248, 255);

        /// <summary>
        /// <b>Honeydew</b>
        /// </summary>
        /// <remarks>A very pale, greenish-white, like the flesh of a honeydew melon.</remarks>
        /// <returns>Rgb(240, 255, 240)</returns>
        public static readonly string RgbHoneydew = Rgb(240, 255, 240);

        /// <summary>
        /// <b>Floral White</b>
        /// </summary>
        /// <remarks>A pale, creamy off-white with a hint of yellow.</remarks>
        /// <returns>Rgb(255, 250, 240)</returns>
        public static readonly string RgbFloralWhite = Rgb(255, 250, 240);

        /// <summary>
        /// <b>Azure</b>
        /// </summary>
        /// <remarks>A very pale, light cyan, almost white.</remarks>
        /// <returns>Rgb(240, 255, 255)</returns>
        public static readonly string RgbAzure = Rgb(240, 255, 255);

        /// <summary>
        /// <b>Mint Cream</b>
        /// </summary>
        /// <remarks>A very pale, greenish-white, like cream infused with mint.</remarks>
        /// <returns>Rgb(245, 255, 250)</returns>
        public static readonly string RgbMintCream = Rgb(245, 255, 250);

        /// <summary>
        /// <b>Snow</b>
        /// </summary>
        /// <remarks>A very pale, pinkish-white, almost pure white.</remarks>
        /// <returns>Rgb(255, 250, 250)</returns>
        public static readonly string RgbSnow = Rgb(255, 250, 250);

        /// <summary>
        /// <b>Ivory</b>
        /// </summary>
        /// <remarks>A creamy off-white with a slight yellow tint, like natural ivory.</remarks>
        /// <returns>Rgb(255, 255, 240)</returns>
        public static readonly string RgbIvory = Rgb(255, 255, 240);

        /// <summary>
        /// <b>White</b>
        /// </summary>
        /// <remarks>A pure, brilliant white, representing purity, cleanliness, and neutrality.</remarks>
        /// <returns>Rgb(255, 255, 255)</returns>
        public static readonly string RgbWhite = Rgb(255, 255, 255);
        #endregion

        #region Named RGB Colors - Gray and Black
        /// <summary>
        /// <b>Black</b>
        /// </summary>
        /// <remarks>The darkest color, the result of the absence or complete absorption of visible light.</remarks>
        /// <returns>Rgb(0, 0, 0)</returns>
        public static readonly string RgbBlack = Rgb(0, 0, 0);

        /// <summary>
        /// <b>Dark Slate Gray</b>
        /// </summary>
        /// <remarks>A very dark, cool gray with a hint of blue or green.</remarks>
        /// <returns>Rgb(47, 79, 79)</returns>
        public static readonly string RgbDarkSlateGray = Rgb(47, 79, 79);

        /// <summary>
        /// <b>Dim Gray</b>
        /// </summary>
        /// <remarks>A dark, neutral gray.</remarks>
        /// <returns>Rgb(105, 105, 105)</returns>
        public static readonly string RgbDimGray = Rgb(105, 105, 105);

        /// <summary>
        /// <b>Slate Gray</b>
        /// </summary>
        /// <remarks>A medium gray with a slight blue tint, like natural slate rock.</remarks>
        /// <returns>Rgb(112, 128, 144)</returns>
        public static readonly string RgbSlateGray = Rgb(112, 128, 144);

        /// <summary>
        /// <b>Gray</b>
        /// </summary>
        /// <remarks>A neutral, balanced gray, halfway between black and white.</remarks>
        /// <returns>Rgb(128, 128, 128)</returns>
        public static readonly string RgbGray = Rgb(128, 128, 128);

        /// <summary>
        /// <b>Light Slate Gray</b>
        /// </summary>
        /// <remarks>A light, cool gray with blue undertones.</remarks>
        /// <returns>Rgb(119, 136, 153)</returns>
        public static readonly string RgbLightSlateGray = Rgb(119, 136, 153);

        /// <summary>
        /// <b>Dark Gray</b>
        /// </summary>
        /// <remarks>A standard dark gray, lighter than Dim Gray.</remarks>
        /// <returns>Rgb(169, 169, 169)</returns>
        public static readonly string RgbDarkGray = Rgb(169, 169, 169);

        /// <summary>
        /// <b>Silver</b>
        /// </summary>
        /// <remarks>A light, metallic gray, suggesting sleekness and modernity.</remarks>
        /// <returns>Rgb(192, 192, 192)</returns>
        public static readonly string RgbSilver = Rgb(192, 192, 192);

        /// <summary>
        /// <b>Light Gray</b>
        /// </summary>
        /// <remarks>A standard, pale gray, neutral and soft.</remarks>
        /// <returns>Rgb(211, 211, 211)</returns>
        public static readonly string RgbLightGray = Rgb(211, 211, 211);

        /// <summary>
        /// <b>Gainsboro</b>
        /// </summary>
        /// <remarks>A very pale, light gray, almost white.</remarks>
        /// <returns>Rgb(220, 220, 220)</returns>
        public static readonly string RgbGainsboro = Rgb(220, 220, 220);
        #endregion
    }
}
