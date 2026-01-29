using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Text;
using BDSP.Core.Berries; // Assuming this namespace exists from your previous code

namespace BDSP.Core.CLI
{
    /// <summary>
    /// A static class to render images to the console using colored block characters.
    /// Contains Windows-specific P/Invoke methods for controlling console size and font.
    /// </summary>
    internal static class ImageToConsole
    {
        #region P/Invoke for Console Font Size

        // This attribute marks the following methods as Windows-only.
        // They will not compile or run on Linux or macOS.
        [SupportedOSPlatform("windows")]
        private static class NativeMethods
        {
            private const int STD_OUTPUT_HANDLE = -11;

            [DllImport("kernel32.dll", SetLastError = true)]
            internal static extern IntPtr GetStdHandle(int nStdHandle);

            [DllImport("kernel32.dll", SetLastError = true)]
            internal static extern bool GetCurrentConsoleFontEx(
                IntPtr hConsoleOutput,
                bool bMaximumWindow,
                ref CONSOLE_FONT_INFO_EX lpConsoleCurrentFontEx);

            [DllImport("kernel32.dll", SetLastError = true)]
            internal static extern bool SetCurrentConsoleFontEx(
                IntPtr hConsoleOutput,
                bool bMaximumWindow,
                ref CONSOLE_FONT_INFO_EX lpConsoleCurrentFontEx);

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
        /// Sets the console font size. Only works on Windows in compatible terminals (like cmd.exe).
        /// This method is often blocked by modern terminals like Windows Terminal.
        /// </summary>
        /// <param name="size">The desired font height. A good range is 8-12.</param>
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

                    if (!NativeMethods.GetCurrentConsoleFontEx(hnd, false, ref fontInfo))
                    {
                        return false;
                    }

                    fontInfo.dwFontSize = new NativeMethods.COORD(0, size); // Width 0 lets system choose

                    return NativeMethods.SetCurrentConsoleFontEx(hnd, false, ref fontInfo);
                }
                return false;
            }
            catch
            {
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
            // ... (The SetConsoleFontSize part remains the same) ...
            Console.WriteLine($"Attempting to set font size to {fontSize}...");
            if (SetConsoleFontSize(fontSize))
            {
                Console.WriteLine("SUCCESS: Font size changed.");
            }
            else
            {
                Console.WriteLine("WARNING: Failed to set font size (expected in modern terminals).");
            }
            Thread.Sleep(100);

            int desiredWidth = imageWidth * 2;
            int desiredHeight = imageHeight;

            Console.Clear();

            try
            {
                // Clamp to what the system reports as the maximum possible size.
                // This prevents trying to make a window larger than the physical screen.
                int newWidth = Math.Min(desiredWidth, Console.LargestWindowWidth);
                int newHeight = Math.Min(desiredHeight, Console.LargestWindowHeight);

                // --- THE FIX: This is the new, correct order of operations ---

                // 1. Set the Window size DOWN first to a minimal value.
                //    This ensures the window is not "in the way" when we resize the buffer.
                Console.SetWindowSize(1, 1);

                // 2. Now, set the Buffer size to our desired dimensions.
                //    This now works because the window is tiny.
                Console.SetBufferSize(newWidth, newHeight);

                // 3. Finally, set the Window size to our desired dimensions.
                //    This works because the buffer is already the correct size.
                Console.SetWindowSize(newWidth, newHeight);
            }
            catch (Exception ex)
            {
                // This will now only catch more legitimate errors, like the console not being interactive.
                Console.WriteLine($"Error resizing console. Is it running interactively? {ex.Message}");
            }
        }

        /// <summary>
        /// Resizes an image using high-quality bicubic interpolation.
        /// </summary>
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
        /// Generates a string representation of an image using colored console blocks.
        /// </summary>
        public static string GetImageString(int ID)
        {
            // --- CONTROLS ---
            // Adjust these values to change the output.
            // resolution: The number of characters wide/high the image will be. Higher = more detail.
            // fontSize: The size of each character. Smaller allows for a higher resolution on screen.
            const int resolution = 50;
            const short fontSize = 2;
            // ------------------

            // On Windows, this will set up the console window. It does nothing on other platforms.
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                SetupConsoleForImage(resolution, resolution, fontSize);
            }

            string path = Path.Combine(AppContext.BaseDirectory, "Assets", "Berries", $"{ID}.png");
            if (!File.Exists(path))
            {
                return $"Error: File not found at {path}";
            }

            var sb = new StringBuilder();

