using System;
using System.Text;
using DereTore.Interop.UnityEngine.Extensions;
using DereTore.Interop.UnityEngine.UnityClasses;

namespace DereTore.Interop.UnityEngine {
    partial class AssetsFile {

        private void Initialize() {
            var reader = AssetReader;
            var tableSize = reader.ReadInt32();
            var dataEnd = reader.ReadInt32();
            FormatSignature = reader.ReadInt32();
            var dataOffset = reader.ReadInt32();
            //reference itself because sharedFileIDs start from 1
            SharedAssetsList[0].FileName = FileName;

            InitializeVersionAndPlatform(dataEnd, tableSize);
            FixReaderEndianess();
            InitializePlatformName();
            InitializeClassList();

            if (FormatSignature >= 7 && FormatSignature < 14) {
                reader.Position += 4;
            }

            InitializePreloadDataTable(dataOffset);
            FillRawVersion();
            FixPreloadFiles();
            InitializeSharedFileTable();
        }

        private void InitializeVersionAndPlatform(int dataEnd, int tableSize) {
            var reader = AssetReader;
            switch (FormatSignature) {
                case 6:
                    //2.5.0 - 2.6.1
                    reader.Position = (dataEnd - tableSize);
                    reader.Position += 1;
                    break;
                case 7:
                    //3.0.0 beta
                    reader.Position = (dataEnd - tableSize);
                    reader.Position += 1;
                    VersionString = reader.ReadAsciiStringToNull();
                    break;
                case 8:
                    //3.0.0 - 3.4.2
                    reader.Position = (dataEnd - tableSize);
                    reader.Position += 1;
                    VersionString = reader.ReadAsciiStringToNull();
                    Platform = reader.ReadInt32();
                    break;
                case 9:
                    //3.5.0 - 4.6.x
                    reader.Position += 4;
                    VersionString = reader.ReadAsciiStringToNull();
                    Platform = reader.ReadInt32();
                    break;
                case 14:
                case 15:
                    // 14: 5.0.0 beta and final
                    // 15: 5.0.1 and up
                    reader.Position += 4;
                    VersionString = reader.ReadAsciiStringToNull();
                    Platform = reader.ReadInt32();
                    HasBaseDefinitions = reader.ReadBoolean();
                    break;
                default:
                    throw new FormatException($"Unsupported Unity asset format. Signature = {FormatSignature}");
            }
        }

        private void FixReaderEndianess() {
            var reader = AssetReader;
            var platform = Platform;
            if (platform > 255 || platform < 0) {
                Platform = DereToreHelper.SwapEndian(platform);
                reader.Endian = Endian.LittleEndian;
            }
        }

        private void InitializePlatformName() {
            string platformName;
            switch (Platform) {
                case UnityPlatformID.UnityPackage:
                    platformName = "Unity Package";
                    break;
                case UnityPlatformID.OSX:
                    platformName = "MacOS X";
                    break;
                case UnityPlatformID.PC:
                    platformName = "PC";
                    break;
                case UnityPlatformID.Web:
                    platformName = "Web";
                    break;
                case UnityPlatformID.WebStreamed:
                    platformName = "Web (streamed)";
                    break;
                case UnityPlatformID.iOS:
                    platformName = "iOS";
                    break;
                case UnityPlatformID.PlayStation3:
                    platformName = "PlayStation 3";
                    break;
                case UnityPlatformID.Xbox360:
                    platformName = "Xbox 360";
                    break;
                case UnityPlatformID.Android:
                    platformName = "Android";
                    break;
                case UnityPlatformID.NaCl:
                    platformName = "Google NaCl";
                    break;
                case UnityPlatformID.WindowsPhone8:
                    platformName = "Windows Phone 8";
                    break;
                case UnityPlatformID.Linux:
                    platformName = "Linux";
                    break;
                default:
                    platformName = "Unknown";
                    break;
            }
            PlatformName = platformName;
        }

