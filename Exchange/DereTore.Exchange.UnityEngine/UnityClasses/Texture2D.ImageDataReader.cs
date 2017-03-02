using System;
using System.IO;
using System.Linq;

namespace DereTore.Exchange.UnityEngine.UnityClasses {
    partial class Texture2D {

        private void ReadImageData(AssetPreloadData preloadData) {
            var sourceFile = preloadData.SourceFile;
            var reader = sourceFile.AssetReader;

            if (!string.IsNullOrEmpty(FullFileName)) {
                FullFileName = Path.Combine(Path.GetDirectoryName(sourceFile.FullFileName) ?? string.Empty, FullFileName.Replace("archive:/", string.Empty));
                var fileExists = File.Exists(FullFileName);
                if (!fileExists) {
                    FullFileName = Path.Combine(Path.GetDirectoryName(sourceFile.FullFileName) ?? string.Empty, Path.GetFileName(FullFileName));
                    fileExists = File.Exists(FullFileName);
                }
                if (fileExists) {
                    ImageData = new byte[ImageDataSize];
                    using (var imageFileReader = new BinaryReader(File.OpenRead(FullFileName))) {
                        imageFileReader.BaseStream.Position = Offset;
                        imageFileReader.Read(ImageData, 0, ImageDataSize);
                    }
                } else {
                    throw new FileNotFoundException("Unexpected branch.");
                }
            } else {
                ImageData = new byte[ImageDataSize];
                reader.Read(ImageData, 0, ImageDataSize);
            }

            var textureFormat = (TextureFormat)Format;
            switch (textureFormat) {
                case TextureFormat.Alpha8:
                    var bytes = Enumerable.Repeat<byte>(0xFF, ImageDataSize * 4).ToArray();
                    for (var i = 0; i < ImageDataSize; i++) {
                        bytes[i * 4] = ImageData[i];
                    }
                    ImageData = bytes;
                    ImageDataSize = ImageDataSize * 4;
                    DdsMiscFlags2 = 0x41;
                    DdsRgbBitCount = 0x20;
                    DdsRBitMask = 0xFF00;
                    DdsGBitMask = 0xFF0000;
                    DdsBBitMask = 0xFF000000;
                    DdsABitMask = 0xFF;
                    break;
                case TextureFormat.ARGB4444:
                    FixupXbox360(sourceFile);
                    DdsMiscFlags2 = 0x41;
                    DdsRgbBitCount = 0x10;
                    DdsRBitMask = 0xF00;
                    DdsGBitMask = 0xF0;
                    DdsBBitMask = 0xF;
                    DdsABitMask = 0xF000;
                    break;
                case TextureFormat.RGB24:
                    DdsMiscFlags2 = 0x40;
                    DdsRgbBitCount = 0x18;
                    DdsRBitMask = 0xFF;
                    DdsGBitMask = 0xFF00;
                    DdsBBitMask = 0xFF0000;
                    DdsABitMask = 0x0;
                    break;
                case TextureFormat.RGBA32:
                    DdsMiscFlags2 = 0x41;
                    DdsRgbBitCount = 0x20;
                    DdsRBitMask = 0xFF;
                    DdsGBitMask = 0xFF00;
                    DdsBBitMask = 0xFF0000;
                    DdsABitMask = 0xFF000000;
                    break;
                case TextureFormat.ARGB32:
                    DdsMiscFlags2 = 0x41;
                    DdsRgbBitCount = 0x20;
                    DdsRBitMask = 0xFF00;
                    DdsGBitMask = 0xFF0000;
                    DdsBBitMask = 0xFF000000;
                    DdsABitMask = 0xFF;
                    break;
                case TextureFormat.RGB565:
                    FixupXbox360(sourceFile);
                    DdsMiscFlags2 = 0x40;
                    DdsRgbBitCount = 0x10;
                    DdsRBitMask = 0xF800;
                    DdsGBitMask = 0x7E0;
                    DdsBBitMask = 0x1F;
                    DdsABitMask = 0x0;
                    break;
                case TextureFormat.R16:
                    break;
                case TextureFormat.DXT1:
                    FixupXbox360(sourceFile);
                    if (HasMipMap) {
                        DdsPitchOrLinearSize = Height * Width / 2;
                    }
                    DdsMiscFlags2 = 0x4;
                    DdsFourCC = 0x31545844;
                    DdsRgbBitCount = 0x0;
                    DdsRBitMask = 0x0;
                    DdsGBitMask = 0x0;
                    DdsBBitMask = 0x0;
                    DdsABitMask = 0x0;
                    break;
                case TextureFormat.DXT5:
                    FixupXbox360(sourceFile);
                    if (HasMipMap) {
                        DdsPitchOrLinearSize = Height * Width / 2;
                    }
                    DdsMiscFlags2 = 0x4;
                    DdsFourCC = 0x35545844;
                    DdsRgbBitCount = 0x0;
                    DdsRBitMask = 0x0;
                    DdsGBitMask = 0x0;
                    DdsBBitMask = 0x0;
                    DdsABitMask = 0x0;
                    break;
                case TextureFormat.RGBA4444:
                    DdsMiscFlags2 = 0x41;
                    DdsRgbBitCount = 0x10;
                    DdsRBitMask = 0xF000;
                    DdsGBitMask = 0xF00;
                    DdsBBitMask = 0xF0;
                    DdsABitMask = 0xF;
                    break;
                case TextureFormat.BGRA32:
                    DdsMiscFlags2 = 0x41;
                    DdsRgbBitCount = 0x20;
                    DdsRBitMask = 0xFF0000;
                    DdsGBitMask = 0xFF00;
                    DdsBBitMask = 0xFF;
                    DdsABitMask = 0xFF000000;
                    break;
                case TextureFormat.ETC_RGB4:
                    PvrPixelFormat = 0x16;
                    break;
                case TextureFormat.ETC2_RGB:
                    PvrPixelFormat = 22;
                    break;
                case TextureFormat.ETC2_RGBA1:
                    PvrPixelFormat = 24;
                    break;
                case TextureFormat.ETC2_RGBA8:
                    PvrPixelFormat = 23;
                    break;
                default:
                    throw new NotSupportedException($"Not supported: {textureFormat}");
            }
        }

        private void FixupXbox360(AssetsFile sourceFile) {
            if (sourceFile.Platform != UnityPlatformID.Xbox360) {
                return;
            }
            for (var i = 0; i < ImageDataSize / 2; i++) {
                var b0 = ImageData[i * 2];
                ImageData[i * 2] = ImageData[i * 2 + 1];
                ImageData[i * 2 + 1] = b0;
            }
        }

    }
}
