using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DereTore.ACB {
    public sealed class CriAcbFile : CriUtfTable {

        public CriAcbFile(string fileName, Stream stream, long offset, bool includeCueIdInFileName) {
            // initialize UTF
            Initialize(fileName, stream, offset);

            // initialize ACB specific items
            InitializeCueList(fileName, stream);
            InitializeCueNameToWaveformMap(fileName, stream, includeCueIdInFileName);

            // initialize internal AWB
            if (InternalAwbFileSize > 0) {
                _internalAwb = new CriAfs2Archive(fileName, stream, (long)InternalAwbFileOffset);
            }

            // initialize external AWB
            if (StreamAwbAfs2HeaderSize > 0) {
                _externalAwb = InitializeExternalAwbArchive();
            }
        }

        public string Name {
            get { return GetUtfFieldForRow<string>(this, 0, "Name"); }
        }

        public string VersionString {
            get { return GetUtfFieldForRow<string>(this, 0, "VersionString"); }
        }

        public ulong InternalAwbFileOffset {
            get { return GetOffsetForUtfFieldForRow(this, 0, "AwbFile"); }
        }

        public ulong InternalAwbFileSize {
            get { return GetSizeForUtfFieldForRow(this, 0, "AwbFile"); }
        }

        public ulong CueTableOffset {
            get { return GetOffsetForUtfFieldForRow(this, 0, "CueTable"); }
        }

        public ulong CueNameTableOffset {
            get { return GetOffsetForUtfFieldForRow(this, 0, "CueNameTable"); }
        }

        public ulong WaveformTableOffset {
            get { return GetOffsetForUtfFieldForRow(this, 0, "WaveformTable"); }
        }

        public ulong SynthTableOffset {
            get { return GetOffsetForUtfFieldForRow(this, 0, "SynthTable"); }
        }

        public CriAcbCueRecord[] CueList {
            get { return _cueList; }
        }

        public Dictionary<string, ushort> CueNamesToWaveforms {
            get { return _cueNamesToWaveforms; }
        }

        public byte[] AcfMd5Hash {
            get { return GetUtfFieldForRow<byte[]>(this, 0, "AcfMd5Hash"); }
        }

        public ulong AwbFileOffset {
            get { return GetOffsetForUtfFieldForRow(this, 0, "AwbFile"); }
        }

        public CriAfs2Archive InternalAwb {
            get { return _internalAwb; }
        }

        public byte[] StreamAwbHash {
            get { return GetUtfFieldForRow<byte[]>(this, 0, "StreamAwbHash"); }
        }

        public ulong StreamAwbAfs2HeaderOffset {
            get { return GetOffsetForUtfFieldForRow(this, 0, "StreamAwbAfs2Header"); }
        }

        public ulong StreamAwbAfs2HeaderSize {
            get { return GetSizeForUtfFieldForRow(this, 0, "StreamAwbAfs2Header"); }
        }

        public CriAfs2Archive ExternalAwb {
            get { return _externalAwb; }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fileName">Example: "song_1004.hca". Case sensitive.</param>
        /// <returns></returns>
        public bool FileExists(string fileName) {
            return CueList.Any(cueItem => fileName == cueItem.CueName);
        }

        public string[] GetFileNames() {
            return CueList.Select(cueItem => cueItem.CueName).ToArray();
        }

        public Stream OpenReadStream(string fileName) {
            foreach (var cueItem in CueList) {
                if (cueItem.CueName == fileName) {
                    if (cueItem.IsWaveformIdentified) {
                        FileStream fs;
                        if (cueItem.IsStreaming) {
                            if (ExternalAwb != null) {
                                using (fs = File.Open(ExternalAwb.SourceFile, FileMode.Open, FileAccess.Read)) {
                                    return AcbHelper.ExtractChunkToStream(fs, (ulong)ExternalAwb.Files[cueItem.WaveformId].FileOffsetByteAligned, (ulong)ExternalAwb.Files[cueItem.WaveformId].FileLength);
                                }
                            }
                        } else {
                            if (InternalAwb != null) {
                                using (fs = File.Open(InternalAwb.SourceFile, FileMode.Open, FileAccess.Read)) {
                                    return AcbHelper.ExtractChunkToStream(fs, (ulong)InternalAwb.Files[cueItem.WaveformId].FileOffsetByteAligned, (ulong)InternalAwb.Files[cueItem.WaveformId].FileLength);
                                }
                            }
                        }
                    }
                }
            }
            // All other situations are not needed.
            return null;
        }

        public static CriAcbFile FromFile(string fileName) {
            return FromFile(fileName, false);
        }

        public static CriAcbFile FromFile(string fileName, bool includeCueInFileName) {
            using (var fs = File.Open(fileName, FileMode.Open, FileAccess.Read)) {
                return new CriAcbFile(fileName, fs, 0, includeCueInFileName);
            }
        }

        private void InitializeCueNameToWaveformMap(string fileName, Stream stream, bool includeCueIdInFileName) {
            var cueNameTableUtf = new CriUtfTable();
            cueNameTableUtf.Initialize(fileName, stream, (long)CueNameTableOffset);

            for (var i = 0; i < cueNameTableUtf.NumberOfRows; i++) {
                var cueIndex = GetUtfFieldForRow<ushort>(cueNameTableUtf, i, "CueIndex");

                // skip cues with unidentified waveforms (see 'vc05_0140.acb, vc10_0372.acb' in Kidou_Senshi_Gundam_AGE_Universe_Accel (PSP))
                var cueItem = CueList[cueIndex];
                if (cueItem.IsWaveformIdentified) {
                    var cueName = GetUtfFieldForRow<string>(cueNameTableUtf, i, "CueName");

                    cueItem.CueName = cueName;
                    cueItem.CueName += GetFileExtensionForEncodeType(cueItem.EncodeType);

                    if (includeCueIdInFileName) {
                        cueItem.CueName = string.Format("{0}_{1}", cueItem.CueId.ToString("D5"), cueItem.CueName);
                    }

                    CueNamesToWaveforms.Add(cueItem.CueName, cueItem.WaveformId);
                }
            }
        }

        private void InitializeCueList(string fileName, Stream stream) {
            _cueNamesToWaveforms = new Dictionary<string, ushort>();

            ulong referenceItemsOffset = 0;
            ulong referenceItemsSize = 0;
            ulong referenceCorrection = 0;

            var cueTableUtf = new CriUtfTable();
            cueTableUtf.Initialize(fileName, stream, (long)CueTableOffset);

            var waveformTableUtf = new CriUtfTable();
            waveformTableUtf.Initialize(fileName, stream, (long)WaveformTableOffset);

            var synthTableUtf = new CriUtfTable();
            synthTableUtf.Initialize(fileName, stream, (long)SynthTableOffset);

            _cueList = new CriAcbCueRecord[cueTableUtf.NumberOfRows];

            for (var i = 0; i < cueTableUtf.NumberOfRows; i++) {
                var cueItem = new CriAcbCueRecord {
                    IsWaveformIdentified = false,
                    CueId = GetUtfFieldForRow<uint>(cueTableUtf, i, "CueId"),
                    ReferenceType = GetUtfFieldForRow<byte>(cueTableUtf, i, "ReferenceType"),
                    ReferenceIndex = GetUtfFieldForRow<ushort>(cueTableUtf, i, "ReferenceIndex")
                };
                CueList[i] = cueItem;

                switch (cueItem.ReferenceType) {
                    case 2:
                        referenceItemsOffset = GetOffsetForUtfFieldForRow(synthTableUtf, cueItem.ReferenceIndex, "ReferenceItems");
                        referenceItemsSize = GetSizeForUtfFieldForRow(synthTableUtf, cueItem.ReferenceIndex, "ReferenceItems");
                        referenceCorrection = referenceItemsSize + 2;
                        break;
                    case 3:
                    case 8:
                        if (i == 0) {
                            referenceItemsOffset = GetOffsetForUtfFieldForRow(synthTableUtf, 0, "ReferenceItems");
                            referenceItemsSize = GetSizeForUtfFieldForRow(synthTableUtf, 0, "ReferenceItems");
                            referenceCorrection = referenceItemsSize - 2; // samples found have only had a '01 00' record => Always Waveform[0]?.                       
                        } else {
                            referenceCorrection += 4; // relative to previous offset, do not lookup
                            // @TODO: Should this do a referenceItemsSize - 2 for the ReferenceIndex?  Need to find
                            //    one where size > 4.
                            //referenceItemsOffset = (ulong)CriUtfTable.GetOffsetForUtfFieldForRow(synthTableUtf, this.CueList[i].ReferenceIndex, "ReferenceItems");
                            //referenceItemsSize = CriUtfTable.GetSizeForUtfFieldForRow(synthTableUtf, this.CueList[i].ReferenceIndex, "ReferenceItems");
                            //referenceCorrection = referenceItemsSize - 2;
                        }
                        break;
                    default:
                        throw new FormatException(string.Format("Unexpected ReferenceType: '{0}' for CueIndex: '{1}.'", CueList[i].ReferenceType.ToString("D"), i.ToString("D")));
                }

                if (referenceItemsSize != 0) {
                    // get wave form info
                    cueItem.WaveformIndex = stream.ReadUInt16BE((long)(referenceItemsOffset + referenceCorrection));

                    // get waveform id and encode type from corresponding waveform
                    ushort? waveformId = GetUtfFieldForRow<ushort?>(waveformTableUtf, cueItem.WaveformIndex, "Id");
                    if (!waveformId.HasValue) {
                        // MemoryAwbId is for ADX2LE encoder.
                        waveformId = GetUtfFieldForRow<ushort?>(waveformTableUtf, cueItem.WaveformIndex, "MemoryAwbId");
                    }
                    cueItem.WaveformId = waveformId.GetValueOrDefault();
                    cueItem.EncodeType = GetUtfFieldForRow<byte>(waveformTableUtf, cueItem.WaveformIndex, "EncodeType");

                    // get Streaming flag, 0 = in ACB files, 1 = in AWB file
                    var isStreaming = GetUtfFieldForRow<byte>(waveformTableUtf, cueItem.WaveformIndex, "Streaming");
                    cueItem.IsStreaming = isStreaming != 0;

                    // update flag
                    cueItem.IsWaveformIdentified = true;
                }
            }
        }

        private CriAfs2Archive InitializeExternalAwbArchive() {
            CriAfs2Archive afs2;
            var awbDirectory = Path.GetDirectoryName(SourceFile);

            // try format 1
            var acbBaseFileName = Path.GetFileNameWithoutExtension(SourceFile);
            var awbMask = string.Format(AwbFormats.Format1, acbBaseFileName);
            var awbFiles = Directory.GetFiles(awbDirectory, awbMask, SearchOption.TopDirectoryOnly);

            if (awbFiles.Length < 1) {
                // try format 2
                awbMask = string.Format(AwbFormats.Format2, acbBaseFileName);
                awbFiles = Directory.GetFiles(awbDirectory, awbMask, SearchOption.TopDirectoryOnly);
            }

            if (awbFiles.Length < 1) {
                // try format 3
                awbMask = string.Format(AwbFormats.Format3, acbBaseFileName);
                awbFiles = Directory.GetFiles(awbDirectory, awbMask, SearchOption.TopDirectoryOnly);
            }

            // file not found
            if (awbFiles.Length < 1) {
                throw new FileNotFoundException(string.Format("Cannot find AWB file. Please verify corresponding AWB file is named '{0}', '{1}', or '{2}'.",
                    string.Format(AwbFormats.Format1, acbBaseFileName), string.Format(AwbFormats.Format2, acbBaseFileName), string.Format(AwbFormats.Format3, acbBaseFileName)));
            }

            if (awbFiles.Length > 1) {
                throw new FileNotFoundException(string.Format("More than one matching AWB file for this ACB. Please verify only one AWB file is named '{0}' or '{1}'.",
                    string.Format(AwbFormats.Format1, acbBaseFileName), string.Format(AwbFormats.Format2, acbBaseFileName)));
            }

            // initialize AFS2 file                        
            using (var fs = File.Open(awbFiles[0], FileMode.Open, FileAccess.Read, FileShare.Read)) {
                // validate MD5 checksum
                var awbMd5Calculated = AcbHelper.GetMd5Checksum(fs);

                if (AcbHelper.CompareSegment(awbMd5Calculated, 0, StreamAwbHash)) {
                    afs2 = new CriAfs2Archive(fs.Name, fs, 0);
                } else {
                    throw new FormatException(string.Format("AWB file, <{0}>, did not match MD5 checksum inside ACB file.", Path.GetFileName(fs.Name)));
                }
            }

            return afs2;
        }

        private static ushort GetWaveformRowIndexForWaveformId(CriUtfTable utfTable, ushort waveformId) {
            var ret = ushort.MaxValue;
            for (var i = 0; i < utfTable.NumberOfRows; i++) {
                var tempId = GetUtfFieldForRow<ushort>(utfTable, i, "Id");
                if (tempId == waveformId) {
                    ret = (ushort)i;
                }
            }
            return ret;
        }

        private static string GetFileExtensionForEncodeType(byte encodeType) {
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
                    ext = string.Format(".et-{0}.bin", encodeType.ToString("D2"));
                    break;
            }
            return ext;
        }

        private Dictionary<string, ushort> _cueNamesToWaveforms;
        private CriAcbCueRecord[] _cueList;
        private readonly CriAfs2Archive _internalAwb;
        private readonly CriAfs2Archive _externalAwb;

    }
}