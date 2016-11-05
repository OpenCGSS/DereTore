using CommandLine;

namespace DereTore.Applications.JacketCreator {
    internal sealed class Options {

        [Option("image", HelpText = "Expected image.", Required = true)]
        public string ImageFileName { get; set; }

        [Option("song", DefaultValue = 1001, HelpText = "The song ID. Example: 1001", Required = false)]
        public int SongID { get; set; }

        [Option('o', "output", HelpText = "The generated jacket file.", Required = true)]
        public string OutputDirectory { get; set; }

    }
}
