using System;
using System.IO;
using System.Text;

namespace DereTore.ACB {
    internal static class StreamExtensions {

        public static sbyte ReadSByte(this Stream stream, long offset) {
            var position = stream.Position;
            stream.Seek(offset, SeekOrigin.Begin);
            var value = stream.ReadByte();
            stream.Position = position;
            unchecked {
                return (sbyte)value;
            }
        }

        public static byte ReadByte(this Stream stream, long offset) {
            var position = stream.Position;
            stream.Seek(offset, SeekOrigin.Begin);
            var value = stream.ReadByte();
            stream.Position = position;
            return (byte)value;
        }

        public static short ReadInt16BE(this Stream stream, long offset) {
            return (short)stream.ReadNumber(offset, 2, false, false);
        }

        public static short ReadInt16LE(this Stream stream, long offset) {
            return (short)stream.ReadNumber(offset, 2, false, true);
        }

        public static ushort ReadUInt16BE(this Stream stream, long offset) {
            return (ushort)stream.ReadNumber(offset, 2, true, false);
        }

        public static ushort ReadUInt16LE(this Stream stream, long offset) {
            return (ushort)stream.ReadNumber(offset, 2, true, true);
        }

        public static int ReadInt32BE(this Stream stream, long offset) {
            return (int)stream.ReadNumber(offset, 4, false, false);
        }

        public static int ReadInt32LE(this Stream stream, long offset) {
            return (int)stream.ReadNumber(offset, 4, false, true);
        }

        public static uint ReadUInt32BE(this Stream stream, long offset) {
            return (uint)stream.ReadNumber(offset, 4, true, false);
        }

        public static uint ReadUInt32LE(this Stream stream, long offset) {
            return (uint)stream.ReadNumber(offset, 4, true, true);
        }

        public static long ReadInt64BE(this Stream stream, long offset) {
            return (long)stream.ReadNumber(offset, 8, false, false);
        }

        public static long ReadInt64LE(this Stream stream, long offset) {
            return (long)stream.ReadNumber(offset, 8, false, true);
        }

        public static ulong ReadUInt64BE(this Stream stream, long offset) {
            return (ulong)stream.ReadNumber(offset, 8, true, false);
        }

        public static ulong ReadUInt64LE(this Stream stream, long offset) {
            return (ulong)stream.ReadNumber(offset, 8, true, true);
        }

        public static float ReadSingleBE(this Stream stream, long offset) {
            return (ulong)stream.ReadNumber(offset, -1, false, false);
        }

        public static float ReadSingleLE(this Stream stream, long offset) {
            return (ulong)stream.ReadNumber(offset, -1, false, true);
        }

        public static double ReadDoubleBE(this Stream stream, long offset) {
            return (ulong)stream.ReadNumber(offset, -2, false, false);
        }

        public static double ReadDoubleLE(this Stream stream, long offset) {
            return (ulong)stream.ReadNumber(offset, -2, false, true);
        }

        public static string ReadAsciiString(this Stream inStream, long offset) {
            var streamLength = inStream.Length;
            var stringLength = 0;

            // move pointer
            inStream.Position = offset;

            // read until NULL
            for (var i = offset; i <= streamLength; i++) {
                var dummy = inStream.ReadByte();
                if (dummy > 0) {
                    stringLength++;
                } else {
                    break;
                }
            }

            var stringBytes = inStream.ParseSimpleOffset(offset, stringLength);
            var ret = Encoding.ASCII.GetString(stringBytes);
            return ret;
        }

        public static byte[] ParseSimpleOffset(this byte[] sourceArray, int startingOffset, int lengthToCut) {
            var ret = new byte[lengthToCut];
            uint j = 0;
            for (var i = startingOffset; i < startingOffset + lengthToCut; i++) {
                ret[j] = sourceArray[i];
                j++;
            }
            return ret;
        }

        public static byte[] ParseSimpleOffset(this Stream stream, int startingOffset, int lengthToCut) {
            var currentStreamPosition = stream.Position;
            stream.Seek(startingOffset, SeekOrigin.Begin);
            // TODO: Do not dispose?
            var reader = new BinaryReader(stream);
            var ret = reader.ReadBytes(lengthToCut);
            stream.Position = currentStreamPosition;
            return ret;
        }

        public static byte[] ParseSimpleOffset(this Stream stream, long startingOffset, int lengthToCut) {
            var currentStreamPosition = stream.Position;
            stream.Seek(startingOffset, SeekOrigin.Begin);
            // Do not dispose?
            var reader = new BinaryReader(stream);
            var ret = reader.ReadBytes(lengthToCut);
            stream.Position = currentStreamPosition;
            return ret;
        }

        private static decimal ReadNumber(this Stream stream, long offset, int byteCount, bool isUnsigned, bool isLittleEndian) {
            if (byteCount == 1) {
                stream.Seek(offset, SeekOrigin.Begin);
                return stream.ReadByte();
            }
            var realByteCount = byteCount == -1 ? 4 : (byteCount == -2 ? 8 : byteCount);
            var data = stream.ParseSimpleOffset(offset, realByteCount);
            if (BitConverter.IsLittleEndian != isLittleEndian) {
                Array.Reverse(data);
            }
            switch (byteCount) {
                case 2:
                    return isUnsigned ? (decimal)BitConverter.ToUInt16(data, 0) : BitConverter.ToInt16(data, 0);
                case 4:
                    return isUnsigned ? (decimal)BitConverter.ToUInt32(data, 0) : BitConverter.ToInt32(data, 0);
                case 8:
                    return isUnsigned ? (decimal)BitConverter.ToUInt64(data, 0) : BitConverter.ToInt64(data, 0);
                case -1:
                    return (decimal)BitConverter.ToSingle(data, 0);
                case -2:
                    return (decimal)BitConverter.ToDouble(data, 0);
                default:
                    throw new ArgumentOutOfRangeException("byteCount");
            }
        }

    }
}