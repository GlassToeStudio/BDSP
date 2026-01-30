// Using statements for core .NET libraries for console, drawing, and runtime services.
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Text;
using BDSP.Core.Berries; // Your project-specific namespace

namespace BDSP.Core.CLI
{
    /// <summary>
    /// A static utility class to render images from files to the console using colored ANSI block characters.
    /// Contains Windows-specific (P/Invoke) methods for controlling console size and font for an optimal viewing experience.
    /// </summary>
    internal static class ImageToConsole
    {
        #region Public API

        /// <summary>
        /// Loads, resizes, and converts an image into a string of colored ANSI characters for console display.
        /// This is the primary method with the core logic.
        /// </summary>
        /// <param name="ID">The ID of the image to load (e.g., a Berry ID).</param>
        /// <param name="resolution">The character resolution (width and height) to render the image at. Higher values provide more detail.</param>
        /// <param name="fontSize">The desired console font size. Smaller values allow for a larger, more detailed image on screen (works best in legacy cmd.exe).</param>
        /// <returns>A string containing ANSI escape codes to display the image in a compatible terminal.</returns>
        [SupportedOSPlatform("windows")]
        public static string GetImageString(int ID, int resolution = 250, short fontSize = 2)
        {
            // On Windows, this will attempt to set up the console window for optimal viewing.
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                SetupConsoleForImage(resolution, resolution, fontSize);
            }

            string path = Path.Combine(AppContext.BaseDirectory, "Assets", "Berries", $"{ID}.png");
            if (!File.Exists(path))
            {
                return $"Error: File not found at {path}";
            }

            // Pre-allocate StringBuilder capacity for a significant performance boost by reducing re-allocations.
            // Estimate: (width * height * (block chars + color code chars)) + height for newlines.
            var sb = new StringBuilder(resolution * resolution * (2 + 20) + resolution);

