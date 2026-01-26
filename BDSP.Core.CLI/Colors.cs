using System;
using System.Globalization;
using System.Linq;

namespace BDSP.Core.CLI
{
    /// <summary>
    /// ANSI color and cursor helpers (ported from the Python colors.py).
    /// </summary>
    public static class Colors
    {
        public const string Esc = "\u001b[";

        public static string Rgb(int r, int g, int b) => $"{Esc}38;2;{r};{g};{b}m";
        public static string RgbBg(int r, int g, int b) => $"{Esc}48;2;{r};{g};{b}m";
        public static string Color256(int value) => $"{Esc}38;5;{value}m";
        public static string Color256Bg(int value) => $"{Esc}48;5;{value}m";

        public static string Move(int rows, int columns) => $"{Esc}{rows};{columns}H";
        public static string Up(int lines) => $"{Esc}{lines}A";
        public static string Down(int lines) => $"{Esc}{lines}B";
        public static string Right(int columns) => $"{Esc}{columns}C";
        public static string Left(int columns) => $"{Esc}{columns}D";
        public static string StartDown(int amount) => $"{Esc}{amount}E";
        public static string StartUp(int amount) => $"{Esc}{amount}F";
        public static string MoveToColumn(int column) => $"{Esc}{column}G";

        public static string ChainSequence(params int[] args)
        {
            string chained = string.Join(";", args.Select(a => a.ToString(CultureInfo.InvariantCulture)));
            return $"{Esc}{chained}m";
        }

        // Base codes
        public const int _bl_ = 30;
        public const int _r_ = 31;
        public const int _g_ = 32;
        public const int _y_ = 33;
        public const int _b_ = 34;
        public const int _m_ = 35;
        public const int _c_ = 36;
        public const int _w_ = 37;
        public const int _d_ = 39;

        // Text bright
        public const int _bl_br_ = 90;
        public const int _r_br_ = 91;
        public const int _g_br_ = 92;
        public const int _y_br_ = 93;
        public const int _b_br_ = 94;
        public const int _m_br_ = 95;
        public const int _c_br_ = 96;
        public const int _w_br_ = 97;

        // Background
        public const int _bl_bg_ = 40;
        public const int _r_bg_ = 41;
        public const int _g_bg_ = 42;
        public const int _y_bg_ = 43;
        public const int _b_bg_ = 44;
        public const int _m_bg_ = 45;
        public const int _c_bg_ = 46;
        public const int _w_bg_ = 47;
        public const int _d_bg_ = 49;

        // Background bright
        public const int _bl_br_bg_ = 100;
        public const int _r_br_bg_ = 101;
        public const int _g_br_bg_ = 102;
        public const int _y_br_bg_ = 103;
        public const int _b_br_bg_ = 104;
        public const int _m_br_bg_ = 105;
        public const int _c_br_bg_ = 106;
        public const int _w_br_bg_ = 107;

        public const int _bold_ = 1;
        public const int _n_bold_ = 21;
        public const int _dim_ = 2;
        public const int _n_dim_ = 22;
        public const int _italic_ = 3;
        public const int _n_italic_ = 23;
        public const int _underline_ = 4;
        public const int _n_underline_ = 24;
        public const int _slow_blink_ = 5;
        public const int _n_slow_blink_ = 25;
        public const int _rapid_blink_ = 6;
        public const int _n_rapid_blink_ = 26;
        public const int _strike_through_ = 9;
        public const int _n_strike_through_ = 29;

        public const int _reset_ = 0;
        public const string _inv_ = "?25l";
        public const string _visible_ = "?25h";
        public const int _hide_ = 8;
        public const string _home_ = "H";

