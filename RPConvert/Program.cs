using CommandLine;
using RIConvert;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

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
                Bitmap InputBmp = new(opts.InputFilePath);

                var sizes = opts.Sizes.ToList();
                int squareSize = sizes.ElementAtOrDefault(0) != 0 ? sizes[0] : 8;
                int squaresPerRow = sizes.ElementAtOrDefault(1) != 0 ? sizes[1] : 8;
                int squaresMerge = sizes.ElementAtOrDefault(2) != 0 ? sizes[2] : 0;

                if(opts.OutFormat == SupportedFormat.png)
                {
                    if (string.IsNullOrEmpty(opts.OutputFilePath))
                    {
                        Palette.ExportImg(InputBmp, opts.OutFormat, squareSize, squaresPerRow, squaresMerge);                        
                    }
                    else
                    {
                        Palette.ExportImg(InputBmp, opts.OutFormat, squareSize, squaresPerRow, squaresMerge, opts.OutputFilePath);
                    }
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
