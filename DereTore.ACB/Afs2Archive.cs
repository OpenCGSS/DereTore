﻿using System;
using System.Collections.Generic;
using System.IO;

namespace DereTore.ACB {
    public sealed class Afs2Archive : DisposableBase {

        public Afs2Archive(Stream stream, long offset, string fileName, bool disposeStream) {
            _fileName = fileName;
            _stream = stream;
            _streamOffset = offset;
            _disposeStream = disposeStream;
        }

        public void Initialize() {
            var stream = _stream;
            var offset = _streamOffset;
            var acbFileName = _fileName;
            if (!IsAfs2Archive(stream, offset)) {
                throw new FormatException(string.Format("File '{0}' does not contain a valid AFS2 archive at offset {1}.", acbFileName, offset));
            }
            var fileCount = (int)stream.PeekUInt32LE(offset + 8);
            if (fileCount > ushort.MaxValue) {
                throw new IndexOutOfRangeException(string.Format("File count {0} exceeds maximum possible value (65535).", fileCount));
            }
            var files = new Dictionary<int, Afs2FileRecord>(fileCount);
            _files = files;
            var byteAlignment = stream.PeekUInt32LE(offset + 12);
            _byteAlignment = byteAlignment;
            var version = stream.PeekUInt32LE(offset + 4);
            _version = version;
            var offsetFieldSize = (int)(version >> 8) & 0xff; // versionBytes[1]
            uint offsetMask = 0;
            for (var j = 0; j < offsetFieldSize; j++) {
                offsetMask |= (uint)(0xff << (j * 8));
            }

            const int invalidCueId = -1;
            var previousCueId = invalidCueId;
            for (ushort i = 0; i < fileCount; ++i) {
                var record = new Afs2FileRecord {
                    CueId = stream.PeekUInt16LE(offset + (0x10 + 2 * i)),
                    FileOffsetRaw = stream.PeekUInt32LE(offset + (0x10 + fileCount * 2 + offsetFieldSize * i))
                };
                record.FileOffsetRaw &= offsetMask;
                record.FileOffsetRaw += offset;
                record.FileOffsetAligned = (record.FileOffsetRaw % ByteAlignment) != 0 ? AcbHelper.RoundUpToAlignment(record.FileOffsetRaw, ByteAlignment) : record.FileOffsetRaw;
                if (i == fileCount - 1) {
                    record.FileLength = stream.PeekUInt32LE(offset + (0x10 + fileCount * 2 + offsetFieldSize * i) + offsetFieldSize) + offset - record.FileOffsetAligned;
                }
                if (previousCueId != invalidCueId) {
                    files[previousCueId].FileLength = record.FileOffsetRaw - files[previousCueId].FileOffsetAligned;
                }
                files.Add(record.CueId, record);
                previousCueId = record.CueId;
            }
        }

        public static bool IsAfs2Archive(Stream stream, long offset) {
            var fileSignature = stream.PeekBytes(offset, 4);
            return AcbHelper.AreDataIdentical(fileSignature, Afs2Signature);
        }

        public string FileName {
            get { return _fileName; }
        }

        public uint ByteAlignment {
            get { return _byteAlignment; }
        }

        public Dictionary<int, Afs2FileRecord> Files {
            get { return _files; }
        }

        public uint Version {
            get { return _version; }
        }

        protected override void Dispose(bool disposing) {
            if (_disposeStream) {
                try {
                    _stream.Dispose();
                } catch (ObjectDisposedException) {
                }
            }
        }

        private static readonly byte[] Afs2Signature = { 0x41, 0x46, 0x53, 0x32 }; // 'AFS2'

        private uint _byteAlignment;
        private Dictionary<int, Afs2FileRecord> _files;
        private uint _version;
        private readonly string _fileName;
        private readonly Stream _stream;
        private readonly long _streamOffset;
        private readonly bool _disposeStream;

    }
}