using Avalonia;
using Avalonia.Controls; // <--- CRITICAL: This brings Application.Current into scope
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Platform.Storage;
using BDSP.Core.Berries;
using HarfBuzzSharp;
using System;
using System.Runtime.InteropServices;
using System.Text;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Drawing;

namespace BDSP.Core.CLI
{
    internal static class ImageToConsole
    {
        public static Bitmap LoadImageAsset(int ID)
        {

            var uri = new Uri(
                $"avares://BDSP.UI/Assets/Berries/{BerryNames.GetName(ID)}.png");

            return new Bitmap(
                AssetLoader.Open(uri));
        }

        public static Bitmap LoadImageAsset(BerryId ID)
        {

            return LoadImageAsset(ID.Value);
        }

        public static Bitmap LoadImageAsset(Berry berry)
        {

            return LoadImageAsset(berry.Id);
        }

        public struct PixelColor
        {
            public byte R;
            public byte G;
            public byte B;
            public byte A;
        }
    }


    public class ExampleUsage
    {
        public async Task RunExample()
        {
            var loader = new ImageLoader();

            // Single image
            string pixelData = await loader.LoadImageAndGetPixelDataAsync("1.png");
            Console.WriteLine(pixelData);

            // Multiple images in parallel
            var imageNames = new[] { "2.png", "3.png", "4.png" };
            var allPixelData = await loader.LoadMultipleImagesAsync(imageNames);

            foreach (var kvp in allPixelData)
            {
                Console.WriteLine($"=== {kvp.Key} ===");
                Console.WriteLine(kvp.Value);
            }
        }
    }

    public class ImageLoader
    {
        /// <summary>
        /// Loads a PNG image from the Assets folder and extracts pixel RGB values as a formatted string.
        /// Optimized for large images and batch processing.
        /// </summary>
        /// <param name="imageName">The name of the image file (e.g., "myimage.png")</param>
        /// <returns>A string containing RGB values for each pixel, organized by rows</returns>
        public async Task<string> LoadImageAndGetPixelDataAsync(string imageName)
        {
            // Validate input to prevent path traversal attacks
            if (string.IsNullOrWhiteSpace(imageName))
                throw new ArgumentException("Image name cannot be null or empty.", nameof(imageName));

            // Sanitize the file name - prevent directory traversal
            string sanitizedName = Path.GetFileName(imageName);
            if (string.IsNullOrEmpty(sanitizedName) || sanitizedName != imageName)
                throw new ArgumentException("Invalid image name. Path traversal is not allowed.", nameof(imageName));

            // Validate file extension
            string extension = Path.GetExtension(sanitizedName).ToLowerInvariant();
            if (extension != ".png")
                throw new ArgumentException("Only PNG files are supported.", nameof(imageName));

            // Construct the safe path to the assets folder
            string assetsPath = Path.Combine(AppContext.BaseDirectory, "Assets", sanitizedName);

            // Verify the file exists
            if (!File.Exists(assetsPath))
                throw new FileNotFoundException($"Image not found in Assets folder.", sanitizedName);

            // Verify the resolved path is still within the Assets directory (defense in depth)
            string fullAssetsDir = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "Assets"));
            string fullFilePath = Path.GetFullPath(assetsPath);
            if (!fullFilePath.StartsWith(fullAssetsDir, StringComparison.OrdinalIgnoreCase))
                throw new UnauthorizedAccessException("Access to the specified path is denied.");

