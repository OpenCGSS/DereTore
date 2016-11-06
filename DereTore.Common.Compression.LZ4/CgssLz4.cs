using System.IO;
using LZ4;

namespace DereTore.Common.Compression.LZ4 {
    public static class CgssLz4 {

        public static byte[] Compress(byte[] data) {
            var compressedData = LZ4Codec.EncodeHC(data, 0, data.Length);
            var outputBuffer = new byte[compressedData.Length + HeaderSize];
            using (var memoryStream = new MemoryStream(outputBuffer)) {
                // CGSS LZ4 header
                memoryStream.WriteInt32LE(0x00000064);
                memoryStream.WriteInt32LE(data.Length);
                memoryStream.WriteInt32LE(compressedData.Length);
                memoryStream.WriteInt32LE(0x00000001);
                // File data
                memoryStream.WriteBytes(compressedData);
            }
            return outputBuffer;
        }

        public static byte[] Decompress(byte[] data) {
            int originalDataLength;
            using (var memoryStream = new MemoryStream(data, false)) {
                using (var reader = new EndianBinaryReader(memoryStream, Endian.LittleEndian)) {
                    reader.Seek(4, SeekOrigin.Begin);
                    originalDataLength = reader.ReadInt32();
                }
            }
            var decompressedData = LZ4Codec.Decode(data, HeaderSize, data.Length - HeaderSize, originalDataLength);
            return decompressedData;
        }

        private const int HeaderSize = 4 * sizeof(int);

    }
}
