using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using SharpDX.Direct3D9;

namespace DereTore.Interop.D3DX9 {
    public static class DdsUtilities {

        public static byte[] GetDdsTextureFromImage(Bitmap bitmap) {
            return GetDdsTextureFromImage(bitmap, false);
        }

        public static byte[] GetDdsTextureFromImage(Bitmap bitmap, bool withHeader) {
            using (var d3d = new Direct3D()) {
                var pp = new PresentParameters(100, 100);

                pp.DeviceWindowHandle = IntPtr.Zero;
                pp.Windowed = true;
                pp.BackBufferCount = 1;
                pp.BackBufferFormat = Format.A8R8G8B8;
                pp.SwapEffect = SwapEffect.Discard;
                pp.PresentationInterval = PresentInterval.Default;

                var hWnd = IntPtr.Zero;

                try {
                    hWnd = CreateWindowEx(0, "STATIC", "dummy", 0, 0, 0, 100, 100, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero);

                    pp.DeviceWindowHandle = hWnd;

                    var ppRef = new[] { pp };

                    using (var bmp = (Bitmap)bitmap.Clone()) {
                        bmp.RotateFlip(RotateFlipType.RotateNoneFlipY);

                        using (var device = new Device(d3d, 0, DeviceType.Hardware, IntPtr.Zero, CreateFlags.MixedVertexProcessing, ppRef)) {
                            return GetDdsData(device, bmp, withHeader);
                        }
                    }
                } finally {
                    if (hWnd != IntPtr.Zero) {
                        DestroyWindow(hWnd);
                    }
                }
            }
        }

        public static readonly int DefaultDdsMipLevels = 8;

        private static byte[] GetDdsData(Device device, Bitmap bitmap, bool withHeader) {
            byte[] result;

            var argb = GetArgb8888(bitmap, out var stride, out var width, out var height);
            var rgb = Argb8888ToRgb565(argb, stride, width, height);

            using (var texture = new Texture(device, width, height, DefaultDdsMipLevels, Usage.None, Format.R5G6B5, Pool.Scratch)) {
                texture.LockRectangle(0, LockFlags.None, out var dataStream);

                dataStream.Write(rgb, 0, rgb.Length);

                texture.UnlockRectangle(0);

                using (var ds = BaseTexture.ToStream(texture, ImageFileFormat.Dds)) {
                    using (var memory = new MemoryStream()) {
                        var buffer = new byte[10240];
                        var read = 1;

                        while (read > 0) {
                            read = ds.Read(buffer, 0, buffer.Length);

                            if (read > 0) {
                                memory.Write(buffer, 0, read);
                            }
                        }

                        result = memory.ToArray();
                    }
                }
            }

            if (!withHeader) {
                const int ddsHeaderSize = 0x80;

                var newArray = new byte[result.Length - ddsHeaderSize];

                Array.Copy(result, ddsHeaderSize, newArray, 0, newArray.Length);

                result = newArray;
            }

            return result;
        }

        private static byte[] GetArgb8888(Bitmap bitmap, out int stride, out int width, out int height) {
            var bitmapData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);

            stride = bitmapData.Stride;
            width = bitmapData.Width;
            height = bitmapData.Height;

            var result = new byte[stride * height];

            unsafe {
                var scan0 = (byte*)bitmapData.Scan0.ToPointer();

                fixed (byte* pResult = result) {
                    for (var j = 0; j < height; ++j) {
                        var srcRowBegin = (uint*)(scan0 + j * stride);
                        var dstRowBegin = (uint*)(pResult + j * stride);

                        for (var i = 0; i < width; ++i) {
                            dstRowBegin[i] = srcRowBegin[i];
                        }
                    }
                }
            }

            bitmap.UnlockBits(bitmapData);

            return result;
        }

        private static unsafe byte[] Argb8888ToRgb565(byte[] data, int stride, int width, int height) {
            var newStride = width * sizeof(ushort);

            if (newStride % 4 != 0) {
                newStride += 4 - (newStride % 4);
            }

            var result = new byte[newStride * height];

            fixed (byte* pData = data) {
                fixed (byte* pResult = result) {
                    for (var j = 0; j < height; ++j) {
                        var srcRowBegin = pData + j * stride;
                        var dstRowBegin = pResult + j * newStride;

                        for (var i = 0; i < width; ++i) {
                            var argb = *(uint*)(srcRowBegin + sizeof(uint) * i);

                            var r = (byte)((argb & ArgbRedMask) >> 19);
                            var g = (byte)((argb & ArgbGreenMask) >> 10);
                            var b = (byte)((argb & ArgbBlueMask) >> 3);

                            *(ushort*)(dstRowBegin + sizeof(ushort) * i) = (ushort)(r << 11 | g << 5 | b);
                        }
                    }
                }
            }

            return result;
        }

        [DllImport("user32.dll", EntryPoint = "CreateWindowEx", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CreateWindowEx(uint dwExStyle, [MarshalAs(UnmanagedType.LPTStr)] string lpClassName, [MarshalAs(UnmanagedType.LPTStr)] string lpWindowName, uint dwStyle, int x, int y, int nWidth, int nHeight, IntPtr hWndParent, IntPtr hMenu, IntPtr hInstance, IntPtr lpParam);

        [DllImport("user32.dll", EntryPoint = "DestroyWindow", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool DestroyWindow(IntPtr hWnd);

        private const uint ArgbRedMask = 0x00ff0000;
        private const uint ArgbGreenMask = 0x0000ff00;
        private const uint ArgbBlueMask = 0x000000ff;

    }
}
