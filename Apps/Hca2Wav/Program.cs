using System;
using System.Globalization;
using System.IO;
using CommandLine;
using DereTore.Common.StarlightStage;
using DereTore.Exchange.Audio.HCA;

namespace DereTore.Apps.Hca2Wav {
    internal static class Program {

        private static int Main(string[] args) {
            const int defaultExitCodeFail = -1;

            var parser = new Parser(settings => { settings.IgnoreUnknownArguments = true; });
            var parsedResult = parser.ParseArguments<Options>(args);

            var succeeded = parsedResult.Tag == ParserResultType.Parsed;

            Options options = null;

            if (succeeded) {
                options = ((Parsed<Options>)parsedResult).Value;
            }

            if (succeeded) {
                if (string.IsNullOrWhiteSpace(options.InputFileName)) {
                    succeeded = false;
                }
            }

            if (!succeeded) {
                var helpText = CommandLine.Text.HelpText.AutoBuild(parsedResult);
                helpText.AddPreOptionsLine(" ");
                helpText.AddPreOptionsLine("Usage: hca2wav <input HCA> [options]");
                Console.Error.WriteLine(helpText);
                return defaultExitCodeFail;
            }

            if (!File.Exists(options.InputFileName)) {
                Console.Error.WriteLine("File not found: {0}", options.InputFileName);
                return defaultExitCodeFail;
            }

            if (string.IsNullOrWhiteSpace(options.OutputFileName)) {
                var fileInfo = new FileInfo(options.InputFileName);
                options.OutputFileName = fileInfo.FullName.Substring(0, fileInfo.FullName.Length - fileInfo.Extension.Length);
                options.OutputFileName += ".wav";
            }

            uint key1, key2;
            var formatProvider = new NumberFormatInfo();
            if (!string.IsNullOrWhiteSpace(options.Key1)) {
                if (!uint.TryParse(options.Key1, NumberStyles.HexNumber, formatProvider, out key1)) {
                    Console.WriteLine("ERROR: key 1 is of wrong format. It should look like \"a1b2c3d4\".");
                    return defaultExitCodeFail;
                }
            } else {
                key1 = CgssCipher.Key1;
            }
            if (!string.IsNullOrWhiteSpace(options.Key2)) {
                if (!uint.TryParse(options.Key2, NumberStyles.HexNumber, formatProvider, out key2)) {
                    Console.WriteLine("ERROR: key 2 is of wrong format. It should look like \"a1b2c3d4\".");
                    return defaultExitCodeFail;
                }
            } else {
                key2 = CgssCipher.Key2;
            }

            using (var inputFileStream = File.Open(options.InputFileName, FileMode.Open, FileAccess.Read)) {
                using (var outputFileStream = File.Open(options.OutputFileName, FileMode.Create, FileAccess.Write)) {
                    var decodeParams = DecodeParams.CreateDefault();
                    decodeParams.Key1 = key1;
                    decodeParams.Key2 = key2;
                    if (options.OverridesCipherType) {
                        decodeParams.CipherTypeOverrideEnabled = true;
                        decodeParams.OverriddenCipherType = (CipherType)options.OverriddenCipherType;
                    }
                    var audioParams = AudioParams.CreateDefault();
                    audioParams.InfiniteLoop = options.InfiniteLoop;
                    audioParams.SimulatedLoopCount = options.SimulatedLoopCount;
                    audioParams.OutputWaveHeader = !options.NoWaveHeader;
                    using (var hcaStream = new HcaAudioStream(inputFileStream, decodeParams, audioParams)) {
                        var read = 1;
                        var dataBuffer = new byte[1024];
                        while (read > 0) {
                            read = hcaStream.Read(dataBuffer, 0, dataBuffer.Length);
                            if (read > 0) {
                                outputFileStream.Write(dataBuffer, 0, read);
                            }
                        }
                    }
                }
            }

            return 0;
        }

    }
}
