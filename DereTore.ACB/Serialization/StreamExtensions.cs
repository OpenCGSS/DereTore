using System;
using System.IO;
using System.Text;

namespace DereTore.ACB.Serialization {
    internal static class StreamExtensions {

        public static byte[] Append(this byte[] data, byte[] toAppend) {
            var buffer = new byte[data.Length + toAppend.Length];
            data.CopyTo(buffer, 0);
            toAppend.CopyTo(buffer, data.Length);
            return buffer;
        }

        public static byte[] Append(this byte[] data, byte toAppend) {
            var buffer = new byte[data.Length + 1];
            data.CopyTo(buffer, 0);
            buffer[buffer.Length - 1] = toAppend;
            return buffer;
        }

        public static byte[] RoundUpTo(this byte[] data, int alignment) {
            var newLength = AcbHelper.RoundUpToAlignment(data.Length, alignment);
            var buffer = new byte[newLength];
            data.CopyTo(buffer, 0);
            return buffer;
        }

        public static byte[] RoundUpTo(this byte[] data, uint alignment) {
            var newLength = AcbHelper.RoundUpToAlignment(data.Length, alignment);
            var buffer = new byte[newLength];
            data.CopyTo(buffer, 0);
            return buffer;
        }

        public static void SeekAndWriteBytes(this Stream stream, byte[] data, long offset) {
            stream.Seek(offset, SeekOrigin.Begin);
            WriteBytes(stream, data);
        }

        public static void SeekAndWriteByte(this Stream stream, byte value, long offset) {
            stream.Seek(offset, SeekOrigin.Begin);
            WriteByte(stream, value);
        }

        public static void SeekAndWriteSByte(this Stream stream, sbyte value, long offset) {
            stream.Seek(offset, SeekOrigin.Begin);
            WriteSByte(stream, value);
        }

        public static void SeekAndWriteUInt16LE(this Stream stream, ushort value, long offset) {
            stream.Seek(offset, SeekOrigin.Begin);
            WriteUInt16LE(stream, value);
        }

        public static void SeekAndWriteUInt16BE(this Stream stream, ushort value, long offset) {
            stream.Seek(offset, SeekOrigin.Begin);
            WriteUInt16BE(stream, value);
        }

        public static void SeekAndWriteInt16LE(this Stream stream, short value, long offset) {
            stream.Seek(offset, SeekOrigin.Begin);
            WriteInt16LE(stream, value);
        }

        public static void SeekAndWriteInt16BE(this Stream stream, short value, long offset) {
            stream.Seek(offset, SeekOrigin.Begin);
            WriteInt16BE(stream, value);
        }

        public static void SeekAndWriteUInt32LE(this Stream stream, uint value, long offset) {
            stream.Seek(offset, SeekOrigin.Begin);
            WriteUInt32LE(stream, value);
        }

        public static void SeekAndWriteUInt32BE(this Stream stream, uint value, long offset) {
            stream.Seek(offset, SeekOrigin.Begin);
            WriteUInt32BE(stream, value);
        }

        public static void SeekAndWriteInt32LE(this Stream stream, int value, long offset) {
            stream.Seek(offset, SeekOrigin.Begin);
            WriteInt32LE(stream, value);
        }

        public static void SeekAndWriteInt32BE(this Stream stream, int value, long offset) {
            stream.Seek(offset, SeekOrigin.Begin);
            WriteInt32BE(stream, value);
        }

        public static void SeekAndWriteUInt64LE(this Stream stream, ulong value, long offset) {
            stream.Seek(offset, SeekOrigin.Begin);
            WriteUInt64LE(stream, value);
        }

        public static void SeekAndWriteUInt64BE(this Stream stream, ulong value, long offset) {
            stream.Seek(offset, SeekOrigin.Begin);
            WriteUInt64BE(stream, value);
        }

        public static void SeekAndWriteInt64LE(this Stream stream, long value, long offset) {
            stream.Seek(offset, SeekOrigin.Begin);
            WriteInt64LE(stream, value);
        }

        public static void SeekAndWriteInt64BE(this Stream stream, long value, long offset) {
            stream.Seek(offset, SeekOrigin.Begin);
            WriteInt64BE(stream, value);
        }

        public static void SeekAndWriteSingleLE(this Stream stream, float value, long offset) {
            stream.Seek(offset, SeekOrigin.Begin);
            WriteSingleLE(stream, value);
        }

        public static void SeekAndWriteSingleBE(this Stream stream, float value, long offset) {
            stream.Seek(offset, SeekOrigin.Begin);
            WriteSingleBE(stream, value);
        }

        public static void SeekAndWriteDoubleLE(this Stream stream, double value, long offset) {
            stream.Seek(offset, SeekOrigin.Begin);
            WriteDoubleLE(stream, value);
        }

        public static void SeekAndWriteDoubleBE(this Stream stream, double value, long offset) {
            stream.Seek(offset, SeekOrigin.Begin);
            WriteDoubleBE(stream, value);
        }

        public static void SeekAndWriteZeroEndedStringAsAscii(this Stream stream, string value, long offset) {
            stream.Seek(offset, SeekOrigin.Begin);
            WriteZeroEndedStringAsAscii(stream, value);
        }

