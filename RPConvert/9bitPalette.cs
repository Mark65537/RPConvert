using RPConvert;
using System;
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
        /// <param name="inFilePath">Path of the file to write the palette string.</param>
        public static void ExportToBexFile(Bitmap bmp, string inFilePath, string outFilePath = "")
        {
            string basiegaxorzString = GenerateBasiePaletteString(bmp, inFilePath);
            outFilePath = string.IsNullOrEmpty(outFilePath) ? Path.ChangeExtension(inFilePath, ".bex") : outFilePath ;

            File.WriteAllText(outFilePath, basiegaxorzString);
        }

        /// <summary>
        /// Generate to BEX string palette format.
        /// </summary>
        /// <param name="bmp">Bitmap image to extract palette from.</param>
        /// <param name="filePath">Path of the file to write the palette string.</param>
        private static string GenerateBasiePaletteString(Bitmap bmp, string filePath)
        {
            string paletteString = GenerateHexPaletteString(bmp);
            string fileName = Path.GetFileNameWithoutExtension(filePath);
            return $"{fileName}_pal: dataint     {paletteString}";
        }

        /// <summary>
        /// Generates a hex palette string from a bitmap.
        /// </summary>
        /// <param name="bmp">Bitmap image to extract palette from.</param>
        /// <returns>Hex palette string.</returns>
        public static string GenerateHexPaletteString(Bitmap bmp)
        {
            HashSet<Color> bmpPalette = Palette.GetPalette(bmp);

            if (bmpPalette.Count > 16)
            {
                Console.WriteLine("Color palette has more than 16 colors");
            }

            List<int> palette9bit = ConvertColorsTo9bit(bmpPalette);

            List<string> hexPalette = palette9bit.Select(x => $"${x:X3}").ToList();

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
            return palette.Select(ConvertColorTo9bit).ToList();
        }
    }
}
