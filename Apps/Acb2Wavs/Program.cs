using System;
using System.Globalization;
using System.IO;
using CommandLine;
using DereTore.Common.StarlightStage;
using DereTore.Exchange.Archive.ACB;
using DereTore.Exchange.Audio.HCA;

namespace DereTore.Apps.Acb2Wavs {
    internal static class Program {

        private static int Main(string[] args) {
            var r = ParseOptions(args, out var options);

            if (r < 0) {
                return r;
            }

            if (!File.Exists(options.InputFileName)) {
                Console.Error.WriteLine("File not found: {0}", options.InputFileName);
                return DefaultExitCodeFail;
            }

            r = CreateDecodeParams(options, out var decodeParams);

            if (r < 0) {
                return r;
            }

            r = DoWork(options, decodeParams);

            return r;
        }

        private static int ParseOptions(string[] args, out Options options) {
            var parser = new Parser(settings => { settings.IgnoreUnknownArguments = true; });
            var parsedResult = parser.ParseArguments<Options>(args);

            var succeeded = parsedResult.Tag == ParserResultType.Parsed;

            options = null;

            if (succeeded) {
                options = ((Parsed<Options>)parsedResult).Value;
            }

            if (succeeded) {
                if (string.IsNullOrWhiteSpace(options.InputFileName)) {
                    succeeded = false;
                }
            }

            if (!succeeded) {
                var helpText = CommandLine.Text.HelpText.AutoBuild(parsedResult, null, null);

                helpText.AddPreOptionsLine(" ");
                helpText.AddPreOptionsLine("Usage: acb2wavs <input ACB> [options]");

                Console.Error.WriteLine(helpText);

                return DefaultExitCodeFail;
            }

            return 0;
        }

        private static int CreateDecodeParams(Options options, out DecodeParams decodeParams) {
            uint key1, key2;
            var formatProvider = new NumberFormatInfo();
            decodeParams = DecodeParams.Default;

            if (!string.IsNullOrWhiteSpace(options.Key1)) {
                if (!uint.TryParse(options.Key1, NumberStyles.HexNumber, formatProvider, out key1)) {
                    Console.WriteLine("ERROR: key 1 is in wrong format. It should look like \"a1b2c3d4\".");

                    return DefaultExitCodeFail;
                }
            } else {
                key1 = CgssCipher.Key1;
            }

            if (!string.IsNullOrWhiteSpace(options.Key2)) {
                if (!uint.TryParse(options.Key2, NumberStyles.HexNumber, formatProvider, out key2)) {
                    Console.WriteLine("ERROR: key 2 is in wrong format. It should look like \"a1b2c3d4\".");

                    return DefaultExitCodeFail;
                }
            } else {
                key2 = CgssCipher.Key2;
            }

            decodeParams = DecodeParams.CreateDefault(key1, key2);

            return 0;
        }

        private static int DoWork(Options options, DecodeParams baseDecodeParams) {
            var fileInfo = new FileInfo(options.InputFileName);
            var baseExtractDirPath = Path.Combine(fileInfo.DirectoryName ?? string.Empty, string.Format(DirTemplate, fileInfo.Name));

            if (!Directory.Exists(baseExtractDirPath)) {
                Directory.CreateDirectory(baseExtractDirPath);
            }

            using (var acb = AcbFile.FromFile(options.InputFileName)) {
                var formatVersion = acb.FormatVersion;

                if (acb.InternalAwb != null) {
                    var internalDirPath = Path.Combine(baseExtractDirPath, "internal");

                    ProcessAllBinaries(formatVersion, baseDecodeParams, internalDirPath, acb.InternalAwb, acb.Stream, true);
                }

                if (acb.ExternalAwb != null) {
                    var externalDirPath = Path.Combine(baseExtractDirPath, "external");

                    using (var fs = File.Open(acb.ExternalAwb.FileName, FileMode.Open, FileAccess.Read, FileShare.Read)) {
                        ProcessAllBinaries(formatVersion, baseDecodeParams, externalDirPath, acb.ExternalAwb, fs, false);
                    }
                }
            }

            return 0;
        }

        private static void ProcessAllBinaries(uint acbFormatVersion, DecodeParams baseDecodeParams, string extractDir, Afs2Archive archive, Stream dataStream, bool isInternal) {
            if (!Directory.Exists(extractDir)) {
                Directory.CreateDirectory(extractDir);
            }

            var afsSource = isInternal ? "internal" : "external";
            var decodeParams = baseDecodeParams;

            if (acbFormatVersion >= NewEncryptionVersion) {
                decodeParams.KeyModifier = archive.HcaKeyModifier;
            } else {
                decodeParams.KeyModifier = 0;
            }

            foreach (var entry in archive.Files) {
                var record = entry.Value;
                var extractFileName = AcbFile.GetSymbolicFileNameFromCueId(record.CueId);

                extractFileName = extractFileName.ReplaceExtension(".bin", ".wav");

                var extractFilePath = Path.Combine(extractDir, extractFileName);

                using (var fileData = AcbHelper.ExtractToNewStream(dataStream, record.FileOffsetAligned, (int)record.FileLength)) {
                    var isHcaStream = HcaReader.IsHcaStream(fileData);

                    Console.Write("Processing {0} AFS: #{1} (offset={2} size={3})...   ", afsSource, record.CueId, record.FileOffsetAligned, record.FileLength);

                    if (isHcaStream) {
                        try {
                            using (var fs = File.Open(extractFilePath, FileMode.Create, FileAccess.Write, FileShare.Write)) {
                                DecodeHca(fileData, fs, decodeParams);
                            }

                            Console.WriteLine("decoded");
                        } catch (Exception ex) {
                            if (File.Exists(extractFilePath)) {
                                File.Delete(extractFilePath);
                            }

                            Console.WriteLine(ex.ToString());

                            if (ex.InnerException != null) {
                                Console.WriteLine("Details:");
                                Console.WriteLine(ex.InnerException.ToString());
                            }
                        }
                    } else {
                        Console.WriteLine("skipped (not HCA)");
                    }
                }
            }
        }

        private static void DecodeHca(Stream hcaDataStream, Stream waveStream, DecodeParams decodeParams) {
            using (var hcaStream = new OneWayHcaAudioStream(hcaDataStream, decodeParams, true)) {
                var buffer = new byte[10240];
                var read = 1;

                while (read > 0) {
                    read = hcaStream.Read(buffer, 0, buffer.Length);

                    if (read > 0) {
                        waveStream.Write(buffer, 0, read);
                    }
                }
            }
        }

        private static string ReplaceExtension(this string str, string oldExt, string newExt) {
            if (str == null || oldExt == null || newExt == null) {
                throw new ArgumentNullException();
            }

            if (str.Length < oldExt.Length) {
                return str;
            }

            if (str.Substring(str.Length - oldExt.Length).ToLowerInvariant() != oldExt.ToLowerInvariant()) {
                return str;
            }

            return str.Substring(0, str.Length - oldExt.Length) + newExt;
        }

        private static readonly string DirTemplate = "_acb_{0}";

        private const int DefaultExitCodeFail = -1;
        private const uint NewEncryptionVersion = 0x01300000;

    }
}
