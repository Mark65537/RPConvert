using RPConvert;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
namespace RPalConvert
{
    internal class _9bitPalette
    {
        public static void GenerateBasicPaletteString(Bitmap bmp, string filePath)
        {
            string hexPaletteString = GenerateHexPaletteString(bmp);

            File.WriteAllText(filePath, hexPaletteString);
        }
        public static string GenerateHexPaletteString(Bitmap bmp)
        {
            HashSet<Color> bmpPalette = Palette.GetPalette(bmp);

            List<int> palette9bit = ConvertColorsTo9bit(bmpPalette);

            List<string> hexPalette = palette9bit.Select(x => string.Format("${0:X3}", x)).ToList();

            string hexPaletteString = string.Join(", ", hexPalette);

            return hexPaletteString;
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
    }
}
