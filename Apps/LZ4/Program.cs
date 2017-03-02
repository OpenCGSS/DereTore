using System;
using System.IO;
using DereTore.Common;
using DereTore.Compression.LZ4;

namespace DereTore.Apps.LZ4 {
    internal static class Program {

        private static int Main(string[] args) {
            if (args.Length < 1 && args.Length > 2) {
                Console.WriteLine(HelpMessage);
                return 0;
            }
            var inputFileName = args[0];
            string outputFileName;
            var isDecompressMode = inputFileName.ToLowerInvariant().EndsWith(".lz4");
            if (args.Length == 2) {
                outputFileName = args[1];
            } else {
                var fileInfo = new FileInfo(inputFileName);
                outputFileName = isDecompressMode ? fileInfo.FullName.Substring(0, fileInfo.FullName.Length - 4) : fileInfo.FullName + ".lz4";
            }
            if (isDecompressMode) {
                DecompressFile(inputFileName, outputFileName);
            } else {
                CompressFile(inputFileName, outputFileName);
            }
            return 0;
        }

        private static void CompressFile(string inputFileName, string outputFileName) {
            var fileData = File.ReadAllBytes(inputFileName);
            var compressedFileData = CgssLz4.Compress(fileData);
            using (var compressedFileStream = File.Open(outputFileName, FileMode.Create, FileAccess.Write)) {
                compressedFileStream.WriteBytes(compressedFileData);
            }
        }

        private static void DecompressFile(string inputFileName, string outputFileName) {
            var fileData = File.ReadAllBytes(inputFileName);
            var decompressedFileData = CgssLz4.Decompress(fileData);
            using (var decompressedFileStream = File.Open(outputFileName, FileMode.Create, FileAccess.Write)) {
                decompressedFileStream.WriteBytes(decompressedFileData);
            }
        }

        private static readonly string HelpMessage = "LZ4 <input file> [<output file>]";

    }
}
