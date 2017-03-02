using DereTore.Exchange.Archive.ACB.Serialization;

namespace DereTore.Apps.AcbMaker.Cgss {
    public sealed class SynthTable : UtfRowBase {

        [UtfField(0)]
        public byte Type;
        [UtfField(1)]
        public string VoiceLimitGroupName;
        [UtfField(2)]
        public ushort CommandIndex;
        [UtfField(3)]
        public byte[] ReferenceItems;
        [UtfField(4)]
        public byte[] LocalAisacs;
        [UtfField(5)]
        public ushort GlobalAisacStartIndex;
        [UtfField(6)]
        public ushort GlobalAisacNumRefs;
        [UtfField(7)]
        public ushort ControlWorkArea1;
        [UtfField(8)]
        public ushort ControlWorkArea2;
        [UtfField(9)]
        public byte[] TrackValues;
        [UtfField(10)]
        public ushort ParameterPallet;
        [UtfField(11)]
        public ushort ActionTrackStartIndex;
        [UtfField(12)]
        public ushort NumActionTracks;

    }
}
