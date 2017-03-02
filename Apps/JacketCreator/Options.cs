using CommandLine;

namespace DereTore.Apps.JacketCreator {
    internal sealed class Options {

        [Option("image", HelpText = "Expected image.", Required = true)]
        public string ImageFileName { get; set; }

        [Option("song", DefaultValue = 1001, HelpText = "The song ID. Example: 1001", Required = false)]
        public int SongID { get; set; }

        [Option('o', "output", HelpText = "The generated jacket file.", Required = true)]
        public string OutputDirectory { get; set; }

        [Option("path_id_m", DefaultValue = 0x547e3042158b3095, HelpText = "Path ID for medium-sized texture.", Required = true)]
        public long DdsPathID { get; set; }

        [Option("path_id_s", DefaultValue = unchecked((long)0xed362ad73c325b56), HelpText = "Path ID for small-sized texture.", Required = true)]
        public long PvrPathID { get; set; }

    }
}
