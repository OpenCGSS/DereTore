using System;
using System.IO;
using System.Text;

namespace DereTore.Interop.UnityEngine {
    public sealed class EndianBinaryWriter : BinaryWriter {

        public EndianBinaryWriter(Stream stream)
            : this(stream, EndianHelper.UnityDefaultEndian) {
        }

        public EndianBinaryWriter(Stream stream, Endian endian)
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

        public override void Write(short value) {
            if (Endian != EndianHelper.SystemEndian) {
                value = DereToreHelper.SwapEndian(value);
                base.Write(value);
            } else {
                base.Write(value);
            }
        }

        public override void Write(int value) {
            if (Endian != EndianHelper.SystemEndian) {
                value = DereToreHelper.SwapEndian(value);
                base.Write(value);
            } else {
                base.Write(value);
            }
        }

        public override void Write(long value) {
            if (Endian != EndianHelper.SystemEndian) {
                value = DereToreHelper.SwapEndian(value);
                base.Write(value);
            } else {
                base.Write(value);
            }
        }

        public override void Write(ushort value) {
            if (Endian != EndianHelper.SystemEndian) {
                value = DereToreHelper.SwapEndian(value);
                base.Write(value);
            } else {
                base.Write(value);
            }
        }

        public override void Write(uint value) {
            if (Endian != EndianHelper.SystemEndian) {
                value = DereToreHelper.SwapEndian(value);
                base.Write(value);
            } else {
                base.Write(value);
            }
        }

        public override void Write(ulong value) {
            if (Endian != EndianHelper.SystemEndian) {
                value = DereToreHelper.SwapEndian(value);
                base.Write(value);
            } else {
                base.Write(value);
            }
        }

        public override void Write(float value) {
            if (Endian != EndianHelper.SystemEndian) {
                value = DereToreHelper.SwapEndian(value);
                base.Write(value);
            } else {
                base.Write(value);
            }
        }

        public override void Write(double value) {
            if (Endian != EndianHelper.SystemEndian) {
                value = DereToreHelper.SwapEndian(value);
                base.Write(value);
            } else {
                base.Write(value);
            }
        }

        public void WriteAlignedUtf8String(string str) {
            var bytes = Encoding.UTF8.GetBytes(str);
            Write(bytes);
            AlignStream(4);
        }

        public void WriteAsciiString(string str) {
            var bytes = Encoding.ASCII.GetBytes(str);
            Write(bytes);
        }

        public void WriteAsciiStringAndNull(string str) {
            WriteAsciiString(str);
            Write((byte)0);
        }

        public void AlignStream(int alignment) {
            var pos = BaseStream.Position;
            if (pos % alignment != 0) {
                BaseStream.Position += alignment - (pos % alignment);
            }
        }

    }
}
