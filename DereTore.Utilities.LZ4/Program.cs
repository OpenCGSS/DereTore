using System;
using System.IO;
using Lz4Net;

namespace DereTore.Utilities.LZ4 {
    internal static class Program {

        private static int Main(string[] args) {
            if (args.Length != 2) {
                Console.WriteLine(HelpMessage);
                return 0;
            }
            var inputFileName = args[0];
            var outputFileName = args[1];
            using (var input = File.Open(inputFileName, FileMode.Open, FileAccess.Read)) {
                using (var output = File.Open(outputFileName, FileMode.Create, FileAccess.Write)) {
                    using (var lz4Stream = new Lz4CompressionStream(output)) {
                        var buffer = new byte[10240];
                        var read = 1;
                        while (read > 0) {
                            read = input.Read(buffer, 0, buffer.Length);
                            lz4Stream.Write(buffer, 0, read);
                            if (read < buffer.Length) {
                                break;
                            }
                        }
                    }
                }
            }
            return 0;
        }

        private static readonly string HelpMessage = "LZ4 <input file> <output file>";

    }
}
