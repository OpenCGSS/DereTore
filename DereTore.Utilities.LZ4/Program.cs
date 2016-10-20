using System;
using System.IO;
using LZ4;

namespace DereTore.Utilities.LZ4 {
    internal static class Program {

        private static int Main(string[] args) {
            if (args.Length < 1 && args.Length > 2) {
                Console.WriteLine(HelpMessage);
                return 0;
            }
            var inputFileName = args[0];
            string outputFileName;
            if (args.Length == 2) {
                outputFileName = args[1];
            } else {
                var fileInfo = new FileInfo(inputFileName);
                outputFileName = fileInfo.FullName + ".lz4";
            }
            var fileData = File.ReadAllBytes(inputFileName);
            var compressedFileData = LZ4Codec.EncodeHC(fileData, 0, fileData.Length);
            using (var compressedFileStream = File.Open(outputFileName, FileMode.Create, FileAccess.Write)) {
                // LZ4 header
                compressedFileStream.WriteInt32LE(0x00000064);
                compressedFileStream.WriteInt32LE(fileData.Length);
                compressedFileStream.WriteInt32LE(compressedFileData.Length);
                compressedFileStream.WriteInt32LE(0x00000001);
                // File data
                compressedFileStream.WriteBytes(compressedFileData);
            }
            return 0;
        }

        private static readonly string HelpMessage = "LZ4 <input file> [<output file>]";

    }
}
