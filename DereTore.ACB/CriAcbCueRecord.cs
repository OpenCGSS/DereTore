namespace DereTore.ACB {
    public sealed class CriAcbCueRecord {

        public uint CueId { get; internal set; }
        public byte ReferenceType { get; internal set; }
        public ushort ReferenceIndex { get; internal set; }

        public bool IsWaveformIdentified { get; internal set; }
        public ushort WaveformIndex { get; internal set; }
        public ushort WaveformId { get; internal set; }
        public byte EncodeType { get; internal set; }
        public bool IsStreaming { get; internal set; }

        public string CueName { get; internal set; }

    }
}