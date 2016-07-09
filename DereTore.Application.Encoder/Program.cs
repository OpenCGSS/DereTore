using System;
using System.Runtime.InteropServices;

namespace DereTore.Application.Encoder {
    internal static class Program {

        private static int Main(string[] args) {
            if (args.Length != 2) {
                Console.WriteLine(HelpMessage);
                return 0;
            }
            int quality = 1, cutoff = 0;
            ulong key = 0;
            return hcaencEncodeToFile(args[0], args[1], quality, cutoff, key);
        }

        [DllImport("hcaenc_lite", CallingConvention = CallingConvention.StdCall)]
        private static extern int hcaencEncodeToFile([MarshalAs(UnmanagedType.LPStr)] string lpstrInputFile, [MarshalAs(UnmanagedType.LPStr)] string lpstrOutputFile, int nQuality, int nCutoff, ulong ullKey);

        private static readonly string HelpMessage = "Usage: hcaenc.exe <input WAVE> <output HCA>";

    }
}
