using System;
using System.IO;
using System.Runtime.InteropServices;

namespace DereTore.Apps.Hcaenc {
    internal static class Program {

        private static int Main(string[] args) {
            if (Environment.Is64BitProcess) {
                Console.WriteLine(UnsupportedBuildMessage);
                return -2;
            }

            if (args.Length < 1) {
                Console.WriteLine(HelpMessage);
                return -1;
            }

            var inputFile = args[0];

            inputFile = Path.GetFullPath(inputFile);

            string outputFile;

            if (args.Length >= 2) {
                outputFile = args[1];
            } else {
                var inputFileInfo = new FileInfo(inputFile);
                var inputFileDir = inputFileInfo.DirectoryName;
                outputFile = Path.Combine(inputFileDir, inputFileInfo.Name.Substring(0, inputFileInfo.Name.Length - inputFileInfo.Extension.Length) + ".hca");
            }

            Console.WriteLine("Encoding {0} to {1} ...", inputFile, outputFile);

            // Quality = 3 (~128 Kbps) for MLTD, 1 (~256 Kbps) for CGSS
            int quality = 3, cutoff = 0;
            ulong key = 0;

            return hcaencEncodeToFile(inputFile, outputFile, quality, cutoff, key);
        }

        [DllImport("hcaenc_lite", CallingConvention = CallingConvention.StdCall)]
        private static extern int hcaencEncodeToFile([MarshalAs(UnmanagedType.LPStr)] string lpstrInputFile, [MarshalAs(UnmanagedType.LPStr)] string lpstrOutputFile, int nQuality, int nCutoff, ulong ullKey);

        private static readonly string HelpMessage = "Usage: hcaenc.exe <input WAVE> [<output HCA>]";
        private static readonly string UnsupportedBuildMessage = "hcaenc only has 32-bit version due to the limits of hcaenc_lite.dll.";

    }
}
