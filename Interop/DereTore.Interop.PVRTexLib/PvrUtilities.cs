using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace DereTore.Interop.PVRTexLib {
    public static class PvrUtilities {

        public static byte[] GetPvrTextureFromImage(Bitmap bitmap) {
            using (var bmp = (Bitmap)bitmap.Clone()) {
                bmp.RotateFlip(RotateFlipType.RotateNoneFlipY);

                return GetPvrTextureFromImageInternal(bmp);
            }
        }

        private static byte[] GetPvrTextureFromImageInternal(Bitmap bitmap) {
            var bitmapData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);

            NativeMethods.MpvrCompressPvrTextureFrom32bppArgb(bitmapData.Scan0, bitmapData.Width, bitmapData.Height, bitmapData.Stride, DefaultPvrMipLevels, out var textureData, out var textureDataSize);

            var result = new byte[textureDataSize];

            Marshal.Copy(textureData, result, 0, textureDataSize);

            bitmap.UnlockBits(bitmapData);

            NativeMethods.MpvrFreeTexture(textureData);

            return result;
        }

        private static readonly int DefaultPvrMipLevels = 7;

    }
}
