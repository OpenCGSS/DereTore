using System.Collections.Generic;
using System.IO;
using System.Text;
using DereTore.Common;
using DereTore.Exchange.UnityEngine.Extensions;

namespace DereTore.Exchange.UnityEngine {
    public sealed partial class AssetsFile {

        public AssetsFile(string fullFileName, EndianBinaryReader reader) {
            AssetReader = reader;
            FullFileName = fullFileName;
            FileName = Path.GetFileName(fullFileName);
            Initialize();
        }

        public string FullFileName { get; }

        public string FileName { get; }

        public int FormatSignature { get; private set; }

        public string VersionString { get; private set; }

        public string[] BuildTypes { get; private set; }

        public int Platform { get; private set; }

        public string PlatformName { get; private set; }

        public EndianBinaryReader AssetReader { get; }

        public Dictionary<long, AssetPreloadData> PreloadTable { get; } = new Dictionary<long, AssetPreloadData>();

        public List<AssetPreloadData> ExportableAssets { get; } = new List<AssetPreloadData>();

        public List<SharedAssetInfo> SharedAssetsList { get; } = new List<SharedAssetInfo> {
            new SharedAssetInfo()
        };

        public bool HasBaseDefinitions { get; private set; }

        public SortedDictionary<int, ClassDescriptor> ClassStructures { get; } = new SortedDictionary<int, ClassDescriptor>();

        // ------- Properties for serialization only ------
        public int TableSize { get; internal set; }

        public int DataEnd { get; internal set; }

        internal AssetsFile() {
        }

        private void ReadClassStructure(StringBuilder stringBuilder, int level) {
            var reader = AssetReader;
            var varType = reader.ReadAsciiStringToNull();
            var varName = reader.ReadAsciiStringToNull();
            var size = reader.ReadInt32();
            var index = reader.ReadInt32();
            var isArray = reader.ReadInt32();
            var num0 = reader.ReadInt32();
            var num1 = reader.ReadInt16();
            var num2 = reader.ReadInt16();
            var childrenCount = reader.ReadInt32();
            stringBuilder.AppendFormat("{0}{1} {2} {3}\n", new string('\t', level), varType, varName, size);
            for (var i = 0; i < childrenCount; i++) {
                ReadClassStructure(stringBuilder, level + 1);
            }
        }

        private void ReadClassStructure5() {
            var reader = AssetReader;
            var classID = reader.ReadInt32();
            if (classID < 0) {
                reader.Position += 16;
            }
            reader.Position += 16;

            if (!HasBaseDefinitions) {
                return;
            }

            var varCount = reader.ReadInt32();
            var stringSize = reader.ReadInt32();

            reader.Position += varCount * 24;
            var varStrings = Encoding.UTF8.GetString(reader.ReadBytes(stringSize));
            var className = string.Empty;
            var classVarStr = new StringBuilder();

            //build Class Structures
            reader.Position -= varCount * 24 + stringSize;
            for (var i = 0; i < varCount; ++i) {
                var num0 = reader.ReadUInt16();
                var level = reader.ReadByte();
                var isArray = reader.ReadBoolean();

                var varTypeIndex = reader.ReadUInt16();
                var test = reader.ReadUInt16();
                string varTypeStr;
                if (test == 0) {
                    //varType is an offset in the string block
                    varTypeStr = varStrings.Substring(varTypeIndex, varStrings.IndexOf('\0', varTypeIndex) - varTypeIndex);
                } else {
                    //varType is an index in an internal strig array
                    varTypeStr = BaseIDReference.GetName(varTypeIndex) ?? varTypeIndex.ToString();
                }

                var varNameIndex = reader.ReadUInt16();
                test = reader.ReadUInt16();
                string varNameStr;
                if (test == 0) {
                    varNameStr = varStrings.Substring(varNameIndex, varStrings.IndexOf('\0', varNameIndex) - varNameIndex);
                } else {
                    varNameStr = BaseIDReference.GetName(varNameIndex) ?? varNameIndex.ToString();
                }

                var size = reader.ReadInt32();
                var index = reader.ReadInt32();
                var num1 = reader.ReadInt32();

                if (index == 0) {
                    className = varTypeStr + " " + varNameStr;
                } else {
                    classVarStr.AppendFormat("{0}{1} {2} {3}\r", new string('\t', level), varTypeStr, varNameStr, size);
                }
            }
            reader.Position += stringSize;

            var aClass = new ClassDescriptor {
                ID = classID,
                Text = className,
                Members = classVarStr.ToString()
            };
            ClassStructures.Add(classID, aClass);
        }

        internal int[] RawVersion = new int[4];

    }
}
