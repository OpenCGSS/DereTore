using CommandLine;

namespace DereTore.Apps.JacketCreator {
    internal sealed class Options {

        [Option("image", HelpText = "Expected image.", Required = true)]
        public string ImageFileName { get; set; }

        [Option("song", Default = 1001, HelpText = "The song ID. Example: 1001", Required = false)]
        public int SongID { get; set; }

        [Option('o', "output", HelpText = "The output directory of generated jacket file.", Required = true)]
        public string OutputDirectory { get; set; }

        [Option("path_id_m", Default = 0x547e3042158b3095, HelpText = "Path ID for medium-sized texture.", Required = false)]
        public long DdsPathID { get; set; }

        [Option("path_id_s", Default = unchecked((long)0xed362ad73c325b56), HelpText = "Path ID for small-sized texture.", Required = false)]
        public long PvrPathID { get; set; }

    }
}
