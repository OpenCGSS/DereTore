using System;
using System.Collections.Generic;
using System.IO;

namespace DereTore.ACB {
    public sealed class CriAfs2Archive {

        public CriAfs2Archive(string fileName, Stream stream, long offset) {
            var previousCueId = ushort.MaxValue;

            if (IsCriAfs2Archive(stream, offset)) {
                SourceFile = fileName;

                MagicBytes = stream.ParseSimpleOffset(offset, Signature.Length);
                Version = stream.ParseSimpleOffset(offset + 4, 4);
                FileCount = stream.ReadUInt32LE(offset + 8);

                // setup offset field size
                int offsetFieldSize = Version[1]; // known values: 2 and 4.  4 is most common.  I've only seen 2 in 'se_enemy_gurdon_galaga_bee.acb' from Sonic Lost World.
                uint offsetMask = 0;

                for (var j = 0; j < offsetFieldSize; j++) {
                    offsetMask |= (uint)(0xFF << (j * 8));
                }

                if (FileCount > ushort.MaxValue) {
                    throw new FormatException("ERROR, file count exceeds max value for ushort." + Environment.NewLine + fileName);
                }

                ByteAlignment = stream.ReadUInt32LE(offset + 0xC);
                Files = new Dictionary<ushort, CriAfs2File>((int)FileCount);

                for (ushort i = 0; i < FileCount; i++) {
                    var dummy = new CriAfs2File {
                        CueId = stream.ReadUInt16LE(offset + (0x10 + (2 * i))),
                        FileOffsetRaw = stream.ReadUInt32LE(offset + (0x10 + (FileCount * 2) + (offsetFieldSize * i)))
                    };
                    
                    // mask off unneeded info
                    dummy.FileOffsetRaw &= offsetMask;

                    // add offset
                    dummy.FileOffsetRaw += offset; // for AFS2 files inside of other files (ACB, etc.)

                    // set file offset to byte alignment
                    if ((dummy.FileOffsetRaw % ByteAlignment) != 0) {
                        dummy.FileOffsetByteAligned = AcbHelper.RoundUpToByteAlignment(dummy.FileOffsetRaw, ByteAlignment);
                    } else {
                        dummy.FileOffsetByteAligned = dummy.FileOffsetRaw;
                    }

                    //---------------
                    // set file size
                    //---------------
                    // last file will use final offset entry
                    if (i == FileCount - 1) {
                        dummy.FileLength = (stream.ReadUInt32LE(offset + (0x10 + (FileCount * 2) + ((offsetFieldSize) * i)) + offsetFieldSize) + offset) - dummy.FileOffsetByteAligned;
                    }

                    // else set length for previous cue id
                    if (previousCueId != ushort.MaxValue) {
                        Files[previousCueId].FileLength = dummy.FileOffsetRaw - Files[previousCueId].FileOffsetByteAligned;
                    }

                    Files.Add(dummy.CueId, dummy);
                    previousCueId = dummy.CueId;
                }
            } else {
                throw new FormatException(string.Format("AFS2 magic bytes not found at offset: 0x{0}.", offset.ToString("X8")));
            }
        }

        public static bool IsCriAfs2Archive(Stream stream, long offset) {
            var checkBytes = stream.ParseSimpleOffset(offset, Signature.Length);
            return AcbHelper.CompareSegment(checkBytes, 0, Signature);
        }

        private static readonly byte[] Signature = { 0x41, 0x46, 0x53, 0x32 };

        public string SourceFile {  get; private set; }
        public byte[] MagicBytes {  get; private set; }
        private byte[] Version { set; get; }
        public uint FileCount { private set; get; }
        public uint ByteAlignment { private set; get; }
        public Dictionary<ushort, CriAfs2File> Files { private set; get; }


    }
}