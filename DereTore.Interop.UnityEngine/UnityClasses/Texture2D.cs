using System;
using DereTore.Interop.UnityEngine.Extensions;

namespace DereTore.Interop.UnityEngine.UnityClasses {
    public sealed partial class Texture2D {

        public Texture2D(AssetPreloadData data) {
            Initialize(data);
        }

        [UnitySerializedField("m_Name")]
        public string Name;
        [UnitySerializedField("m_Width")]
        public int Width;
        [UnitySerializedField("m_Height")]
        public int Height;
        [UnitySerializedField("m_CompleteImageSize")]
        // stored
        public int CompleteImageSize;
        [UnitySerializedField("m_TextureFormat")]
        public int Format;
        [UnitySerializedField("m_MipMap")]
        public bool HasMipMap;
        [UnitySerializedField("m_IsReadable")]
        public bool IsReadable;
        [UnitySerializedField("m_ReadAllowed")]
        public bool IsReadAllowed;
        [UnitySerializedField("m_ImageCount")]
        public int ImageCount;
        [UnitySerializedField("m_TextureDimension")]
        public int TextureDimension;
        [UnitySerializedField("m_TextureSettings")]
        public object TextureSettings;
        [UnitySerializedField("m_FilterMode")]
        public int FilterMode;
        [UnitySerializedField("m_Aniso")]
        public int AnisotropicLevel;
        [UnitySerializedField("m_MipBias")]
        public float MipBias;
        [UnitySerializedField("m_WrapMode")]
        public int WrapMode;
        [UnitySerializedField("m_LightmapFormat")]
        public int LightmapFormat;
        [UnitySerializedField("m_ColorSpace")]
        public int ColorSpace;
        [UnitySerializedField("m_StreamData")]
        public object StreamData;
        [UnitySerializedField("image data")]
        public byte[] ImageData;

        // TextureConverter (???)
        public int QFormat;
        // Texture data (decompressed?)
        // calculated
        public int ImageDataSize;

        // Shared by DDS and PVR
        public int MipMapCount = 1;

        public byte[] DdsMagic = { 0x44, 0x44, 0x53, 0x20, 0x7c };
        public int DdsFlags = 0x1 | 0x2 | 0x4 | 0x1000;
        public int DdsPitchOrLinearSize;
        public int DdsMiscFlags = 0x20;
        public int DdsMiscFlags2;
        public int DdsFourCC;
        public int DdsRgbBitCount;
        public uint DdsRBitMask;
        public uint DdsGBitMask;
        public uint DdsBBitMask;
        public uint DdsABitMask;
        public int DdsCaps = 0x1000;
        public int DdsCaps2;

        public int PvrVersion = 0x03525650; // '3rvp'
        public int PvrFlags;
        public long PvrPixelFormat;
        public int PvrColorSpace;
        public int PvrChannelType;
        public int PvrDepth = 1;
        public int PvrSurfaceCount = 1;
        public int PvrFaceCount = 1;
        public int PvrMetadataSize;

        public TextureType TextureType { get; private set; }

        public uint Offset { get; private set; }

        public uint Size { get; private set; }

        public string FullFileName { get; private set; }

        private void Initialize(AssetPreloadData preloadData) {
            var sourceFile = preloadData.SourceFile;
            var reader = sourceFile.AssetReader;
            reader.Position = preloadData.Offset;

            if (sourceFile.Platform == UnityPlatformID.UnityPackage) {
                var objectHideFlags = reader.ReadUInt32();
                var prefabParentObject = sourceFile.ReadPPtr();
                var prefabInternal = sourceFile.ReadPPtr();
            }

            var nameLength = reader.ReadInt32();
            Name = reader.ReadAlignedUtf8String(nameLength);
            Width = reader.ReadInt32();
            Height = reader.ReadInt32();
            CompleteImageSize = reader.ReadInt32();
            Format = reader.ReadInt32();

            if (sourceFile.RawVersion[0] < 5 || (sourceFile.RawVersion[0] == 5 && sourceFile.RawVersion[1] < 2)) {
                HasMipMap = reader.ReadBoolean();
            } else {
                DdsFlags |= 0x20000;
                //is this with or without main image?
                MipMapCount = reader.ReadInt32();
                DdsCaps |= 0x400008;
            }

            //2.6.0 and up
            IsReadable = reader.ReadBoolean();
            //3.0.0 and up
            IsReadAllowed = reader.ReadBoolean();
            reader.AlignStream(4);

            ImageCount = reader.ReadInt32();
            TextureDimension = reader.ReadInt32();
            //m_TextureSettings
            FilterMode = reader.ReadInt32();
            AnisotropicLevel = reader.ReadInt32();
            MipBias = reader.ReadSingle();
            WrapMode = reader.ReadInt32();

            if (sourceFile.RawVersion[0] >= 3) {
                LightmapFormat = reader.ReadInt32();
                if (sourceFile.RawVersion[0] >= 4 || sourceFile.RawVersion[1] >= 5) {
                    //3.5.0 and up (hozuki: >=4.5.0?)
                    ColorSpace = reader.ReadInt32();
                }
            }

            ImageDataSize = reader.ReadInt32();

            if (HasMipMap) {
                DdsFlags |= 0x20000;
                MipMapCount = Convert.ToInt32(Math.Log(Math.Max(Width, Height)) / Math.Log(2));
                DdsCaps |= 0x400008;
            }

            if (ImageDataSize == 0 && ((sourceFile.RawVersion[0] == 5 && sourceFile.RawVersion[1] >= 3) || sourceFile.RawVersion[0] > 5)) {
                //5.3.0 and up
                Offset = reader.ReadUInt32();
                Size = reader.ReadUInt32();
                ImageDataSize = (int)Size;
                var pathLength = reader.ReadInt32();
                FullFileName = reader.ReadAlignedUtf8String(pathLength);
            }

            ReadImageInfo(preloadData);
            ReadImageData(preloadData);
        }

