using System;

namespace DereTore.Interop.UnityEngine {
    public static class EndianHelper {

        public static long ToInt64(byte[] bytes, int startIndex, Endian sourceEndian) {
            if (SystemEndian == sourceEndian) {
                return BitConverter.ToInt64(bytes, startIndex);
            } else {
                var b2 = (byte[])bytes.Clone();
                Array.Reverse(b2);
                return BitConverter.ToInt64(b2, 0);
            }
        }

        public static byte[] GetBytes(long value, Endian endian) {
            var r = BitConverter.GetBytes(value);
            if (SystemEndian != endian) {
                Array.Reverse(r);
            }
            return r;
        }

        public static readonly Endian SystemEndian = BitConverter.IsLittleEndian ? Endian.LittleEndian : Endian.BigEndian;
        public static readonly Endian UnityDefaultEndian = Endian.BigEndian;

    }
}
