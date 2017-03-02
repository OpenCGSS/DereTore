using System;
using System.IO;
using DereTore.Exchange.Archive.ACB;

namespace DereTore.Apps.AcbUnzip {
    internal static class Program {

        private static void Main(string[] args) {
            if (args.Length < 1) {
                Console.WriteLine("Usage: AcbUnzip <Input ACB File>");
                return;
            }
            var fileName = args[0];
            var fileInfo = new FileInfo(fileName);

            var fullDirPath = Path.Combine(fileInfo.DirectoryName ?? string.Empty, string.Format(DirTemplate, fileInfo.Name));
            if (!Directory.Exists(fullDirPath)) {
                Directory.CreateDirectory(fullDirPath);
            }

            using (var acb = AcbFile.FromFile(fileName)) {
                var fileNames = acb.GetFileNames();
                foreach (var s in fileNames) {
                    var extractName = Path.Combine(fullDirPath, s);
                    using (var fs = new FileStream(extractName, FileMode.Create, FileAccess.Write)) {
                        using (var source = acb.OpenDataStream(s)) {
                            WriteFile(source, fs);
                        }
                    }
                    Console.WriteLine(s);
                }
            }
        }

        private static void WriteFile(Stream sourceStream, FileStream outputStream) {
            var buffer = new byte[4096];
            int read = 0;
            do {
                if (sourceStream.CanRead) {
                    read = sourceStream.Read(buffer, 0, buffer.Length);
                    outputStream.Write(buffer, 0, read);
                }
            } while (read == buffer.Length);
        }

        private static readonly string DirTemplate = "_acb_{0}";

    }
}
