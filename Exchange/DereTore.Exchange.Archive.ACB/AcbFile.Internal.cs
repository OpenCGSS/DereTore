using System;
using System.Collections.Generic;
using System.IO;
using DereTore.Common;

namespace DereTore.Exchange.Archive.ACB {
    public partial class AcbFile {

        internal override void Initialize() {
            base.Initialize();
            InitializeAcbTables();
            InitializeCueNameToWaveformTable();
            InitializeAwbArchives();
        }

        protected override void Dispose(bool disposing) {
            _internalAwb?.Dispose();
            _externalAwb?.Dispose();
            base.Dispose(disposing);
        }

        private AcbFile(Stream stream, long offset, long size, string acbFileName, bool disposeStream)
           : this(stream, offset, size, acbFileName, false, disposeStream) {
        }

        private AcbFile(Stream stream, long offset, long size, string acbFileName, bool includeCueIdInFileName, bool disposeStream)
            : base(stream, offset, size, acbFileName, disposeStream) {
            _includeCueIdInFileName = includeCueIdInFileName;
            Initialize();
        }

        private void InitializeAcbTables() {
            var stream = Stream;
            long refItemOffset = 0, refItemSize = 0, refCorrection = 0;

            var tables = new Dictionary<string, UtfTable>();
            _tables = tables;

            var cueTable = GetTable("CueTable");
            var waveformTable = GetTable("WaveformTable");
            var synthTable = GetTable("SynthTable");
            var cues = new AcbCueRecord[cueTable.Rows.Length];
            _cues = cues;

            for (var i = 0; i < cues.Length; ++i) {
                var cue = new AcbCueRecord();
                cue.IsWaveformIdentified = false;
                cue.CueId = cueTable.GetFieldValueAsNumber<uint>(i, "CueId").Value;
                cue.ReferenceType = cueTable.GetFieldValueAsNumber<byte>(i, "ReferenceType").Value;
                cue.ReferenceIndex = cueTable.GetFieldValueAsNumber<ushort>(i, "ReferenceIndex").Value;
                cues[i] = cue;

                switch (cue.ReferenceType) {
                    case 2:
                        refItemOffset = synthTable.GetFieldOffset(cue.ReferenceIndex, "ReferenceItems").Value;
                        refItemSize = synthTable.GetFieldSize(cue.ReferenceIndex, "ReferenceItems").Value;
                        refCorrection = refItemSize + 2;
                        break;
                    case 3:
                    case 8:
                        if (i == 0) {
                            refItemOffset = synthTable.GetFieldOffset(0, "ReferenceItems").Value;
                            refItemSize = synthTable.GetFieldSize(0, "ReferenceItems").Value;
                            refCorrection = refItemSize - 2;
                        } else {
                            refCorrection += 4;
                        }
                        break;
                    default:
                        throw new FormatException($"Unexpected ReferenceType '{cues[i].ReferenceType}' for CueIndex: '{i}.'");
                }

                if (refItemSize > 0) {
                    cue.WaveformIndex = stream.PeekUInt16BE(refItemOffset + refCorrection);
                    var waveformId = waveformTable.GetFieldValueAsNumber<ushort>(cue.WaveformIndex, "Id");
                    if (!waveformId.HasValue) {
                        // MemoryAwbId is for ADX2LE encoder.
                        waveformId = waveformTable.GetFieldValueAsNumber<ushort>(cue.WaveformIndex, "MemoryAwbId");
                    }
                    cue.WaveformId = waveformId.GetValueOrDefault();
                    cue.EncodeType = waveformTable.GetFieldValueAsNumber<byte>(cue.WaveformIndex, "EncodeType").Value;
                    var isStreaming = waveformTable.GetFieldValueAsNumber<byte>(cue.WaveformIndex, "Streaming").Value;
                    cue.IsStreaming = isStreaming != 0;
                    cue.IsWaveformIdentified = true;
                }
            }
        }

        private void InitializeCueNameToWaveformTable() {
            var cueNameTable = GetTable("CueNameTable");
            var cues = Cues;
            var includeCueIdInFileName = _includeCueIdInFileName;
            var cueNameToWaveform = new Dictionary<string, ushort>();
            _cueNameToWaveform = cueNameToWaveform;

            for (var i = 0; i < cueNameTable.Rows.Length; ++i) {
                var cueIndex = cueNameTable.GetFieldValueAsNumber<ushort>(i, "CueIndex").Value;
                var cue = cues[cueIndex];
                if (cue.IsWaveformIdentified) {
                    var cueName = cueNameTable.GetFieldValueAsString(i, "CueName");
                    cueName += GetExtensionForEncodeType(cue.EncodeType);
                    if (includeCueIdInFileName) {
                        cueName = cue.CueId.ToString("D5") + "_" + cueName;
                    }
                    cue.CueName = cueName;
                    cueNameToWaveform.Add(cueName, cue.WaveformId);
                }
            }
        }

