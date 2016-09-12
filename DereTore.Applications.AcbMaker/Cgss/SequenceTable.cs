using DereTore.ACB.Serialization;

namespace DereTore.Applications.AcbMaker.Cgss {
    public sealed class SequenceTable : UtfRowBase {

        [UtfField(0)]
        public ushort PlaybackRatio;
        [UtfField(1)]
        public ushort NumTracks;
        [UtfField(2)]
        public byte[] TrackIndex;
        [UtfField(3)]
        public ushort CommandIndex;
        [UtfField(4)]
        public byte[] LocalAisacs;
        [UtfField(5)]
        public ushort GlobalAisacStartIndex;
        [UtfField(6)]
        public ushort GlobalAisacNumRefs;
        [UtfField(7)]
        public ushort ParameterPallet;
        [UtfField(8)]
        public ushort ActionTrackStartIndex;
        [UtfField(9)]
        public ushort NumActionTracks;
        [UtfField(10)]
        public byte[] TrackValues;
        [UtfField(11)]
        public byte Type;
        [UtfField(12)]
        public ushort ControlWorkArea1;
        [UtfField(13)]
        public ushort ControlWorkArea2;

    }
}