        // Text
        public static readonly string BLACK = $"{Esc}30m";
        public static readonly string RED = $"{Esc}31m";
        public static readonly string GREEN = $"{Esc}32m";
        public static readonly string YELLOW = $"{Esc}33m";
        public static readonly string BLUE = $"{Esc}34m";
        public static readonly string MAGENTA = $"{Esc}35m";
        public static readonly string CYAN = $"{Esc}36m";
        public static readonly string WHITE = $"{Esc}37m";
        public static readonly string DEFAULT = $"{Esc}39m";

        // Text bright
        public static readonly string BLACK_BR = $"{Esc}90m";
        public static readonly string RED_BR = $"{Esc}91m";
        public static readonly string GREEN_BR = $"{Esc}92m";
        public static readonly string YELLOW_BR = $"{Esc}93m";
        public static readonly string BLUE_BR = $"{Esc}94m";
        public static readonly string MAGENTA_BR = $"{Esc}95m";
        public static readonly string CYAN_BR = $"{Esc}96m";
        public static readonly string WHITE_BR = $"{Esc}97m";

        // Background
        public static readonly string BLACK_BG = $"{Esc}40m";
        public static readonly string RED_BG = $"{Esc}41m";
        public static readonly string GREEN_BG = $"{Esc}42m";
        public static readonly string YELLOW_BG = $"{Esc}43m";
        public static readonly string BLUE_BG = $"{Esc}44m";
        public static readonly string MAGENTA_BG = $"{Esc}45m";
        public static readonly string CYAN_BG = $"{Esc}46m";
        public static readonly string WHITE_BG = $"{Esc}47m";
        public static readonly string DEFAULT_BG = $"{Esc}49m";

        // Background bright
        public static readonly string BLACK_BR_BG = $"{Esc}100m";
        public static readonly string RED_BR_BG = $"{Esc}101m";
        public static readonly string GREEN_BR_BG = $"{Esc}102m";
        public static readonly string YELLOW_BR_BG = $"{Esc}103m";
        public static readonly string BLUE_BR_BG = $"{Esc}104m";
        public static readonly string MAGENTA_BR_BG = $"{Esc}105m";
        public static readonly string CYAN_BR_BG = $"{Esc}106m";
        public static readonly string WHITE_BR_BG = $"{Esc}107m";

        public static readonly string BOLD = $"{Esc}1m";
        public static readonly string N_BOLD = $"{Esc}22m";
        public static readonly string DIM = $"{Esc}2m";
        public static readonly string N_DIM = $"{Esc}22m";
        public static readonly string ITALIC = $"{Esc}3m";
        public static readonly string N_ITALIC = $"{Esc}23m";
        public static readonly string UNDERLINE = $"{Esc}4m";
        public static readonly string N_UNDERLINE = $"{Esc}24m";
        public static readonly string SLOW_BLINK = $"{Esc}5m";
        public static readonly string N_SLOW_BLINK = $"{Esc}25m";
        public static readonly string RAPID_BLINK = $"{Esc}6m";
        public static readonly string N_RAPID_BLINK = $"{Esc}26m";
        public static readonly string STRIKE_THROUGH = $"{Esc}9m";
        public static readonly string N_STRIKE_THROUGH = $"{Esc}29m";

        public static readonly string RESET = $"{Esc}0m";
        public static readonly string INV = $"{Esc}?25l";
        public static readonly string VISIBLE = $"{Esc}?25h";
        public static readonly string HIDE = $"{Esc}8m";
        public static readonly string N_HIDE = $"{Esc}28m";
        public static readonly string HOME = $"{Esc}{_home_}";

        // roygbiv
        public static readonly string RGB_RED = Rgb(255, 0, 0);
        public static readonly string RGB_ORANGE = Rgb(255, 127, 0);
        public static readonly string RGB_YELLOW = Rgb(255, 255, 0);
        public static readonly string RGB_GREEN = Rgb(0, 127, 0);
        public static readonly string RGB_BLUE = Rgb(0, 0, 255);
        public static readonly string RGB_INDIGO = Rgb(75, 0, 130);
        public static readonly string RGB_VIOLET = Rgb(238, 130, 238);

