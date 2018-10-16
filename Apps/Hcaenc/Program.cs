using System;
using System.IO;
using CommandLine;
using VGAudio.Codecs.CriHca;
using VGAudio.Containers.Hca;
using VGAudio.Containers.Wave;
using VGAudio.Formats;

namespace DereTore.Apps.Hcaenc {
    internal static class Program {

        private static int Main(string[] args) {
            var parser = new Parser(settings => {
                settings.IgnoreUnknownArguments = true;
            });

            var parserResult = parser.ParseArguments<Options>(args);

            if (parserResult.Tag == ParserResultType.NotParsed) {
                Console.WriteLine(HelpMessage);
                return -1;
            }

            var options = ((Parsed<Options>)parserResult).Value;

            var inputFile = Path.GetFullPath(options.InputFileName);

            string outputFile;

            if (!string.IsNullOrEmpty(options.OutputFileName)) {
                outputFile = options.OutputFileName;
            } else {
                var inputFileInfo = new FileInfo(inputFile);
                var inputFileDir = inputFileInfo.DirectoryName;

                if (inputFileDir == null) {
                    inputFileDir = string.Empty;
                }

                outputFile = Path.Combine(inputFileDir, inputFileInfo.Name.Substring(0, inputFileInfo.Name.Length - inputFileInfo.Extension.Length) + ".hca");
            }

            // Quality = 3 (~128 Kbps) for MLTD, 1 (~256 Kbps) for CGSS
            // Update 2018-10: CGSS uses q=2 and MLTD uses q=4, under the new encoder?
            var quality = options.Quaility;

            if (quality < 1 || quality > 5) {
                Console.WriteLine("Warning: Quality should be 1 to 5. Using q=2.");
                quality = 2;
            }

            Console.WriteLine("Encoding {0} to {1} (q={2}) ...", inputFile, outputFile, quality);

            var waveReader = new WaveReader();
            AudioData audioData;

            using (var fileStream = File.Open(inputFile, FileMode.Open, FileAccess.Read, FileShare.Read)) {
                audioData = waveReader.Read(fileStream);
            }

            var hcaWriter = new HcaWriter();

            hcaWriter.Configuration.Quality = (CriHcaQuality)quality;
            hcaWriter.Configuration.LimitBitrate = false;
            // Encryption keys are not set here, so the output HCA will always be type 0.

            var fileData = hcaWriter.GetFile(audioData);

            using (var fileStream = File.Open(outputFile, FileMode.Create, FileAccess.Write, FileShare.Write)) {
                fileStream.Write(fileData, 0, fileData.Length);
            }

            return 0;
        }

        private static readonly string HelpMessage = "Usage: hcaenc.exe <input WAVE> [<output HCA> -q <quality>]";

    }
}
