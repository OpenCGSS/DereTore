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

            var baseExtractDirPath = Path.Combine(fileInfo.DirectoryName ?? string.Empty, string.Format(DirTemplate, fileInfo.Name));

            if (!Directory.Exists(baseExtractDirPath)) {
                Directory.CreateDirectory(baseExtractDirPath);
            }

            using (var acb = AcbFile.FromFile(fileName)) {
                var archivedEntryNames = acb.GetFileNames();

                for (var i = 0; i < archivedEntryNames.Length; ++i) {
                    var isCueNonEmpty = archivedEntryNames[i] != null;
                    var s = archivedEntryNames[i] ?? AcbFile.GetSymbolicFileNameFromCueId((uint)i);
                    var extractName = Path.Combine(baseExtractDirPath, s);

                    try {
                        using (var source = isCueNonEmpty ? acb.OpenDataStream(s) : acb.OpenDataStream((uint)i)) {
                            using (var fs = new FileStream(extractName, FileMode.Create, FileAccess.Write, FileShare.Write)) {
                                WriteFile(source, fs);
                            }
                        }

                        Console.WriteLine("Extracted from cue: {0}", s);
                    } catch (Exception ex) {
                        PrintColoredErrorMessage(ex.Message);
                    }
                }

                if (acb.InternalAwb != null) {
                    var internalDirPath = Path.Combine(baseExtractDirPath, "internal");

                    ExtractAllBinaries(internalDirPath, acb.InternalAwb, acb.Stream, true);
                }

                if (acb.ExternalAwb != null) {
                    var externalDirPath = Path.Combine(baseExtractDirPath, "external");

                    using (var fs = File.Open(acb.ExternalAwb.FileName, FileMode.Open, FileAccess.Read, FileShare.Read)) {
                        ExtractAllBinaries(externalDirPath, acb.ExternalAwb, fs, false);
                    }
                }
            }
        }

        private static void PrintColoredErrorMessage(string message) {
            var origForeground = ConsoleColor.Gray;

            try {
                origForeground = Console.ForegroundColor;
            } catch {
            }

            try {
                Console.ForegroundColor = ConsoleColor.Red;
            } catch {
            }

            Console.WriteLine(message);

            try {
                Console.ForegroundColor = origForeground;
            } catch {
            }
        }

        private static void ExtractAllBinaries(string extractDir, Afs2Archive archive, Stream dataStream, bool isInternal) {
            if (!Directory.Exists(extractDir)) {
                Directory.CreateDirectory(extractDir);
            }

            var afsSource = isInternal ? "internal" : "external";

            foreach (var entry in archive.Files) {
                var record = entry.Value;
                var extractFileName = AcbFile.GetSymbolicFileNameFromCueId(record.CueId);
                var extractFilePath = Path.Combine(extractDir, extractFileName);

                using (var fs = File.Open(extractFilePath, FileMode.Create, FileAccess.Write, FileShare.Write)) {
                    using (var fileData = AcbHelper.ExtractToNewStream(dataStream, record.FileOffsetAligned, (int)record.FileLength)) {
                        WriteFile(fileData, fs);
                    }
                }

                Console.WriteLine("Extracted from {0} AFS: #{1} (offset={2} size={3})", afsSource, record.CueId, record.FileOffsetAligned, record.FileLength);
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