        // purple colors
        public static readonly string RGB_PURPLE = Rgb(128, 0, 128);
        public static readonly string RGB_DARK_MAGENTA = Rgb(139, 0, 139);
        public static readonly string RGB_DARK_VIOLET = Rgb(148, 0, 211);
        public static readonly string RGB_DARK_SLATEBLUE = Rgb(72, 61, 139);
        public static readonly string RGB_BLUE_VIOLET = Rgb(138, 43, 226);
        public static readonly string RGB_DARK_ORCHID = Rgb(153, 50, 204);
        public static readonly string RGB_FUSCIA = Rgb(255, 0, 255);
        public static readonly string RGB_SLATEBLUE = Rgb(106, 90, 205);
        public static readonly string RGB_MEDIUM_SLATEBLUE = Rgb(123, 104, 238);
        public static readonly string RGB_MEDIUM_ORCHID = Rgb(186, 85, 211);
        public static readonly string RGB_MEDIUM_PURPLE = Rgb(147, 112, 219);
        public static readonly string RGB_ORCHID = Rgb(218, 112, 214);
        public static readonly string RGB_PLUM = Rgb(221, 160, 221);
        public static readonly string RGB_THISTLE = Rgb(216, 191, 216);
        public static readonly string RGB_LAVENDER = Rgb(230, 230, 250);

        // pink colors
        public static readonly string RGB_MEDIUM_VIOLET_RED = Rgb(199, 21, 133);
        public static readonly string RGB_DEEP_PINK = Rgb(255, 20, 147);
        public static readonly string RGB_PALE_VIOLET_RED = Rgb(219, 112, 147);
        public static readonly string RGB_HOT_PINK = Rgb(255, 105, 180);
        public static readonly string RGB_LIGHT_PINK = Rgb(255, 182, 193);
        public static readonly string RGB_PINK = Rgb(255, 192, 203);

        // red colors
        public static readonly string RGB_MAROON = Rgb(128, 0, 0);
        public static readonly string RGB_DARK_RED = Rgb(139, 0, 0);
        public static readonly string RGB_FIRE_BRICK = Rgb(178, 34, 34);
        public static readonly string RGB_CRIMSON = Rgb(220, 20, 60);
        public static readonly string RGB_INDIAN_RED = Rgb(205, 92, 92);
        public static readonly string RGB_LIGHT_CORAL = Rgb(240, 128, 128);
        public static readonly string RGB_SALMON = Rgb(250, 128, 114);
        public static readonly string RGB_DARK_SALMON = Rgb(233, 150, 122);
        public static readonly string RGB_LIGHT_SALMON = Rgb(255, 160, 122);

        // orange colors
        public static readonly string RGB_ORANGE_RED = Rgb(255, 69, 0);
        public static readonly string RGB_TOMATO = Rgb(255, 99, 71);
        public static readonly string RGB_DARK_ORANGE = Rgb(255, 140, 0);
        public static readonly string RGB_CORAL = Rgb(255, 127, 80);
        public static readonly string RGB_LIGHT_ORANGE = Rgb(255, 165, 0);

        // yellow colors
        public static readonly string RGB_DARK_KHAKI = Rgb(189, 183, 107);
        public static readonly string RGB_GOLD = Rgb(255, 215, 0);
        public static readonly string RGB_KHAKI = Rgb(240, 230, 140);
        public static readonly string RGB_PEACH_PUFF = Rgb(255, 218, 185);
        public static readonly string RGB_PALE_GOLDENROD = Rgb(238, 232, 170);
        public static readonly string RGB_MOCCASIN = Rgb(255, 228, 181);
        public static readonly string RGB_PAPAYA_WHIP = Rgb(255, 239, 213);
        public static readonly string RGB_LIGHT_GOLDENROD_YELLOW = Rgb(250, 250, 210);
        public static readonly string RGB_LEMON_CHIFFON = Rgb(255, 250, 205);
        public static readonly string RGB_LIGHT_YELLOW = Rgb(255, 255, 224);

