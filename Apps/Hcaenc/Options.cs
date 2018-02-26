using CommandLine;

namespace DereTore.Apps.Hcaenc {
    public sealed class Options {

        [Option('q', "quality", HelpText = "Audio quality (1 to 5)", Default = 1, Required = false)]
        public int Quaility { get; set; } = 1;

        [Value(0, HelpText = "Input file name", Required = true)]
        public string InputFileName { get; set; }

        [Value(1, HelpText = "Output HCA file name", Default = null, Required = false)]
        public string OutputFileName { get; set; }

    }
}