        public static void SeekAndWriteZeroEndedStringAsUtf8(this Stream stream, string value, long offset) {
            stream.Seek(offset, SeekOrigin.Begin);
            WriteZeroEndedStringAsUtf8(stream, value);
        }

        public static void WriteBytes(this Stream stream, byte[] data) {
            stream.Write(data, 0, data.Length);
        }

        public static void WriteByte(this Stream stream, byte value) {
            stream.WriteByte(value);
        }

        public static void WriteSByte(this Stream stream, sbyte value) {
            unchecked {
                stream.WriteByte((byte)value);
            }
        }

        public static void WriteUInt16LE(this Stream stream, ushort value) {
            var bytes = BitConverter.GetBytes(value);
            if (!BitConverter.IsLittleEndian) {
                Array.Reverse(bytes);
            }
            WriteBytes(stream, bytes);
        }

        public static void WriteUInt16BE(this Stream stream, ushort value) {
            var bytes = BitConverter.GetBytes(value);
            if (BitConverter.IsLittleEndian) {
                Array.Reverse(bytes);
            }
            WriteBytes(stream, bytes);
        }

        public static void WriteInt16LE(this Stream stream, short value) {
            var bytes = BitConverter.GetBytes(value);
            if (!BitConverter.IsLittleEndian) {
                Array.Reverse(bytes);
            }
            WriteBytes(stream, bytes);
        }

        public static void WriteInt16BE(this Stream stream, short value) {
            var bytes = BitConverter.GetBytes(value);
            if (BitConverter.IsLittleEndian) {
                Array.Reverse(bytes);
            }
            WriteBytes(stream, bytes);
        }

        public static void WriteUInt32LE(this Stream stream, uint value) {
            var bytes = BitConverter.GetBytes(value);
            if (!BitConverter.IsLittleEndian) {
                Array.Reverse(bytes);
            }
            WriteBytes(stream, bytes);
        }

        public static void WriteUInt32BE(this Stream stream, uint value) {
            var bytes = BitConverter.GetBytes(value);
            if (BitConverter.IsLittleEndian) {
                Array.Reverse(bytes);
            }
            WriteBytes(stream, bytes);
        }

        public static void WriteInt32LE(this Stream stream, int value) {
            var bytes = BitConverter.GetBytes(value);
            if (!BitConverter.IsLittleEndian) {
                Array.Reverse(bytes);
            }
            WriteBytes(stream, bytes);
        }

        public static void WriteInt32BE(this Stream stream, int value) {
            var bytes = BitConverter.GetBytes(value);
            if (BitConverter.IsLittleEndian) {
                Array.Reverse(bytes);
            }
            WriteBytes(stream, bytes);
        }

        public static void WriteUInt64LE(this Stream stream, ulong value) {
            var bytes = BitConverter.GetBytes(value);
            if (!BitConverter.IsLittleEndian) {
                Array.Reverse(bytes);
            }
            WriteBytes(stream, bytes);
        }

        public static void WriteUInt64BE(this Stream stream, ulong value) {
            var bytes = BitConverter.GetBytes(value);
            if (BitConverter.IsLittleEndian) {
                Array.Reverse(bytes);
            }
            WriteBytes(stream, bytes);
        }

        public static void WriteInt64LE(this Stream stream, long value) {
            var bytes = BitConverter.GetBytes(value);
            if (!BitConverter.IsLittleEndian) {
                Array.Reverse(bytes);
            }
            WriteBytes(stream, bytes);
        }

        public static void WriteInt64BE(this Stream stream, long value) {
            var bytes = BitConverter.GetBytes(value);
            if (BitConverter.IsLittleEndian) {
                Array.Reverse(bytes);
            }
            WriteBytes(stream, bytes);
        }

        public static void WriteSingleLE(this Stream stream, float value) {
            var bytes = BitConverter.GetBytes(value);
            if (!BitConverter.IsLittleEndian) {
                Array.Reverse(bytes);
            }
            WriteBytes(stream, bytes);
        }

        public static void WriteSingleBE(this Stream stream, float value) {
            var bytes = BitConverter.GetBytes(value);
            if (BitConverter.IsLittleEndian) {
                Array.Reverse(bytes);
            }
            WriteBytes(stream, bytes);
        }

        public static void WriteDoubleLE(this Stream stream, double value) {
            var bytes = BitConverter.GetBytes(value);
            if (!BitConverter.IsLittleEndian) {
                Array.Reverse(bytes);
            }
            WriteBytes(stream, bytes);
        }

        public static void WriteDoubleBE(this Stream stream, double value) {
            var bytes = BitConverter.GetBytes(value);
            if (BitConverter.IsLittleEndian) {
                Array.Reverse(bytes);
            }
            WriteBytes(stream, bytes);
        }

        public static void WriteZeroEndedStringAsAscii(this Stream stream, string value) {
            WriteZeroEndedString(stream, value, Encoding.ASCII);
        }

        public static void WriteZeroEndedStringAsUtf8(this Stream stream, string value) {
            WriteZeroEndedString(stream, value, Encoding.UTF8);
        }

        private static void WriteZeroEndedString(this Stream stream, string value, Encoding encoding) {
            if (value != null) {
                var bytes = encoding.GetBytes(value);
                WriteBytes(stream, bytes);
            }
            stream.WriteByte(0);
        }

    }
}
