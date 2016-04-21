using System.IO;
using System.Security.Cryptography;

namespace DereTore.ACB {
    internal static class AcbHelper {

        public static bool CompareSegment(byte[] sourceArray, int offset, byte[] target) {
            var ret = true;
            uint j = 0;
            if (sourceArray.Length > 0) {
                for (var i = offset; i < target.Length; i++) {
                    if (sourceArray[i] != target[j]) {
                        ret = false;
                        break;
                    }
                    j++;
                }
            } else {
                ret = false;
            }
            return ret;
        }

        public static bool CompareSegment(byte[] sourceArray, long offset, byte[] target) {
            var ret = true;
            uint j = 0;
            for (var i = offset; i < target.Length; i++) {
                if (sourceArray[i] != target[j]) {
                    ret = false;
                    break;
                }
                j++;
            }
            return ret;
        }

        public static long RoundUpToByteAlignment(long valueToRound, long byteAlignment) {
            var roundedValue = (valueToRound + byteAlignment - 1) / byteAlignment * byteAlignment;
            return roundedValue;
        }

        public static ulong RoundUpToByteAlignment(ulong valueToRound, ulong byteAlignment) {
            var roundedValue = (valueToRound + byteAlignment - 1) / byteAlignment * byteAlignment;
            return roundedValue;
        }

        public static MemoryStream ExtractChunkToStream(Stream inStream, ulong startingOffset, ulong length) {
            var memory = new MemoryStream();
            // Do not dispose?
            var writer = new BinaryWriter(memory);
            int read;
            ulong totalBytes = 0;
            var bytes = new byte[Constants.FileChunkSize];

            inStream.Seek(0, SeekOrigin.Begin);

            while (startingOffset > long.MaxValue) {
                inStream.Seek(long.MaxValue, SeekOrigin.Current);
                startingOffset -= long.MaxValue;
            }

            inStream.Seek((long)startingOffset, SeekOrigin.Current);

            var maxRead = length > (ulong)bytes.Length ? bytes.Length : (int)length;

            while ((read = inStream.Read(bytes, 0, maxRead)) > 0) {
                writer.Write(bytes, 0, read);
                totalBytes += (ulong)read;
                maxRead = (length - totalBytes) > (ulong)bytes.Length ? bytes.Length : (int)(length - totalBytes);
            }
            memory.Seek(0, SeekOrigin.Begin);
            memory.Capacity = (int)memory.Length;
            return memory;
        }

        public static byte[] GetMd5Checksum(Stream stream) {
            var position = stream.Position;
            stream.Seek(0, SeekOrigin.Begin);

            byte[] checksum;
            using (var ms = new MemoryStream()) {
                var buffer = new byte[Constants.FileChunkSize];
                int read;
                do {
                    read = stream.Read(buffer, 0, buffer.Length);
                    if (read > 0) {
                        ms.Write(buffer, 0, read);
                    }
                } while (read > 0);
                ms.Capacity = (int)ms.Length;
                var bytes = ms.GetBuffer();
                checksum = _md5.ComputeHash(bytes);
            }
            stream.Position = position;
            return checksum;
        }

        private static MD5 _md5 = new MD5CryptoServiceProvider();

    }
}