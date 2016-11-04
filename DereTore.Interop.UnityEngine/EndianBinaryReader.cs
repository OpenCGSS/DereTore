using System;
using System.IO;
using System.Text;

namespace DereTore.Interop.UnityEngine {
    public class EndianBinaryReader : BinaryReader {

        public EndianBinaryReader(Stream stream)
            : this(stream, EndianHelper.UnityDefaultEndian) {
        }

        public EndianBinaryReader(Stream stream, Endian endian)
            : base(stream) {
            Endian = endian;
        }

        public Endian Endian { get; internal set; }

        public long Position {
            get {
                return BaseStream.Position;
            }
            set {
                BaseStream.Position = value;
            }
        }

        public override byte ReadByte() {
            try {
                return base.ReadByte();
            } catch {
                return 0;
            }
        }

        public override short ReadInt16() {
            if (Endian != EndianHelper.SystemEndian) {
                var value = base.ReadInt16();
                value = DereToreHelper.SwapEndian(value);
                return value;
            } else {
                return base.ReadInt16();
            }
        }

        public override int ReadInt32() {
            if (Endian != EndianHelper.SystemEndian) {
                var value = base.ReadInt32();
                value = DereToreHelper.SwapEndian(value);
                return value;
            } else {
                return base.ReadInt32();
            }
        }

        public override long ReadInt64() {
            if (Endian != EndianHelper.SystemEndian) {
                var value = base.ReadInt64();
                value = DereToreHelper.SwapEndian(value);
                return value;
            } else {
                return base.ReadInt64();
            }
        }

        public override ushort ReadUInt16() {
            if (Endian != EndianHelper.SystemEndian) {
                var value = base.ReadUInt16();
                value = DereToreHelper.SwapEndian(value);
                return value;
            } else {
                return base.ReadUInt16();
            }
        }

        public override uint ReadUInt32() {
            if (Endian != EndianHelper.SystemEndian) {
                var value = base.ReadUInt32();
                value = DereToreHelper.SwapEndian(value);
                return value;
            } else {
                return base.ReadUInt32();
            }
        }

        public override ulong ReadUInt64() {
            if (Endian != EndianHelper.SystemEndian) {
                var value = base.ReadUInt64();
                value = DereToreHelper.SwapEndian(value);
                return value;
            } else {
                return base.ReadUInt64();
            }
        }

        public override float ReadSingle() {
            if (Endian != EndianHelper.SystemEndian) {
                var value = base.ReadSingle();
                value = DereToreHelper.SwapEndian(value);
                return value;
            } else {
                return base.ReadSingle();
            }
        }

        public override double ReadDouble() {
            if (Endian != EndianHelper.SystemEndian) {
                var value = base.ReadDouble();
                value = DereToreHelper.SwapEndian(value);
                return value;
            } else {
                return base.ReadDouble();
            }
        }

        public string ReadAlignedUtf8String(int length) {
            // crude failsafe
            if (length > 0 && length < (BaseStream.Length - BaseStream.Position)) {
                var stringData = new byte[length];
                Read(stringData, 0, length);
                //must verify strange characters in PS3
                var result = Encoding.UTF8.GetString(stringData);
                AlignStream(4);
                return result;
            } else {
                return string.Empty;
            }
        }

        public string ReadAsciiString(int length) {
            var bytes = ReadBytes(length);
            return Encoding.ASCII.GetString(bytes);
        }

        public string ReadAsciiStringToNull() {
            var stringBuilder = new StringBuilder();
            for (var i = 0; i < BaseStream.Length; i++) {
                var c = base.ReadByte();
                if (c == 0) {
                    break;
                }
                stringBuilder.Append((char)c);
            }
            return stringBuilder.ToString();
        }

        public void AlignStream(int alignment) {
            var pos = BaseStream.Position;
            if (pos % alignment != 0) {
                BaseStream.Position += alignment - (pos % alignment);
            }
        }

    }
}
