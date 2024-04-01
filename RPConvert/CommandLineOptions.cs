using CommandLine.Text;
using CommandLine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RIConvert
{
    internal class CommandLineOptions
    {
        [Value(0, MetaName = "input", HelpText = "Путь к файлу изображения.")]
        public string? InputFilePath { get; set; }

        [Option('q', "quiet", Required = false, HelpText = "Quiet mode.")]
        public bool Quiet { get; set; }

        [Option('o', "out", Required = false, HelpText = "Тип выходного файла")]
        public SupportedFormat Out { get; set; }

        [Value(1, MetaName = "output",  HelpText = "Путь к файлу, в который будет сохранен результат.")]
        public string? OutputFilePath { get; set; }
    }
}
