using CommandLine;
using DereTore.Common.StarlightStage;
using DereTore.Exchange.Audio.HCA;

namespace DereTore.Apps.Hca2Wav {
    public sealed class Options {

        [Value(0, HelpText = "Input file name", Required = true)]
        public string InputFileName { get; set; } = string.Empty;

        [Option('o', "out", HelpText = "Output file name", Required = false)]
        public string OutputFileName { get; set; } = string.Empty;

        [Option('a', "key1", HelpText = "Key 1 (8 hex digits)", Required = false, Default = "f27e3b22")]
        public string Key1 { get; set; } = CgssCipher.Key1.ToString("x8");

        [Option('b', "key2", HelpText = "Key 2 (8 hex digits)", Required = false, Default = "00003657")]
        public string Key2 { get; set; } = CgssCipher.Key2.ToString("x8");

        [Option("key-mod", HelpText = "Key modifier (4 hex digits)", Required = false, Default = "0000")]
        public string KeyModifier { get; set; } = "0000";

        [Option("infinite", HelpText = "Enables infinite loop", Required = false, Default = false)]
        public bool InfiniteLoop { get; set; } = AudioParams.Default.InfiniteLoop;

        [Option('l', "loop", HelpText = "Number of simulated loops", Required = false, Default = 0u)]
        public uint SimulatedLoopCount { get; set; } = AudioParams.Default.SimulatedLoopCount;

        [Option('e', "no-header", HelpText = "Do not emit wave header", Required = false, Default = false)]
        public bool NoWaveHeader { get; set; }

        [Option("overrides-cipher", HelpText = "Overrides original cipher type", Required = false, Default = false)]
        public bool OverridesCipherType { get; set; }

        [Option('c', "cipher", HelpText = "Overridden cipher type", Required = false, Default = 0u)]
        public uint OverriddenCipherType { get; set; }

    }
}
