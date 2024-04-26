using RPConvert;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;


namespace RPalConvert
{
    /// <summary>
    /// Class to convert Bitmap RGB palette to 9 bit format.
    /// </summary>
    internal class _9bitPalette
    {
        /// <summary>
        /// Generates a string for BEX palette format and writes it to a file.
        /// </summary>
        /// <param name="bmp">Bitmap image to extract palette from.</param>
        /// <param name="filePath">Path of the file to write the palette string.</param>
        public static void GenerateBasiePaletteString(Bitmap bmp, string filePath)
        {
            string fileName = Path.GetFileNameWithoutExtension(filePath);

            string basiegaxorzString = $"{fileName}: dataint     {GenerateHexPaletteString(bmp)}";        

            File.WriteAllText(Path.ChangeExtension(filePath, ".bex"), basiegaxorzString);            
        }

        /// <summary>
        /// Generates a hex palette string from a bitmap.
        /// </summary>
        /// <param name="bmp">Bitmap image to extract palette from.</param>
        /// <returns>Hex palette string.</returns>
        public static string GenerateHexPaletteString(Bitmap bmp)
        {
            HashSet<Color> bmpPalette = Palette.GetPalette(bmp);

            List<int> palette9bit = ConvertColorsTo9bit(bmpPalette);

            List<string> hexPalette = palette9bit.Select(x => string.Format("${0:X3}", x)).ToList();

            string hexPaletteString = string.Join(", ", hexPalette);

            return hexPaletteString;
        }

        /// <summary>
        /// Converts a color to 9 bit format.
        /// </summary>
        /// <param name="color">Color to convert.</param>
        /// <returns>9 bit color.</returns>
        private static int ConvertColorTo9bit(Color color)
        {
            return ((color.B >> 5) << 9) |
                        ((color.G >> 5) << 5) |
                            ((color.R >> 5) << 1);
        }

        /// <summary>
        /// Converts a palette of colors to 9 bit format.
        /// </summary>
        /// <param name="palette">Palette of colors to convert.</param>
        /// <returns>List of 9 bit colors.</returns>
        public static List<int> ConvertColorsTo9bit(HashSet<Color> palette)
        {
            List<int> palette9bit = new();
            foreach (var color in palette)
            {
                palette9bit.Add(ConvertColorTo9bit(color));
            }
            return palette9bit;
        }
    }
}
