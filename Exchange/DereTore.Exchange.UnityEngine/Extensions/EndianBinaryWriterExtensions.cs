using System.Text;
using DereTore.Common;

namespace DereTore.Exchange.UnityEngine.Extensions {
    public static class EndianBinaryWriterExtensions {

        public static void WriteAlignedUtf8String(this EndianBinaryWriter writer, string str) {
            var bytes = Encoding.UTF8.GetBytes(str);
            writer.Write(bytes);
            writer.AlignStream(4);
        }

        public static void WriteAsciiString(this EndianBinaryWriter writer, string str) {
            var bytes = Encoding.ASCII.GetBytes(str);
            writer.Write(bytes);
        }

        public static void WriteAsciiStringAndNull(this EndianBinaryWriter writer, string str) {
            writer.WriteAsciiString(str);
            writer.Write((byte)0);
        }

        public static void AlignStream(this EndianBinaryWriter writer, int alignment) {
            var pos = writer.BaseStream.Position;
            if (pos % alignment != 0) {
                writer.BaseStream.Position += alignment - (pos % alignment);
            }
        }

    }
}
