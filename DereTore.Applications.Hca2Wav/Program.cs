using System;
using System.Globalization;
using System.IO;
using DereTore.HCA;
using DereTore.StarlightStage;

namespace DereTore.Applications.Hca2Wav {
    internal static class Program {

        private static void Main(string[] args) {
            if (args.Length < 1 || args.Length > 6) {
                Console.WriteLine(HelpMessage);
                return;
            }
            var inputFileName = args[0];
            var varArgStart = 1;
            string outputFileName;
            if (args.Length > 1 && args[1][0] != '-') {
                outputFileName = args[1];
                varArgStart = 2;
            } else {
                var fileInfo = new FileInfo(inputFileName);
                outputFileName = fileInfo.FullName.Substring(0, fileInfo.FullName.Length - fileInfo.Extension.Length);
                outputFileName += ".wav";
            }
            uint key1 = CgssCipher.Key1, key2 = CgssCipher.Key2;
            for (var i = varArgStart; i < args.Length; ++i) {
                var arg = args[i];
                if (arg[0] == '-' || arg[0] == '/') {
                    switch (arg.Substring(1)) {
                        case "a":
                            key1 = uint.Parse(args[++i], NumberStyles.HexNumber);
                            break;
                        case "b":
                            key2 = uint.Parse(args[++i], NumberStyles.HexNumber);
                            break;
                        default:
                            break;
                    }
                }
            }

            using (var inputFileStream = File.Open(inputFileName, FileMode.Open, FileAccess.Read)) {
                using (var outputFileStream = File.Open(outputFileName, FileMode.Create, FileAccess.Write)) {
                    var decoder = new OneWayHcaDecoder(inputFileStream, new DecodeParams {
                        Key1 = key1,
                        Key2 = key2
                    });
                    var waveHeaderBuffer = new byte[decoder.GetMinWaveHeaderBufferSize()];
                    decoder.WriteWaveHeader(waveHeaderBuffer);
                    outputFileStream.Write(waveHeaderBuffer, 0, waveHeaderBuffer.Length);
                    var hasMore = true;
                    var read = 1;
                    var dataBuffer = new byte[decoder.GetMinWaveDataBufferSize() * 10];
                    while (hasMore && read > 0) {
                        read = decoder.DecodeData(dataBuffer, out hasMore);
                        outputFileStream.Write(dataBuffer, 0, read);
                    }
                }
            }
        }

        private static readonly string HelpMessage = "Usage: hca2wav.exe <input HCA> [<output WAVE = <input HCA>.wav>] [-a <key 1>] [-b <key 2>]";

    }
}
