using System;
using System.IO;

namespace DereTore.HCA {
    internal class WaveHelper {

        public static int DecodeToStreamInR32(float f, Stream stream) {
            return stream.Write(f);
        }

        public static int DecodeToStreamInS16(float f, Stream stream) {
            return stream.Write((short)(f * 0x7fff));
        }

        public static int DecodeToStreamInS32(float f, Stream stream) {
            return stream.Write((int)(f * 0x7fffffff));
        }

        public static int DecodeToBufferInR32(float f, byte[] buffer, int startIndex) {
            if (!BitConverter.IsLittleEndian) {
                f = HcaHelper.SwapEndian(f);
            }
            var bytes = BitConverter.GetBytes(f);
            for (var i = 0; i < 4; ++i) {
                buffer[startIndex + i] = bytes[i];
            }
            return 4;
        }

        public static int DecodeToBufferInS16(float f, byte[] buffer, int startIndex) {
            var value = (short)(f * 0x7fff);
            if (!BitConverter.IsLittleEndian) {
                value = HcaHelper.SwapEndian(value);
            }
            var bytes = BitConverter.GetBytes(value);
            for (var i = 0; i < 2; ++i) {
                buffer[startIndex + i] = bytes[i];
            }
            return 2;
        }

    }
}