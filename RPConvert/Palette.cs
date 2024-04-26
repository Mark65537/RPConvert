using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Reflection.Emit;
using System.Collections;
//using ImageMagick;

namespace RIConvert
{
    public class Palette
    {
        public static void ConvImgToSegaStr (Bitmap bmp, string savePath)
        {
            HashSet<Color> bmpPalette = GetPalette(bmp);

            List<int> palette9bit = ConvertColorsTo9bit(bmpPalette);

            List<string> hexPalette = palette9bit.Select(x => string.Format("${0:X3}", x)).ToList();

            string hexPaletteString = string.Join(", ", hexPalette);
        }

        private static int ConvertColorTo9bit(Color color)
        {
            return ((color.B >> 5) << 9) |
                        ((color.G >> 5) << 5) |
                            ((color.R >> 5) << 1);
        }
        public static List<int> ConvertColorsTo9bit(HashSet<Color> palette)
        {
            List<int> palette9bit = new();
            foreach (var color in palette)
            {
                palette9bit.Add(ConvertColorTo9bit(color));
            }
            return palette9bit;
        }
        public static Bitmap ConvertToXColors(Bitmap originalImage, int colorCount, HashSet<Color> customPal = null)
        {

            if (colorCount < 2)
            {
                throw new ArgumentException("The colorCount parameter must be greater than or equal to 2.", nameof(colorCount));
            }

            HashSet<Color> palette;

            if (customPal == null)
            {
                palette = GetPalette(originalImage);
            }
            else
            {
                palette = customPal;
            }

            if (palette.Count <= colorCount)
            {
                return originalImage;
            }

            List<Color> sortedPal = palette.ToList(); // Преобразуем HashSet в List
            //sortedPal.Sort(); // Сортируем List

            HashSet<Color> newPal = new();
            newPal.UnionWith(sortedPal.Where(c => c == Color.White || c == Color.Black));

            newPal.Add(FindMostFrequentColor(sortedPal));

            int min = 0;
            int max = palette.Count;
            int mid = (max - min);

            while (newPal.Count != colorCount)
            {
                mid /= 2;
                for (int k = mid; k < max; k += mid)
                {
                    if (newPal.Count < colorCount)
                    {
                        newPal.Add(sortedPal[k]);
                    }
                    else
                    {
                        break;
                    }
                }
            }          

            return ApplyNewPaletteToImgFast(originalImage, newPal);
        }

        public static Bitmap ApplyNewPaletteToImg(Bitmap bmp, HashSet<Color> newPal)
        {
            for (int y = 0; y < bmp.Height; y++)
            {
                for (int x = 0; x < bmp.Width; x++)
                {
                    Color pixelColor = bmp.GetPixel(x, y);

                    Color newColor = FindMostSimilarColor(newPal, pixelColor);

                    bmp.SetPixel(x, y, newColor);
                }
            }
            return bmp;
        }

        private static Bitmap ApplyNewPaletteToImgFast(Bitmap bmp, HashSet<Color> newPal)
        {
            BitmapData data = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadWrite, bmp.PixelFormat);
            unsafe
            {
                byte* ptr = (byte*)data.Scan0;
                for (int y = 0; y < data.Height; y++)
                {
                    for (int x = 0; x < data.Width; x++)
                    {
                        Color pixelColor = Color.FromArgb(ptr[2], ptr[1], ptr[0]);
                        Color newColor = FindMostSimilarColor(newPal, pixelColor);
                        ptr[0] = newColor.B;
                        ptr[1] = newColor.G;
                        ptr[2] = newColor.R;

                        ptr += 4;
                    }
                    ptr += data.Stride - data.Width * 4;
                }
            }
            bmp.UnlockBits(data);
            return bmp;
        }

        private static Color GetMostFrequentColor(Bitmap bmp)
        {
            List<Color> colors = new List<Color>();

            for (int x = 0; x < bmp.Width; x++)
            {
                for (int y = 0; y < bmp.Height; y++)
                {
                    Color color = bmp.GetPixel(x, y);
                    colors.Add(color);
                }
            }

            Color mostFrequentColor = FindMostFrequentColor(colors);
            return mostFrequentColor;
        }

        private static Color FindMostFrequentColor(List<Color> colors)
        {
            Dictionary<Color, int> colorCounts = new Dictionary<Color, int>();

            foreach (Color color in colors)
            {
                if (colorCounts.ContainsKey(color))
                {
                    colorCounts[color]++;
                }
                else
                {
                    colorCounts.Add(color, 1);
                }
            }

            int maxCount = colorCounts.Values.Max();
            Color mostFrequentColor = colorCounts.FirstOrDefault(x => x.Value == maxCount).Key;

            return mostFrequentColor;
        }



