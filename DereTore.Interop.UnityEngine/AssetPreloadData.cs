using DereTore.Interop.UnityEngine.UnityClasses;

namespace DereTore.Interop.UnityEngine {
    public sealed class AssetPreloadData {

        [UnitySerializedField("m_PathID")]
        public long PathID;
        public int Offset { get; internal set; }
        public int Size { get; internal set; }
        public int Type1 { get; internal set; }
        public ushort Type2 { get; internal set; }

        public string TypeString { get; internal set; }
        public PreloadDataType Type { get; internal set; }
        public int ExportSize { get; internal set; }
        public string InfoText { get; internal set; }
        public string Extension { get; internal set; }

        public AssetsFile SourceFile { get; internal set; }
        public string UniqueID { get; internal set; }

    }
}