        // brown colors
        public static readonly string RGB_BROWN = Rgb(165, 42, 42);
        public static readonly string RGB_SADDLE_BROWN = Rgb(139, 69, 19);
        public static readonly string RGB_SIENNA = Rgb(160, 82, 45);
        public static readonly string RGB_CHOCOLATE = Rgb(210, 105, 30);
        public static readonly string RGB_DARK_GOLDENROD = Rgb(184, 134, 11);
        public static readonly string RGB_PERU = Rgb(205, 133, 63);
        public static readonly string RGB_ROSY_BROWN = Rgb(188, 143, 143);
        public static readonly string RGB_GOLDENROD = Rgb(218, 165, 32);
        public static readonly string RGB_SANDY_BROWN = Rgb(244, 164, 96);
        public static readonly string RGB_TAN = Rgb(210, 180, 140);
        public static readonly string RGB_BURLY_WOOD = Rgb(222, 184, 135);
        public static readonly string RGB_WHEAT = Rgb(245, 222, 179);
        public static readonly string RGB_NAVAJO_WHITE = Rgb(255, 222, 173);
        public static readonly string RGB_BISQUE = Rgb(255, 228, 196);
        public static readonly string RGB_BLANCHED_ALMOND = Rgb(255, 235, 205);
        public static readonly string RGB_CORNSILK = Rgb(255, 248, 220);

        // green colors
        public static readonly string RGB_DARK_GREEN = Rgb(0, 100, 0);
        public static readonly string RGB_DARK_OLIVE_GREEN = Rgb(85, 107, 47);
        public static readonly string RGB_FOREST_GREEN = Rgb(34, 139, 34);
        public static readonly string RGB_SEAGREEN = Rgb(46, 139, 87);
        public static readonly string RGB_OLIVE = Rgb(128, 128, 0);
        public static readonly string RGB_OLIVEDRAB = Rgb(107, 142, 35);
        public static readonly string RGB_MEDIUM_SEAGREEN = Rgb(60, 179, 113);
        public static readonly string RGB_LIME_GREEN = Rgb(50, 205, 50);
        public static readonly string RGB_LIME = Rgb(0, 255, 0);
        public static readonly string RGB_SPRING_GREEN = Rgb(0, 255, 127);
        public static readonly string RGB_MEDIUM_SPRING_GREEN = Rgb(0, 250, 154);
        public static readonly string RGB_DARK_SEAGREEN = Rgb(143, 188, 143);
        public static readonly string RGB_MEDIUM_AQUAMARINE = Rgb(102, 205, 170);
        public static readonly string RGB_YELLOW_GREEN = Rgb(154, 205, 50);
        public static readonly string RGB_LAWN_GREEN = Rgb(124, 252, 0);
        public static readonly string RGB_CHARTREUSE = Rgb(127, 255, 0);
        public static readonly string RGB_LIGHT_GREEN = Rgb(144, 238, 144);
        public static readonly string RGB_GREEN_YELLOW = Rgb(173, 255, 47);
        public static readonly string RGB_PALE_GREEN = Rgb(152, 251, 152);

        // cyan colors
        public static readonly string RGB_TEAL = Rgb(0, 128, 128);
        public static readonly string RGB_DARK_CYAN = Rgb(0, 139, 139);
        public static readonly string RGB_LIGHT_SEAGREEN = Rgb(32, 178, 170);
        public static readonly string RGB_CADETBLUE = Rgb(95, 158, 160);
        public static readonly string RGB_DARK_TURQUOISE = Rgb(0, 206, 209);
        public static readonly string RGB_MEDIUM_TURQUOISE = Rgb(72, 209, 204);
        public static readonly string RGB_TURQUOISE = Rgb(64, 224, 208);
        public static readonly string RGB_AQUA = Rgb(0, 255, 255);
        public static readonly string RGB_CYAN = Rgb(0, 255, 255);
        public static readonly string RGB_AQUAMARINE = Rgb(127, 255, 212);
        public static readonly string RGB_PALE_TURQUOISE = Rgb(175, 238, 238);
        public static readonly string RGB_LIGHT_CYAN = Rgb(224, 255, 255);

