using CommandLine.Text;
using CommandLine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RPConvert
{
    internal class CommandLineOptions
    {
        [Value(0, MetaName = "input", HelpText = "Путь к файлу изображения.")]
        public string? InputFilePath { get; set; }

        [Option('q', "quiet", Required = false, HelpText = "Quiet mode.")]
        public bool Quiet { get; set; }

        private string _out;
        [Option('o', "out", Required = false, HelpText = "Тип выходного файла")]
        public string Out
        {
            get { return _out; }
            set { _out = value.ToLower(); }
        }

        public BasicFormat OutFormat
        {
            get { return Enum.Parse<BasicFormat>(_out, true); }
        }

        [Option('s', "sizes", Required = false, HelpText = "Sizes for the image squares.", Separator = ',')]
        public IEnumerable<int>? Sizes { get; set; }

        [Value(1, Required = false, MetaName = "output",  HelpText = "Путь к файлу, в который будет сохранен результат.")]
        public string? OutputFilePath { get; set; }
    }
}
