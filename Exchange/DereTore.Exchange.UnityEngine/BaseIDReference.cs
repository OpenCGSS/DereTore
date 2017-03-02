using System.Collections.Generic;

namespace DereTore.Exchange.UnityEngine {
    public static class BaseIDReference {

        static BaseIDReference() {
            Initialize();
        }

        private static void Initialize() {
            NameList[0] = "AABB";
            NameList[5] = "AnimationClip";
            NameList[19] = "AnimationCurve";
            NameList[49] = "Array";
            NameList[55] = "Base";
            NameList[60] = "BitField";
            NameList[76] = "bool";
            NameList[81] = "char";
            NameList[86] = "ColorRGBA";
            NameList[106] = "data";
            NameList[138] = "FastPropertyName";
            NameList[155] = "first";
            NameList[161] = "float";
            NameList[167] = "Font";
            NameList[172] = "GameObject";
            NameList[183] = "Generic Mono";
            NameList[208] = "GUID";
            NameList[222] = "int";
            NameList[241] = "map";
            NameList[245] = "Matrix4x4f";
            NameList[262] = "NavMeshSettings";
            NameList[263] = "MonoBehaviour";
            NameList[277] = "MonoScript";
            NameList[299] = "m_Curve";
            NameList[349] = "m_Enabled";
            NameList[374] = "m_GameObject";
            NameList[427] = "m_Name";
            NameList[490] = "m_Script";
            NameList[519] = "m_Type";
            NameList[526] = "m_Version";
            NameList[543] = "pair";
            NameList[548] = "PPtr<Component>";
            NameList[564] = "PPtr<GameObject>";
            NameList[581] = "PPtr<Material>";
            NameList[616] = "PPtr<MonoScript>";
            NameList[633] = "PPtr<Object>";
            NameList[688] = "PPtr<Texture>";
            NameList[702] = "PPtr<Texture2D>";
            NameList[718] = "PPtr<Transform>";
            NameList[741] = "Quaternionf";
            NameList[753] = "Rectf";
            NameList[778] = "second";
            NameList[795] = "size";
            NameList[800] = "SInt16";
            NameList[814] = "int64";
            NameList[840] = "string";
            NameList[874] = "Texture2D";
            NameList[884] = "Transform";
            NameList[894] = "TypelessData";
            NameList[907] = "UInt16";
            NameList[928] = "UInt8";
            NameList[934] = "unsigned int";
            NameList[981] = "vector";
            NameList[988] = "Vector2f";
            NameList[997] = "Vector3f";
            NameList[1006] = "Vector4f";
        }

        public static string GetName(int id) => NameList.ContainsKey(id) ? NameList[id] : null;

        private static readonly Dictionary<int, string> NameList = new Dictionary<int, string>();

    }
}
