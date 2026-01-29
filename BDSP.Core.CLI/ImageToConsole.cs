using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Text;
using BDSP.Core.Berries;

namespace BDSP.Core.CLI
{
    internal static class ImageToConsole
    {

        public static Bitmap ResizeImage(Image image, int width, int height)
        {
            // Create a new Bitmap object with the desired dimensions
            Bitmap resizedImage = new Bitmap(width, height);

            // Use a Graphics object to draw the original image onto the new Bitmap,
            // which performs the resizing
            using (Graphics g = Graphics.FromImage(resizedImage))
            {
                // For higher quality results, set the interpolation mode
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                g.DrawImage(image, 0, 0, width, height);
            }

            return resizedImage;
        }
        public static byte[] GetPixelsFaster(Bitmap bitmap)
        {
            Rectangle rect = new Rectangle(0, 0, bitmap.Width, bitmap.Height);
            BitmapData bmpData = bitmap.LockBits(rect, ImageLockMode.ReadOnly, bitmap.PixelFormat);

            // Get the address of the first line
            IntPtr ptr = bmpData.Scan0;

            // Declare an array to hold the bytes of the bitmap
            int bytes = Math.Abs(bmpData.Stride) * bitmap.Height;
            byte[] rgbValues = new byte[bytes];

            // Copy the RGB values into the array
            Marshal.Copy(ptr, rgbValues, 0, bytes);


            bitmap.UnlockBits(bmpData);

            return rgbValues;
        }
        public static string GetImageString(int ID)
        {
            // string path = Path.Combine(AppContext.BaseDirectory, "Assets", $"{BerryNames.GetName(ID)}_berry.png");
            string path = Path.Combine(AppContext.BaseDirectory, "Assets\\Berries", $"{ID}.png");
            StringBuilder sb = new();
            var resolution = 50;
            
            using (Bitmap bmp = new Bitmap(path))
            {
                bmp.SetResolution(resolution, resolution);
                using (Bitmap resizedImage = ResizeImage(bmp, resolution, resolution)) {
                    for (int y = 0; y < bmp.Height; y++)
                    {
                        for (int x = 0; x < bmp.Width; x++)
                        {
                            Color pixelColor = bmp.GetPixel(x, y);
                            byte r = pixelColor.R;
                            byte g = pixelColor.G;
                            byte b = pixelColor.B;
                            //byte a = pixelColor.A;
                            string block = "█"; // You can choose different characters for different effects
                            sb.Append($"{Colors.Rgb(r, g, b)}{block}"); // Placeholder for pixel
                        }
                    }
                }
            }
            sb.Append(Colors.Reset);
            return sb.ToString();
        }

        public static string GetImageString(BerryId ID)
        {

            return GetImageString(ID.Value);
        }

        public static string GetImageString(Berry berry)
        {

            return GetImageString(berry.Id);
        }

        public struct PixelColor
        {
            public byte R;
            public byte G;
            public byte B;
            public byte A;
        }
    }
}