        private static Color GetMostFrequentColor(string filePath)
        {
            return GetMostFrequentColor(new Bitmap(filePath));
        }

        public static Bitmap ConvTo16Color(Bitmap originalImage)
        {
            return ConvertToXColors(originalImage, 16);
        }
        public static Bitmap ConvTo4Color(Bitmap originalImage)
        {
            return ConvertToXColors(originalImage, 4);
        }

        private static Color FindMostSimilarColor(HashSet<Color> colors, Color pixelColor)
        {
            if (colors.Contains(pixelColor))
            {
                return pixelColor;
            }

            double minDistance = double.MaxValue;
            Color mostSimilarColor = Color.Empty;

            foreach (Color color in colors)
            {
                double distance = CalculateColorDistance(color, pixelColor);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    mostSimilarColor = color;
                }
            }

            return mostSimilarColor;
        }

        private static double CalculateColorDistance(Color color1, Color color2)
        {
            double redDifference = color1.R - color2.R;
            double greenDifference = color1.G - color2.G;
            double blueDifference = color1.B - color2.B;

            double distance = Math.Sqrt(redDifference * redDifference + greenDifference * greenDifference + blueDifference * blueDifference);

            return distance;
        }

        //<summary>
        //переводит в граничные цвета
        public static Bitmap ConvertTo16Color(Bitmap originalImage)
        {
            // Create a new 16-color bitmap
            Bitmap convertedImage = originalImage.Clone(
                new Rectangle(0, 0, originalImage.Width, originalImage.Height),
                PixelFormat.Format4bppIndexed);

            // Set the palette to a 16-color palette
            ColorPalette palette = convertedImage.Palette;
            for (int i = 0; i < 16; i++)
            {

                //int r = (i & 0x4) != 0 ? 0xFF : 0x00;
                //int g = (i & 0x2) != 0 ? 0xFF : 0x00;
                //int b = (i & 0x1) != 0 ? 0xFF : 0x00;
                //palette.Entries[i] = Color.FromArgb(r, g, b);

                //palette.Entries[i] = Color.FromArgb(i * 16, i * 16, i * 16);//BAD algoritm. Used for GrayScale

            }
            convertedImage.Palette = palette;

            // Create a blank bitmap with the same dimensions
            Bitmap tempBitmap = new Bitmap(convertedImage.Width, convertedImage.Height);
            tempBitmap.Palette = palette;
            // Draw the original image onto the converted bitmap using a Graphics object
            using (Graphics g = Graphics.FromImage(tempBitmap))
            {
                g.DrawImage(convertedImage, 0, 0, originalImage.Width, originalImage.Height);
            }

            return tempBitmap;
        }

        public static Bitmap ConvertTo4Color(Bitmap bmp)
        {
            Bitmap result = new Bitmap(bmp.Width, bmp.Height);

            for (int x = 0; x < bmp.Width; x++)
            {
                for (int y = 0; y < bmp.Height; y++)
                {
                    Color color = bmp.GetPixel(x, y);
                    Color convertedColor = ConvertTo4Color(color);
                    result.SetPixel(x, y, convertedColor);
                }
            }

            return result;
        }

        private static Color ConvertTo4Color(Color color)
        {
            int red = color.R;
            int green = color.G;
            int blue = color.B;

            if (red < 128)
                red = 0;
            else
                red = 255;

            if (green < 128)
                green = 0;
            else
                green = 255;

            if (blue < 128)
                blue = 0;
            else
                blue = 255;

            Color convertedColor = Color.FromArgb(red, green, blue);
            return convertedColor;
        }

        public static Bitmap ConvertTo4ColorWithGrayscale(Bitmap bmp)
        {
            Bitmap result = new Bitmap(bmp.Width, bmp.Height);

            for (int x = 0; x < bmp.Width; x++)
            {
                for (int y = 0; y < bmp.Height; y++)
                {
                    Color color = bmp.GetPixel(x, y);
                    Color convertedColor = ConvertTo4ColorWithGrayscale(color);
                    result.SetPixel(x, y, convertedColor);
                }
            }

            return result;
        }

        //public static Bitmap ConvertToXColorsIM(Bitmap bmp, int targetColorsCount, int maxColorCount)
        //{
        //    Bitmap tempBmp = null;
        //    HashSet<Color> palette = new();
        //    for (int i = targetColorsCount; i <= maxColorCount; i++)
        //    {
        //        tempBmp = ConvertToXColorsIM(bmp, i);
        //        //palette = GetPalette(tempBmp, colorsCount);
        //        palette = GetPalette(tempBmp);
        //        if (palette.Count > targetColorsCount)
        //        {
        //            palette = GetPaletteStright(tempBmp);
        //            if (palette.Count > targetColorsCount)
        //            {
        //                break;
        //            }
        //        }
        //        if (palette.Count == targetColorsCount)
        //        {
        //            break;
        //        }
        //    }
        //    return tempBmp ?? bmp;
        //}

        private static Color ConvertTo4ColorWithGrayscale(Color color)
        {
            int gray = (color.R + color.G + color.B) / 3;
            int threshold = 128;

            if (gray < threshold)
            {
                return Color.FromArgb(0, 0, 0); // Black
            }
            else if (gray < (threshold * 2))
            {
                return Color.FromArgb(85, 85, 85); // Dark Gray
            }
            else if (gray < (threshold * 3))
            {
                return Color.FromArgb(170, 170, 170); // Light Gray
            }
            else
            {
                return Color.FromArgb(255, 255, 255); // White
            }
        }


        public static Bitmap ConvertTo16(Bitmap image)
        {
            Bitmap newBitmap = new Bitmap(image.Width, image.Height, PixelFormat.Format16bppRgb555);
            using (Graphics g = Graphics.FromImage(newBitmap))
            {
                g.DrawImage(image, new Rectangle(0, 0, newBitmap.Width, newBitmap.Height));
            }
            return newBitmap;
        }
        public static HashSet<Color> GetPalette(Bitmap bmp, int convColorsCount = -1)
        {
            HashSet<Color> colors = GetPaletteFast(bmp);
            // Получить палитру цветов из изображения
            if (colors.Count == 0 && (convColorsCount == -1 || colors.Count != convColorsCount))
            {
                colors = GetPaletteStright(bmp);
            }
            return colors;
        }

        public static HashSet<Color> GetPaletteStright(Bitmap image)
        {
            // Create a HashSet to store the unique colors
            HashSet<Color> uniqueColors = new HashSet<Color>();

            // Iterate over each pixel in the image
            for (int y = 0; y < image.Height; y++)
            {
                for (int x = 0; x < image.Width; x++)
                {
                    // Get the color of the current pixel
                    Color pixelColor = image.GetPixel(x, y);

                    // Add the color to the HashSet
                    uniqueColors.Add(pixelColor);
                }
            }
            return uniqueColors;
        }

        public static HashSet<Color> GetPalette(string filePath)
        {
            return GetPalette(new Bitmap(filePath));
        }
        public static HashSet<Color> GetPaletteFast(Bitmap image)
        {
            HashSet<Color> uniqueColors = new HashSet<Color>();
            BitmapData bmpData = image.LockBits(new Rectangle(0, 0, image.Width, image.Height), ImageLockMode.ReadOnly, image.PixelFormat);
            unsafe
            {
                byte* ptr = (byte*)bmpData.Scan0;
                int bytesPerPixel = Image.GetPixelFormatSize(image.PixelFormat) / 8;
                int heightInPixels = bmpData.Height;
                int widthInBytes = bmpData.Width * bytesPerPixel;
                for (int y = 0; y < heightInPixels; y++)
                {
                    byte* currentLine = ptr + (y * bmpData.Stride);
                    for (int x = 0; x < widthInBytes; x = x + bytesPerPixel)
                    {
                        int b = currentLine[x];
                        int g = currentLine[x + 1];
                        int r = currentLine[x + 2];
                        int a = bytesPerPixel > 3 ? currentLine[x + 3] : 255; // If 32bpp
                        uniqueColors.Add(Color.FromArgb(a, r, g, b));
                    }
                }
            }
            image.UnlockBits(bmpData);
            return uniqueColors;
        }

        public static void Check16Colors(Bitmap bitmap)
        {
            if (!Has16Colors(bitmap))
            {
                throw new Exception("в изображении больше чем 16 цветов");
            }
        }
        public static bool Has16Colors(Bitmap image)
        {
            // Create a HashSet to store the unique colors
            HashSet<Color> uniqueColors = GetPalette(image);

            if (uniqueColors.Count > 16)
            {
                return false;
            }

            // Return true if the HashSet contains exactly 16 colors
            return uniqueColors.Count <= 16;
        }
        public static Bitmap MakeTransperentColor(Image img, Color color)
        {
            return MakeTransperentColor(new Bitmap(img), color);
        }
        public static Bitmap MakeTransperentColor(Bitmap bmp, Color color)
        {
            bmp.MakeTransparent(color);
            return bmp;
        }
        public static Bitmap ChangeColor(Image bmp, Color prevColor, Color newColor)
        {
            return ChangeColor(new Bitmap(bmp), prevColor, newColor);
        }
        public static Bitmap ChangeColor(Bitmap bmp, Color prevColor, Color newColor)
        {
            for (int y = 0; y < bmp.Height; y++)            
                for (int x = 0; x < bmp.Width; x++)
                {
                    // Get the color of the current pixel
                    Color pixelColor = bmp.GetPixel(x, y);
                    if (pixelColor.Equals(prevColor))
                    {
                        bmp.SetPixel(x, y, newColor);
                    }
                }
            
            return bmp;
        }

        //public Image ConvertTo16BitGrayscale(string inputImagePath)
        //{
        //    using (MagickImage image = new MagickImage(inputImagePath))
        //    {
        //        // Set the color type to grayscale
        //        image.ColorType = ColorType.Grayscale;

        //        // Set the bit depth to 16
        //        image.Depth = 16;

        //        // Сохраняем преобразованное изображение
        //        using (var memStream = new MemoryStream())
        //        {
        //            // Write the image to the memorystream
        //            image.Write(memStream);

        //            return new Bitmap(memStream);
        //        }
        //    }
        //}

        public static Bitmap SetCustomPal(Bitmap bmp, HashSet<Color> oldPal, HashSet<Color> newPal)
        {
            //Bitmap newBmp = new Bitmap(bmp.Width, bmp.Height, bmp.PixelFormat);
            Dictionary<Color, Color> newColors = new();

            if (oldPal.Count != newPal.Count)
            {
                throw new ArgumentException("Both hashsets should have the same count.");
            }

            using (var enumerator1 = oldPal.GetEnumerator())
            using (var enumerator2 = newPal.GetEnumerator())
            {
                while (enumerator1.MoveNext() && enumerator2.MoveNext())
                {
                    newColors.Add(enumerator1.Current, enumerator2.Current);
                }
            }

            for (int y = 0; y < bmp.Height; y++)
            {
                for (int x = 0; x < bmp.Width; x++)
                {
                    Color color = bmp.GetPixel(x, y);
                    bmp.SetPixel(x, y, newColors[color]);
                }
            }

            return bmp;
        }

        public static void ExportImg(Bitmap inputBmp, SupportedFormat format, int squareSize = 8, int squaresPerRow = 8, int squareMerge = 0, string filename = "output")
        {
            HashSet<Color> palette = GetPalette(inputBmp);
            int squaresCount = palette.Count;
            int rowsCount = (int)Math.Ceiling((double)squaresCount / squaresPerRow);
            int borderWidth = 1;
            int bitmapWidth = squaresPerRow * squareSize + (squaresPerRow * squareMerge) - squareMerge + borderWidth;
            int bitmapHeight = rowsCount * squareSize + (rowsCount * squareMerge) - squareMerge + borderWidth;

            Bitmap outputBmp = new Bitmap(bitmapWidth, bitmapHeight);
            Graphics g = Graphics.FromImage(outputBmp);
            g.Clear(Color.White);
            int x = 0;
            int y = 0;
            Pen borderPen = new Pen(Color.Black, borderWidth); // Pen for square borders
            foreach (Color color in palette)
            {
                g.FillRectangle(new SolidBrush(color), x, y, squareSize, squareSize);
                g.DrawRectangle(borderPen, x, y, squareSize, squareSize); // Draw square border
                x += squareSize + squareMerge;
                if (x >= bitmapWidth)
                {
                    x = 0;
                    y += squareSize + squareMerge;
                }
            }
            g.Dispose();

            switch (format)
            {
                case SupportedFormat.jpeg:
                    outputBmp.Save($"{filename}.jpeg", ImageFormat.Jpeg);
                    break;
                case SupportedFormat.jpg:
                    outputBmp.Save($"{filename}.jpg", ImageFormat.Jpeg);
                    break;
                case SupportedFormat.png:
                    outputBmp.Save($"{filename}.png", ImageFormat.Png);
                    break;
                case SupportedFormat.bmp:
                    outputBmp.Save($"{filename}.bmp", ImageFormat.Bmp);
                    break;
                default:
                    throw new Exception("Unsupported image format");
            }
        }

    }
}
