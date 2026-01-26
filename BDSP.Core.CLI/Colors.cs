using System;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

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

        #region Basic ANSI Colors and Styles

        // --- Basic text colors ---
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

        // --- Bright text colors ---
        /// <summary>Bright Black text 90m</summary>
        public static readonly string BlackBright = $"{Esc}90m";
        /// <summary>Bright Red text 91m</summary>
        public static readonly string RedBright = $"{Esc}91m";
        // ... (and so on for all basic colors)

        // --- Text formatting ---
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
        // ... (and so on for all styles)

        // --- Special formatting ---
        /// <summary>Reset all styles 0m</summary>
        public static readonly string Reset = $"{Esc}0m";
        /// <summary>Cursor invisible ?25l</summary>
        public static readonly string CursorInvisible = $"{Esc}?25l";
        /// <summary>Cursor visible ?25h</summary>
        public static readonly string CursorVisible = $"{Esc}?25h";

        #endregion

        #region Named RGB Colors - Rainbow Spectrum
        /// <summary>Red text. Calls Rgb(255, 0, 0)</summary>
        public static readonly string RgbRed = Rgb(255, 0, 0);
        /// <summary>Orange text. Calls Rgb(255, 127, 0)</summary>
        public static readonly string RgbOrange = Rgb(255, 127, 0);
        /// <summary>Yellow text. Calls Rgb(255, 255, 0)</summary>
        public static readonly string RgbYellow = Rgb(255, 255, 0);
        /// <summary>Green text. Calls Rgb(0, 127, 0)</summary>
        public static readonly string RgbGreen = Rgb(0, 127, 0);
        /// <summary>Blue text. Calls Rgb(0, 0, 255)</summary>
        public static readonly string RgbBlue = Rgb(0, 0, 255);
        /// <summary>Indigo text. Calls Rgb(75, 0, 130)</summary>
        public static readonly string RgbIndigo = Rgb(75, 0, 130);
        /// <summary>Violet text. Calls Rgb(238, 130, 238)</summary>
        public static readonly string RgbViolet = Rgb(238, 130, 238);
        #endregion

        #region Named RGB Colors - Purple
        /// <summary>Purple text. Calls Rgb(128, 0, 128)</summary>
        public static readonly string RgbPurple = Rgb(128, 0, 128);
        /// <summary>Dark Magenta text. Calls Rgb(139, 0, 139)</summary>
        public static readonly string RgbDarkMagenta = Rgb(139, 0, 139);
        /// <summary>Dark Violet text. Calls Rgb(148, 0, 211)</summary>
        public static readonly string RgbDarkViolet = Rgb(148, 0, 211);
        /// <summary>Dark Slate Blue text. Calls Rgb(72, 61, 139)</summary>
        public static readonly string RgbDarkSlateBlue = Rgb(72, 61, 139);
        /// <summary>Blue Violet text. Calls Rgb(138, 43, 226)</summary>
        public static readonly string RgbBlueViolet = Rgb(138, 43, 226);
        /// <summary>Dark Orchid text. Calls Rgb(153, 50, 204)</summary>
        public static readonly string RgbDarkOrchid = Rgb(153, 50, 204);
        /// <summary>Fuchsia text. Calls Rgb(255, 0, 255)</summary>
        public static readonly string RgbFuchsia = Rgb(255, 0, 255);
        /// <summary>Slate Blue text. Calls Rgb(106, 90, 205)</summary>
        public static readonly string RgbSlateBlue = Rgb(106, 90, 205);
        /// <summary>Medium Slate Blue text. Calls Rgb(123, 104, 238)</summary>
        public static readonly string RgbMediumSlateBlue = Rgb(123, 104, 238);
        /// <summary>Medium Orchid text. Calls Rgb(186, 85, 211)</summary>
        public static readonly string RgbMediumOrchid = Rgb(186, 85, 211);
        /// <summary>Medium Purple text. Calls Rgb(147, 112, 219)</summary>
        public static readonly string RgbMediumPurple = Rgb(147, 112, 219);
        /// <summary>Orchid text. Calls Rgb(218, 112, 214)</summary>
        public static readonly string RgbOrchid = Rgb(218, 112, 214);
        /// <summary>Plum text. Calls Rgb(221, 160, 221)</summary>
        public static readonly string RgbPlum = Rgb(221, 160, 221);
        /// <summary>Thistle text. Calls Rgb(216, 191, 216)</summary>
        public static readonly string RgbThistle = Rgb(216, 191, 216);
        /// <summary>Lavender text. Calls Rgb(230, 230, 250)</summary>
        public static readonly string RgbLavender = Rgb(230, 230, 250);
        #endregion

        #region Named RGB Colors - Pink
        /// <summary>Medium Violet Red text. Calls Rgb(199, 21, 133)</summary>
        public static readonly string RgbMediumVioletRed = Rgb(199, 21, 133);
        /// <summary>Deep Pink text. Calls Rgb(255, 20, 147)</summary>
        public static readonly string RgbDeepPink = Rgb(255, 20, 147);
        /// <summary>Pale Violet Red text. Calls Rgb(219, 112, 147)</summary>
        public static readonly string RgbPaleVioletRed = Rgb(219, 112, 147);
        /// <summary>Hot Pink text. Calls Rgb(255, 105, 180)</summary>
        public static readonly string RgbHotPink = Rgb(255, 105, 180);
        /// <summary>Light Pink text. Calls Rgb(255, 182, 193)</summary>
        public static readonly string RgbLightPink = Rgb(255, 182, 193);
        /// <summary>Pink text. Calls Rgb(255, 192, 203)</summary>
        public static readonly string RgbPink = Rgb(255, 192, 203);
        #endregion

        #region Named RGB Colors - Red
        /// <summary>Maroon text. Calls Rgb(128, 0, 0)</summary>
        public static readonly string RgbMaroon = Rgb(128, 0, 0);
        /// <summary>Dark Red text. Calls Rgb(139, 0, 0)</summary>
        public static readonly string RgbDarkRed = Rgb(139, 0, 0);
        /// <summary>Fire Brick text. Calls Rgb(178, 34, 34)</summary>
        public static readonly string RgbFireBrick = Rgb(178, 34, 34);
        /// <summary>Crimson text. Calls Rgb(220, 20, 60)</summary>
        public static readonly string RgbCrimson = Rgb(220, 20, 60);
        /// <summary>Indian Red text. Calls Rgb(205, 92, 92)</summary>
        public static readonly string RgbIndianRed = Rgb(205, 92, 92);
        /// <summary>Light Coral text. Calls Rgb(240, 128, 128)</summary>
        public static readonly string RgbLightCoral = Rgb(240, 128, 128);
        /// <summary>Salmon text. Calls Rgb(250, 128, 114)</summary>
        public static readonly string RgbSalmon = Rgb(250, 128, 114);
        /// <summary>Dark Salmon text. Calls Rgb(233, 150, 122)</summary>
        public static readonly string RgbDarkSalmon = Rgb(233, 150, 122);
        /// <summary>Light Salmon text. Calls Rgb(255, 160, 122)</summary>
        public static readonly string RgbLightSalmon = Rgb(255, 160, 122);
        #endregion

        #region Named RGB Colors - Orange
        /// <summary>Orange Red text. Calls Rgb(255, 69, 0)</summary>
        public static readonly string RgbOrangeRed = Rgb(255, 69, 0);
        /// <summary>Tomato text. Calls Rgb(255, 99, 71)</summary>
        public static readonly string RgbTomato = Rgb(255, 99, 71);
        /// <summary>Dark Orange text. Calls Rgb(255, 140, 0)</summary>
        public static readonly string RgbDarkOrange = Rgb(255, 140, 0);
        /// <summary>Coral text. Calls Rgb(255, 127, 80)</summary>
        public static readonly string RgbCoral = Rgb(255, 127, 80);
        /// <summary>Light Orange text. Calls Rgb(255, 165, 0)</summary>
        public static readonly string RgbLightOrange = Rgb(255, 165, 0);
        #endregion

        #region Named RGB Colors - Yellow
        /// <summary>Dark Khaki text. Calls Rgb(189, 183, 107)</summary>
        public static readonly string RgbDarkKhaki = Rgb(189, 183, 107);
        /// <summary>Gold text. Calls Rgb(255, 215, 0)</summary>
        public static readonly string RgbGold = Rgb(255, 215, 0);
        /// <summary>Khaki text. Calls Rgb(240, 230, 140)</summary>
        public static readonly string RgbKhaki = Rgb(240, 230, 140);
        /// <summary>Peach Puff text. Calls Rgb(255, 218, 185)</summary>
        public static readonly string RgbPeachPuff = Rgb(255, 218, 185);
        /// <summary>Pale Goldenrod text. Calls Rgb(238, 232, 170)</summary>
        public static readonly string RgbPaleGoldenrod = Rgb(238, 232, 170);
        /// <summary>Moccasin text. Calls Rgb(255, 228, 181)</summary>
        public static readonly string RgbMoccasin = Rgb(255, 228, 181);
        /// <summary>Papaya Whip text. Calls Rgb(255, 239, 213)</summary>
        public static readonly string RgbPapayaWhip = Rgb(255, 239, 213);
        /// <summary>Light Goldenrod Yellow text. Calls Rgb(250, 250, 210)</summary>
        public static readonly string RgbLightGoldenrodYellow = Rgb(250, 250, 210);
        /// <summary>Lemon Chiffon text. Calls Rgb(255, 250, 205)</summary>
        public static readonly string RgbLemonChiffon = Rgb(255, 250, 205);
        /// <summary>Light Yellow text. Calls Rgb(255, 255, 224)</summary>
        public static readonly string RgbLightYellow = Rgb(255, 255, 224);
        #endregion

        #region Named RGB Colors - Brown
        /// <summary>Brown text. Calls Rgb(165, 42, 42)</summary>
        public static readonly string RgbBrown = Rgb(165, 42, 42);
        /// <summary>Saddle Brown text. Calls Rgb(139, 69, 19)</summary>
        public static readonly string RgbSaddleBrown = Rgb(139, 69, 19);
        /// <summary>Sienna text. Calls Rgb(160, 82, 45)</summary>
        public static readonly string RgbSienna = Rgb(160, 82, 45);
        /// <summary>Chocolate text. Calls Rgb(210, 105, 30)</summary>
        public static readonly string RgbChocolate = Rgb(210, 105, 30);
        /// <summary>Dark Goldenrod text. Calls Rgb(184, 134, 11)</summary>
        public static readonly string RgbDarkGoldenrod = Rgb(184, 134, 11);
        /// <summary>Peru text. Calls Rgb(205, 133, 63)</summary>
        public static readonly string RgbPeru = Rgb(205, 133, 63);
        /// <summary>Rosy Brown text. Calls Rgb(188, 143, 143)</summary>
        public static readonly string RgbRosyBrown = Rgb(188, 143, 143);
        /// <summary>Goldenrod text. Calls Rgb(218, 165, 32)</summary>
        public static readonly string RgbGoldenrod = Rgb(218, 165, 32);
        /// <summary>Sandy Brown text. Calls Rgb(244, 164, 96)</summary>
        public static readonly string RgbSandyBrown = Rgb(244, 164, 96);
        /// <summary>Tan text. Calls Rgb(210, 180, 140)</summary>
        public static readonly string RgbTan = Rgb(210, 180, 140);
        /// <summary>Burly Wood text. Calls Rgb(222, 184, 135)</summary>
        public static readonly string RgbBurlyWood = Rgb(222, 184, 135);
        /// <summary>Wheat text. Calls Rgb(245, 222, 179)</summary>
        public static readonly string RgbWheat = Rgb(245, 222, 179);
        /// <summary>Navajo White text. Calls Rgb(255, 222, 173)</summary>
        public static readonly string RgbNavajoWhite = Rgb(255, 222, 173);
        /// <summary>Bisque text. Calls Rgb(255, 228, 196)</summary>
        public static readonly string RgbBisque = Rgb(255, 228, 196);
        /// <summary>Blanched Almond text. Calls Rgb(255, 235, 205)</summary>
        public static readonly string RgbBlanchedAlmond = Rgb(255, 235, 205);
        /// <summary>Cornsilk text. Calls Rgb(255, 248, 220)</summary>
        public static readonly string RgbCornsilk = Rgb(255, 248, 220);
        #endregion

        #region Named RGB Colors - Green
        /// <summary>Dark Green text. Calls Rgb(0, 100, 0)</summary>
        public static readonly string RgbDarkGreen = Rgb(0, 100, 0);
        /// <summary>Dark Olive Green text. Calls Rgb(85, 107, 47)</summary>
        public static readonly string RgbDarkOliveGreen = Rgb(85, 107, 47);
        /// <summary>Forest Green text. Calls Rgb(34, 139, 34)</summary>
        public static readonly string RgbForestGreen = Rgb(34, 139, 34);
        /// <summary>Sea Green text. Calls Rgb(46, 139, 87)</summary>
        public static readonly string RgbSeaGreen = Rgb(46, 139, 87);
        /// <summary>Olive text. Calls Rgb(128, 128, 0)</summary>
        public static readonly string RgbOlive = Rgb(128, 128, 0);
        /// <summary>Olive Drab text. Calls Rgb(107, 142, 35)</summary>
        public static readonly string RgbOliveDrab = Rgb(107, 142, 35);
        /// <summary>Medium Sea Green text. Calls Rgb(60, 179, 113)</summary>
        public static readonly string RgbMediumSeaGreen = Rgb(60, 179, 113);
        /// <summary>Lime Green text. Calls Rgb(50, 205, 50)</summary>
        public static readonly string RgbLimeGreen = Rgb(50, 205, 50);
        /// <summary>Lime text. Calls Rgb(0, 255, 0)</summary>
        public static readonly string RgbLime = Rgb(0, 255, 0);
        /// <summary>Spring Green text. Calls Rgb(0, 255, 127)</summary>
        public static readonly string RgbSpringGreen = Rgb(0, 255, 127);
        /// <summary>Medium Spring Green text. Calls Rgb(0, 250, 154)</summary>
        public static readonly string RgbMediumSpringGreen = Rgb(0, 250, 154);
        /// <summary>Dark Sea Green text. Calls Rgb(143, 188, 143)</summary>
        public static readonly string RgbDarkSeaGreen = Rgb(143, 188, 143);
        /// <summary>Medium Aquamarine text. Calls Rgb(102, 205, 170)</summary>
        public static readonly string RgbMediumAquamarine = Rgb(102, 205, 170);
        /// <summary>Yellow Green text. Calls Rgb(154, 205, 50)</summary>
        public static readonly string RgbYellowGreen = Rgb(154, 205, 50);
        /// <summary>Lawn Green text. Calls Rgb(124, 252, 0)</summary>
        public static readonly string RgbLawnGreen = Rgb(124, 252, 0);
        /// <summary>Chartreuse text. Calls Rgb(127, 255, 0)</summary>
        public static readonly string RgbChartreuse = Rgb(127, 255, 0);
        /// <summary>Light Green text. Calls Rgb(144, 238, 144)</summary>
        public static readonly string RgbLightGreen = Rgb(144, 238, 144);
        /// <summary>Green Yellow text. Calls Rgb(173, 255, 47)</summary>
        public static readonly string RgbGreenYellow = Rgb(173, 255, 47);
        /// <summary>Pale Green text. Calls Rgb(152, 251, 152)</summary>
        public static readonly string RgbPaleGreen = Rgb(152, 251, 152);
        #endregion

        #region Named RGB Colors - Cyan
        /// <summary>Teal text. Calls Rgb(0, 128, 128)</summary>
        public static readonly string RgbTeal = Rgb(0, 128, 128);
        /// <summary>Dark Cyan text. Calls Rgb(0, 139, 139)</summary>
        public static readonly string RgbDarkCyan = Rgb(0, 139, 139);
        /// <summary>Light Sea Green text. Calls Rgb(32, 178, 170)</summary>
        public static readonly string RgbLightSeaGreen = Rgb(32, 178, 170);
        /// <summary>Cadet Blue text. Calls Rgb(95, 158, 160)</summary>
        public static readonly string RgbCadetBlue = Rgb(95, 158, 160);
        /// <summary>Dark Turquoise text. Calls Rgb(0, 206, 209)</summary>
        public static readonly string RgbDarkTurquoise = Rgb(0, 206, 209);
        /// <summary>Medium Turquoise text. Calls Rgb(72, 209, 204)</summary>
        public static readonly string RgbMediumTurquoise = Rgb(72, 209, 204);
        /// <summary>Turquoise text. Calls Rgb(64, 224, 208)</summary>
        public static readonly string RgbTurquoise = Rgb(64, 224, 208);
        /// <summary>Aqua text. Calls Rgb(0, 255, 255)</summary>
        public static readonly string RgbAqua = Rgb(0, 255, 255);
        /// <summary>Cyan text. Calls Rgb(0, 255, 255)</summary>
        public static readonly string RgbCyan = Rgb(0, 255, 255);
        /// <summary>Aquamarine text. Calls Rgb(127, 255, 212)</summary>
        public static readonly string RgbAquamarine = Rgb(127, 255, 212);
        /// <summary>Pale Turquoise text. Calls Rgb(175, 238, 238)</summary>
        public static readonly string RgbPaleTurquoise = Rgb(175, 238, 238);
        /// <summary>Light Cyan text. Calls Rgb(224, 255, 255)</summary>
        public static readonly string RgbLightCyan = Rgb(224, 255, 255);
        #endregion

        #region Named RGB Colors - Blue
        /// <summary>Navy text. Calls Rgb(0, 0, 128)</summary>
        public static readonly string RgbNavy = Rgb(0, 0, 128);
        /// <summary>Dark Blue text. Calls Rgb(0, 0, 139)</summary>
        public static readonly string RgbDarkBlue = Rgb(0, 0, 139);
        /// <summary>Medium Blue text. Calls Rgb(0, 0, 205)</summary>
        public static readonly string RgbMediumBlue = Rgb(0, 0, 205);
        /// <summary>Midnight Blue text. Calls Rgb(25, 25, 112)</summary>
        public static readonly string RgbMidnightBlue = Rgb(25, 25, 112);
        /// <summary>Royal Blue text. Calls Rgb(65, 105, 225)</summary>
        public static readonly string RgbRoyalBlue = Rgb(65, 105, 225);
        /// <summary>Steel Blue text. Calls Rgb(70, 130, 180)</summary>
        public static readonly string RgbSteelBlue = Rgb(70, 130, 180);
        /// <summary>Dodger Blue text. Calls Rgb(30, 144, 255)</summary>
        public static readonly string RgbDodgerBlue = Rgb(30, 144, 255);
        /// <summary>Deep Sky Blue text. Calls Rgb(0, 191, 255)</summary>
        public static readonly string RgbDeepSkyBlue = Rgb(0, 191, 255);
        /// <summary>Cornflower Blue text. Calls Rgb(100, 149, 237)</summary>
        public static readonly string RgbCornflowerBlue = Rgb(100, 149, 237);
        /// <summary>Sky Blue text. Calls Rgb(135, 206, 235)</summary>
        public static readonly string RgbSkyBlue = Rgb(135, 206, 235);
        /// <summary>Light Sky Blue text. Calls Rgb(135, 206, 250)</summary>
        public static readonly string RgbLightSkyBlue = Rgb(135, 206, 250);
        /// <summary>Light Steel Blue text. Calls Rgb(176, 196, 222)</summary>
        public static readonly string RgbLightSteelBlue = Rgb(176, 196, 222);
        /// <summary>Light Blue text. Calls Rgb(173, 216, 230)</summary>
        public static readonly string RgbLightBlue = Rgb(173, 216, 230);
        /// <summary>Powder Blue text. Calls Rgb(176, 224, 230)</summary>
        public static readonly string RgbPowderBlue = Rgb(176, 224, 230);
        #endregion

        #region Named RGB Colors - White Tones
        /// <summary>
        /// <b>Misty Rose</b>
        /// </summary>
        /// <remarks>
        /// A delicate, pale pastel pink with soft, warm undertones, resembling a light-pink rose muted by haze or mist.
        /// </remarks><returns>Rgb(255, 228, 225)</returns>
        public static readonly string RgbMistyRose = Rgb(255, 228, 225);
        /// <summary>Antique White text. Calls Rgb(250, 235, 215)</summary>
        public static readonly string RgbAntiqueWhite = Rgb(250, 235, 215);
        /// <summary>Linen text. Calls Rgb(250, 240, 230)</summary>
        public static readonly string RgbLinen = Rgb(250, 240, 230);
        /// <summary>Beige text. Calls Rgb(245, 245, 220)</summary>
        public static readonly string RgbBeige = Rgb(245, 245, 220);
        /// <summary>White Smoke text. Calls Rgb(245, 245, 245)</summary>
        public static readonly string RgbWhiteSmoke = Rgb(245, 245, 245);
        /// <summary>Lavender Blush text. Calls Rgb(255, 240, 245)</summary>
        public static readonly string RgbLavenderBlush = Rgb(255, 240, 245);
        /// <summary>Old Lace text. Calls Rgb(253, 245, 230)</summary>
        public static readonly string RgbOldLace = Rgb(253, 245, 230);
        /// <summary>Alice Blue text. Calls Rgb(240, 248, 255)</summary>
        public static readonly string RgbAliceBlue = Rgb(240, 248, 255);
        /// <summary>Seashell text. Calls Rgb(255, 245, 238)</summary>
        public static readonly string RgbSeashell = Rgb(255, 245, 238);
        /// <summary>Ghost White text. Calls Rgb(248, 248, 255)</summary>
        public static readonly string RgbGhostWhite = Rgb(248, 248, 255);
        /// <summary>Honeydew text. Calls Rgb(240, 255, 240)</summary>
        public static readonly string RgbHoneydew = Rgb(240, 255, 240);
        /// <summary>Floral White text. Calls Rgb(255, 250, 240)</summary>
        public static readonly string RgbFloralWhite = Rgb(255, 250, 240);
        /// <summary>Azure text. Calls Rgb(240, 255, 255)</summary>
        public static readonly string RgbAzure = Rgb(240, 255, 255);
        /// <summary>Mint Cream text. Calls Rgb(245, 255, 250)</summary>
        public static readonly string RgbMintCream = Rgb(245, 255, 250);
        /// <summary>Snow text. Calls Rgb(255, 250, 250)</summary>
        public static readonly string RgbSnow = Rgb(255, 250, 250);
        /// <summary>Ivory text. Calls Rgb(255, 255, 240)</summary>
        public static readonly string RgbIvory = Rgb(255, 255, 240);
        /// <summary>White text. Calls Rgb(255, 255, 255)</summary>
        public static readonly string RgbWhite = Rgb(255, 255, 255);
        #endregion

        #region Named RGB Colors - Gray and Black
        /// <summary>Black text. Calls Rgb(0, 0, 0)</summary>
        public static readonly string RgbBlack = Rgb(0, 0, 0);
        /// <summary>Dark Slate Gray text. Calls Rgb(47, 79, 79)</summary>
        public static readonly string RgbDarkSlateGray = Rgb(47, 79, 79);
        /// <summary>Dim Gray text. Calls Rgb(105, 105, 105)</summary>
        public static readonly string RgbDimGray = Rgb(105, 105, 105);
        /// <summary>Slate Gray text. Calls Rgb(112, 128, 144)</summary>
        public static readonly string RgbSlateGray = Rgb(112, 128, 144);
        /// <summary>Gray text. Calls Rgb(128, 128, 128)</summary>
        public static readonly string RgbGray = Rgb(128, 128, 128);
        /// <summary>Light Slate Gray text. Calls Rgb(119, 136, 153)</summary>
        public static readonly string RgbLightSlateGray = Rgb(119, 136, 153);
        /// <summary>Dark Gray text. Calls Rgb(169, 169, 169)</summary>
        public static readonly string RgbDarkGray = Rgb(169, 169, 169);
        /// <summary>Silver text. Calls Rgb(192, 192, 192)</summary>
        public static readonly string RgbSilver = Rgb(192, 192, 192);
        /// <summary>Light Gray text. Calls Rgb(211, 211, 211)</summary>
        public static readonly string RgbLightGray = Rgb(211, 211, 211);
        /// <summary>Gainsboro text. Calls Rgb(220, 220, 220)</summary>
        public static readonly string RgbGainsboro = Rgb(220, 220, 220);
        #endregion
    }
}
