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

            var fullDirPath = Path.Combine(fileInfo.DirectoryName, string.Format(DirTemplate, fileInfo.Name));
            if (!Directory.Exists(fullDirPath)) {
                Directory.CreateDirectory(fullDirPath);
            }

            using (var acb = AcbFile.FromFile(fileName)) {
                var archivedEntryNames = acb.GetFileNames();
                for (var i = 0; i < archivedEntryNames.Length; ++i) {
                    var isCueNonEmpty = archivedEntryNames[i] != null;
                    var s = archivedEntryNames[i] ?? AcbFile.GetSymbolicFileNameFromCueId((uint)i);
                    var extractName = Path.Combine(fullDirPath, s);
                    try {
                        using (var source = isCueNonEmpty ? acb.OpenDataStream(s) : acb.OpenDataStream((uint)i)) {
                            using (var fs = new FileStream(extractName, FileMode.Create, FileAccess.Write, FileShare.Write)) {
                                WriteFile(source, fs);
                            }
                        }
                        Console.WriteLine(s);
                    } catch (Exception ex) {
                        var origForeground = ConsoleColor.Gray;
                        try {
                            origForeground = Console.ForegroundColor;
                        } catch {
                        }
                        try {
                            Console.ForegroundColor = ConsoleColor.Red;
                        } catch {
                        }
                        Console.WriteLine(ex.Message);
                        try {
                            Console.ForegroundColor = origForeground;
                        } catch {
                        }
                    }
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
