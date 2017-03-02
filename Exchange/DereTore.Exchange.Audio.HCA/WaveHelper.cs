using System;
using System.IO;
using DereTore.Common;

namespace DereTore.Exchange.Audio.HCA {
    public static class WaveHelper {

        public static readonly IWaveWriter U8 = new WaveWriterU8();
        public static readonly IWaveWriter S16 = new WaveWriterS16();
        public static readonly IWaveWriter S32 = new WaveWriterS32();
        public static readonly IWaveWriter R32 = new WaveWriterR32();

        private class WaveWriterU8 : IWaveWriter {

            public uint BytesPerSample => 1;

            public SamplingMode SamplingMode => SamplingMode.U8;

            public uint DecodeToBuffer(float f, byte[] buffer, uint offset) {
                if (offset >= buffer.Length) {
                    throw new ArgumentOutOfRangeException(nameof(offset));
                }
                var s = (sbyte)((int)(f * 0xff) - 0x80);
                unchecked {
                    buffer[offset] = (byte)s;
                }
                return 1;
            }

            public uint DecodeToStream(float f, Stream stream) {
                return (uint)stream.Write((sbyte)((int)(f * 0xff) - 0x80));
            }

        }

        private class WaveWriterS16 : IWaveWriter {

            public uint BytesPerSample => 2;

            public SamplingMode SamplingMode => SamplingMode.S16;

            public uint DecodeToBuffer(float f, byte[] buffer, uint offset) {
                if (offset >= buffer.Length) {
                    throw new ArgumentOutOfRangeException(nameof(offset));
                }
                var value = (short)(f * 0x7fff);
                if (!BitConverter.IsLittleEndian) {
                    value = DereToreHelper.SwapEndian(value);
                }
                var bytes = BitConverter.GetBytes(value);
                var bytesWritten = 0u;
                for (var i = 0; i < 2; ++i) {
                    if (offset + i > buffer.Length) {
                        break;
                    }
                    buffer[offset + i] = bytes[i];
                    ++bytesWritten;
                }
                return bytesWritten;
            }

            public uint DecodeToStream(float f, Stream stream) {
                return (uint)stream.Write((short)(f * 0x7fff));
            }
        }

        private class WaveWriterS32 : IWaveWriter {

            public uint BytesPerSample => 4;

            public SamplingMode SamplingMode => SamplingMode.S32;

            public uint DecodeToBuffer(float f, byte[] buffer, uint offset) {
                if (offset >= buffer.Length) {
                    throw new ArgumentOutOfRangeException(nameof(offset));
                }
                var value = (short)(f * 0x7fffffff);
                if (!BitConverter.IsLittleEndian) {
                    value = DereToreHelper.SwapEndian(value);
                }
                var bytes = BitConverter.GetBytes(value);
                var bytesWritten = 0u;
                for (var i = 0; i < 4; ++i) {
                    if (offset + i >= buffer.Length) {
                        break;
                    }
                    buffer[offset + i] = bytes[i];
                    ++bytesWritten;
                }
                return bytesWritten;
            }

            public uint DecodeToStream(float f, Stream stream) {
                return (uint)stream.Write((short)(f * 0x7fffffff));
            }
        }

        private class WaveWriterR32 : IWaveWriter {

            public uint BytesPerSample => 4;

            public SamplingMode SamplingMode => SamplingMode.R32;

            public uint DecodeToBuffer(float f, byte[] buffer, uint offset) {
                if (offset >= buffer.Length) {
                    throw new ArgumentOutOfRangeException(nameof(offset));
                }
                if (!BitConverter.IsLittleEndian) {
                    f = DereToreHelper.SwapEndian(f);
                }
                var bytes = BitConverter.GetBytes(f);
                var bytesWritten = 0u;
                for (var i = 0; i < 4; ++i) {
                    if (offset + i >= buffer.Length) {
                        break;
                    }
                    buffer[offset + i] = bytes[i];
                    ++bytesWritten;
                }
                return bytesWritten;
            }

            public uint DecodeToStream(float f, Stream stream) {
                return (uint)stream.Write(f);
            }
        }

    }
}
