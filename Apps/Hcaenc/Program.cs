using System;
using System.Runtime.InteropServices;

namespace DereTore.Apps.Hcaenc {
    internal static class Program {

        private static int Main(string[] args) {
            if (Environment.Is64BitProcess) {
                Console.WriteLine(UnsupportedBuildMessage);
                return -2;
            }
            if (args.Length != 2) {
                Console.WriteLine(HelpMessage);
                return -1;
            }
            int quality = 1, cutoff = 0;
            ulong key = 0;
            return hcaencEncodeToFile(args[0], args[1], quality, cutoff, key);
        }

        [DllImport("hcaenc_lite", CallingConvention = CallingConvention.StdCall)]
        private static extern int hcaencEncodeToFile([MarshalAs(UnmanagedType.LPStr)] string lpstrInputFile, [MarshalAs(UnmanagedType.LPStr)] string lpstrOutputFile, int nQuality, int nCutoff, ulong ullKey);

        private static readonly string HelpMessage = "Usage: hcaenc.exe <input WAVE> <output HCA>";
        private static readonly string UnsupportedBuildMessage = "hcaenc only has 32-bit version due to the limits of hcaenc_lite.dll.";

    }
}