        // blue colors
        public static readonly string RGB_NAVY = Rgb(0, 0, 128);
        public static readonly string RGB_DARK_BLUE = Rgb(0, 0, 139);
        public static readonly string RGB_MEDIUM_BLUE = Rgb(0, 0, 205);
        public static readonly string RGB_MIDNIGHT_BLUE = Rgb(25, 25, 112);
        public static readonly string RGB_ROYAL_BLUE = Rgb(65, 105, 225);
        public static readonly string RGB_STEEL_BLUE = Rgb(70, 130, 180);
        public static readonly string RGB_DODGER_BLUE = Rgb(30, 144, 255);
        public static readonly string RGB_DEEP_SKYBLUE = Rgb(0, 191, 255);
        public static readonly string RGB_CORNFLOWER_BLUE = Rgb(100, 149, 237);
        public static readonly string RGB_SKYBLUE = Rgb(135, 206, 235);
        public static readonly string RGB_LIGHT_SKYBLUE = Rgb(135, 206, 250);
        public static readonly string RGB_LIGHT_STEEL_BLUE = Rgb(176, 196, 222);
        public static readonly string RGB_LIGHT_BLUE = Rgb(173, 216, 230);
        public static readonly string RGB_POWDER_BLUE = Rgb(176, 224, 230);

        // white colors
        public static readonly string RGB_MISTY_ROSE = Rgb(255, 228, 225);
        public static readonly string RGB_ANTIQUE_WHITE = Rgb(250, 235, 215);
        public static readonly string RGB_LINEN = Rgb(250, 240, 230);
        public static readonly string RGB_BEIGE = Rgb(245, 245, 220);
        public static readonly string RGB_WHITE_SMOKE = Rgb(245, 245, 245);
        public static readonly string RGB_LAVENDER_BLUSH = Rgb(255, 240, 245);
        public static readonly string RGB_OLD_LACE = Rgb(253, 245, 230);
        public static readonly string RGB_ALICE_BLUE = Rgb(240, 248, 255);
        public static readonly string RGB_SEASHELL = Rgb(255, 245, 238);
        public static readonly string RGB_GHOST_WHITE = Rgb(248, 248, 255);
        public static readonly string RGB_HONEYDEW = Rgb(240, 255, 240);
        public static readonly string RGB_FLORAL_WHITE = Rgb(255, 250, 240);
        public static readonly string RGB_AZURE = Rgb(240, 255, 255);
        public static readonly string RGB_MINT_CREAM = Rgb(245, 255, 250);
        public static readonly string RGB_SNOW = Rgb(255, 250, 250);
        public static readonly string RGB_IVORY = Rgb(255, 255, 240);
        public static readonly string RGB_WHITE = Rgb(255, 255, 255);

        // gray and black
        public static readonly string RGB_BLACK = Rgb(0, 0, 0);
        public static readonly string RGB_DARK_SLATE_GRAY = Rgb(47, 79, 79);
        public static readonly string RGB_DIM_GRAY = Rgb(105, 105, 105);
        public static readonly string RGB_SLATE_GRAY = Rgb(112, 128, 144);
        public static readonly string RGB_GRAY = Rgb(128, 128, 128);
        public static readonly string RGB_LIGHT_SLATE_GRAY = Rgb(119, 136, 153);
        public static readonly string RGB_DARK_GRAY = Rgb(169, 169, 169);
        public static readonly string RGB_SILVER = Rgb(192, 192, 192);
        public static readonly string RGB_LIGHT_GRAY = Rgb(211, 211, 211);
        public static readonly string RGB_GAINSBORO = Rgb(220, 220, 220);
    }
}