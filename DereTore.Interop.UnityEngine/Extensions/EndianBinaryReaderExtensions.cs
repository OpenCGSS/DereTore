using System.Text;

namespace DereTore.Interop.UnityEngine.Extensions {
    public static class EndianBinaryReaderExtensions {

        public static string ReadAlignedUtf8String(this EndianBinaryReader reader, int length) {
            // crude failsafe
            if (length > 0 && length < (reader.BaseStream.Length - reader.BaseStream.Position)) {
                var stringData = new byte[length];
                reader.Read(stringData, 0, length);
                //must verify strange characters in PS3
                var result = Encoding.UTF8.GetString(stringData);
                reader.AlignStream(4);
                return result;
            } else {
                return string.Empty;
            }
        }

        public static string ReadAsciiString(this EndianBinaryReader reader, int length) {
            var bytes = reader.ReadBytes(length);
            return Encoding.ASCII.GetString(bytes);
        }

        public static string ReadAsciiStringToNull(this EndianBinaryReader reader) {
            var stringBuilder = new StringBuilder();
            for (var i = 0; i < reader.BaseStream.Length; i++) {
                var c = reader.ReadByte();
                if (c == 0) {
                    break;
                }
                stringBuilder.Append((char)c);
            }
            return stringBuilder.ToString();
        }

        public static void AlignStream(this EndianBinaryReader reader, int alignment) {
            var pos = reader.BaseStream.Position;
            if (pos % alignment != 0) {
                reader.BaseStream.Position += alignment - (pos % alignment);
            }
        }

    }
}
