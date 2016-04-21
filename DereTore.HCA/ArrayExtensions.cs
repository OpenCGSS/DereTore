using System;
using System.Runtime.InteropServices;

namespace DereTore.HCA {
    internal static class ArrayExtensions {

        public static void ZeroMem(this int[] array) {
            if (array == null) {
                return;
            }
            for (var i = 0; i < array.Length; ++i) {
                array[i] = 0;
            }
        }

        public static void ZeroMem(this uint[] array) {
            if (array == null) {
                return;
            }
            for (var i = 0; i < array.Length; ++i) {
                array[i] = 0;
            }
        }

        public static void ZeroMem(this byte[] array) {
            if (array == null) {
                return;
            }
            for (var i = 0; i < array.Length; ++i) {
                array[i] = 0;
            }
        }

        public static void ZeroMem(this float[] array) {
            if (array == null) {
                return;
            }
            for (var i = 0; i < array.Length; ++i) {
                array[i] = 0f;
            }
        }

        public static int Write(this byte[] array, uint value, int index) {
            if (!BitConverter.IsLittleEndian) {
                value = HcaHelper.SwapEndian(value);
            }
            var bytes = BitConverter.GetBytes(value);
            for (var i = 0; i < 4; ++i) {
                array[index + i] = bytes[i];
            }
            return 4;
        }

        public static int Write<T>(this byte[] array, T value, int index) where T : struct {
            var size = Marshal.SizeOf(typeof(T));
            var bytes = new byte[size];
            var ptr = Marshal.AllocHGlobal(size);
            Marshal.StructureToPtr(value, ptr, true);
            Marshal.Copy(ptr, bytes, 0, size);
            Marshal.FreeHGlobal(ptr);
            Array.Copy(bytes, 0, array, index, size);
            return size;
        }

        public static int Write(this byte[] array, byte[] value, int index) {
            var length = value.Length;
            Array.Copy(value, 0, array, index, length);
            return length;
        }

    }
}
