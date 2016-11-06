using System;
using System.IO;

namespace DereTore {
    public class EndianBinaryReader : BinaryReader {

        public EndianBinaryReader(Stream stream, Endian endian)
            : base(stream) {
            Endian = endian;
        }

        public Endian Endian { get; set; }

        public void Seek(long position, SeekOrigin origin) {
            switch (origin) {
                case SeekOrigin.Begin:
                    Position = position;
                    break;
                case SeekOrigin.Current:
                    Position += position;
                    break;
                case SeekOrigin.End:
                    Position = BaseStream.Length - position;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(origin), origin, null);
            }
        }

        public long Position {
            get { return BaseStream.Position; }
            set { BaseStream.Position = value; }
        }

        public override short ReadInt16() {
            if (Endian != SystemEndian.Type) {
                var value = base.ReadInt16();
                value = DereToreHelper.SwapEndian(value);
                return value;
            } else {
                return base.ReadInt16();
            }
        }

        public override int ReadInt32() {
            if (Endian != SystemEndian.Type) {
                var value = base.ReadInt32();
                value = DereToreHelper.SwapEndian(value);
                return value;
            } else {
                return base.ReadInt32();
            }
        }

        public override long ReadInt64() {
            if (Endian != SystemEndian.Type) {
                var value = base.ReadInt64();
                value = DereToreHelper.SwapEndian(value);
                return value;
            } else {
                return base.ReadInt64();
            }
        }

        public override ushort ReadUInt16() {
            if (Endian != SystemEndian.Type) {
                var value = base.ReadUInt16();
                value = DereToreHelper.SwapEndian(value);
                return value;
            } else {
                return base.ReadUInt16();
            }
        }

        public override uint ReadUInt32() {
            if (Endian != SystemEndian.Type) {
                var value = base.ReadUInt32();
                value = DereToreHelper.SwapEndian(value);
                return value;
            } else {
                return base.ReadUInt32();
            }
        }

        public override ulong ReadUInt64() {
            if (Endian != SystemEndian.Type) {
                var value = base.ReadUInt64();
                value = DereToreHelper.SwapEndian(value);
                return value;
            } else {
                return base.ReadUInt64();
            }
        }

        public override float ReadSingle() {
            if (Endian != SystemEndian.Type) {
                var value = base.ReadSingle();
                value = DereToreHelper.SwapEndian(value);
                return value;
            } else {
                return base.ReadSingle();
            }
        }

        public override double ReadDouble() {
            if (Endian != SystemEndian.Type) {
                var value = base.ReadDouble();
                value = DereToreHelper.SwapEndian(value);
                return value;
            } else {
                return base.ReadDouble();
            }
        }

    }
}
