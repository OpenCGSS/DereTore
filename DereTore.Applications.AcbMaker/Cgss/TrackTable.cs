using DereTore.ACB.Serialization;

namespace DereTore.Applications.AcbMaker.Cgss {
    public sealed class TrackTable : UtfRowBase {

        [UtfField(0)]
        public ushort EventIndex;
        [UtfField(1)]
        public ushort CommandIndex;
        [UtfField(2)]
        public byte[] LocalAisacs;
        [UtfField(3)]
        public ushort GlobalAisacStartIndex;
        [UtfField(4)]
        public ushort GlobalAisacNumRefs;
        [UtfField(5)]
        public ushort ParameterPallet;
        [UtfField(6)]
        public byte TargetType;
        [UtfField(7)]
        public string TargetName;
        [UtfField(8)]
        public uint TargetId;
        [UtfField(9)]
        public string TargetAcbName;
        [UtfField(10)]
        public byte Scope;
        [UtfField(11)]
        public ushort TargetTrackNo;

    }
}