        private void InitializeAwbArchives() {
            var internalAwbSize = GetFieldSize(0, "AwbFile");
            if (internalAwbSize.HasValue && internalAwbSize.Value > 0) {
                _internalAwb = GetInternalAwbArchive();
            }
            var externalAwbSize = GetFieldSize(0, "StreamAwbAfs2Header");
            if (externalAwbSize.HasValue && externalAwbSize.Value > 0) {
                _externalAwb = GetExternalAwbArchive();
            }
        }

        private Afs2Archive GetInternalAwbArchive() {
            var internalAwbOffset = GetFieldOffset(0, "AwbFile");
            var internalAwb = new Afs2Archive(Stream, internalAwbOffset.Value, AcbFileName, false);
            internalAwb.Initialize();
            return internalAwb;
        }

        private Afs2Archive GetExternalAwbArchive() {
            var acbFileName = AcbFileName;
            var awbDirPath = Path.GetDirectoryName(acbFileName);
            var awbBaseFileName = Path.GetFileNameWithoutExtension(acbFileName);
            string[] awbFiles = null;
            string awbMask1 = null, awbMask2 = null, awbMask3 = null;

            if (awbFiles == null || awbFiles.Length < 1) {
                awbMask1 = string.Format(AwbFileNameFormats.Format1, awbBaseFileName);
                awbFiles = Directory.GetFiles(awbDirPath, awbMask1, SearchOption.TopDirectoryOnly);
            }
            if (awbFiles == null || awbFiles.Length < 1) {
                awbMask2 = string.Format(AwbFileNameFormats.Format2, awbBaseFileName);
                awbFiles = Directory.GetFiles(awbDirPath, awbMask2, SearchOption.TopDirectoryOnly);
            }
            if (awbFiles == null || awbFiles.Length < 1) {
                awbMask3 = string.Format(AwbFileNameFormats.Format3, awbBaseFileName);
                awbFiles = Directory.GetFiles(awbDirPath, awbMask3, SearchOption.TopDirectoryOnly);
            }
            if (awbFiles.Length < 1) {
                throw new FileNotFoundException($"Cannot find AWB file. Please verify corresponding AWB file is named '{awbMask1}', '{awbMask2}', or '{awbMask3}'.");
            }
            if (awbFiles.Length > 1) {
                throw new FileNotFoundException($"More than one matching AWB file for this ACB. Please verify only one AWB file is named '{awbMask1}', '{awbMask2}' or '{awbMask3}'.");
            }

            var externalAwbHash = GetFieldValueAsData(0, "StreamAwbHash");
            var fs = File.Open(awbFiles[0], FileMode.Open, FileAccess.Read);
            var awbHash = AcbHelper.GetMd5Checksum(fs);
            Afs2Archive archive;
            if (AcbHelper.AreDataIdentical(awbHash, externalAwbHash)) {
                archive = new Afs2Archive(fs, 0, fs.Name, true);
                archive.Initialize();
            } else {
                fs.Dispose();
                throw new FormatException($"Checksum of AWB file '{fs.Name}' ({BitConverter.ToString(awbHash)}) does not match MD5 checksum inside ACB file '{acbFileName}' ({BitConverter.ToString(externalAwbHash)}).");
            }
            return archive;
        }

        private UtfTable ResolveTable(string tableName) {
            var tableOffset = GetFieldOffset(0, tableName);
            if (!tableOffset.HasValue) {
                return null;
            }
            var tableSize = GetFieldSize(0, tableName);
            if (!tableSize.HasValue) {
                return null;
            }
            var table = new UtfTable(Stream, tableOffset.Value, tableSize.Value, AcbFileName, false);
            table.Initialize();
            return table;
        }

        private static string GetExtensionForEncodeType(byte encodeType) {
            string ext;
            switch ((WaveformEncodeType)encodeType) {
                case WaveformEncodeType.Adx:
                    ext = ".adx";
                    break;
                case WaveformEncodeType.Hca:
                    ext = ".hca";
                    break;
                case WaveformEncodeType.Atrac3:
                    ext = ".at3";
                    break;
                case WaveformEncodeType.Vag:
                    ext = ".vag";
                    break;
                case WaveformEncodeType.BcWav:
                    ext = ".bcwav";
                    break;
                case WaveformEncodeType.NintendoDsp:
                    ext = ".dsp";
                    break;
                default:
                    ext = $".et-{encodeType.ToString("D2")}.bin";
                    break;
            }
            return ext;
        }

        private readonly bool _includeCueIdInFileName;

        private Dictionary<string, UtfTable> _tables;
        private Dictionary<string, ushort> _cueNameToWaveform;
        private AcbCueRecord[] _cues;
        private Afs2Archive _internalAwb;
        private Afs2Archive _externalAwb;
        private string[] _fileNames;

    }
}
