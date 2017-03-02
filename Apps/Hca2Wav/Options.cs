using CommandLine;
using DereTore.Common.StarlightStage;
using DereTore.Exchange.Audio.HCA;

namespace DereTore.Apps.Hca2Wav {
    public sealed class Options {

        [Option('i', "in", Required = true)]
        public string InputFileName { get; set; } = string.Empty;

        [Option('o', "out", Required = false)]
        public string OutputFileName { get; set; } = string.Empty;

        [Option('a', "key1", Required = false)]
        public string Key1 { get; set; } = CgssCipher.Key1.ToString("x8");

        [Option('b', "key2", Required = false)]
        public string Key2 { get; set; } = CgssCipher.Key2.ToString("x8");

        [Option("infinite", Required = false)]
        public bool InfiniteLoop { get; set; } = AudioParams.Default.InfiniteLoop;

        [Option('l', "loop", Required = false)]
        public uint SimulatedLoopCount { get; set; } = AudioParams.Default.SimulatedLoopCount;

        [Option('e', "header", Required = false)]
        public bool OutputWaveHeader { get; set; } = true;

        [Option('c', "cipher", Required = false)]
        public uint OverriddenCipherType { get; set; }

    }
}
