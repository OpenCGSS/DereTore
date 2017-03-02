using DereTore.Exchange.Archive.ACB.Serialization;

namespace DereTore.Apps.AcbMaker.Cgss {
    public sealed class CueTable : UtfRowBase {

        [UtfField(0)]
        public uint CueId;
        [UtfField(1)]
        public byte ReferenceType;
        [UtfField(2)]
        public ushort ReferenceIndex;
        [UtfField(3)]
        public string UserData;
        [UtfField(4)]
        public ushort WorkSize;
        [UtfField(5)]
        public byte[] AisacControlMap;
        [UtfField(6)]
        public uint Length;
        [UtfField(7)]
        public byte NumAisacControlMaps;
        [UtfField(8)]
        public byte HeaderVisibility;

    }
}