            using (var bmp = new Bitmap(path))
            using (var resizedImage = ResizeImage(bmp, resolution, resolution))
            {
                var rect = new Rectangle(0, 0, resizedImage.Width, resizedImage.Height);
                BitmapData bmpData = null;

                try
                {
                    // Lock the bitmap's bits for direct memory access. This is much faster than GetPixel().
                    bmpData = resizedImage.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);

                    int bytesPerPixel = 4; // For a 32-bit ARGB image
                    int byteCount = Math.Abs(bmpData.Stride) * resizedImage.Height;
                    byte[] pixels = new byte[byteCount];

                    // Perform one single, fast copy from the bitmap memory to our array.
                    Marshal.Copy(bmpData.Scan0, pixels, 0, byteCount);

                    // Loop through the byte array instead of calling GetPixel() repeatedly.
                    sb.Append(Colors.WhiteBg + Colors.Black);
                    for (int i = 0; i < resizedImage.Width; i++)
                    {
                        // Append something related to 'i'
                        sb.Append($"{i}."); 
                    }
                    sb.Append("\n");
                    for (int y = 0; y < resizedImage.Height; y++)
                    {
                        sb.Append($"{Colors.WhiteBg}{Colors.Black}{y}.\t");
                        int rowIndex = y * bmpData.Stride;
                        for (int x = 0; x < resizedImage.Width; x++)
                        {
                            // Byte order in Format32bppArgb is BGRA (Blue, Green, Red, Alpha)
                            int pixelIndex = rowIndex + (x * bytesPerPixel);
                            byte b = pixels[pixelIndex];
                            byte g = pixels[pixelIndex + 1];
                            byte r = pixels[pixelIndex + 2];
                            byte a = pixels[pixelIndex + 3]; // Alpha is available if you need it

                            /*
                             * 
                             * Character	Density     (Approx.)	    Notes
                             *  █	        Very High   (100%)	        Full block. Best for fully opaque, bold colors.
                             *  ▓	        High        (75%)           Dark shade.
                             *  ▒	        Medium      (50%)	        Medium shade.
                             *  ░	        Low         (25%)	        Light shade.
                             *  •	        Very Low	                A bullet point can give a speckled effect.
                             *  .	        Very Low	                A simple dot, barely there.
                             *  None (0%)	The space character.        Useful for very bright or transparent areas.
                             *  
                             */


                            // Inside GetImageString, in the inner loop (where you get r, g, b):

                            // Assuming 'r', 'g', 'b' are your pixel color components
                            //double luminance = (0.299 * r + 0.587 * g + 0.114 * b) / 255.0; // Normalize 0.0 to 1.0
                            //string block = GetShadeCharacter_INVERTED(luminance); // Get character based on brightness
                            string block = "█";
;

                            if (a <= .99)
                            {
                                sb.Append("  ");
                                continue;
                            }


                            // Assuming you have a 'Colors' class for ANSI escape codes
                            sb.Append($"{Colors.Rgb(r, g, b)}{block}{block}");
                        }
                        sb.Append('\n');
                    }
                }
                finally
                {
                    // CRITICAL: Always unlock the bits, even if an error occurs.
                    if (bmpData != null)
                    {
                        resizedImage.UnlockBits(bmpData);
                    }
                }
            }

            sb.Append(Colors.Reset); // Reset color at the end
            return sb.ToString();
        }

        // --- THIS PRODUCES A NEGATIVE/INVERTED IMAGE ---
        private static string GetShadeCharacter_INVERTED(double luminance)
        {
            if (luminance > 0.9) return "█";   // Very BRIGHT pixel -> MOST ink (Incorrect for standard image)
            if (luminance > 0.8) return "▓";
            if (luminance > 0.7) return "▒";
            if (luminance > 0.6) return "░";
            if (luminance > 0.4) return "•";
            if (luminance > 0.2) return ".";   // Fairly DARK pixel -> LESS ink (Incorrect)
            return " ";                        // Very DARK pixel -> LEAST ink (Incorrect)
        }
        private static string GetShadeCharacter(double luminance)
        {
            if (luminance > 0.9) return " ";   // Very bright
            if (luminance > 0.8) return ".";   // Bright
            if (luminance > 0.7) return "•";   // Less bright
            if (luminance > 0.6) return "░";   // Light shade
            if (luminance > 0.4) return "▒";   // Medium shade
            if (luminance > 0.2) return "▓";   // Dark shade
            return "█";                        // Very dark
        }

        public static string GetImageString(BerryId ID) => GetImageString(ID.Value);

        public static string GetImageString(Berry berry) => GetImageString(berry.Id);
    }
}
