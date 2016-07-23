using System;
using System.IO;
using System.Text;

namespace DereTore.ACB {
    internal static class StreamExtensions {

        public static sbyte PeekSByte(this Stream stream, long offset) {
            unchecked {
                return (sbyte)PeekByte(stream, offset);
            }
        }

        public static byte PeekByte(this Stream stream, long offset) {
            var position = stream.Position;
            stream.Seek(offset, SeekOrigin.Begin);
            var value = stream.ReadByte();
            stream.Position = position;
            return (byte)value;
        }

        public static short PeekInt16BE(this Stream stream, long offset) {
            var bytes = GetNumberBytes(stream, offset, sizeof(short), false);
            return BitConverter.ToInt16(bytes, 0);
        }

        public static short PeekInt16LE(this Stream stream, long offset) {
            var bytes = GetNumberBytes(stream, offset, sizeof(short), true);
            return BitConverter.ToInt16(bytes, 0);
        }

        public static ushort PeekUInt16BE(this Stream stream, long offset) {
            var bytes = GetNumberBytes(stream, offset, sizeof(ushort), false);
            return BitConverter.ToUInt16(bytes, 0);
        }

        public static ushort PeekUInt16LE(this Stream stream, long offset) {
            var bytes = GetNumberBytes(stream, offset, sizeof(ushort), true);
            return BitConverter.ToUInt16(bytes, 0);
        }

        public static int PeekInt32BE(this Stream stream, long offset) {
            var bytes = GetNumberBytes(stream, offset, sizeof(int), false);
            return BitConverter.ToInt32(bytes, 0);
        }

        public static int PeekInt32LE(this Stream stream, long offset) {
            var bytes = GetNumberBytes(stream, offset, sizeof(int), true);
            return BitConverter.ToInt32(bytes, 0);
        }

        public static uint PeekUInt32BE(this Stream stream, long offset) {
            var bytes = GetNumberBytes(stream, offset, sizeof(uint), false);
            return BitConverter.ToUInt32(bytes, 0);
        }

        public static uint PeekUInt32LE(this Stream stream, long offset) {
            var bytes = GetNumberBytes(stream, offset, sizeof(uint), true);
            return BitConverter.ToUInt32(bytes, 0);
        }

        public static long PeekInt64BE(this Stream stream, long offset) {
            var bytes = GetNumberBytes(stream, offset, sizeof(long), false);
            return BitConverter.ToInt64(bytes, 0);
        }

        public static long PeekInt64LE(this Stream stream, long offset) {
            var bytes = GetNumberBytes(stream, offset, sizeof(long), true);
            return BitConverter.ToInt64(bytes, 0);
        }

        public static ulong PeekUInt64BE(this Stream stream, long offset) {
            var bytes = GetNumberBytes(stream, offset, sizeof(ulong), false);
            return BitConverter.ToUInt64(bytes, 0);
        }

        public static ulong PeekUInt64LE(this Stream stream, long offset) {
            var bytes = GetNumberBytes(stream, offset, sizeof(ulong), true);
            return BitConverter.ToUInt64(bytes, 0);
        }

        public static float PeekSingleBE(this Stream stream, long offset) {
            var bytes = GetNumberBytes(stream, offset, sizeof(float), false);
            return BitConverter.ToSingle(bytes, 0);
        }

        public static float PeekSingleLE(this Stream stream, long offset) {
            var bytes = GetNumberBytes(stream, offset, sizeof(float), true);
            return BitConverter.ToSingle(bytes, 0);
        }

        public static double PeekDoubleBE(this Stream stream, long offset) {
            var bytes = GetNumberBytes(stream, offset, sizeof(double), false);
            return BitConverter.ToDouble(bytes, 0);
        }

        public static double PeekDoubleLE(this Stream stream, long offset) {
            var bytes = GetNumberBytes(stream, offset, sizeof(double), true);
            return BitConverter.ToDouble(bytes, 0);
        }

        public static string PeekZeroEndedString(this Stream stream, long offset, Encoding encoding) {
            var streamLength = stream.Length;
            var stringLength = 0;
            var originalPosition = stream.Position;
            stream.Seek(offset, SeekOrigin.Begin);
            for (var i = offset; i <= streamLength; i++) {
                var dummy = stream.ReadByte();
                if (dummy > 0) {
                    stringLength++;
                } else {
                    break;
                }
            }
            var stringBytes = PeekBytes(stream, offset, stringLength);
            var ret = encoding.GetString(stringBytes);
            stream.Position = originalPosition;
            return ret;
        }

        public static string PeekZeroEndedStringAsAscii(this Stream stream, long offset) {
            return PeekZeroEndedString(stream, offset, Encoding.ASCII);
        }

        public static string PeekZeroEndedStringAsUtf8(this Stream stream, long offset) {
            return PeekZeroEndedString(stream, offset, Encoding.UTF8);
        }

        public static byte[] ReadBytes(byte[] array, int offset, int length) {
            var ret = new byte[length];
            Array.Copy(array, offset, ret, 0, length);
            return ret;
        }

        public static byte[] PeekBytes(this Stream stream, int offset, int length) {
            return PeekBytes(stream, (long)offset, length);
        }

        public static byte[] PeekBytes(this Stream stream, long offset, int length) {
            var originalPosition = stream.Position;
            stream.Seek(offset, SeekOrigin.Begin);
            var finalBuffer = new byte[length];
            var secondBuffer = new byte[length];
            var bytesLeft = length;
            var currentIndex = 0;
            do {
                var read = stream.Read(secondBuffer, 0, bytesLeft);
                Array.Copy(secondBuffer, 0, finalBuffer, currentIndex, read);
                bytesLeft -= read;
                currentIndex += read;
            } while (bytesLeft > 0);
            stream.Position = originalPosition;
            return finalBuffer;
        }

        private static byte[] GetNumberBytes(Stream stream, long offset, int byteCount, bool isLittleEndian) {
            var data = PeekBytes(stream, offset, byteCount);
            if (BitConverter.IsLittleEndian != isLittleEndian) {
                Array.Reverse(data);
            }
            return data;
        }

    }
}
