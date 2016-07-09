using System;
using System.IO;
using System.Runtime.InteropServices;

namespace DereTore.HCA {
    internal static class StreamExtensions {

        public static int Write(this Stream stream, int value) {
            if (!BitConverter.IsLittleEndian) {
                value = HcaHelper.SwapEndian(value);
            }
            stream.Write(BitConverter.GetBytes(value), 0, 4);
            return 4;
        }

        public static int Write(this Stream stream, short value) {
            if (!BitConverter.IsLittleEndian) {
                value = HcaHelper.SwapEndian(value);
            }
            stream.Write(BitConverter.GetBytes(value), 0, 2);
            return 2;
        }

        public static int Write(this Stream stream, float value) {
            if (!BitConverter.IsLittleEndian) {
                value = HcaHelper.SwapEndian(value);
            }
            stream.Write(BitConverter.GetBytes(value), 0, 4);
            return 4;
        }

        public static int Write<T>(this Stream stream, T value) where T : struct {
            var size = Marshal.SizeOf(typeof(T));
            var bytes = new byte[size];
            var ptr = Marshal.AllocHGlobal(size);
            Marshal.StructureToPtr(value, ptr, true);
            Marshal.Copy(ptr, bytes, 0, size);
            Marshal.FreeHGlobal(ptr);
            stream.Write(bytes, 0, size);
            return size;
        }

        public static void WriteRange<T>(this Stream stream, T[] value) where T : struct {
            var size = Marshal.SizeOf(typeof(T));
            var count = value.Length;
            var bytes = new byte[size * count];
            var ptr = Marshal.AllocHGlobal(size);
            var baseIndex = 0;
            for (var i = 0; i < count; ++i) {
                Marshal.StructureToPtr(value[i], ptr, true);
                stream.Write(bytes, 0, size);
                Marshal.Copy(ptr, bytes, baseIndex, size);
                baseIndex += size;
            }
            Marshal.FreeHGlobal(ptr);
        }

        public static int Read<T>(this Stream stream, out T value) where T : struct {
            var size = Marshal.SizeOf(typeof(T));
            var bytes = new byte[size];
            var bytesRead = stream.Read(bytes, 0, size);
            var ptr = Marshal.AllocHGlobal(size);
            Marshal.Copy(bytes, 0, ptr, size);
            value = (T)Marshal.PtrToStructure(ptr, typeof(T));
            Marshal.FreeHGlobal(ptr);
            return bytesRead;
        }

        public static T Read<T>(this Stream stream) where T : struct {
            T value;
            Read(stream, out value);
            return value;
        }

        public static uint PeekUInt32(this Stream stream) {
            var bytes = new byte[4];
            stream.Read(bytes, 0, 4);
            stream.Seek(-4, SeekOrigin.Current);
            return BitConverter.ToUInt32(bytes, 0);
        }

        public static int Skip(this Stream stream, int length) {
            var buffer = new byte[length];
            return stream.Read(buffer, 0, buffer.Length);
        }

    }
}