        private void InitializeClassList() {
            var reader = AssetReader;
            var baseCount = reader.ReadInt32();
            var stringBuilder = new StringBuilder();
            for (var i = 0; i < baseCount; ++i) {
                if (FormatSignature < 14) {
                    var classID = reader.ReadInt32();
                    var baseType = reader.ReadAsciiStringToNull();
                    var baseName = reader.ReadAsciiStringToNull();
                    reader.Position += 20;
                    var memberCount = reader.ReadInt32();

                    for (var m = 0; m < memberCount; ++m) {
                        stringBuilder.Clear();
                        ReadClassStructure(stringBuilder, 1);
                    }

                    var clazz = new ClassDescriptor {
                        ID = classID,
                        Text = baseType + " " + baseName,
                        Members = stringBuilder.ToString()
                    };
                    ClassStructures.Add(classID, clazz);
                } else {
                    ReadClassStructure5();
                }
            }
        }

        private void InitializePreloadDataTable(int dataOffset) {
            var reader = AssetReader;
            var assetCount = reader.ReadInt32();
            //format for unique ID
            var assetIDfmt = "D" + assetCount.ToString().Length;

            for (var i = 0; i < assetCount; ++i) {
                //each table entry is aligned individually, not the whole table
                if (FormatSignature >= 14) {
                    reader.AlignStream(4);
                }

                var asset = new AssetPreloadData();
                asset.PathID = FormatSignature < 14 ? reader.ReadInt32() : reader.ReadInt64();
                asset.Offset = reader.ReadInt32();
                asset.Offset += dataOffset;
                asset.Size = reader.ReadInt32();
                asset.Type1 = reader.ReadInt32();
                asset.Type2 = reader.ReadUInt16();
                reader.Position += 2;
                if (FormatSignature >= 15) {
                    var unknownByte = reader.ReadByte();
                    //this is a single byte, not an int32
                    //the next entry is aligned after this
                    //but not the last!
                    if (unknownByte != 0) {
                        //bool investigate = true;
                    }
                }

                var typeString = ClassIDReference.GetName(asset.Type2);
                if (typeString != null) {
                    asset.TypeString = typeString;
                    asset.Type = (PreloadDataType)asset.Type2;
                } else {
                    asset.TypeString = "Unknown";
                    asset.Type = PreloadDataType.Unknown;
                }

                asset.UniqueID = i.ToString(assetIDfmt);

                asset.ExportSize = asset.Size;
                asset.SourceFile = this;

                PreloadTable.Add(asset.PathID, asset);

                // Read BuildSettings to get version for unity 2.x files
                if (asset.Type2 == 141 && FormatSignature == 6) {
                    var nextAsset = reader.Position;
                    var buildSettings = new BuildSettings(asset);
                    VersionString = buildSettings.VersionString;
                    reader.Position = nextAsset;
                }
            }
        }

        private void FillRawVersion() {
            BuildTypes = VersionString.Split(MainVersionSeparators, StringSplitOptions.RemoveEmptyEntries);
            var strver = VersionString.Split(BuildVersionSeparators, StringSplitOptions.RemoveEmptyEntries);
            RawVersion = Array.ConvertAll(strver, int.Parse);
        }

        private void FixPreloadFiles() {
            var reader = AssetReader;
            if (FormatSignature >= 14) {
                //this looks like a list of assets that need to be preloaded in memory before anytihng else
                var someCount = reader.ReadInt32();
                for (var i = 0; i < someCount; i++) {
                    var num1 = reader.ReadInt32();
                    reader.AlignStream(4);
                    var pathID = reader.ReadInt64();
                }
            }
        }

        private void InitializeSharedFileTable() {
            var reader = AssetReader;
            var sharedFileCount = reader.ReadInt32();
            for (var i = 0; i < sharedFileCount; i++) {
                var shared = new SharedAssetInfo();
                shared.ArchiveName = reader.ReadAsciiStringToNull();
                reader.Position += 20;
                // relative path
                var sharedFileName = reader.ReadAsciiStringToNull();
                shared.FileName = sharedFileName.Replace("/", "\\");
                SharedAssetsList.Add(shared);
            }
        }

        private static readonly string[] MainVersionSeparators = { ".", "0", "1", "2", "3", "4", "5", "6", "7", "8", "9" };
        private static readonly string[] BuildVersionSeparators = { ".", "a", "b", "c", "d", "e", "f", "g", "h", "i", "j", "k", "l", "m", "n", "o", "p", "q", "r", "s", "t", "u", "v", "w", "x", "y", "z", "\n" };

    }
}
