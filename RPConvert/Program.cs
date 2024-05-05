﻿using CommandLine;
using RPalConvert;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Xml;

namespace RPConvert
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Parser.Default.ParseArguments<CommandLineOptions>(args)
                .MapResult(opts => RunOptionsAndReturnExitCode(opts),
                            errs => HandleParseError(errs));
        }

        private static int RunOptionsAndReturnExitCode(CommandLineOptions opts)
        {
            try
            {
                if (opts.PrintRGBto9bitSet)
                {
                    _9bitPalette.PrintRGBto9bitSet();
                    return 0; // Завершить программу после вывода, так как другие опции не нужны
                }

                string? inFormat = Path.GetExtension(opts.InputFilePath)?.ToLower().TrimStart('.');
                Bitmap? InputBmp = null;
                HashSet<Color> palette = null;
                Dictionary<string, ImageFormat> formatMap = new Dictionary<string, ImageFormat>(StringComparer.OrdinalIgnoreCase)
                {
                    { "bmp", ImageFormat.Bmp },
                    { "jpg", ImageFormat.Jpeg },
                    { "jpeg", ImageFormat.Jpeg },
                    { "gif", ImageFormat.Gif },
                    { "png", ImageFormat.Png },
                    { "tiff", ImageFormat.Tiff }
                };


                if (Enum.IsDefined(typeof(ImageFormat), inFormat))
                {
                    InputBmp = new(opts.InputFilePath);
                    palette = Palette.GetPalette(opts.InputFilePath);
                }
                else if (inFormat == AdvanceFormat.BEX.ToString())
                {
                    palette = _9bitPalette.GetPaletteFromBex(opts.InputFilePath);
                }
                else
                {
                    Console.WriteLine( $"Unsupported file informat: {inFormat}");
                    return -1;
                }

                var sizes = opts.Sizes.ToList();
                int squareSize = sizes.ElementAtOrDefault(0) != 0 ? sizes[0] : 8;
                int squaresPerRow = sizes.ElementAtOrDefault(1) != 0 ? sizes[1] : 8;
                int squaresMerge = sizes.ElementAtOrDefault(2) != 0 ? sizes[2] : 0;

                if(Enum.IsDefined(typeof(ImageFormat), opts.OutFormat))
                {
                    if (!formatMap.TryGetValue(opts.OutFormat, out ImageFormat format))
                    {
                        throw new NotSupportedException("Unsupported image format: " + opts.OutFormat);
                    }

                    if (string.IsNullOrEmpty(opts.OutputFilePath))
                    {
                        Palette.ExportImg(palette, format, squareSize, squaresPerRow, squaresMerge);                        
                    }
                    else
                    {
                        Palette.ExportImg(palette, format, squareSize, squaresPerRow, squaresMerge, opts.OutputFilePath);
                    }
                }
                else if (opts.OutFormat.ToLower() == AdvanceFormat.BEX.ToString().ToLower())
                {
                    if (string.IsNullOrEmpty(opts.OutputFilePath))
                    {
                        _9bitPalette.ExportToBexFile(palette, opts.InputFilePath);
                    }
                    else
                    {
                        _9bitPalette.ExportToBexFile(palette, opts.InputFilePath, opts.OutputFilePath);
                    }
                }
                else
                {
                    Console.WriteLine($"Unsupported file outformat: {inFormat}");
                    return -1;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return -3; // Unhandled error
            }
            return 0;
        }

        //in case of errors or --help or --version
        private static int HandleParseError(IEnumerable<Error> errs)
        {
            if (errs.IsVersion() || errs.IsHelp())
            {
                // Если был запрос версии или помощи, просто вернуть 0 и не делать ничего
                return 0;
            }

            //var result = -2;
            //Console.WriteLine("errors {0}", errs.Count());
            //if (errs.Any(x => x is HelpRequestedError || x is VersionRequestedError))
            //    result = -1;
            //Console.WriteLine("Exit code {0}", result);
            //return result;

            foreach (var err in errs)
            {
                switch (err.Tag)
                {
                    case ErrorType.MissingValueOptionError:
                        Console.WriteLine("Пропущено значение для одной из опций.");
                        break;
                    case ErrorType.MissingRequiredOptionError:
                        Console.WriteLine("Пропущена одна из обязательных опций.");
                        break;
                    case ErrorType.UnknownOptionError:
                        Console.WriteLine("Неизвестная опция.");
                        break;
                    // добавьте другие типы ошибок по мере необходимости
                    default:
                        Console.WriteLine($"Неизвестная ошибка: {err}");
                        break;
                }
            }
            return -1;
        }
    }
}
