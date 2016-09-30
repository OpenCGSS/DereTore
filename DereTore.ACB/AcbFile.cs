using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DereTore.ACB {
    public sealed partial class AcbFile : UtfTable {

        public static AcbFile FromStream(FileStream stream) {
            return FromStream(stream, 0, stream.Length, false);
        }

        public static AcbFile FromStream(FileStream stream, bool disposeStream) {
            return FromStream(stream, 0, stream.Length, disposeStream);
        }

        public static AcbFile FromStream(FileStream stream, long offset, long size) {
            return new AcbFile(stream, offset, size, stream.Name, false);
        }

        public static AcbFile FromStream(FileStream stream, long offset, long size, bool disposeStream) {
            return new AcbFile(stream, offset, size, stream.Name, disposeStream);
        }

        public static AcbFile FromStream(FileStream stream, long offset, long size, bool includeCueIdInFileName, bool disposeStream) {
            return new AcbFile(stream, offset, size, stream.Name, includeCueIdInFileName, disposeStream);
        }

        public static AcbFile FromStream(Stream stream, string acbFileName, bool disposeStream) {
            return FromStream(stream, 0, stream.Length, acbFileName, disposeStream);
        }

        public static AcbFile FromStream(Stream stream, long offset, long size, string acbFileName, bool disposeStream) {
            return new AcbFile(stream, offset, size, acbFileName, disposeStream);
        }

        public static AcbFile FromStream(Stream stream, long offset, long size, string acbFileName, bool includeCueIdInFileName, bool disposeStream) {
            return new AcbFile(stream, offset, size, acbFileName, includeCueIdInFileName, disposeStream);
        }

        public static AcbFile FromFile(string fileName) {
            using (var fs = File.Open(fileName, FileMode.Open, FileAccess.Read)) {
                return FromStream(fs, false);
            }
        }

        public Dictionary<string, UtfTable> Tables => _tables;

        public AcbCueRecord[] Cues => _cues;

        public Afs2Archive InternalAwb => _internalAwb;

        public Afs2Archive ExternalAwb => _externalAwb;

        public UtfTable GetTable(string tableName) {
            if (_tables == null) {
                return null;
            }
            var tables = _tables;
            UtfTable table;
            if (tables.ContainsKey(tableName)) {
                table = tables[tableName];
            } else {
                table = ResolveTable(tableName);
                if (table != null) {
                    tables.Add(tableName, table);
                }
            }
            return table;
        }

        public string[] GetFileNames() {
            return _fileNames ?? (_fileNames = _cues?.Select(cue => cue.CueName).ToArray());
        }

        public bool FileExists(string fileName) {
            return _fileNames != null && _fileNames.Contains(fileName);
        }

        public Stream OpenDataStream(string fileName) {
            AcbCueRecord cue;
            try {
                cue = Cues.Single(c => c.CueName == fileName);
            } catch (InvalidOperationException ex) {
                throw new InvalidOperationException($"File '{fileName}' is not found or it has multiple entries.", ex);
            }
            if (!cue.IsWaveformIdentified) {
                throw new InvalidOperationException($"File '{fileName}' is not identified.");
            }
            if (cue.IsStreaming) {
                var externalAwb = ExternalAwb;
                if (externalAwb == null) {
                    throw new InvalidOperationException($"External AWB does not exist for streaming file '{fileName}'.");
                }
                if (!externalAwb.Files.ContainsKey(cue.WaveformId)) {
                    throw new InvalidOperationException($"Waveform ID {cue.WaveformId} is not found in AWB file {externalAwb.FileName}.");
                }
                var targetExternalFile = externalAwb.Files[cue.WaveformId];
                using (var fs = File.Open(externalAwb.FileName, FileMode.Open, FileAccess.Read)) {
                    return AcbHelper.ExtractToNewStream(fs, targetExternalFile.FileOffsetAligned, (int)targetExternalFile.FileLength);
                }
            } else {
                var internalAwb = InternalAwb;
                if (internalAwb == null) {
                    throw new InvalidOperationException($"Internal AWB is not found for memory file '{fileName}' in '{AcbFileName}'.");
                }
                if (!internalAwb.Files.ContainsKey(cue.WaveformId)) {
                    throw new InvalidOperationException($"Waveform ID {cue.WaveformId} is not found in internal AWB in {AcbFileName}.");
                }
                var targetInternalFile = internalAwb.Files[cue.WaveformId];
                return AcbHelper.ExtractToNewStream(Stream, targetInternalFile.FileOffsetAligned, (int)targetInternalFile.FileLength);
            }
        }

    }
}
