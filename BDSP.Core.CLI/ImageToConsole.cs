// Using statements for core .NET libraries for console, drawing, and runtime services.
// Note: Unnecessary 'using Avalonia.*' statements have been removed.
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
    /// A static class to render images from files to the console using colored block characters.
    /// Contains Windows-specific (P/Invoke) methods for controlling console size and font for an optimal viewing experience.
    /// </summary>
    internal static class ImageToConsole
    {
        #region P/Invoke for Windows Console Font Size

        // This attribute marks the nested class and its methods as supported only on the Windows operating system.
        // This prevents build warnings and runtime errors on other platforms like Linux or macOS.
        [SupportedOSPlatform("windows")]
        private static class NativeMethods
        {
            private const int STD_OUTPUT_HANDLE = -11;

            // Imports the GetStdHandle function from kernel32.dll to get a handle to the standard output.
            [DllImport("kernel32.dll", SetLastError = true)]
            internal static extern IntPtr GetStdHandle(int nStdHandle);

            // Imports the GetCurrentConsoleFontEx function to retrieve extended information about the current console font.
            [DllImport("kernel32.dll", SetLastError = true)]
            internal static extern bool GetCurrentConsoleFontEx(
                IntPtr hConsoleOutput,
                bool bMaximumWindow,
                ref CONSOLE_FONT_INFO_EX lpConsoleCurrentFontEx);

            // Imports the SetCurrentConsoleFontEx function to set extended information for the console font.
            [DllImport("kernel32.dll", SetLastError = true)]
            internal static extern bool SetCurrentConsoleFontEx(
                IntPtr hConsoleOutput,
                bool bMaximumWindow,
                ref CONSOLE_FONT_INFO_EX lpConsoleCurrentFontEx);

            // A struct to represent coordinates, used for font size.
            [StructLayout(LayoutKind.Sequential)]
            internal struct COORD
            {
                internal short X;
                internal short Y;

                internal COORD(short x, short y)
                {
                    X = x;
                    Y = y;
                }
            }

            // A struct that holds all the extended font information for the console.
            // The CharSet.Unicode and ByValTStr attributes ensure strings are handled correctly.
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

        /// <summary>
        /// Sets the console font size using Windows-specific P/Invoke calls.
        /// This method is often blocked by modern terminals like Windows Terminal but works in the legacy cmd.exe.
        /// </summary>
        /// <param name="size">The desired font height. A good range is 8-12 for high-resolution images.</param>
        /// <returns>True if successful, false otherwise.</returns>
        [SupportedOSPlatform("windows")]
        public static bool SetConsoleFontSize(short size)
        {
            try
            {
                IntPtr hnd = NativeMethods.GetStdHandle(-11);
                if (hnd != IntPtr.Zero && hnd.ToInt64() != -1)
                {
                    var fontInfo = new NativeMethods.CONSOLE_FONT_INFO_EX();
                    fontInfo.cbSize = (uint)Marshal.SizeOf(fontInfo);

                    // Get the current font info to modify it.
                    if (!NativeMethods.GetCurrentConsoleFontEx(hnd, false, ref fontInfo))
                    {
                        return false;
                    }

                    // Set the new font size. Width 0 lets the system choose the best width.
                    fontInfo.dwFontSize = new NativeMethods.COORD(0, size);

                    // Apply the new settings.
                    return NativeMethods.SetCurrentConsoleFontEx(hnd, false, ref fontInfo);
                }
                return false;
            }
            catch
            {
                // Catch any exceptions during P/Invoke calls and return false.
                return false;
            }
        }

        #endregion

        /// <summary>
        /// Configures the console window size and font for displaying the image.
        /// This is a Windows-specific operation.
        /// </summary>
        [SupportedOSPlatform("windows")]
        public static void SetupConsoleForImage(int imageWidth, int imageHeight, short fontSize)
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

            // A small delay can help the console apply font changes before resizing.
            Thread.Sleep(100);

            // Each pixel is rendered as two block characters ("██") to approximate a square aspect ratio.
            int desiredWidth = imageWidth * 2;
            int desiredHeight = imageHeight;

            Console.Clear();

            try
            {
                // Clamp the desired size to the maximum possible on the current display to prevent errors.
                int newWidth = Math.Min(desiredWidth, Console.LargestWindowWidth);
                int newHeight = Math.Min(desiredHeight, Console.LargestWindowHeight);

                // This is the robust "shrink-first" method to prevent crashes when making the console smaller.
                // 1. Set the Window size DOWN first to a minimal value.
                Console.SetWindowSize(1, 1);
                // 2. Now, set the Buffer size to our desired dimensions.
                Console.SetBufferSize(newWidth, newHeight);
                // 3. Finally, set the Window size up to match the buffer.
                Console.SetWindowSize(newWidth, newHeight);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error resizing console. Is it running in an interactive window? {ex.Message}");
            }
        }

        /// <summary>
        /// Resizes an image using high-quality bicubic interpolation for better downscaling results.
        /// </summary>
        [SupportedOSPlatform("windows")]
        public static Bitmap ResizeImage(Image image, int width, int height)
        {
            var resizedImage = new Bitmap(width, height);
            using (var g = Graphics.FromImage(resizedImage))
            {
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.DrawImage(image, 0, 0, width, height);
            }
            return resizedImage;
        }

        /// <summary>
        /// Loads, resizes, and converts an image into a string of colored ANSI characters for console display.
        /// </summary>
        [SupportedOSPlatform("windows")]
        public static string GetImageString(int ID)
        {
            // --- CONTROLS ---
            // Adjust these values to change the output appearance.
            // resolution: The number of characters wide/high the image will be. Higher = more detail.
            // fontSize: The size of each character. Smaller allows for a higher resolution on screen (in compatible terminals).
            const int resolution = 250;
            const short fontSize = 2;
            // ------------------

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

            // Pre-allocate StringBuilder capacity for a small performance boost by reducing re-allocations.
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

                            // Skip fully transparent pixels to avoid rendering a black background.
                            if (a < 20)
                            {
                                sb.Append("  "); // Use two spaces to match the width of "{block}{block}"
                                continue;
                            }

                            // For semi-transparent pixels, blend them against a black background for smoother edges.
                            //if (a < 255)
                            //{
                            //    double alphaFactor = a / 255.0;
                            //    r = (byte)(r * alphaFactor);
                            //    g = (byte)(g * alphaFactor);
                            //    b = (byte)(b * alphaFactor);
                            //}

                            // Calculate the perceived brightness (luminance) of the pixel.
                            double luminance = (0.299 * r + 0.587 * g + 0.114 * b) / 255.0;

                            // Get the character that best represents this brightness level.
                            string block = "█"; //GetShadeCharacter(luminance);

                            // Append the colored character twice to create a more square-like "pixel".
                            sb.Append($"{Colors.Rgb(r, g, b)}{block}{block}");
                        }
                        sb.Append('\n');
                    }
                }
                finally
                {
                    // CRITICAL: Always unlock the bits in a finally block to prevent memory leaks.
                    if (bmpData != null)
                    {
                        resizedImage.UnlockBits(bmpData);
                    }
                }
            }
            sb.Append(Colors.Reset); // Reset console color at the end.
            return sb.ToString();
        }

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
        private static string GetShadeCharacter(double luminance) // luminance is 0.0 (dark) to 1.0 (bright)
        {
            // C# 9+ switch expression
            return luminance switch
            {
                > 0.2000 => "█",   // Very BRIGHT pixel -> Use MOST "light" (full block)
                > 0.1500 => "▓",
                > 0.1000 => "▒",
                > 0.0500 => "░",
                > 0.0250 => "•",
                > 0.0125 => ".",   // Fairly DARK pixel -> Use LESS "light" (a dot)
                _ => " "    // Black pixel (or very dark) -> Use NO "light" (space)
            };
        }

        // --- Helper Overloads ---
        public static string GetImageString(BerryId ID) => GetImageString(ID.Value);
        public static string GetImageString(Berry berry) => GetImageString(berry.Id);
    }
}


