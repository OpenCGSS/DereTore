using System;
using System.IO;
using System.Runtime.InteropServices;
using CommandLine;

namespace DereTore.Apps.Hcaenc {
    internal static class Program {

        private static int Main(string[] args) {
            if (Environment.Is64BitProcess) {
                Console.WriteLine(UnsupportedBuildMessage);
                return -2;
            }

            var parser = new Parser(settings => { settings.IgnoreUnknownArguments = true; });

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
                outputFile = Path.Combine(inputFileDir, inputFileInfo.Name.Substring(0, inputFileInfo.Name.Length - inputFileInfo.Extension.Length) + ".hca");
            }

            var quality = options.Quaility;

            if (quality < 1 || quality > 5) {
                Console.WriteLine("Warning: Quality should be 1 to 5. Using q=1.");
                quality = 1;
            }

            Console.WriteLine("Encoding {0} to {1} (q={2}) ...", inputFile, outputFile, quality);

            // Quality = 3 (~128 Kbps) for MLTD, 1 (~256 Kbps) for CGSS
            int cutoff = 0;
            ulong key = 0;

            return hcaencEncodeToFile(inputFile, outputFile, quality, cutoff, key);
        }

        [DllImport("hcaenc_lite", CallingConvention = CallingConvention.StdCall)]
        private static extern int hcaencEncodeToFile([MarshalAs(UnmanagedType.LPStr)] string lpstrInputFile, [MarshalAs(UnmanagedType.LPStr)] string lpstrOutputFile, int nQuality, int nCutoff, ulong ullKey);

        private static readonly string HelpMessage = "Usage: hcaenc.exe <input WAVE> [<output HCA> -q <quality>]";
        private static readonly string UnsupportedBuildMessage = "hcaenc only has 32-bit version due to the limits of hcaenc_lite.dll.";

    }
}