            return await Task.Run(() => ProcessImage(fullFilePath));
        }

        /// <summary>
        /// Processes the image and extracts pixel data using CopyPixels with IntPtr.
        /// </summary>
        private string ProcessImage(string filePath)
        {
            using var stream = File.OpenRead(filePath);
            using var bitmap = new Bitmap(stream);

            // Get the pixel size
            var pixelSize = bitmap.PixelSize;
            int width = pixelSize.Width;
            int height = pixelSize.Height;

            // Calculate stride (bytes per row) - Avalonia uses Bgra8888 format (4 bytes per pixel)
            int bytesPerPixel = 4; // BGRA format
            int stride = width * bytesPerPixel;
            int bufferSize = height * stride;

            // Allocate unmanaged memory for pixel data
            IntPtr bufferPtr = Marshal.AllocHGlobal(bufferSize);

            try
            {
                // Copy pixel data from the bitmap using CopyPixels with IntPtr
                // Signature: CopyPixels(PixelRect sourceRect, IntPtr buffer, int bufferSize, int stride)
                bitmap.CopyPixels(new PixelRect(0, 0, width, height), bufferPtr, bufferSize, stride);

                // Pre-calculate approximate string size for StringBuilder capacity
                // Each pixel: "(255, 255, 255) " = ~16 chars, plus newlines
                int estimatedCapacity = (width * 18 + 2) * height;
                var sb = new StringBuilder(estimatedCapacity);

                // Process pixels row by row using Span for better performance
                unsafe
                {
                    byte* pixelData = (byte*)bufferPtr;

                    for (int y = 0; y < height; y++)
                    {
                        int rowOffset = y * stride;

                        for (int x = 0; x < width; x++)
                        {
                            int pixelOffset = rowOffset + (x * bytesPerPixel);

                            // Avalonia uses BGRA format
                            byte b = pixelData[pixelOffset];
                            byte g = pixelData[pixelOffset + 1];
                            byte r = pixelData[pixelOffset + 2];
                            // byte a = pixelData[pixelOffset + 3]; // Alpha if needed

                            // Append RGB values
                            sb.Append('(');
                            sb.Append(r);
                            sb.Append(", ");
                            sb.Append(g);
                            sb.Append(", ");
                            sb.Append(b);
                            sb.Append(") ");
                        }
                        sb.AppendLine();
                    }
                }

                return sb.ToString();
            }
            finally
            {
                // Always free the unmanaged memory
                Marshal.FreeHGlobal(bufferPtr);
            }
        }

        /// <summary>
        /// Alternative safe version without unsafe code (slightly slower but no unsafe keyword needed)
        /// </summary>
        private string ProcessImageSafe(string filePath)
        {
            using var stream = File.OpenRead(filePath);
            using var bitmap = new Bitmap(stream);

            var pixelSize = bitmap.PixelSize;
            int width = pixelSize.Width;
            int height = pixelSize.Height;

            int bytesPerPixel = 4;
            int stride = width * bytesPerPixel;
            int bufferSize = height * stride;

            // Allocate managed array and pin it
            byte[] pixelBuffer = new byte[bufferSize];
            GCHandle handle = GCHandle.Alloc(pixelBuffer, GCHandleType.Pinned);

            try
            {
                IntPtr bufferPtr = handle.AddrOfPinnedObject();
                bitmap.CopyPixels(new PixelRect(0, 0, width, height), bufferPtr, bufferSize, stride);

                int estimatedCapacity = (width * 18 + 2) * height;
                var sb = new StringBuilder(estimatedCapacity);

                for (int y = 0; y < height; y++)
                {
                    int rowOffset = y * stride;

                    for (int x = 0; x < width; x++)
                    {
                        int pixelOffset = rowOffset + (x * bytesPerPixel);

                        byte b = pixelBuffer[pixelOffset];
                        byte g = pixelBuffer[pixelOffset + 1];
                        byte r = pixelBuffer[pixelOffset + 2];

                        sb.Append('(');
                        sb.Append(r);
                        sb.Append(", ");
                        sb.Append(g);
                        sb.Append(", ");
                        sb.Append(b);
                        sb.Append(") ");
                    }
                    sb.AppendLine();
                }

                return sb.ToString();
            }
            finally
            {
                handle.Free();
            }
        }

        /// <summary>
        /// Processes multiple images in parallel for maximum throughput.
        /// </summary>
        /// <param name="imageNames">Collection of image file names</param>
        /// <returns>Dictionary mapping image names to their pixel data strings</returns>
        public async Task<Dictionary<string, string>> LoadMultipleImagesAsync(IEnumerable<string> imageNames)
        {
            var results = new System.Collections.Concurrent.ConcurrentDictionary<string, string>();

            await Parallel.ForEachAsync(imageNames, async (imageName, cancellationToken) =>
            {
                try
                {
                    string pixelData = await LoadImageAndGetPixelDataAsync(imageName);
                    results.TryAdd(imageName, pixelData);
                }
                catch (Exception ex)
                {
                    // Log the error but continue processing other images
                    results.TryAdd(imageName, $"Error: {ex.Message}");
                }
            });

            return new Dictionary<string, string>(results);
        }
    }

    //public class Avalonia11PixelReader
    //{
    //    [StructLayout(LayoutKind.Sequential)]
    //    public struct PixelColor { public byte B, G, R, A; }

    //    /// <summary>
    //    /// Loads a PNG from assets and reads its pixel data.
    //    /// This is the corrected, working method for Avalonia 11.
    //    /// </summary>
    //    public static unsafe void ProcessImageFromAssets(string assetName)
    //    {
    //        // 1. Build the asset URI.
    //        var assemblyName = System.Reflection.Assembly.GetExecutingAssembly().GetName().Name;
    //        var uri = new Uri($"avares://{assemblyName}/Assets/{assetName}");

    //        Bitmap bitmap;
    //        try
    //        {
    //            // 2. Open the asset stream using the STATIC AssetLoader.Open method.
    //            // This is the simple, correct way. No service locator needed.
    //            using (var stream = AssetLoader.Open(uri))
    //            {
    //                bitmap = new Bitmap(stream);
    //            }
    //        }
    //        catch (Exception ex)
    //        {
    //            Console.WriteLine($"[ERROR] Could not load asset '{uri}'.");
    //            Console.WriteLine("Please ensure three things:");
    //            Console.WriteLine($"1. The file '{assetName}' exists in your '/Assets' folder.");
    //            Console.WriteLine("2. The file's 'Build Action' property is set to 'AvaloniaResource'.");
    //            Console.WriteLine($"3. The assembly name '{assemblyName}' is correct.");
    //            Console.WriteLine($"Exception: {ex.Message}");
    //            return;
    //        }

    //        using (bitmap)
    //        {
    //            // 3. Lock the bitmap's memory for direct access.
    //            // This 'using' block ensures it's always unlocked.
    //            // If this line fails, your project setup is incorrect (missing <AllowUnsafeBlocks> or package reference).
    //            using (var framebuffer = bitmap.Lock())
    //            {
    //                if (framebuffer.Format != PixelFormat.Bgra8888)
    //                {
    //                    Console.WriteLine($"[ERROR] Image format is not Bgra8888. Actual: {framebuffer.Format}");
    //                    return;
    //                }

    //                int width = framebuffer.Size.Width;
    //                int height = framebuffer.Size.Height;
    //                long byteCount = (long)width * height * sizeof(PixelColor);

    //                var pixelData = new PixelColor[width * height];

    //                // 4. Perform a fast copy of the entire pixel buffer.
    //                GCHandle handle = GCHandle.Alloc(pixelData, GCHandleType.Pinned);
    //                try
    //                {
    //                    System.Buffer.MemoryCopy(
    //                        (void*)framebuffer.Address,
    //                        (void*)handle.AddrOfPinnedObject(),
    //                        byteCount,
    //                        byteCount
    //                    );
    //                }
    //                finally
    //                {
    //                    handle.Free();
    //                }

    //                // 5. Build and print the string for each row.
    //                Console.WriteLine($"--- Reading Image: {assetName} ({width}x{height}) ---");
    //                for (int y = 0; y < height; y++)
    //                {
    //                    var rowBuilder = new StringBuilder();
    //                    rowBuilder.Append($"Row {y,3}: ");
    //                    for (int x = 0; x < width; x++)
    //                    {
    //                        PixelColor pixel = pixelData[y * width + x];
    //                        rowBuilder.Append($"[R:{pixel.R}, G:{pixel.G}, B:{pixel.B}] ");
    //                    }
    //                    Console.WriteLine(rowBuilder.ToString());
    //                }
    //            }
    //        }
    //    }
    //}
}
