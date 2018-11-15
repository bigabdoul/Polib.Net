using Plossum.CommandLine;
using static System.Console;

namespace Polib.Net.Cmd
{
    class Program
    {
        static int Main(string[] args)
        {
            var options = new Options();
            var parser = new CommandLineParser(options);
            parser.Parse();
            WriteLine(parser.UsageInfo.GetHeaderAsString(78));

            if (options.Help)
            {
                WriteLine(parser.UsageInfo.GetOptionsAsString(78));
                return 0;
            }
            else if (parser.HasErrors)
            {
                WriteLine(parser.UsageInfo.GetErrorsAsString(78));
                return -1;
            }
            WriteLine("Hello {0}!", options.Name);
            return 0;
        }
    }

    [CommandLineManager(ApplicationName = "Hello World", Copyright = "Copyright (c) Peter Palotas")]
    class Options
    {
        [CommandLineOption(Description = "Displays this help text")]
        public bool Help { get; set; }

        [CommandLineOption(Description = "Specifies the input file", MinOccurs = 1)]
        public string Name
        {
            get => mName;
            set => mName = value ?? throw new InvalidOptionValueException("The name must not be empty", false);
        }

        private string mName;
    }
}
