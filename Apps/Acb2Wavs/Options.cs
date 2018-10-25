using CommandLine;
using DereTore.Common.StarlightStage;

namespace DereTore.Apps.Acb2Wavs {
    public sealed class Options {

        [Value(0, HelpText = "Input file name", Required = true)]
        public string InputFileName { get; set; } = string.Empty;

        [Option('a', "key1", HelpText = "Key 1 (8 hex digits)", Required = false, Default = "f27e3b22")]
        public string Key1 { get; set; } = CgssCipher.Key1.ToString("x8");

        [Option('b', "key2", HelpText = "Key 2 (8 hex digits)", Required = false, Default = "00003657")]
        public string Key2 { get; set; } = CgssCipher.Key2.ToString("x8");

    }
}
