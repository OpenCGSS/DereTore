using System;
using System.Runtime.InteropServices;

namespace DereTore.Interop.PVRTexLib {
    internal static class NativeMethods {

        public static IntPtr MpvrCompressPvrTexture(IntPtr pData, int width, int height, int stride, int mipLevels, PixelType pixelType, [MarshalAs(UnmanagedType.Bool)] bool isPremultiplied, out IntPtr ppDataSizes) {
            if (Is64Bit) {
                return X64.MpvrCompressPvrTexture(pData, width, height, stride, mipLevels, pixelType, isPremultiplied, out ppDataSizes);
            } else {
                return X86.MpvrCompressPvrTexture(pData, width, height, stride, mipLevels, pixelType, isPremultiplied, out ppDataSizes);
            }
        }

        public static bool MpvrCompressPvrTextureFrom32bppArgb(IntPtr pData, int width, int height, int stride, int mipLevels, out IntPtr pTextureData, out int textureDataSize) {
            if (Is64Bit) {
                return X64.MpvrCompressPvrTextureFrom32bppArgb(pData, width, height, stride, mipLevels, out pTextureData, out textureDataSize);
            } else {
                return X86.MpvrCompressPvrTextureFrom32bppArgb(pData, width, height, stride, mipLevels, out pTextureData, out textureDataSize);
            }
        }

        public static void MpvrFreeTexture(IntPtr pTextureData) {
            if (Is64Bit) {
                X64.MpvrFreeTexture(pTextureData);
            } else {
                X86.MpvrFreeTexture(pTextureData);
            }
        }

        private static bool Is64Bit => Environment.Is64BitProcess;

        private static class X86 {

            [DllImport(DllName, CallingConvention = CallingConvention.StdCall)]
            public static extern IntPtr MpvrCompressPvrTexture(IntPtr pData, int width, int height, int stride, int mipLevels, PixelType pixelType, [MarshalAs(UnmanagedType.Bool)] bool isPremultiplied, out IntPtr ppDataSizes);

            [DllImport(DllName, CallingConvention = CallingConvention.StdCall)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool MpvrCompressPvrTextureFrom32bppArgb(IntPtr pData, int width, int height, int stride, int mipLevels, out IntPtr pTextureData, out int textureDataSize);

            [DllImport(DllName, CallingConvention = CallingConvention.StdCall)]
            public static extern void MpvrFreeTexture(IntPtr pTextureData);

            private const string DllName = "x86/PVRTexLibW.dll";

        }

        private static class X64 {

            [DllImport(DllName, CallingConvention = CallingConvention.StdCall)]
            public static extern IntPtr MpvrCompressPvrTexture(IntPtr pData, int width, int height, int stride, int mipLevels, PixelType pixelType, [MarshalAs(UnmanagedType.Bool)] bool isPremultiplied, out IntPtr ppDataSizes);

            [DllImport(DllName, CallingConvention = CallingConvention.StdCall)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool MpvrCompressPvrTextureFrom32bppArgb(IntPtr pData, int width, int height, int stride, int mipLevels, out IntPtr pTextureData, out int textureDataSize);

            [DllImport(DllName, CallingConvention = CallingConvention.StdCall)]
            public static extern void MpvrFreeTexture(IntPtr pTextureData);

            private const string DllName = "x64/PVRTexLibW.dll";

        }

    }
}