            using (var bmp = new Bitmap(path))
            using (var resizedImage = ResizeImage(bmp, resolution, resolution))
            {
                var rect = new Rectangle(0, 0, resizedImage.Width, resizedImage.Height);
                BitmapData? bmpData = null;
                try
                {
                    // Lock the bitmap's bits for direct memory access. This is orders of magnitude faster than GetPixel().
                    bmpData = resizedImage.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);

                    int bytesPerPixel = 4; // For a 32-bit ARGB image
                    int byteCount = Math.Abs(bmpData.Stride) * resizedImage.Height;
                    byte[] pixels = new byte[byteCount];

                    // Perform one single, fast block copy from unmanaged bitmap memory to our managed byte array.
                    Marshal.Copy(bmpData.Scan0, pixels, 0, byteCount);

                    for (int y = 0; y < resizedImage.Height; y++)
                    {
                        int rowIndex = y * bmpData.Stride;
                        for (int x = 0; x < resizedImage.Width; x++)
                        {
                            // Byte order in Format32bppArgb is BGRA (Blue, Green, Red, Alpha)
                            int pixelIndex = rowIndex + (x * bytesPerPixel);
                            byte b = pixels[pixelIndex];
                            byte g = pixels[pixelIndex + 1];
                            byte r = pixels[pixelIndex + 2];
                            byte a = pixels[pixelIndex + 3];

                            // OPTIMIZATION: Skip fully transparent pixels to avoid rendering a black background.
                            if (a < 20)
                            {
                                sb.Append("  "); // Use two spaces to match the width of "{block}{block}"
                                continue;
                            }

                            // VISUAL IMPROVEMENT: For semi-transparent pixels, blend them against a black background for smoother edges.
                            if (a < 255)
                            {
                                double alphaFactor = a / 255.0;
                                r = (byte)(r * alphaFactor);
                                g = (byte)(g * alphaFactor);
                                b = (byte)(b * alphaFactor);
                            }

                            // Calculate the perceived brightness (luminance) of the pixel to select a shading character.
                            double luminance = (0.299 * r + 0.587 * g + 0.114 * b) / 255.0;

                            // Get the character that best represents this brightness level.
                            string block = "█"; // GetShadeCharacter(luminance);

                            // Append the colored character twice to create a more square-like "pixel" on most console fonts.
                            sb.Append($"{Colors.Rgb(r, g, b)}{block}{block}");
                        }
                        sb.Append('\n');
                    }
                }
                finally
                {
                    // CRITICAL: Always unlock the bits in a finally block to prevent memory leaks, even if errors occur.
                    if (bmpData != null)
                    {
                        resizedImage.UnlockBits(bmpData);
                    }
                }
            }
            sb.Append(Colors.Reset); // Reset console color at the end.
            return sb.ToString();
        }

        // --- Helper Overloads for convenience ---
        public static string GetImageString(BerryId ID, int resolution = 100, short fontSize = 8) => GetImageString(ID.Value, resolution, fontSize);
        public static string GetImageString(Berry berry, int resolution = 100, short fontSize = 8) => GetImageString(berry.Id, resolution, fontSize);

        #endregion

        #region Private Helpers and Implementations

        /// <summary>
        /// Selects a character to represent a pixel's brightness.
        /// This table uses the "Light on a Black Screen" model, where characters act as light sources.
        /// A bright pixel needs a dense character (a big light), and a dark pixel needs a sparse
        /// character (a small light or no light).
        /// </summary>
        /// <remarks>
        /// <code>
        /// | Character | Density   | Usage (for a Black Console Background)                     |
        /// | --------- | --------- | ---------------------------------------------------------- |
        /// |  █        | Very High | Used for the **brightest** pixels (whites, bright colors). |
        /// |  ▓        | High      | Used for bright pixels.                                    |
        /// |  ▒        | Medium    | Used for mid-range brightness pixels.                      |
        /// |  ░        | Low       | Used for dim pixels.                                       |
        /// |  •        | Very Low  | Used for very dim, near-black pixels.                      |
        /// |  .        | Minimal   | Used for pixels that are just barely visible.              |
        /// |           | None      | Used for **black** or fully transparent pixels (no light). |
        /// |------------------------------------------------------------------------------------|
        /// </code></remarks>
        private static string GetShadeCharacter(double luminance)
        {
            // This C# 9+ switch expression maps a luminance value (0.0=dark, 1.0=bright) to a character.
            // Bright pixels get dense characters (more "light"), dark pixels get sparse characters (less "light").
            return luminance switch
            {
                > 0.9 => "█",
                > 0.8 => "▓",
                > 0.7 => "▒",
                > 0.6 => "░",
                > 0.3 => "•",
                > 0.1 => ".",
                _ => " " // Represents black (no light).
            };
        }

        /// <summary>
        /// Resizes an image using high-quality bicubic interpolation for better downscaling results.
        /// </summary>
        [SupportedOSPlatform("windows")]
        private static Bitmap ResizeImage(Image image, int width, int height)
        {
            var resizedImage = new Bitmap(width, height);
            using (var g = Graphics.FromImage(resizedImage))
            {
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.DrawImage(image, 0, 0, width, height);
            }
            return resizedImage;
        }

        #endregion

        #region Windows-Specific Console Manipulation

        /// <summary>
        /// Configures the console window size and font for displaying the image.
        /// This is a Windows-specific operation.
        /// </summary>
        [SupportedOSPlatform("windows")]
        private static void SetupConsoleForImage(int imageWidth, int imageHeight, short fontSize)
        {
            Console.WriteLine($"Attempting to set font size to {fontSize}...");
            if (SetConsoleFontSize(fontSize))
            {
                Console.WriteLine("SUCCESS: Font size API call succeeded. (Visibility depends on terminal).");
            }
            else
            {
                Console.WriteLine("WARNING: Failed to set font size. (This is expected in modern terminals like Windows Terminal).");
            }

            Thread.Sleep(100);

            int desiredWidth = imageWidth * 2;
            int desiredHeight = imageHeight;

            Console.Clear();

            try
            {
                int newWidth = Math.Min(desiredWidth, Console.LargestWindowWidth);
                int newHeight = Math.Min(desiredHeight, Console.LargestWindowHeight);

                // This robust "shrink-first" method prevents crashes when making the console smaller than its current size.
                Console.SetWindowSize(1, 1);
                Console.SetBufferSize(newWidth, newHeight);
                Console.SetWindowSize(newWidth, newHeight);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error resizing console. Is it running in an interactive window? {ex.Message}");
            }
        }

        /// <summary>
        /// Sets the console font size using Windows P/Invoke calls.
        /// </summary>
        [SupportedOSPlatform("windows")]
        private static bool SetConsoleFontSize(short size)
        {
            try
            {
                IntPtr hnd = NativeMethods.GetStdHandle(-11);
                if (hnd != IntPtr.Zero && hnd.ToInt64() != -1)
                {
                    var fontInfo = new NativeMethods.CONSOLE_FONT_INFO_EX();
                    fontInfo.cbSize = (uint)Marshal.SizeOf(fontInfo);
                    if (!NativeMethods.GetCurrentConsoleFontEx(hnd, false, ref fontInfo)) return false;
                    fontInfo.dwFontSize = new NativeMethods.COORD { X = 0, Y = size };
                    return NativeMethods.SetCurrentConsoleFontEx(hnd, false, ref fontInfo);
                }
                return false;
            }
            catch { return false; }
        }

        /// <summary>
        /// Contains the P/Invoke signatures for interacting with the Windows Kernel.
        /// </summary>
        [SupportedOSPlatform("windows")]
        private static class NativeMethods
        {
            private const int STD_OUTPUT_HANDLE = -11;

            [DllImport("kernel32.dll", SetLastError = true)]
            internal static extern IntPtr GetStdHandle(int nStdHandle);

            [DllImport("kernel32.dll", SetLastError = true)]
            internal static extern bool GetCurrentConsoleFontEx(IntPtr h, bool b, ref CONSOLE_FONT_INFO_EX lp);

            [DllImport("kernel32.dll", SetLastError = true)]
            internal static extern bool SetCurrentConsoleFontEx(IntPtr h, bool b, ref CONSOLE_FONT_INFO_EX lp);

            [StructLayout(LayoutKind.Sequential)]
            internal struct COORD { internal short X; internal short Y; }

            [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
            internal struct CONSOLE_FONT_INFO_EX
            {
                internal uint cbSize;
                internal uint nFont;
                internal COORD dwFontSize;
                internal int FontFamily;
                internal int FontWeight;
                [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
                internal string FaceName;
            }
        }

        #endregion
    }
}
