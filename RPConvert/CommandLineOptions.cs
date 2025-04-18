﻿using CommandLine;
using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Linq;

namespace RPConvert
{
    internal class CommandLineOptions
    {
        [Value(0, MetaName = "input", HelpText = "Путь к файлу изображения.")]
        public string? InputFilePath { get; set; }

        [Option('q', "quiet", Required = false, HelpText = "Quiet mode.")]
        public bool Quiet { get; set; }

        private string _outFormat;
        [Option('o', "out", Required = false, HelpText = "Тип выходного файла")]
        public string OutFormat
        {
            get { return _outFormat; }
            set
            {
                if (AllFormats.Contains(value.ToLower()))
                {
                    _outFormat = value.ToLower();
                }
                else
                {
                    throw new ArgumentException("Invalid format. Please choose a valid format.");
                }
            }
        }

        private static List<string> AllFormats = [.. Enum.GetNames(typeof(ImageFormat))
                                                        .Concat(Enum.GetNames(typeof(AdvanceFormat)))
                                                        .Select(name => name.ToLower())];

        [Option('s', "sizes", Required = false, HelpText = "Sizes for the image squares.", Separator = ',')]
        public IEnumerable<int>? Sizes { get; set; }

        [Option('p', "palette", Required = false, HelpText = "preset palette", Separator = ',')]
        public string? Palettes { get; set; }

        [Value(1, Required = false, MetaName = "output", HelpText = "Путь к файлу, в который будет сохранен результат.")]
        public string? OutputFilePath { get; set; }

        [Option("PrintRGBto9bitSet", Required = false, HelpText = "", Hidden = true)]
        public bool PrintRGBto9bitSet { get; set; }

    }
}