        private void ReadImageInfo(AssetPreloadData preloadData) {
            preloadData.InfoText = $"Width: {Width}\nHeight: {Height}\nFormat: ";
            preloadData.ExportSize = ImageDataSize;

            var textureFormat = (TextureFormat)Format;
            preloadData.InfoText += textureFormat.ToString();
            switch (textureFormat) {
                case TextureFormat.Alpha8:
                case TextureFormat.ARGB4444:
                case TextureFormat.RGB24:
                case TextureFormat.RGBA32:
                case TextureFormat.ARGB32:
                case TextureFormat.RGB565:
                case TextureFormat.DXT1:
                case TextureFormat.DXT5:
                case TextureFormat.RGBA4444:
                case TextureFormat.BGRA32:
                    preloadData.Extension = ".dds";
                    TextureType = TextureType.DDS;
                    preloadData.ExportSize += 128;
                    break;
                case TextureFormat.DXT1Crunched:
                case TextureFormat.DXT5Crunched:
                    preloadData.Extension = ".crn";
                    TextureType = TextureType.CRN;
                    break;
                case TextureFormat.YUY2:
                case TextureFormat.PVRTC_RGB2:
                case TextureFormat.PVRTC_RGBA2:
                case TextureFormat.PVRTC_RGB4:
                case TextureFormat.PVRTC_RGBA4:
                case TextureFormat.ETC_RGB4:
                case TextureFormat.ETC2_RGB:
                case TextureFormat.ETC2_RGBA1:
                case TextureFormat.ETC2_RGBA8:
                case TextureFormat.EAC_R:
                case TextureFormat.EAC_R_SIGNED:
                case TextureFormat.EAC_RG:
                case TextureFormat.EAC_RG_SIGNED:
                    preloadData.Extension = ".pvr";
                    TextureType = TextureType.PVR;
                    preloadData.ExportSize += 52;
                    break;
                case TextureFormat.RHalf:
                case TextureFormat.RGHalf:
                case TextureFormat.RGBAHalf:
                case TextureFormat.RFloat:
                case TextureFormat.RGFloat:
                case TextureFormat.RGBAFloat:
                case TextureFormat.ATC_RGB4:
                case TextureFormat.ATC_RGBA8:
                    preloadData.Extension = ".ktx";
                    TextureType = TextureType.KTX;
                    preloadData.ExportSize += 68;
                    break;
                case TextureFormat.ASTC_RGB_4x4:
                case TextureFormat.ASTC_RGB_5x5:
                case TextureFormat.ASTC_RGB_6x6:
                case TextureFormat.ASTC_RGB_8x8:
                case TextureFormat.ASTC_RGB_10x10:
                case TextureFormat.ASTC_RGB_12x12:
                case TextureFormat.ASTC_RGBA_4x4:
                case TextureFormat.ASTC_RGBA_5x5:
                case TextureFormat.ASTC_RGBA_6x6:
                case TextureFormat.ASTC_RGBA_8x8:
                case TextureFormat.ASTC_RGBA_10x10:
                case TextureFormat.ASTC_RGBA_12x12:
                    preloadData.Extension = ".astc";
                    preloadData.ExportSize += 10;
                    break;
                default:
                    preloadData.Extension = $"_{textureFormat}.tex";
                    TextureType = TextureType.Unknown;
                    break;
            }

            switch (FilterMode) {
                case UnityEngine.FilterMode.Point:
                    preloadData.InfoText += "\nFilter Mode: Point";
                    break;
                case UnityEngine.FilterMode.Bilinear:
                    preloadData.InfoText += "\nFilter Mode: Bilinear";
                    break;
                case UnityEngine.FilterMode.Trilinear:
                    preloadData.InfoText += "\nFilter Mode: Trilinear";
                    break;
            }

            preloadData.InfoText += $"\nAnisotropic level: {AnisotropicLevel}\nMip map bias: {MipBias}";

            switch (WrapMode) {
                case UnityEngine.WrapMode.Repeat:
                    preloadData.InfoText += "\nWrap mode: Repeat";
                    break;
                case UnityEngine.WrapMode.Clamp:
                    preloadData.InfoText += "\nWrap mode: Clamp";
                    break;
            }
        }

    }
}
